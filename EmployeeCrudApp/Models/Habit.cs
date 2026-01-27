using System;
using System.Collections.Generic;

namespace EmployeeCrudApp.Models
{
    public class Habit
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Frequency { get; set; } = "Daily"; // "Daily" or "Custom"
        public List<DayOfWeek> CustomDays { get; set; } = new List<DayOfWeek>();
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<DateTime> CompletedDates { get; set; } = new List<DateTime>();

        // Helper to check if completed today
        public bool IsCompletedToday => CompletedDates.Any(d => d.Date == DateTime.Today);

        // Calculate current streak
        public int CurrentStreak
        {
            get
            {
                int streak = 0;
                var date = DateTime.Today;
                
                // Safety check to prevent infinite loops if data is weird, though while(date >= StartDate) is safe enough
                if (StartDate > DateTime.Today) return 0;

                while (date >= StartDate)
                {
                    bool isScheduled = Frequency == "Daily" || 
                                       (Frequency == "Custom" && CustomDays.Contains(date.DayOfWeek));

                    if (isScheduled)
                    {
                        if (CompletedDates.Any(d => d.Date == date))
                        {
                            streak++;
                        }
                        else
                        {
                            // If it's today and not done, we don't break the streak, 
                            // we just check yesterday. 
                            // If it's any other day and not done, streak is broken.
                            if (date != DateTime.Today)
                            {
                                break;
                            }
                        }
                    }
                    
                    // Move to previous day
                    date = date.AddDays(-1);
                }
                
                return streak;
            }
        }
    }
}
