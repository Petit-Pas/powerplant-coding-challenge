namespace PowerPlantCodingChallenge.API.Controllers.Dtos
{
    public class PowerPlantDto
    {
        public PowerPlantDto(string name, string type, double efficiency, double pMin, double pMax)
        {
            Name = name;
            Type = type;
            Efficiency = efficiency;
            PMin = pMin;
            PMax = pMax;
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public double Efficiency { get; set; }
        public double PMin { get; set; }
        public double PMax { get; set; }
    }
}
