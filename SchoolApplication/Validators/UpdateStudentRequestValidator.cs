using FluentValidation;
using SchoolApplication.Contracts.Students;

namespace SchoolApplication.Validators;

public sealed class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
    public UpdateStudentRequestValidator()
    {
        RuleFor(x => x.SectionId).GreaterThan(0);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AdmissionNo).MaximumLength(50).When(x => x.AdmissionNo is not null);
    }
}
