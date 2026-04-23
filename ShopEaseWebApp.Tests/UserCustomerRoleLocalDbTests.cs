using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShopEaseWebApp.Data;

namespace ShopEaseWebApp.Tests;

public sealed class UserCustomerRoleLocalDbTests
{
    private const string CustomerRoleName = "Customer";

    [Fact]
    public async Task CreateUser_PersistsUser_AndAssignsCustomerRole()
    {
        var databaseName = $"ShopEaseWebApp_UserRoleTests_{Guid.NewGuid():N}";
        var connectionString =
            $"Server=(localdb)\\mssqllocaldb;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true";

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services
            .AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        await using var provider = services.BuildServiceProvider();

        var context = provider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        try
        {
            var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync(CustomerRoleName))
            {
                var createRoleResult = await roleManager.CreateAsync(new IdentityRole(CustomerRoleName));
                Assert.True(createRoleResult.Succeeded);
            }

            var email = $"user_{Guid.NewGuid():N}@example.com";
            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, "Aa1!xzyz");
            Assert.True(createResult.Succeeded);

            var addRoleResult = await userManager.AddToRoleAsync(user, CustomerRoleName);
            Assert.True(addRoleResult.Succeeded);

            var dbUser = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
            Assert.NotNull(dbUser);

            var roleId = await context.Roles.AsNoTracking()
                .Where(r => r.Name == CustomerRoleName)
                .Select(r => r.Id)
                .SingleAsync();

            var hasCustomerRole = await context.UserRoles.AsNoTracking()
                .AnyAsync(ur => ur.UserId == dbUser.Id && ur.RoleId == roleId);

            Assert.True(hasCustomerRole);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }
}
