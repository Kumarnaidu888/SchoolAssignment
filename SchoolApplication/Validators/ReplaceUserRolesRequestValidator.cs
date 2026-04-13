using FluentValidation;
using SchoolApplication.Contracts.Users;

namespace SchoolApplication.Validators;

public sealed class ReplaceUserRolesRequestValidator : AbstractValidator<ReplaceUserRolesRequest>
{
    public ReplaceUserRolesRequestValidator()
    {
        RuleFor(x => x.RoleNames).NotNull().NotEmpty();
    }
}
