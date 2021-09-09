using PowerPlantCodingChallenge.API.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerplantCodingChallenge.API.Models
{
    public class ProductionPlanScenario
    {
        public ProductionPlanScenario(List<PowerPlant> powerPlants)
        {
            PowerPlants = powerPlants;
        }

        private double _pMax;
        private double _pMin;

        public List<PowerPlant> PowerPlants { get; private set; } = new();
        public double PDelivered { get; private set; }
        public double TotalCost { get; private set; }

        /// <summary>
        ///     Will update TotalCost depending on the PDelivered of each PowerPlant
        /// </summary>
        private void ComputeTotalCost()
        {
            TotalCost = PowerPlants.Sum(x => x.PDelivered * x.CostPerMW);
        }

        /// <summary>
        ///     Should only be called on scenarios where PMin < load < PMax => will throw otherwise
        ///     Will modify the PDelivered to reach the given load
        ///     MinimalistPowerPlants should be ordered by CostPerMW to enable the optimization of the scenarios
        /// </summary>
        /// <param name="requiredLoad"></param>
        public void FineTune(double requiredLoad)
        {
            ComputePs();
            if (_pMin > requiredLoad || _pMax < requiredLoad)
                throw new InvalidLoadException("This scenario cannot be finetuned to meet the given load");

            double remainingLoad = requiredLoad - PDelivered;

            foreach (var powerPlant in PowerPlants.Where(x => x.IsTurnedOn))
            {
                if (powerPlant.PDelivered.CompareTo(powerPlant.PMax) != 0)
                {
                    double additionalLoad = Math.Min(remainingLoad, powerPlant.PMax - powerPlant.PDelivered);

                    powerPlant.IncreasePDeliveredBy(additionalLoad);
                    remainingLoad -= additionalLoad;
                }
            }
            ComputePDelivered();
            ComputeTotalCost();
        }

        public void ComputePs()
        {
            ComputePBoundaries();
            ComputePDelivered();
        }

        private void ComputePBoundaries()
        {
            var turnedOn = PowerPlants.Where(x => x.IsTurnedOn).ToList();
            _pMax = turnedOn.Sum(x => x.PMax);
            _pMin = turnedOn.Sum(x => x.PMin);
        }

        private void ComputePDelivered()
        {
            PDelivered = PowerPlants.Where(x => x.IsTurnedOn).Sum(x => x.PDelivered);
        }

    }
}
