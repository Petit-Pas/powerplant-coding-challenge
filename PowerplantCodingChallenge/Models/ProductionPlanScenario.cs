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
            PMax = PowerPlants.Where(x => x.IsTurnedOn).Select(x => x.PMax).Sum();
            PMin = PowerPlants.Where(x => x.IsTurnedOn).Select(x => x.PMin).Sum();
            PDelivered = PowerPlants.Where(x => x.IsTurnedOn).Select(x => x.PDelivered).Sum();
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
        /// <param name="load"></param>
        public void FineTune(double load)
        {
            RefreshPs();
            if (PMin > load || PMax < load)
                throw new InvalidLoadException("This scenario cannot finetune to meet the given load");
            load -= PDelivered;
            foreach (MinimalistPowerPlant powerPlant in PowerPlants)
            {
                if (powerPlant.IsTurnedOn && load != 0)
                {
                    if (powerPlant.PDelivered != powerPlant.PMax)
                    {
                        double modifier = Math.Min(load, powerPlant.PMax - powerPlant.PDelivered);
                        powerPlant.PDelivered += modifier;
                        PDelivered += modifier;
                        load -= modifier;
                    }
                }
            }
        }
    }
}
