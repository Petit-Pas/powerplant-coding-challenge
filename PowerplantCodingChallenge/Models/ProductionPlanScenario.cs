using PowerplantCodingChallenge.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Models
{
    public class ProductionPlanScenario
    {
        public List<MinimalistPowerPlant> PowerPlants { get; set; } = new List<MinimalistPowerPlant>();
        public double PMax { get; set; }
        public double PMin { get; set; }
        public double PDelivered { get; set; }
        public double TotalCost { get; set; }


        public void RefreshPs()
        {
            var TurnedOn = PowerPlants.Where(x => x.IsTurnedOn);
            PMax = TurnedOn.Select(x => x.PMax).Sum();
            PMin = TurnedOn.Select(x => x.PMin).Sum();
            PDelivered = TurnedOn.Select(x => x.PDelivered).Sum();
        }

        public void ComputeTotalCost()
        {
            TotalCost = 0;
            foreach (MinimalistPowerPlant powerPlant in PowerPlants)
            {
                TotalCost += powerPlant.PDelivered * powerPlant.CostPerMW;
            }
        }

        /// <summary>
        ///     Should only be called on scenarios where PMin < load < PMax
        ///     Will modify the PDelivered to reach the given load
        ///     MinimalistPowerPlants should be ordered by CostPerMW to enable the optimization of the scenarios
        /// </summary>
        /// <param name="requiredLoad"></param>
        public void FineTune(double requiredLoad)
        {
            RefreshPs();
            if (PMin > requiredLoad || PMax < requiredLoad)
                throw new InvalidLoadException("This scenario cannot be finetuned to meet the given load");
            requiredLoad -= PDelivered;
            foreach (MinimalistPowerPlant powerPlant in PowerPlants.Where(x => x.IsTurnedOn))
            {
                if (powerPlant.PDelivered != powerPlant.PMax)
                {
                    double additionalLoad = Math.Min(requiredLoad, powerPlant.PMax - powerPlant.PDelivered);
                    powerPlant.PDelivered += additionalLoad;
                    PDelivered += additionalLoad;
                    requiredLoad -= additionalLoad;
                }
            }
        }
    }
}
