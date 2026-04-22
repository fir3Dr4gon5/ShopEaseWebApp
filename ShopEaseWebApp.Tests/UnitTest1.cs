using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ShopEaseWebApp.Tests;

public class AccountPasswordValidationTests
{
    private readonly PasswordValidator<IdentityUser> _passwordValidator = new();
    private readonly IdentityUser _user = new()
    {
        UserName = "newuser@example.com",
        Email = "newuser@example.com"
    };

    [Fact]
    public async Task CreateAccount_Fails_WhenPasswordIsLessThan8Characters()
    {
        var result = await ValidatePasswordAsync("Aa1!xyz");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task CreateAccount_Fails_WhenPasswordHasNoCapitalLetters()
    {
        var result = await ValidatePasswordAsync("aa1!xyza");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task CreateAccount_Fails_WhenPasswordHasNoSpecialCharacters()
    {
        var result = await ValidatePasswordAsync("Aa1xyzab");

        Assert.False(result.Succeeded);
    }

    private Task<IdentityResult> ValidatePasswordAsync(string password)
    {
        var options = Options.Create(new IdentityOptions());
        options.Value.Password.RequiredLength = 8;
        options.Value.Password.RequireDigit = true;
        options.Value.Password.RequireUppercase = true;
        options.Value.Password.RequireNonAlphanumeric = true;
        var userManager = BuildUserManager(options);

        return _passwordValidator.ValidateAsync(userManager, _user, password);
    }

    private static UserManager<IdentityUser> BuildUserManager(IOptions<IdentityOptions> options)
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        var passwordHasher = new Mock<IPasswordHasher<IdentityUser>>();
        var userValidators = Array.Empty<IUserValidator<IdentityUser>>();
        var passwordValidators = Array.Empty<IPasswordValidator<IdentityUser>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new IdentityErrorDescriber();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<IdentityUser>>>();

        return new UserManager<IdentityUser>(
            store.Object,
            options,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors,
            services.Object,
            logger.Object);
    }
}
