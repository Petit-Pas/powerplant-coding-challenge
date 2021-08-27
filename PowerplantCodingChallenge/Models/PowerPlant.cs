using Newtonsoft.Json;
using PowerplantCodingChallenge.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Models
{
    // represents a powerPlant as recieved from the input json
    public class PowerPlant
    {
        public PowerPlant()
        {
        }

        public PowerPlant(string name, EnergySource energySource, double efficiency, double pMin, double pMax)
        {
            Name = name;
            EnergySource = energySource;
            Efficiency = efficiency;
            PMax = pMax;
            PMin = pMin;
        }

        public string Name { get; set; }

        // we convert this to an enum to avoid a maximum of string comparison
        public string Type
        {
            set
            {
                switch (value)
                {
                    case "gasfired":
                        EnergySource = EnergySource.Gas;
                        break;
                    case "turbojet":
                        EnergySource = EnergySource.Kerosine;
                        break;
                    case "windturbine":
                        EnergySource = EnergySource.Wind;
                        break;
                    default:
                        throw new InvalidEnergyTypeException($"Unrecognized energy type '{value}'");
                }
            }
            get
            {
                switch (EnergySource)
                {
                    case EnergySource.Gas:
                        return "gasfired";
                    case EnergySource.Kerosine:
                        return "turbojet";
                    case EnergySource.Wind:
                        return "windturbine";
                    default:
                        return "unknown";
                }
            }
        }
        [JsonIgnore]
        public EnergySource EnergySource { get; set; } = EnergySource.Unknown;

        public double Efficiency { get; set; }
        public double PMin { get; set; }
        public double PMax { get; set; }

        [JsonIgnore]
        public double CostPerMW { get; set; }

        // will compute the specific values for wind / fossil energies
        public void Init(EnergyMetrics energyMetrics, bool co2CostEnabled)
        {
            if (EnergySource == EnergySource.Wind)
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
                    EnergySource.Gas => energyMetrics.GasCost,
                    EnergySource.Kerosine => energyMetrics.KersosineCost,
                    _ => throw new InvalidEnergyTypeException($"You have to provide an energy type for powerplant {Name}"),
                };
                CostPerMW = ResourceCostPerMw / Efficiency;
                if (EnergySource == EnergySource.Gas && co2CostEnabled)
                {
                    CostPerMW += 0.3 * energyMetrics.Co2;
                }
            }
        }

        // gets a shorter representation of the Power plant
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
