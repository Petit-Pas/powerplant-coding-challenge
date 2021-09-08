using FluentValidation;

namespace PowerplantCodingChallenge.API.Controllers.Dtos
{
    public class EnergyMetricsDtoValidator : AbstractValidator<EnergyMetricsDto>
    {
        public EnergyMetricsDtoValidator()
        {
            RuleFor(x => x.WindEfficiency)
                .InclusiveBetween(0, 100)
                .WithMessage("wind(%) should be between 0 and 100");

            RuleFor(x => x.KersosineCost)
                .GreaterThanOrEqualTo(0)
                .WithMessage("kerosine cost cannot be negative");

            RuleFor(x => x.GasCost)
                .GreaterThanOrEqualTo(0)
                .WithMessage("gas cost cannot be negative");

            RuleFor(x => x.Co2)
                .GreaterThanOrEqualTo(0)
                .WithMessage("co2 cost cannot be negative");
        }
    }
}
