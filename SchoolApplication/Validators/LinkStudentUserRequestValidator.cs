using FluentValidation;
using SchoolApplication.Contracts.Students;

namespace SchoolApplication.Validators;

public sealed class LinkStudentUserRequestValidator : AbstractValidator<LinkStudentUserRequest>
{
    public LinkStudentUserRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
