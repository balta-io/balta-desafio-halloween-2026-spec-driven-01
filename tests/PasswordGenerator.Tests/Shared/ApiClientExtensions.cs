using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Api.Features.GeneratePassword;

namespace PasswordGenerator.Tests.Shared;

public static class ApiClientExtensions
{
    public static async Task<Guid> CreatePasswordAsync(this HttpClient client)
    {
        var response = await client.PostAsync("/api/v1/passwords", content: null);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<GeneratePasswordResponse>();
        return body!.Id;
    }

    public static Task<string> StoredPasswordAsync(this ApiFactory factory, Guid id) =>
        factory.WithDbAsync(db => db.Passwords.AsNoTracking().Where(p => p.Id == id).Select(p => p.Senha).SingleAsync());
}
