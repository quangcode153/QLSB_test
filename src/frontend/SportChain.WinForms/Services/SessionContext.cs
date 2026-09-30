using SportChain.Shared.DTOs.Auth;
using SportChain.Shared.Enums;

namespace SportChain.WinForms.Services;

public static class SessionContext
{
    public static string? Token { get; private set; }
    public static AuthResponse? CurrentUser { get; private set; }

    public static bool IsLoggedIn => !string.IsNullOrEmpty(Token) && CurrentUser != null;
    public static bool IsSuperAdmin => CurrentUser?.Role == UserRole.SuperAdmin;
    public static bool IsBranchManager => CurrentUser?.Role == UserRole.BranchManager;
    public static bool IsReceptionist => CurrentUser?.Role == UserRole.Receptionist;
    public static bool IsCustomer => CurrentUser?.Role == UserRole.Customer;

    public static event Action? OnAuthStateChanged;

    public static void SetSession(string token, AuthResponse user)
    {
        Token = token;
        CurrentUser = user;
        OnAuthStateChanged?.Invoke();
    }

    public static void ClearSession()
    {
        Token = null;
        CurrentUser = null;
        OnAuthStateChanged?.Invoke();
    }
}
