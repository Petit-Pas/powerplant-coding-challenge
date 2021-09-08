using Newtonsoft.Json;
using PowerplantCodingChallenge.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.API.Controllers.Dtos
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
