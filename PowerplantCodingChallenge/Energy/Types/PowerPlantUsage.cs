using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Energy.Types
{
    public class PowerPlantUsage
    {
        public string Name { get; set; }

        // Should be round to .1
        [JsonProperty(PropertyName = "p")]
        public double Power { get; set; }
    }
}
