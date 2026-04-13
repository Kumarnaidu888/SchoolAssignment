using FluentValidation;
using SchoolApplication.Contracts.Classes;

namespace SchoolApplication.Validators;

public sealed class CreateClassRequestValidator : AbstractValidator<CreateClassRequest>
{
    public CreateClassRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters.");
    }
}
