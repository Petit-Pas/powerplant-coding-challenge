using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Energy.Types
{
    public class ProductionPlanInput
    {
        public ProductionPlanInput()
        {
        }

        public ProductionPlanInput(double load, EnergyMetrics fuels, List<PowerPlant> powerPlants)
        {
            Load = load;
            Fuels = fuels;
            PowerPlants = powerPlants;
        }

        public double Load { get; set; }
        public EnergyMetrics Fuels { get; set; }
        public List<PowerPlant> PowerPlants { get; set; }
    }
}
