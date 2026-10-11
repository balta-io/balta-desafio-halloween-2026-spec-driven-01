using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Api.Shared.Persistence;

namespace PasswordGenerator.Tests.Shared;

public class PersistenceTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly AppDbContext _db;

    public PersistenceTests()
    {
        _connection.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _db.Database.Migrate();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private string Scalar(string sql)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        return (string)command.ExecuteScalar()!;
    }

    [Fact]
    public void Migration_creates_table_with_primary_key_and_check_constraint()
    {
        var ddl = Scalar("SELECT sql FROM sqlite_master WHERE type = 'table' AND name = 'Passwords'");

        Assert.Contains("PRIMARY KEY", ddl);
        Assert.Contains("length(Senha) BETWEEN 16 AND 128", ddl);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(129)]
    public void Check_constraint_rejects_out_of_range_length(int length)
    {
        var insert = $"INSERT INTO Passwords (Id, Senha, CreatedAtUtc) VALUES ('{Guid.NewGuid()}', '{new string('a', length)}', '2026-01-01')";

        using var command = _connection.CreateCommand();
        command.CommandText = insert;

        Assert.Throws<SqliteException>(() => command.ExecuteNonQuery());
    }

    [Fact]
    public void Stored_password_has_no_public_setters()
    {
        var publicSetters = typeof(StoredPassword)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.SetMethod is { IsPublic: true });

        Assert.Empty(publicSetters);
    }
}
