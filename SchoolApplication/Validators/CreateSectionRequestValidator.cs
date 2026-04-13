using FluentValidation;
using SchoolApplication.Contracts.Sections;

namespace SchoolApplication.Validators;

public sealed class CreateSectionRequestValidator : AbstractValidator<CreateSectionRequest>
{
    public CreateSectionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Name must not exceed 50 characters.");
    }
}
