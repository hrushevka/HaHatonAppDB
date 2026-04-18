using HacatonApp.Data;
using HacatonApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

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
})
.AddEntityFrameworkStores<ApplicationDbContext>();
//.AddDefaultTokenProviders();
//.AddErrorDescriber<RussianIdentityErrorDescriber>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
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
app.UseSession();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();
        await DbInit.InitializeAsync(roleManager, userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при инициализации БД");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public class RussianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateEmail(string email) => new IdentityError
    { Code = nameof(DuplicateEmail), Description = $"Email '{email}' уже используется" };

    public override IdentityError DuplicateUserName(string userName) => new IdentityError
    { Code = nameof(DuplicateUserName), Description = $"Имя пользователя:{userName} уже используется" };

    public override IdentityError InvalidEmail(string email) => new IdentityError
    { Code = nameof(InvalidEmail), Description = $"Email {email} не является корректным" };

    public override IdentityError DuplicateRoleName(string role) => new IdentityError
    { Code = nameof(DuplicateRoleName), Description = $"Роль {role} уже существует" };

    public override IdentityError InvalidRoleName(string role) => new IdentityError
    { Code = nameof(InvalidRoleName), Description = $"Имя роли: {role} недопустимо" };

    public override IdentityError PasswordRequiresDigit() => new IdentityError
    { Code = nameof(PasswordRequiresDigit), Description = $"Пароль должен содержать хотя бы одну цифру (0-9)" };

    public override IdentityError PasswordRequiresLower() => new IdentityError
    { Code = nameof(PasswordRequiresLower), Description = $"Пароль должен содержать хотя бы одну строчную букву (a-z)" };

    public override IdentityError PasswordRequiresUpper() => new IdentityError
    { Code = nameof(PasswordRequiresUpper), Description = $"Пароль должен содержать хотя бы одну заглавную букву (A-Z)" };

    public override IdentityError PasswordRequiresNonAlphanumeric() => new IdentityError
    { Code = nameof(PasswordRequiresNonAlphanumeric), Description = $"Пароль должен содержать хотя бы один спецсимвол" };

    public override IdentityError PasswordTooShort(int length) => new IdentityError
    { Code = nameof(PasswordTooShort), Description = $"Пароль должен быть не короче:{length} символов" };

    public override IdentityError PasswordMismatch() => new IdentityError
    { Code = nameof(PasswordMismatch), Description = $"Неверный пароль!" };

    public override IdentityError InvalidUserName(string userName) => new IdentityError
    { Code = nameof(InvalidUserName), Description = $"Имя пользователя:{userName} содержит недопустимые символы" };

    public override IdentityError UserAlreadyInRole(string role) => new IdentityError
    { Code = nameof(UserAlreadyInRole), Description = $"Пользователь уже состоит в роли:{role}" };

    public override IdentityError UserNotInRole(string role) => new IdentityError
    { Code = nameof(UserNotInRole), Description = $"Пользователь не состоит в роли:'{role}'" };

    public override IdentityError DefaultError() => new IdentityError
    { Code = nameof(DefaultError), Description = $"Произошла неизвестная ошибка" };
}