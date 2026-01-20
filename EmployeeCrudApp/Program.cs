using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews(options =>
{
    // Global filter to require authentication for all controllers/actions by default
    var policy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
    
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
})
.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
.AddDataAnnotationsLocalization();

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.Events.OnValidatePrincipal = async context =>
        {
            var userRepository = context.HttpContext.RequestServices.GetRequiredService<EmployeeCrudApp.Services.IUserRepository>();
            var userEmail = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userEmail) || userRepository.GetByEmail(userEmail) == null)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });
builder.Services.AddScoped<EmployeeCrudApp.Services.IEmployeeRepository, EmployeeCrudApp.Services.JsonEmployeeRepository>();
builder.Services.AddScoped<EmployeeCrudApp.Services.IUserRepository, EmployeeCrudApp.Services.JsonUserRepository>();
builder.Services.AddScoped<EmployeeCrudApp.Services.INoteRepository, EmployeeCrudApp.Services.JsonNoteRepository>();
builder.Services.AddScoped<EmployeeCrudApp.Services.ILocationRepository, EmployeeCrudApp.Services.JsonLocationRepository>();
builder.Services.AddScoped<EmployeeCrudApp.Services.IEmailService, EmployeeCrudApp.Services.SmtpEmailService>();
builder.Services.AddHttpClient();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// builder.Services.AddScoped<EmployeeCrudApp.Services.IStudentRepository, EmployeeCrudApp.Services.JsonStudentRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Dashboard/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

var supportedCultures = new[] { "en-US", "gu-IN", "hi-IN", "bn-IN", "mr-IN", "ta-IN", "te-IN", "kn-IN", "ml-IN", "pa-IN", "ur-IN", "or-IN", "as-IN" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
