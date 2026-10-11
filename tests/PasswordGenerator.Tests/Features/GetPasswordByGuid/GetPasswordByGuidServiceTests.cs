using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Api.Features.GetPasswordByGuid;
using PasswordGenerator.Api.Shared.Persistence;
using PasswordGenerator.Tests.Shared;

namespace PasswordGenerator.Tests.Features.GetPasswordByGuid;

public class GetPasswordByGuidServiceTests : IDisposable
{
    private const string Senha = "Aa1!bcdefghijklmnopqr";

    private readonly SqliteTestDb _testDb = new();
    private readonly Guid _id = Guid.NewGuid();

    public GetPasswordByGuidServiceTests()
    {
        using var db = _testDb.CreateContext();
        db.Passwords.Add(new StoredPassword(_id, Senha, DateTime.UtcNow));
        db.SaveChanges();
    }

    public void Dispose() => _testDb.Dispose();

    private async Task<GetPasswordByGuidResult> GetAsync(string id)
    {
        await using var db = _testDb.CreateContext();
        return await new GetPasswordByGuidService(db).GetAsync(id, CancellationToken.None);
    }

    [Fact]
    public async Task Returns_found_for_existing_guid() =>
        Assert.Equal(new GetPasswordByGuidResult.Found(Senha), await GetAsync(_id.ToString()));

    [Fact]
    public async Task Returns_not_found_for_unknown_guid() =>
        Assert.IsType<GetPasswordByGuidResult.NotFound>(await GetAsync(Guid.NewGuid().ToString()));

    [Fact]
    public async Task Normalizes_uppercase_guid() =>
        Assert.Equal(new GetPasswordByGuidResult.Found(Senha), await GetAsync(_id.ToString().ToUpperInvariant()));

    [Fact]
    public async Task Accepts_guid_without_hyphens() =>
        Assert.Equal(new GetPasswordByGuidResult.Found(Senha), await GetAsync(_id.ToString("N")));

    [Fact]
    public async Task Rejects_empty_guid() =>
        Assert.IsType<GetPasswordByGuidResult.InvalidGuid>(await GetAsync(Guid.Empty.ToString()));

    [Theory]
    [InlineData("texto livre")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("3f2504e0-4f89-11d3-9a0c-0305e82c33")]
    public async Task Rejects_malformed_guid(string id) =>
        Assert.IsType<GetPasswordByGuidResult.InvalidGuid>(await GetAsync(id));

    [Fact]
    public async Task Invalid_guid_does_not_query_the_database()
    {
        await using var db = _testDb.CreateContext();
        await db.Database.ExecuteSqlRawAsync("DROP TABLE Passwords");
        var sut = new GetPasswordByGuidService(db);

        Assert.IsType<GetPasswordByGuidResult.InvalidGuid>(await sut.GetAsync("texto livre", CancellationToken.None));
        Assert.IsType<GetPasswordByGuidResult.InvalidGuid>(await sut.GetAsync(Guid.Empty.ToString(), CancellationToken.None));
        await Assert.ThrowsAsync<SqliteException>(() => sut.GetAsync(Guid.NewGuid().ToString(), CancellationToken.None));
    }
}
