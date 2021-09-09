using FluentValidation;

namespace PowerPlantCodingChallenge.API.Controllers.Dtos
{
    public class PowerPlantDtoValidator : AbstractValidator<PowerPlantDto>
    {
        public PowerPlantDtoValidator()
        {
            RuleFor(x => x.Efficiency)
                .InclusiveBetween(0, 1)
                .WithMessage("Efficiency should be between 0 and 1");

            RuleFor(x => x.PMax)
                .GreaterThan(0)
                .WithMessage("PMax must be bigger than 0");

            RuleFor(x => x.PMin)
                .GreaterThanOrEqualTo(0)
                .WithMessage("PMin cannot be lower than 0");

            RuleFor(x => x.PMin)
                .LessThanOrEqualTo(x => x.PMax)
                .WithMessage("PMin must be lower than PMax");
        }
    }
}
