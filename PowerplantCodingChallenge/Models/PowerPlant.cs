using PowerPlantCodingChallenge.API.Controllers.Dtos;

namespace PowerplantCodingChallenge.API.Models
{
    public class PowerPlant
    {
        private double _efficiency;
        private EnergySource _energySource;

        public string Name { get; private set; }
        public double PMin { get; private set; }
        public double PMax { get; private set; }
        public double CostPerMW { get; private set; }

        public double PDelivered { get; private set; }
        public bool IsTurnedOn { get; private set; }

        public PowerPlant(string name, EnergySource energySource, double efficiency, double pMin, double pMax, EnergyMetricsDto energyMetrics, bool co2enabled)
        {
            Name = name;
            this._energySource = energySource;
            PMax = pMax;
            PMin = pMin;
            this._efficiency = efficiency;

            AdaptValuesToEnergyType(energyMetrics);
            // turning a powerplant ON has no impact if PMin is 0
            // Warning, this needs to be done after the AdaptValuesToEnergyType since the wind turbines are recieved as having 0 as PMin, which is wrong
            IsTurnedOn = PMin == 0 ? true : false;
            PDelivered = 0;

            ComputeCostPerMW(energyMetrics, co2enabled);
        }

        public PowerPlant(PowerPlant to_copy)
        {
            Name = to_copy.Name;
            _energySource = to_copy._energySource;
            PMax = to_copy.PMax;
            PMin = to_copy.PMin;
            _efficiency = to_copy._efficiency;

            IsTurnedOn = to_copy.IsTurnedOn;
            PDelivered = to_copy.PDelivered;

            CostPerMW = to_copy.CostPerMW;
        }

        private void AdaptValuesToEnergyType(EnergyMetricsDto energyMetrics)
        {
            if (_energySource == EnergySource.Wind)
            {
                // computing the new max value according to the current wind
                PMax = PMax * energyMetrics.WindEfficiency / 100;
                // since wind turbines can't be partially on, PMin is equal to PMax
                PMin = PMax;
            }
        }

        // will compute the specific values for wind / fossil energies
        private void ComputeCostPerMW(EnergyMetricsDto energyMetrics, bool co2CostEnabled)
        {
            if (_energySource == EnergySource.Wind)
            {
                CostPerMW = 0;
            }
            else if (_energySource == EnergySource.Gas)
            {
                CostPerMW = energyMetrics.GasCost / _efficiency;
                if (co2CostEnabled)
                    CostPerMW += energyMetrics.CO2PerMw * energyMetrics.Co2;
            }
            else
            {
                CostPerMW = energyMetrics.KersosineCost / _efficiency;
            }
        }

        // will override PDelivered value
        public void UpdatePDelivered(double value)
        {
            PDelivered = value;
        }

        // will increase PDelivered by given value
        public void IncreasePDeliveredBy(double amount)
        {
            PDelivered += amount;
        }

        public void TurnOn()
        {
            IsTurnedOn = true;
            PDelivered = PMin;
        }
    }
}
