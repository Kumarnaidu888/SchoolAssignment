using Microsoft.EntityFrameworkCore;
using SchoolApplication.Contracts.Me;
using SchoolApplication.Exceptions;
using SchoolApplication.Models;

namespace SchoolApplication.Services;

public sealed class CurrentUserProfileService : ICurrentUserProfileService
{
    private readonly SchoolAssessmentContext _db;

    public CurrentUserProfileService(SchoolAssessmentContext db) => _db = db;

    public async Task<CurrentUserProfileResponse> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _db.AppUsers
            .AsNoTracking()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user is null)
            throw new NotFoundException("User was not found.");

        var roles = user.Roles.Select(r => r.RoleName).OrderBy(n => n).ToList();

        StudentPortalLinkSummary? portal = null;
        var student = await _db.Students
            .AsNoTracking()
            .Include(s => s.Section)
            .ThenInclude(sec => sec.Class)
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (student is not null)
        {
            portal = new StudentPortalLinkSummary(
                student.StudentId,
                student.FullName,
                student.SectionId,
                student.Section.Name,
                student.Section.ClassId,
                student.Section.Class.Name);
        }

        return new CurrentUserProfileResponse(
            user.UserId,
            user.UserName,
            user.Email,
            user.IsActive == true,
            roles,
            portal);
    }
}
