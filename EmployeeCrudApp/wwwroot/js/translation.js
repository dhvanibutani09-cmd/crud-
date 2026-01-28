class TranslationManager {
    constructor() {
        this.apiEndpoint = '/api/translation/translate';

        // 1. Storage or Browser Detection
        const storedLang = localStorage.getItem('app_language');
        const browserLang = navigator.language.split('-')[0];
        // Supported langs in our UI (expanded for regional support)
        const supported = ['en', 'hi', 'gu', 'mr', 'bn', 'ta', 'es', 'fr', 'de'];


        // Auto-detect: Use stored, or browser if supported, else default to 'en'
        this.currentLang = storedLang || (supported.includes(browserLang) ? browserLang : 'en');

        this.isTranslating = false;
        this.cache = new Map();
        this.cache.set('en', new Map());

        this.observer = null;
        this.debouncedTranslate = this.debounce(this.processQueue.bind(this), 300);
        this.mutationQueue = new Set(); // Stores nodes to translate

        document.addEventListener('DOMContentLoaded', () => {
            this.setupDropdown();
            // Start observing immediately for any dynamic content
            this.setupObserver();

            if (this.currentLang !== 'en') {
                this.translatePage();
            }
        });
    }

    setupDropdown() {
        const select = document.getElementById('language-selector');
        if (select) {
            select.value = this.currentLang;
            select.addEventListener('change', (e) => this.setLanguage(e.target.value));
        }
    }

    setupObserver() {
        // Watch for changes in the body
        this.observer = new MutationObserver((mutations) => {
            if (this.currentLang === 'en') return;

            let hasRelevantMutation = false;

            mutations.forEach(mutation => {
                // If we are actively translating, ignore mutations to prevent loops
                // However, distinguishing our changes from app changes is tricky.

                if (mutation.type === 'childList') {
                    mutation.addedNodes.forEach(node => {
                        if (this.isValidNode(node)) {
                            this.collectTextNodes(node).forEach(bgNode => this.mutationQueue.add(bgNode));
                            hasRelevantMutation = true;
                        }
                    });
                } else if (mutation.type === 'characterData') {
                    // Text node changed content directly
                    const node = mutation.target;
                    // If this change was NOT done by us (we can check a flag, or rudimentary check)
                    if (!node._isApplyingTranslation && this.isValidTextNode(node)) {
                        // It's an external update (e.g. Clock tick)
                        // Important: Update original text
                        node._originalText = node.nodeValue;
                        this.mutationQueue.add(node);
                        hasRelevantMutation = true;
                    }
                }
            });

            if (hasRelevantMutation) {
                this.debouncedTranslate();
            }
        });

        this.observer.observe(document.body, {
            childList: true,
            subtree: true,
            characterData: true
        });
    }

    isValidNode(node) {
        if (!node) return false;
        if (node.nodeType === Node.ELEMENT_NODE) {
            if (node.classList.contains('no-translate')) return false;
            // Ignore scripts, styles
            const tags = ['SCRIPT', 'STYLE', 'NOSCRIPT'];
            if (tags.includes(node.tagName)) return false;
            return true;
        }

        return false;
    }

    isValidTextNode(node) {
        if (node.nodeType !== Node.TEXT_NODE) return false;
        if (!node.nodeValue.trim()) return false;
        if (node.parentElement && !this.isValidNode(node.parentElement)) return false;
        return true;
    }

    collectTextNodes(root) {
        const textNodes = [];
        if (root.nodeType === Node.TEXT_NODE) {
            if (this.isValidTextNode(root)) textNodes.push(root);
            return textNodes;
        }

        const walker = document.createTreeWalker(
            root,
            NodeFilter.SHOW_TEXT,
            {
                acceptNode: (node) => {
                    return this.isValidTextNode(node) ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_REJECT;
                }
            }
        );

        let node;
        while (node = walker.nextNode()) {
            textNodes.push(node);
        }
        return textNodes;
    }

    async setLanguage(lang) {
        if (this.currentLang === lang) return;

        this.currentLang = lang;
        localStorage.setItem('app_language', lang);

        if (lang === 'en') {
            this.restoreEnglish();
            document.documentElement.lang = 'en';
            return;
        }

        // Translate everything visible now
        await this.translatePage();
        document.documentElement.lang = lang;
    }

    restoreEnglish() {
        if (this.observer) this.observer.disconnect();

        const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT, null);
        let node;
        while (node = walker.nextNode()) {
            if (node._originalText) {
                node.nodeValue = node._originalText;
            }
        }

        if (this.observer) this.setupObserver();
    }

    async processQueue() {
        if (this.mutationQueue.size === 0) return;

        const nodes = Array.from(this.mutationQueue);
        this.mutationQueue.clear();

        await this.translateNodes(nodes);
    }

    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    async translatePage() {
        // Scan entire body
        const nodes = this.collectTextNodes(document.body);
        await this.translateNodes(nodes);
    }

    async fetchWithRetry(url, options, retries = 3, backoff = 1000) {
        try {
            const response = await fetch(url, options);
            if (!response.ok) {
                if (response.status === 429 || response.status >= 500) {
                    throw new Error(`Retriable error: ${response.status}`);
                }
                throw new Error(`Request failed: ${response.status}`);
            }
            return response;
        } catch (error) {
            if (retries > 0) {
                console.warn(`Translation fetch failed. Retrying in ${backoff}ms... (${retries} retries left)`, error);
                await new Promise(resolve => setTimeout(resolve, backoff));
                return this.fetchWithRetry(url, options, retries - 1, backoff * 2);
            }
            throw error;
        }
    }

    // Unified translation logic
    async translateNodes(nodes) {
        // Filter out nodes that don't need translation or invalid
        const validNodes = nodes.filter(n => {
            // Ensure original text is captured
            if (!n._originalText) n._originalText = n.nodeValue;
            return n._originalText.trim().length > 0;
        });

        if (validNodes.length === 0) return;

        // Ensure cache for lang
        if (!this.cache.has(this.currentLang)) {
            this.cache.set(this.currentLang, new Map());
        }
        const langCache = this.cache.get(this.currentLang);

        // Identify texts needing API
        const textsToFetch = new Set();
        validNodes.forEach(n => {
            const text = n._originalText.trim();
            if (!langCache.has(text)) {
                textsToFetch.add(text);
            }
        });

        const fetchList = [...textsToFetch];

        // Only show loader if we are fetching a significant amount or it's not a background update
        // Actually, for better UX on small updates (like clock), don't show full screen loader
        if (fetchList.length > 5) this.showLoading(true);

        try {
            // Batch API calls
            if (fetchList.length > 0) {
                const chunkSize = 50;
                for (let i = 0; i < fetchList.length; i += chunkSize) {
                    const chunk = fetchList.slice(i, i + chunkSize);
                    try {
                        const response = await this.fetchWithRetry(this.apiEndpoint, {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({ texts: chunk, targetLanguage: this.currentLang })
                        });
                        if (response.ok) {
                            const result = await response.json();
                            Object.entries(result).forEach(([k, v]) => langCache.set(k, v));
                        }
                    } catch (err) {
                        console.error("Batch fetch error", err);
                    }
                }
            }

            // Apply translations
            validNodes.forEach(node => {
                const source = node._originalText.trim();
                if (langCache.has(source)) {
                    const translated = langCache.get(source);

                    // Mark node as being updated by us to prevent observer loop
                    node._isApplyingTranslation = true;

                    // Apply
                    // Try to preserve whitespace structure: matches replaced in value
                    if (node.nodeValue.includes(source)) {
                        node.nodeValue = node.nodeValue.replace(source, translated);
                    } else {
                        node.nodeValue = translated;
                    }

                    // Reset flag after microtask
                    setTimeout(() => node._isApplyingTranslation = false, 0);
                }
            });

            // Handle attributes like placeholders and titles
            const elementsWithAttrs = document.querySelectorAll('[placeholder], [title]');
            elementsWithAttrs.forEach(el => {
                ['placeholder', 'title'].forEach(attr => {
                    const original = el.getAttribute(`data-org-${attr}`) || el.getAttribute(attr);
                    if (original && original.trim()) {
                        if (!el.getAttribute(`data-org-${attr}`)) el.setAttribute(`data-org-${attr}`, original);

                        const text = original.trim();
                        if (langCache.has(text)) {
                            el.setAttribute(attr, langCache.get(text));
                        }
                    }
                });
            });

        } catch (e) {

            console.error("TranslateNodes error", e);
        } finally {
            this.showLoading(false);
        }
    }

    // Kept for backward compatibility/manual overrides if needed, acts as wrapper for translateNodes
    async translateElement(element) {
        if (this.currentLang === 'en') return;
        const nodes = this.collectTextNodes(element);
        await this.translateNodes(nodes);
    }

    showLoading(show) {
        const loader = document.getElementById('translation-loader');
        if (loader) loader.style.display = show ? 'flex' : 'none';
    }
}

window.TranslationManager = new TranslationManager();
