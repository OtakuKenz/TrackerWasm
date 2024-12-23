using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using TrackerWasm.Services;

namespace TrackerWasm.AuthProvider;

public class CustomAuthStateProvider(UserService userService, ILocalStorageService localStorage)
    : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!await userService.IsUserLoggedIn())
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var userId = await userService.GetUserId() ?? "";
        var displayName = await userService.GetDiplayName() ?? "";
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Name, displayName),
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "Custom");

        var principal = new ClaimsPrincipal(identity);
        return new AuthenticationState(principal);
    }

    public async Task MarkUserAsAuthenticated()
    {
        var userId = await userService.GetUserId() ?? "";
        var displayName = await userService.GetDiplayName() ?? "";
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Name, displayName),
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "Custom");

        var principal = new ClaimsPrincipal(identity);
        var authenticationState = new AuthenticationState(principal);
        NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await userService.Logout();
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);
        var authenticationState = new AuthenticationState(principal);
        NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));
    }
}