using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using TrackerWasm.Models;
using TrackerWasm.ViewModels;

namespace TrackerWasm.Services;

public class UserService(HttpClient http, ILocalStorageService localStorage)
{
    public async Task<bool> AuthenticateUser(Login user)
    {
        var hashedPassword = HashPassword(user.Password);
        var query = new
        {
            structuredQuery = new
            {
                select = new
                {
                    fields = new[] { new { fieldPath = "userId" }, new { fieldPath = "displayName" } }
                },
                from = new[] { new { collectionId = "user" } },
                where = new
                {
                    compositeFilter = new
                    {
                        op = "AND",
                        filters = new[]
                        {
                            new
                            {
                                fieldFilter = new
                                {
                                    field = new { fieldPath = "username" },
                                    op = "EQUAL",
                                    value = new { stringValue = user.Username }
                                }
                            },
                            new
                            {
                                fieldFilter = new
                                {
                                    field = new { fieldPath = "password" },
                                    op = "EQUAL",
                                    value = new { stringValue = hashedPassword }
                                }
                            }
                        }
                    }
                }
            }
        };
        var url = http.BaseAddress + "documents:runQuery";
        var jsonContent = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json");
        var result = await http.PostAsync(url, jsonContent);
        var responseContent = await result.Content.ReadAsStringAsync();
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(responseContent);

        var matchingDocument = jsonElement.EnumerateArray()
            .FirstOrDefault(element => element.TryGetProperty("document", out var document));

        if (matchingDocument.ValueKind == JsonValueKind.Undefined) return false;

        var documentFields = matchingDocument.GetProperty("document").GetProperty("fields");

        if (!documentFields.TryGetProperty("userId", out var userIdElement)) return true;
        var userId = userIdElement.GetProperty("stringValue").GetString() ?? string.Empty;

        if (!documentFields.TryGetProperty("displayName", out var displayNameElement)) return true;
        var displayName = displayNameElement.GetProperty("stringValue").GetString() ?? string.Empty;

        await SetUser(userId, displayName);
        return true;
    }

    private async Task SetUser(string userId, string displayName)
    {
        await localStorage.SetItemAsync("userId", userId);
        await localStorage.SetItemAsync("displayName", displayName);
    }

    public async Task<bool> IsUserLoggedIn()
    {
        var userId = await GetUserId();
        return userId is not null;
    }

    public async Task Logout()
    {
        await localStorage.RemoveItemAsync("userId");
    }

    public async Task<string?> GetUserId()
    {
        return await localStorage.GetItemAsync<string>("userId");
    }

    public async Task<string?> GetDiplayName()
    {
        return await localStorage.GetItemAsync<string>("displayName");
    }

    public async Task<HttpResponseMessage> RegisterUser(User user)
    {
        user.UserId = GenerateUserId(user.Username);
        user.HashedPassword = HashPassword(user.Password);
        var serialized = FirestoreDataService.UserService.SerializeUser(user);
        return await http.PostAsync("documents/user", serialized);
    }

    private static string GenerateUserId(string username)
    {
        var input = $"{username}{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(bytes);
        var hash = BitConverter.ToString(hashBytes).Replace("-", "");
        return hash;
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        var builder = new StringBuilder();
        foreach (var b in bytes) builder.Append(b.ToString("x2"));

        return builder.ToString();
    }
}