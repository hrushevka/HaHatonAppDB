using HacatonApp.Data;
using HacatonApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HacatonApp.Models;
using HacatonApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null

            );
    }));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
}


).AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<RussianIdentityErrorDescriber>();


builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

builder.Services.AddSession(
    options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
app.UseExceptionHandler("/Home/Error");
app.UseHsts();
}
else
{
app.UseDeveloperExceptionPage();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
var services = scope.ServiceProvider;
try
{
var context = services.GetRequiredService<ApplicationDbContext>();
var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
var cartService = services.GetRequiredService<IServiceProvider>();

await context.Database.MigrateAsync();
await DbInit.InitializeAsync(roleManager, userManager);

context.Database.EnsureCreated();
//await 

}
catch (Exception ex)
{
var logger = services.GetRequiredService<ILogger<Program>>();
logger.LogError(ex, "������ � ����������� � ��");
}
}
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
public class RussianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateEmail(string email) => new IdentityError
    { Code = nameof(DuplicateEmail), Description = $"Email '{email}' ��� ������������" };
    public override IdentityError DuplicateUserName(string userName) => new IdentityError
    { Code = nameof(DuplicateUserName), Description = $"��� ������������:{userName} ��� ������������" };
    public override IdentityError InvalidEmail(string email) => new IdentityError
    { Code = nameof(InvalidEmail), Description = $"Email {email} ����� �� ������ ������" };
    public override IdentityError DuplicateRoleName(string role) => new IdentityError
    { Code = nameof(DuplicateRoleName), Description = $"���� {role} ��� ����������" };
    public override IdentityError InvalidRoleName(string role) => new IdentityError
    { Code = nameof(InvalidRoleName), Description = $"�������� ����: {role} �����������" };
    public override IdentityError PasswordRequiresDigit() => new IdentityError
    { Code = nameof(PasswordRequiresDigit), Description = $"������ ������ ��������� ���� �� ���� ����� (0-9)" };
    public override IdentityError PasswordRequiresLower() => new IdentityError
    { Code = nameof(PasswordRequiresLower), Description = $"������ ������ ��������� ���� �� ���� �������� ����� (a-z) " };
    public override IdentityError PasswordRequiresUpper() => new IdentityError
    { Code = nameof(PasswordRequiresUpper), Description = $"������ ������ ��������� ���� �� ���� ��������� ����� (A-Z) " };
    public override IdentityError PasswordRequiresNonAlphanumeric() => new IdentityError
    { Code = nameof(PasswordRequiresNonAlphanumeric), Description = $"������ ������ ��������� ���� �� ���� ������ " };
    public override IdentityError PasswordTooShort(int lenght) => new IdentityError
    { Code = nameof(PasswordTooShort), Description = $"������ ������ ��������� �� �����:{lenght} �������� " };
    public override IdentityError PasswordMismatch() => new IdentityError
    { Code = nameof(PasswordMismatch), Description = $"�������� ������! " };
    public override IdentityError InvalidUserName(string userName) => new IdentityError
    { Code = nameof(InvalidUserName), Description = $"��� ������������:{userName} �������� ������������ ������� " };
    public override IdentityError UserAlreadyInRole(string role) => new IdentityError
    { Code = nameof(UserAlreadyInRole), Description = $"������������ ��� ����� ����:{role}" };
    public override IdentityError UserNotInRole(string role) => new IdentityError
    { Code = nameof(UserNotInRole), Description = $"������������ �� ����� ����:'{role}'" };
    public override IdentityError DefaultError() => new IdentityError
    { Code = nameof(DefaultError), Description = $"��������� ����������� ������" };


}
