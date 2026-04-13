using FluentValidation;
using SchoolApplication.Contracts.Auth;

namespace SchoolApplication.Validators;

public sealed class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
