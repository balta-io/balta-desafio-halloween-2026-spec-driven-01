using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using PasswordGenerator.Api.Features.GeneratePassword;
using PasswordGenerator.Api.Shared.Persistence;
using PasswordGenerator.Tests.Shared;
using Generator = PasswordGenerator.Api.Features.GeneratePassword.PasswordGenerator;

namespace PasswordGenerator.Tests.Features.GeneratePassword;

public class GeneratePasswordServiceTests
{
    private static readonly IPasswordGenerator RealGenerator =
        new Generator(Options.Create(new PasswordGeneratorOptions()));

    private static GeneratePasswordService CreateSut(AppDbContext db, IGuidGenerator guids) =>
        new(db, RealGenerator, guids, TimeProvider.System);

    [Fact]
    public async Task Persists_password_and_returns_guid()
    {
        using var testDb = new SqliteTestDb();
        var id = Guid.NewGuid();
        var guids = Substitute.For<IGuidGenerator>();
        guids.NewGuid().Returns(id);

        await using var db = testDb.CreateContext();
        var response = await CreateSut(db, guids).GenerateAsync(CancellationToken.None);

        Assert.Equal(id, response.Id);
        await using var verify = testDb.CreateContext();
        var stored = await verify.Passwords.SingleAsync();
        Assert.Equal(id, stored.Id);
        Assert.True(PasswordPolicy.IsValid(stored.Senha));
    }

    [Fact]
    public async Task Retries_with_new_guid_on_primary_key_collision()
    {
        using var testDb = new SqliteTestDb();
        var existing = Guid.NewGuid();
        var fresh = Guid.NewGuid();
        await using (var seed = testDb.CreateContext())
        {
            seed.Passwords.Add(new StoredPassword(existing, "Aa1!aaaaaaaaaaaaaaaa", DateTime.UtcNow));
            await seed.SaveChangesAsync();
        }

        var guids = Substitute.For<IGuidGenerator>();
        guids.NewGuid().Returns(existing, fresh);

        await using var db = testDb.CreateContext();
        var response = await CreateSut(db, guids).GenerateAsync(CancellationToken.None);

        Assert.Equal(fresh, response.Id);
        guids.Received(2).NewGuid();
        await using var verify = testDb.CreateContext();
        Assert.Equal(2, await verify.Passwords.CountAsync());
    }

    [Fact]
    public async Task Fails_after_three_consecutive_collisions()
    {
        using var testDb = new SqliteTestDb();
        var existing = Guid.NewGuid();
        await using (var seed = testDb.CreateContext())
        {
            seed.Passwords.Add(new StoredPassword(existing, "Aa1!aaaaaaaaaaaaaaaa", DateTime.UtcNow));
            await seed.SaveChangesAsync();
        }

        var guids = Substitute.For<IGuidGenerator>();
        guids.NewGuid().Returns(existing);

        await using var db = testDb.CreateContext();
        await Assert.ThrowsAsync<DbUpdateException>(() => CreateSut(db, guids).GenerateAsync(CancellationToken.None));

        guids.Received(GeneratePasswordService.MaxAttempts).NewGuid();
        await using var verify = testDb.CreateContext();
        Assert.Equal(1, await verify.Passwords.CountAsync());
    }

    [Fact]
    public async Task Save_failure_does_not_return_guid_and_leaves_no_record()
    {
        var failing = new FailingSaveChangesInterceptor { Enabled = true };
        using var testDb = new SqliteTestDb(failing);
        var guids = Substitute.For<IGuidGenerator>();
        guids.NewGuid().Returns(Guid.NewGuid());

        await using var db = testDb.CreateContext();
        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateSut(db, guids).GenerateAsync(CancellationToken.None));

        failing.Enabled = false;
        await using var verify = testDb.CreateContext();
        Assert.Empty(await verify.Passwords.ToListAsync());
    }

    [Fact]
    public async Task Rejects_password_that_violates_policy()
    {
        using var testDb = new SqliteTestDb();
        var badGenerator = Substitute.For<IPasswordGenerator>();
        badGenerator.Generate().Returns("curta");
        var guids = Substitute.For<IGuidGenerator>();
        guids.NewGuid().Returns(Guid.NewGuid());

        await using var db = testDb.CreateContext();
        var sut = new GeneratePasswordService(db, badGenerator, guids, TimeProvider.System);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GenerateAsync(CancellationToken.None));
        Assert.Empty(await testDb.CreateContext().Passwords.ToListAsync());
    }
}
