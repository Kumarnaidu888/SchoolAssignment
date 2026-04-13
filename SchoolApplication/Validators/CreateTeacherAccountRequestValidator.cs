using FluentValidation;
using SchoolApplication.Contracts.Users;

namespace SchoolApplication.Validators;

public sealed class CreateTeacherAccountRequestValidator : AbstractValidator<CreateTeacherAccountRequest>
{
    public CreateTeacherAccountRequestValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(200);
    }
}
