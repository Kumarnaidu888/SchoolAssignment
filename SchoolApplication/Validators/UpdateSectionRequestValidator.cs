using FluentValidation;
using SchoolApplication.Contracts.Sections;

namespace SchoolApplication.Validators;

public sealed class UpdateSectionRequestValidator : AbstractValidator<UpdateSectionRequest>
{
    public UpdateSectionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Name must not exceed 50 characters.");
    }
}
