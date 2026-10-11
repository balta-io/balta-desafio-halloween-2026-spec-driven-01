using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Api.Shared.Persistence;

namespace PasswordGenerator.Api.Features.GetPasswordByGuid;

public sealed class GetPasswordByGuidService(AppDbContext db) : IGetPasswordByGuidService
{
    public async Task<GetPasswordByGuidResult> GetAsync(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var guid) || guid == Guid.Empty)
            return new GetPasswordByGuidResult.InvalidGuid();

        var senha = await db.Passwords
            .AsNoTracking()
            .Where(p => p.Id == guid)
            .Select(p => p.Senha)
            .FirstOrDefaultAsync(ct);

        return senha is null
            ? new GetPasswordByGuidResult.NotFound()
            : new GetPasswordByGuidResult.Found(senha);
    }
}
