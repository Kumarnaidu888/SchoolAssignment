using FluentValidation;
using SchoolApplication.Contracts.Marks;

namespace SchoolApplication.Validators;

public sealed class SubmitMarksRequestValidator : AbstractValidator<SubmitMarksRequest>
{
    public SubmitMarksRequestValidator()
    {
        RuleFor(x => x.ExamId).GreaterThan(0);
        RuleFor(x => x.Marks).NotEmpty();
        RuleForEach(x => x.Marks).ChildRules(m =>
        {
            m.RuleFor(x => x.StudentId).GreaterThan(0);
            m.RuleFor(x => x.SubjectId).GreaterThan(0);
            m.RuleFor(x => x.Score)
                .InclusiveBetween(0, 1000)
                .When(x => x.Score.HasValue)
                .WithMessage("Score must be between 0 and 1000 when provided.");
        });
    }
}
