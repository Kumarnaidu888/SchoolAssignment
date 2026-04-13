using FluentValidation;
using SchoolApplication.Contracts.Classes;

namespace SchoolApplication.Validators;

public sealed class UpdateClassRequestValidator : AbstractValidator<UpdateClassRequest>
{
    public UpdateClassRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters.");
    }
}
