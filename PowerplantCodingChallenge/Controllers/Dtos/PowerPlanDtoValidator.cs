using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.API.Controllers.Dtos
{
    public class PowerPlanDtoValidator : AbstractValidator<PowerPlanDto>
    {
        public PowerPlanDtoValidator()
        {
            RuleFor(x => x.RequiredLoad)
                .GreaterThan(0)
                .WithMessage("The load should be higher than 0");

            RuleForEach(x => x.PowerPlants)
                .SetValidator(new PowerPlantDtoValidator());

            RuleFor(x => x.Fuels)
                .SetValidator(new EnergyMetricsDtoValidator());
        }
    }
}
