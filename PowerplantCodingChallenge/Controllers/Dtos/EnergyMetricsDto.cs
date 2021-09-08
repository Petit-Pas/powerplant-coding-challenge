using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.API.Controllers.Dtos
{
    public class EnergyMetricsDto
    {
        [JsonProperty(PropertyName = @"gas(euro/MWh)")]
        public double GasCost { get; set; }

        [JsonProperty(PropertyName = @"kerosine(euro/MWh)")]
        public double KersosineCost { get; set; }

        [JsonProperty(PropertyName = @"co2(euro/ton)")]
        public double Co2 { get; set; }

        [JsonProperty(PropertyName = @"wind(%)")]
        public double WindEfficiency { get; set; }

        [JsonProperty(PropertyName = @"co2(ton/MWh)")]
        public double CO2PerMw { get; } = 0.3;
    }
}
