using SportChain.Shared.DTOs.Auth;
using SportChain.Shared.Enums;
using SportChain.WinForms.Services;
using Xunit;

namespace SportChain.Tests.Frontend;

public class SessionContextTests
{
    [Fact]
    public void SetSession_SetsTokenAndCurrentUser_FiresOnAuthStateChanged()
    {
        // Arrange
        SessionContext.ClearSession();
        bool eventFired = false;
        SessionContext.OnAuthStateChanged += () => eventFired = true;

        var authUser = new AuthResponse
        {
            UserId = 20,
            FullName = "Lê Thị Lễ Tân",
            Email = "reception.caugiay@sportchain.vn",
            Role = UserRole.Receptionist,
            BranchId = 1,
            Token = "MOCK_JWT_TOKEN"
        };


        // Act
        SessionContext.SetSession(authUser.Token, authUser);

        // Assert
        Assert.True(SessionContext.IsLoggedIn);
        Assert.Equal("MOCK_JWT_TOKEN", SessionContext.Token);
        Assert.Equal(authUser.FullName, SessionContext.CurrentUser?.FullName);
        Assert.True(SessionContext.IsReceptionist);
        Assert.False(SessionContext.IsSuperAdmin);
        Assert.False(SessionContext.IsCustomer);
        Assert.True(eventFired);
    }

    [Theory]
    [InlineData(UserRole.SuperAdmin, true, false, false, false)]
    [InlineData(UserRole.BranchManager, false, true, false, false)]
    [InlineData(UserRole.Receptionist, false, false, true, false)]
    [InlineData(UserRole.Customer, false, false, false, true)]
    public void RoleFlags_EvaluateCorrectly(
        UserRole role, 
        bool expectedAdmin, 
        bool expectedManager, 
        bool expectedReceptionist, 
        bool expectedCustomer)
    {
        // Arrange
        var user = new AuthResponse
        {
            UserId = 1,
            FullName = "Test User",
            Email = "test@sportchain.vn",
            Role = role,
            Token = "TOKEN"
        };

        // Act
        SessionContext.SetSession("TOKEN", user);

        // Assert
        Assert.Equal(expectedAdmin, SessionContext.IsSuperAdmin);
        Assert.Equal(expectedManager, SessionContext.IsBranchManager);
        Assert.Equal(expectedReceptionist, SessionContext.IsReceptionist);
        Assert.Equal(expectedCustomer, SessionContext.IsCustomer);
    }

    [Fact]
    public void ClearSession_ResetsAllFieldsAndFiresEvent()
    {
        // Arrange
        var user = new AuthResponse
        {
            UserId = 10,
            FullName = "User",
            Email = "u@sportchain.vn",
            Role = UserRole.Customer,
            Token = "TOKEN"
        };

        SessionContext.SetSession("TOKEN", user);
        Assert.True(SessionContext.IsLoggedIn);

        bool eventFired = false;
        SessionContext.OnAuthStateChanged += () => eventFired = true;

        // Act
        SessionContext.ClearSession();

        // Assert
        Assert.False(SessionContext.IsLoggedIn);
        Assert.Null(SessionContext.Token);
        Assert.Null(SessionContext.CurrentUser);
        Assert.True(eventFired);
    }
}
