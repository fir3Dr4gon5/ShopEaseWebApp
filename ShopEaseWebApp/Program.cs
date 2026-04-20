using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopEaseWebApp.Data;
using ShopEaseWebApp.Models;
using ShopEaseWebApp.Options;
using ShopEaseWebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;           
        options.Password.RequireUppercase = true;        

    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddScoped<StripeOrderFinalizationService>();
builder.Services.Configure<StripeOptions>(
    builder.Configuration.GetSection(StripeOptions.SectionName));

var stripeSecretKey = builder.Configuration[$"{StripeOptions.SectionName}:SecretKey"];
if (!string.IsNullOrWhiteSpace(stripeSecretKey))
{
    Stripe.StripeConfiguration.ApiKey = stripeSecretKey;
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeed");
    var productImagePaths = new Dictionary<string, string>
    {
        ["Men's White T-Shirt"] = "/images/Products/White T Shirt.jpg",
        ["Women's Jeans"] = "/images/Products/Womens Jeans.jpg",
        ["Leather Wallet"] = "/images/Products/Leather Wallet.jpg",
        ["Sunglasses"] = "/images/Products/Sun Glasses.jpg",
        ["Canvas Sneakers"] = "/images/Products/Sneakers.jpg",
        ["Wool Scarf"] = "/images/Products/Wool Scarf.jpg",
        ["Leather Belt"] = "/images/Products/Leather Belt.jpg",
        ["Hooded Sweatshirt"] = "/images/Products/Hooded SweatShirt.jpg"
    };

    const string adminRoleName = "Admin";
    if (!roleManager.RoleExistsAsync(adminRoleName).GetAwaiter().GetResult())
    {
        roleManager.CreateAsync(new IdentityRole(adminRoleName)).GetAwaiter().GetResult();
    }

    const string customerRoleName = "Customer";
    if (!roleManager.RoleExistsAsync(customerRoleName).GetAwaiter().GetResult())
    {
        roleManager.CreateAsync(new IdentityRole(customerRoleName)).GetAwaiter().GetResult();
    }

    var adminSeed = app.Configuration.GetSection(AdminSeedOptions.SectionName).Get<AdminSeedOptions>() ?? new AdminSeedOptions();
    var existingAdmins = userManager.GetUsersInRoleAsync(adminRoleName).GetAwaiter().GetResult();
    if (existingAdmins.Count == 0)
    {
        var seedEmail = adminSeed.Email?.Trim() ?? string.Empty;
        var seedPassword = adminSeed.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(seedEmail) || string.IsNullOrWhiteSpace(seedPassword))
        {
            if (app.Environment.IsDevelopment()
                && Environment.UserInteractive
                && !Console.IsInputRedirected)
            {
                Console.WriteLine();
                Console.WriteLine("No user has the Admin role. Enter credentials to create an admin account now.");
                Console.WriteLine("(Or set AdminSeed:Email and AdminSeed:Password in appsettings / user secrets and restart — leave email blank and press Enter to skip.)");
                Console.Write("Admin email: ");
                var lineEmail = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(lineEmail))
                {
                    seedEmail = lineEmail.Trim();
                    Console.Write("Admin password: ");
                    seedPassword = Console.ReadLine() ?? string.Empty;
                }
                Console.WriteLine();
            }
        }

        if (string.IsNullOrWhiteSpace(seedEmail) || string.IsNullOrWhiteSpace(seedPassword))
        {
            logger.LogWarning(
                "No Admin user exists and none was created. Set {EmailKey} and {PasswordKey} in appsettings.json or user secrets, or run from a terminal in Development to be prompted at startup.",
                $"{AdminSeedOptions.SectionName}:{nameof(AdminSeedOptions.Email)}",
                $"{AdminSeedOptions.SectionName}:{nameof(AdminSeedOptions.Password)}");
        }
        else
        {
            var existingUser = userManager.FindByEmailAsync(seedEmail).GetAwaiter().GetResult();
            if (existingUser is not null)
            {
                if (!userManager.IsInRoleAsync(existingUser, adminRoleName).GetAwaiter().GetResult())
                {
                    userManager.AddToRoleAsync(existingUser, adminRoleName).GetAwaiter().GetResult();
                    logger.LogInformation("Assigned Admin role to existing user {Email}.", seedEmail);
                }
            }
            else
            {
                var adminUser = new IdentityUser
                {
                    UserName = seedEmail,
                    Email = seedEmail,
                    EmailConfirmed = true
                };
                var createResult = userManager.CreateAsync(adminUser, seedPassword).GetAwaiter().GetResult();
                if (createResult.Succeeded)
                {
                    userManager.AddToRoleAsync(adminUser, adminRoleName).GetAwaiter().GetResult();
                    logger.LogInformation("Created Admin user {Email}.", seedEmail);
                }
                else
                {
                    foreach (var err in createResult.Errors)
                    {
                        logger.LogWarning("Admin user seed failed: {Code} {Description}", err.Code, err.Description);
                    }
                }
            }
        }
    }

    if (!context.Products.Any())
    {
        context.Products.AddRange(
            new Product { Name = "Men's White T-Shirt", Description = "Classic cotton crew neck t-shirt", Price = 12.99m, StockQuantity = 100 },
            new Product { Name = "Women's Jeans", Description = "Slim fit mid-rise blue denim jeans", Price = 34.99m, StockQuantity = 75 },
            new Product { Name = "Leather Wallet", Description = "Slim bifold genuine leather wallet", Price = 19.99m, StockQuantity = 60 },
            new Product { Name = "Sunglasses", Description = "UV400 polarised sunglasses", Price = 24.99m, StockQuantity = 45 },
            new Product { Name = "Canvas Sneakers", Description = "Casual lace up canvas shoes", Price = 27.99m, StockQuantity = 80 },
            new Product { Name = "Wool Scarf", Description = "Soft knitted winter scarf", Price = 15.99m, StockQuantity = 55 },
            new Product { Name = "Leather Belt", Description = "Classic brown leather belt", Price = 18.99m, StockQuantity = 70 },
            new Product { Name = "Hooded Sweatshirt", Description = "Pullover hoodie in grey marl", Price = 29.99m, StockQuantity = 90 }
        );

        context.SaveChanges();
    }

    var products = context.Products.ToList();
    var didUpdateImageUrls = false;

    foreach (var product in products)
    {
        if (productImagePaths.TryGetValue(product.Name, out var imagePath) &&
            !string.Equals(product.ImageUrl, imagePath, StringComparison.Ordinal))
        {
            product.ImageUrl = imagePath;
            didUpdateImageUrls = true;
        }
    }

    if (didUpdateImageUrls)
    {
        context.SaveChanges();
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.GetUserAsync(context.User);

        if (user is not null)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            if (!userRoles.Any())
            {
                await userManager.AddToRoleAsync(user, "Customer");
            }
        }
    }

    await next();
});

app.MapGet("/", (HttpContext context) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        return Results.Redirect("/Products");
    }

    return Results.Redirect("/Identity/Account/Login");
});

app.MapControllers();
app.MapRazorPages();

app.Run();
