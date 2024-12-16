using Microsoft.JSInterop;

namespace TrackerWasm.Services;

public class FirebaseAuthService(IJSRuntime jsRuntime)
{
    public async Task<string> SignInAsync(string email, string password)
    {
        return await jsRuntime.InvokeAsync<string>("firebaseAuth.signIn", email, password);
    }

    public async Task SignOutAsync()
    {
        await jsRuntime.InvokeVoidAsync("firebaseAuth.signOut");
    }
}