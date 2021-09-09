using FluentValidation;

namespace PowerPlantCodingChallenge.API.Controllers.Dtos
{
    public class PowerPlanDtoValidator : AbstractValidator<PowerPlanDto>
    {
        public PowerPlanDtoValidator()
        {
            RuleFor(x => x.RequiredLoad)
                .GreaterThan(0)
                .WithMessage("The load should be higher than 0");

            RuleForEach(x => x.PowerPlants)
                .NotNull()
                .SetValidator(new PowerPlantDtoValidator());

            RuleFor(x => x.Fuels)
                .NotNull()
                .SetValidator(new EnergyMetricsDtoValidator());
        }
    }
}
