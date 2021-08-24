using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Energy.Types
{
    public class ProductionPlanScenario
    {
        public List<MinimalistPowerPlant> PowerPlants { get; set; }
        public double PMax { get; set; }
        public double PMin { get; set; }

        public void RefreshPs()
        {
            PMax = PowerPlants.Where(x => x.IsTurnedOn).Select(x => x.PMax).Sum();
            PMin = PowerPlants.Where(x => x.IsTurnedOn).Select(x => x.PMin).Sum();
        }
    }
}
