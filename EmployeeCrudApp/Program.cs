using Microsoft.AspNetCore.Authentication;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Global filter to require authentication for all controllers/actions by default
    var policy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
    
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
});

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
builder.Services.AddScoped<EmployeeCrudApp.Services.IEmailService, EmployeeCrudApp.Services.SmtpEmailService>();

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
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
