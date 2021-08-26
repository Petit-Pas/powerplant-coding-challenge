using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Models
{
    // a shorter representation of a powerplant that is used in ProductionPlanScenario
    // since there could be a lot of duplicates of this class, it is important to keep it as minimal as possible
    public class MinimalistPowerPlant
    {
        public double PMax { get; set; }
        public double PMin { get; set; }
        public double PDelivered { get; set; }
        public double CostPerMW { get; set; }
        public bool IsTurnedOn { get; set; }
    }
}
