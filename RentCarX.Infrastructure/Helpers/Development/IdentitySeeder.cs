using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RentCarX.Infrastructure.Helpers.Development;

public static class IdentitySeeder
{
    public static async Task SeedUserAsync(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        RoleManager<IdentityRole<Guid>> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (!await roleManager.RoleExistsAsync("USER"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>("USER"));
        }
    }

    public static async Task SeedAdminAsync(IServiceProvider serviceProvider, string adminEmail, string adminPassword)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        ILogger logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        RoleManager<IdentityRole<Guid>> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        const string roleName = IdentityRoleName.identifier; // admin

        try
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole<Guid>();
                role.Id = Guid.NewGuid();
                role.Name = roleName;
                role.NormalizedName = roleName.ToUpperInvariant();

                IdentityResult roleResult = await roleManager.CreateAsync(role);
                if (!roleResult.Succeeded)
                {
                    logger.LogWarning("Unable to create role {Role}: {Errors}", roleName, string.Join(',', roleResult.Errors));
                }
            }

            User? existing = await userManager.FindByEmailAsync(adminEmail);

            if (existing is null)
            {
                var user = new User();
                user.Id = Guid.NewGuid();
                user.UserName = adminEmail;
                user.NormalizedUserName = adminEmail.ToUpperInvariant();
                user.Email = adminEmail;
                user.NormalizedEmail = adminEmail.ToUpperInvariant();
                user.EmailConfirmed = true;
                

                IdentityResult createResult = await userManager.CreateAsync(user, adminPassword);
                if (!createResult.Succeeded)
                {
                    logger.LogWarning("Unable to create admin user: {Errors}", string.Join(',', createResult.Errors));
                    return;
                }

                IdentityResult addToRole = await userManager.AddToRoleAsync(user, roleName);
                if (!addToRole.Succeeded)
                {
                    logger.LogWarning("Unable to assign role to admin: {Errors}", string.Join(',', addToRole.Errors));
                }
                else
                {
                    logger.LogInformation("Admin user created: {Email}", adminEmail);
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(existing, roleName))
                {
                    await userManager.AddToRoleAsync(existing, roleName);
                    logger.LogInformation("Assigned Admin role to existing user: {Email}", adminEmail);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while seeding admin user");
            throw;
        }
    }
}
