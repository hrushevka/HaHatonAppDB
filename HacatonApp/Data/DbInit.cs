using HacatonApp.Models;
using Microsoft.AspNetCore.Identity;

namespace HacatonApp.Data
{
    public class DbInit
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Создаём роли
            string[] roles = { "Admin", "Jury", "Teamer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Создаём админа, если нет
            var adminEmail = "admin@exmaple.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Админ",
                    LastName = "Тестович",
                };
                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
                    await userManager.AddToRoleAsync(admin, "Jury");
                    await userManager.AddToRoleAsync(admin, "Teamer");
            }
        }
    }
}
