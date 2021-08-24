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
                        EnergyType = EnergySources.Gas;
                        break;
                    case "turbojet":
                        EnergyType = EnergySources.Kerosine;
                        break;
                    case "windturbine":
                        EnergyType = EnergySources.Wind;
                        break;
                    default:
                        EnergyType = EnergySources.Unknown;
                        break;
                }
            }
        }
        public EnergySources EnergyType { get; set; } = EnergySources.Unknown;
        public float Efficiency { get; set; }
        public int PMin { get; set; }
        public int PMax { get; set; }
    }
}
