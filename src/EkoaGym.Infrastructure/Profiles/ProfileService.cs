using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Application.Profiles;
using EkoaGym.Domain.Constants;
using EkoaGym.Domain.Entities;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EkoaGym.Infrastructure.Profiles;

public class ProfileService(EkoaGymDbContext dbContext) : IProfileService
{
    public async Task<ProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.UserProfiles.SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (profile is null)
        {
            throw new ProfileNotFoundException();
        }

        return ToResponse(profile, await GetLanguageAsync(userId, cancellationToken));
    }

    public async Task<ProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.UserProfiles.SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
        {
            profile = new UserProfile { Id = Guid.NewGuid(), UserId = userId, CreatedAt = DateTime.UtcNow };
            dbContext.UserProfiles.Add(profile);
        }

        profile.Name = request.Name.Trim();
        profile.DateOfBirth = request.DateOfBirth;
        profile.Sex = request.Sex;
        profile.HeightCm = request.HeightCm;
        profile.CurrentWeightKg = request.CurrentWeightKg;
        profile.ActivityLevel = request.ActivityLevel;
        profile.UpdatedAt = DateTime.UtcNow;

        var user = await dbContext.Users.SingleAsync(u => u.Id == userId, cancellationToken);
        if (SupportedLanguages.Normalize(request.PreferredLanguage) is { } language)
        {
            user.PreferredLanguage = language;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(profile, user.PreferredLanguage);
    }

    private async Task<string> GetLanguageAsync(Guid userId, CancellationToken cancellationToken) =>
        await dbContext.Users.Where(u => u.Id == userId).Select(u => u.PreferredLanguage).SingleAsync(cancellationToken);

    private static ProfileResponse ToResponse(UserProfile profile, string preferredLanguage) => new(
        profile.Name,
        profile.DateOfBirth,
        profile.Sex,
        profile.HeightCm,
        profile.CurrentWeightKg,
        profile.ActivityLevel,
        preferredLanguage);
}
