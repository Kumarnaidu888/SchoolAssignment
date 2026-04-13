using FluentValidation;
using SchoolApplication.Contracts.Teachers;

namespace SchoolApplication.Validators;

public sealed class ReplaceTeacherSectionsRequestValidator : AbstractValidator<ReplaceTeacherSectionsRequest>
{
    public ReplaceTeacherSectionsRequestValidator()
    {
        RuleFor(x => x.SectionIds).NotNull();
    }
}
