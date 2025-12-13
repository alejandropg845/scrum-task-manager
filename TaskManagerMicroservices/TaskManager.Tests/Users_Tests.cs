
using BCrypt.Net;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Threading.Tasks;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.Users.Interfaces;
using TaskManager.Users.Interfaces.Clients;
using TaskManager.Users.Interfaces.Repository;
using TaskManager.Users.Services;

namespace TaskManager.Tests
{
    public class User_Tests
    {
        private readonly Mock<IUserManagementRepository> _usersManageRepo;
        private readonly Mock<IUserAuthRepository> _usersAuthRepo;
        private readonly Mock<IUserInfoRepository> _usersInfoRepo;
        private readonly Mock<IMessageBusClient> _mbc;
        private readonly Mock<IUserClients> _clients;
        private readonly UsersService _usersServiceMock;

        private readonly Mock<IMailRecoveryPasswordService> _recoverPasswordServiceMock;
        private readonly Mock<IConfiguration> _config;
        private readonly Mock<ITokenService> _tokensServiceMock;
        public User_Tests()
        {
            _recoverPasswordServiceMock = new Mock<IMailRecoveryPasswordService>();
            
            _config = new Mock<IConfiguration>();

            _usersManageRepo = new Mock<IUserManagementRepository>();
            _usersAuthRepo = new Mock<IUserAuthRepository>();
            _usersInfoRepo = new Mock<IUserInfoRepository>();
            _tokensServiceMock = new Mock<ITokenService>();
            _clients = new Mock<IUserClients>();
            _mbc = new Mock<IMessageBusClient>();

            _usersServiceMock = new UsersService(
                _recoverPasswordServiceMock.Object,
                _config.Object,
                _usersManageRepo.Object,
                _usersAuthRepo.Object,
                _usersInfoRepo.Object,
                _mbc.Object,
                _clients.Object,
                _tokensServiceMock.Object
            );
        }
        [Fact]
        public async Task LoginUserAsync_ShouldReturnUserDoesntExist_EqualTrue()
        {
            //Arrange
            var dto = new LoginUserDto("", "");

            _usersInfoRepo
                .Setup(r => r.GetUserAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            //Act
            var result = await _usersServiceMock.LoginUserAsync(dto);

            //Assert
            result.UserDoesntExist.Should().BeTrue();

        }

        [Fact]
        public async Task LoginUser_ShouldReturnIsCorrectEqualFalseIfPasswordIsIncorrect()
        {
            //Arrange
            _usersInfoRepo.Setup(r => r.GetUserAsync(It.IsAny<string>())).ReturnsAsync(new User());

            _usersAuthRepo.Setup(r => r.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            //Act
            var dto = new LoginUserDto("alexwhitney", "123456");

            var result = await _usersServiceMock.LoginUserAsync(dto);

            //Asserts
            result.IsCorrect.Should().BeFalse();
            _tokensServiceMock.Verify(s => s.GenerateToken("", "", ""), Times.Never);
        }

        [Fact]
        public async Task LoginUserAsync_ShouldReturnAccessAndRefreshToken_IfLoginSuccess()
        {
            //Arrange
            var dto = new LoginUserDto("alexwhitney", "string123");

            _usersInfoRepo.Setup(r => r.GetUserAsync(It.IsAny<string>())).ReturnsAsync(new User());

            _usersAuthRepo.Setup(r => r.IsGoogleAccountAsync(It.IsAny<string>())).ReturnsAsync(false);

            _usersAuthRepo.Setup(r => r.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);


            _tokensServiceMock.Setup(s => s.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("token");

            _clients.Setup(c => c.Tokens.SaveRefreshTokenAsync(It.IsAny<string>(), It.IsAny<Token>()));


            //Act
            var response = await _usersServiceMock.LoginUserAsync(dto);

            //Asserts
            response.RefreshToken.Should().NotBeNull();
            response.AccessToken.Should().NotBeNull();
            _tokensServiceMock.Verify(s => s.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        }

        [Fact]
        public async Task LoginUserAsync_IsCorrectShouldBeFalse_IfIsGoogleAccount()
        {
            //Arrange
            _usersInfoRepo.Setup(r => r.GetUserAsync(It.IsAny<string>())).ReturnsAsync(new User());

            _usersAuthRepo.Setup(r => r.IsGoogleAccountAsync(It.IsAny<string>())).ReturnsAsync(true);

            //Act
            var dto = new LoginUserDto("alexwhitney845", "123456");
            var response = await _usersServiceMock.LoginUserAsync(dto);

            //Assert
            response.IsCorrect.Should().BeFalse();
        }

        [Fact]
        public async Task LoginUserAsync_ShouldNotGenerateTokensIfIsGoogleAccount()
        {
            //Arrange
            var dto = new LoginUserDto("alexwhitney", "123456");
            _usersInfoRepo.Setup(r => r.GetUserAsync(It.IsAny<string>())).ReturnsAsync(new User());
            _usersAuthRepo.Setup(r => r.IsGoogleAccountAsync(It.IsAny<string>())).ReturnsAsync(true);

            //Act
            var response = await _usersServiceMock.LoginUserAsync(dto);
            //Asserts
            response.IsCorrect.Should().BeFalse();
            _usersAuthRepo.Verify(r => r.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _tokensServiceMock.Verify(s => s.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturn_UserExistsEqualTrue_IfUserExists()
        {
            //Arrange
            var dto = new RegisterUserDto("alexwhitney", "email@yax.com", "123456");

            _usersAuthRepo.Setup(r => r.UserExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            //Act
            var response = await _usersServiceMock.RegisterUserAsync(dto);

            //Asserts
            response.UserExists.Should().BeTrue();
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturn_EmailExistsEqualTrue_IfEmailExists()
        {
            //Arrange
            var dto = new RegisterUserDto("alexwhitney", "email@yax.com", "123456");
            _usersAuthRepo.Setup(r => r.UserExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _usersAuthRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            //Act
            var response = await _usersServiceMock.RegisterUserAsync(dto);

            //Arrange
            response.EmailExists.Should().BeTrue();
            _usersAuthRepo.Verify(r => r.HashPassword(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnAccessAndRefreshToken()
        {
            //Arrange
            var dto = new RegisterUserDto("alexwhitney", "email@email.com", "string123");

            _usersAuthRepo.Setup(r => r.UserExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

            _usersAuthRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

            _usersAuthRepo.Setup(r => r.HashPassword(It.IsAny<string>())).Returns("hashedPassword");

            _tokensServiceMock.Setup(s => s.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns("any-token");

            _clients.Setup(c => c.Tokens.SaveRefreshTokenAsync(It.IsAny<string>(), It.IsAny<Token>()));

            //Act
            var response = await _usersServiceMock.RegisterUserAsync(dto);

            //Asserts
            response.RefreshToken.Should().NotBeNull();
            response.AccessToken.Should().NotBeNull();
            response.UserExists.Should().BeFalse();
        }

        [Fact]
        public async Task RegisterUserAsync_VerifyPasswordHasBeenAddedHashed_ShouldBeTrue()
        {
            //Arrange
            var dto = new RegisterUserDto("alexwhitney", null, "string123");

            _usersAuthRepo.Setup(r => r.UserExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

            _usersAuthRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

            _usersAuthRepo.Setup(r => r.HashPassword(It.IsAny<string>()))
                .Returns(BCrypt.Net.BCrypt.HashPassword(dto.Password));

            string? hashedPassword = null;


            _usersAuthRepo.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                .Callback<User>(addedUser => hashedPassword = addedUser.Password);

            _clients.Setup(c => c.Tokens.SaveRefreshTokenAsync(It.IsAny<string>(), It.IsAny<Token>()));


            //Act
            await _usersServiceMock.RegisterUserAsync(dto);

            //Asserts
            _usersAuthRepo.Verify(r => r.EmailExistsAsync(It.IsAny<string>()), Times.Never);
            hashedPassword.Should().NotBe(null);
            BCrypt.Net.BCrypt.Verify(dto.Password, hashedPassword).Should().BeTrue();
        }

        [Fact]
        public async Task ContinueWithGoogle_ShouldReturnIsErrorTrueIfGoogleTokenIsNotValid()
        {
            //Arrange
            _usersAuthRepo.Setup(r => r.ValidateGoogleTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(("", "", true));

            //Act
            var result = await _usersServiceMock.ContinueWithGoogleAsync("token-id");

            //Asserts
            result.IsGoogleAuthError.Should().BeTrue();

        }

        [Fact]
        public async Task ContinueWithGoogle_ShouldCreateNewUserIfDoesntExist()
        {
            //Arrange
            string tokenId = Guid.NewGuid().ToString();
            string usernameFromGoogleInfo = "alexwhitney";

            _usersAuthRepo.Setup(r => r.ValidateGoogleTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((usernameFromGoogleInfo, "UserId", false));

            _usersInfoRepo.Setup(r => r.GetUserAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            User? user = null;

            _usersAuthRepo.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                .Callback<User>(addedUser => user = addedUser);

            _tokensServiceMock.Setup(s => s.GenerateToken(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Returns("token");

            _clients.Setup(c => c.Tokens.SaveRefreshTokenAsync(It.IsAny<string>(), It.IsAny<Token>()));

            //Act
            var response = await _usersServiceMock.ContinueWithGoogleAsync(tokenId);

            //Arrange
            user.Should().NotBeNull();
            usernameFromGoogleInfo.Should().NotBeEquivalentTo(user.Username);
            response.IsGoogleAuthError.Should().NotBe(true);
        }

        [Fact]
        public async Task RecoverPasswordAsync_ShouldReturn_ModifiedCountGreaterZero_IfUserExistsAndCodeWasSent()
        {
            //Arrange
            _usersAuthRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            _usersAuthRepo.Setup(r => r.SetRecoveryCodeAndExpirationTimeAsync(It.IsAny<string>(), It.IsAny<string>()));

            _recoverPasswordServiceMock.Setup(r => r.SendCodeToEmailAsync(It.IsAny<string>(), It.IsAny<string>()));

            //Act
            await _usersServiceMock.RecoverPasswordAsync("alexwhitney@email.com");

            //Asserts
            _recoverPasswordServiceMock.Verify(s => s.SendCodeToEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RecoverPasswordAsync_ShouldReturn_ModifiedCountZero_IfUserDoesntExist()
        {
            //Arrange
            _usersAuthRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

            //Act
            await _usersServiceMock.RecoverPasswordAsync("alexwhitney845@email.com");

            //Asserts
            _usersAuthRepo.Verify(r => r.SetRecoveryCodeAndExpirationTimeAsync("", ""), Times.Never);
            _recoverPasswordServiceMock.Verify(r => r.SendCodeToEmailAsync("", ""), Times.Never);
        }

        [Fact]
        public async Task ReceiveRecoveryCodeAsync_ShouldReturn_RecoveryCodeIsOkFalse_IfRecoveryCodeDoesntMatch()
        {
            //Arrange
            _usersAuthRepo.Setup(r => r.CheckReceivedRecoveryCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(("UserId", DateTimeOffset.UtcNow.AddMinutes(30)));

            _usersAuthRepo.Setup(r => r.ChangeUserPasswordAsync(It.IsAny<string>(), It.IsAny<string>()));

            //Act
            var result = await _usersServiceMock.ReceiveRecoveryCodeAsync("123456", "anyEmail", "1234", "1234");

            //Asserts
            result.IsExpired.Should().BeFalse();
            result.RecoveryCodeIsOk.Should().BeTrue();
        }

        [Fact]
        public async Task ReceiveRecoveryCodeAsync_ShouldReturn_IsExpiredTrue_IfIsExpired()
        {
            //Arrange
            _usersAuthRepo.Setup(r => r.CheckReceivedRecoveryCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(("UserId", DateTimeOffset.UtcNow.AddMinutes(-5)));

            //Act
            var result = await _usersServiceMock.ReceiveRecoveryCodeAsync("anyRecoveryCode", "anyEmail", "1234", "1234");

            //Asserts
            result.IsExpired.Should().BeTrue();
            _usersAuthRepo.Verify(r => r.ChangeUserPasswordAsync("", ""), Times.Never);
            result.RecoveryCodeIsOk.Should().BeFalse();
        }
    }
}
