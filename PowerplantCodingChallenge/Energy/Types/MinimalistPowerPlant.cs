using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Energy.Types
{
    public class MinimalistPowerPlant
    {
        public double PMax { get; set; }
        public double PMin { get; set; }
        public double PDelivered { get; set; }
        public double CostPerMW { get; set; }
        public bool IsTurnedOn { get; set; }
    }
}
