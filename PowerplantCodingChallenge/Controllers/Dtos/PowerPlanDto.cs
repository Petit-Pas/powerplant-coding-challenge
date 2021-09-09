using Newtonsoft.Json;
using System.Collections.Generic;

namespace PowerPlantCodingChallenge.API.Controllers.Dtos
{
    public class PowerPlanDto
    {
        public PowerPlanDto(double load, EnergyMetricsDto fuels, List<PowerPlantDto> powerPlants)
        {
            RequiredLoad = load;
            Fuels = fuels;
            PowerPlants = powerPlants;
        }

        [JsonProperty(PropertyName = "Load")]
        public double RequiredLoad { get; set; }
        public EnergyMetricsDto Fuels { get; set; }
        public List<PowerPlantDto> PowerPlants { get; set; }
    }
}
