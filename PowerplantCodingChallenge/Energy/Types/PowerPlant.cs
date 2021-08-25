using PowerplantCodingChallenge.Energy.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Energy.Types
{
    public class PowerPlant
    {
        public string Name { get; set; }

        public string Type
        {
            set
            {
                switch (value)
                {
                    case "gasfired":
                        EnergySource = EnergySources.Gas;
                        break;
                    case "turbojet":
                        EnergySource = EnergySources.Kerosine;
                        break;
                    case "windturbine":
                        EnergySource = EnergySources.Wind;
                        break;
                    default:
                        EnergySource = EnergySources.Unknown;
                        break;
                }
            }
        }
        public EnergySources EnergySource { get; set; } = EnergySources.Unknown;

        public double Efficiency { get; set; }
        public double PMin { get; set; }
        public double PMax { get; set; }
        public double CostPerMW { get; set; }

        // will compute the specific values for wind / fossil energies
        public void Init(EnergyMetrics energyMetrics)
        { 
            if (EnergySource == EnergySources.Wind)
            {
                // computing the new max value according to the current wind
                PMax = PMax * energyMetrics.WindEfficiency / 100;
                // since wind turbines can't be partially on, PMin is equal to PMax
                PMin = PMax;
                CostPerMW = 0;
            }
            else
            {
                double ResourceCostPerMw = EnergySource switch
                {
                    EnergySources.Gas => energyMetrics.GasCost,
                    EnergySources.Kerosine => energyMetrics.KersosineCost,
                    _ => throw new InvalidEnergyTypeException(),
                };
                CostPerMW = ResourceCostPerMw / Efficiency;
            }
        }

        public MinimalistPowerPlant GetMinimalist()
        {
            return new MinimalistPowerPlant()
            {
                CostPerMW = this.CostPerMW,
                IsTurnedOn = false,
                PMax = this.PMax,
                PMin = this.PMin,
            };
        }
    }
}
