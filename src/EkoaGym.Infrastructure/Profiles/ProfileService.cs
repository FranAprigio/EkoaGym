using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Application.Profiles;
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

        return ToResponse(profile);
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

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(profile);
    }

    private static ProfileResponse ToResponse(UserProfile profile) => new(
        profile.Name,
        profile.DateOfBirth,
        profile.Sex,
        profile.HeightCm,
        profile.CurrentWeightKg,
        profile.ActivityLevel);
}
