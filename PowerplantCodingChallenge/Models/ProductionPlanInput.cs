using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Models
{
    // The production plan as recieved from the input json
    public class ProductionPlanInput
    {
        public ProductionPlanInput()
        {
        }

        public ProductionPlanInput(double load, EnergyMetrics fuels, List<PowerPlant> powerPlants)
        {
            RequiredLoad = load;
            Fuels = fuels;
            PowerPlants = powerPlants;
        }

        [JsonProperty(PropertyName = "Load")]
        public double RequiredLoad { get; set; }
        public EnergyMetrics Fuels { get; set; }
        public List<PowerPlant> PowerPlants { get; set; }
    }
}
