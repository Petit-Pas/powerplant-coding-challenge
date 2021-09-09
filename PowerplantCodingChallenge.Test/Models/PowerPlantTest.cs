using NUnit.Framework;
using PowerplantCodingChallenge.API.Models;
using PowerPlantCodingChallenge.API.Controllers.Dtos;

namespace PowerPlantCodingChallenge.Test.Models
{
    public class PowerPlantTest
    {
        private EnergyMetricsDto _energyMetrics;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _energyMetrics = new ()
            {
                Co2 = 20,
                GasCost = 15,
                KersosineCost = 50,
                WindEfficiency = 50
            };
        }

        private PowerPlant BuildPowerPlant(string name = "name", EnergySource energySource = EnergySource.Wind, double efficiency = 1,
                                            double pMin = 0, double pMax = 100, EnergyMetricsDto energyMetrics = null, bool co2enabled = false)
        {
            return new PowerPlant(name, energySource, efficiency, pMin, pMax, energyMetrics, co2enabled);
        }

        [Test]
        public void CostPerMW_Gas()
        {
            // arrange + act
            PowerPlant powerPlant = BuildPowerPlant(efficiency: 0.5d, energySource: EnergySource.Gas, energyMetrics: _energyMetrics);

            // assert
            Assert.AreEqual(30, powerPlant.CostPerMW);
        }

        [Test]
        public void CostPerMW_Kersosine()
        {
            // arrange + act
            PowerPlant powerPlant = BuildPowerPlant(efficiency: 0.5d, energySource: EnergySource.Kerosine, energyMetrics: _energyMetrics); 

            // assert
            Assert.AreEqual(100, powerPlant.CostPerMW);
        }

        [Test]
        public void CostPerMW_Wind()
        {
            // arrage + act
            PowerPlant powerPlant = BuildPowerPlant(efficiency: 1, energySource: EnergySource.Wind, energyMetrics: _energyMetrics, pMax: 100);

            // assert
            Assert.AreEqual(0, powerPlant.CostPerMW);
            Assert.AreEqual(50, powerPlant.PMax);
        }

        [Test]
        public void UpdateDelivered()
        {
            // arrange
            PowerPlant powerPlant = BuildPowerPlant(efficiency: 1, energySource: EnergySource.Wind, energyMetrics: _energyMetrics, pMax: 100);
            double delivered = powerPlant.PDelivered;

            // act
            powerPlant.UpdatePDelivered(delivered + 10);

            // assert
            Assert.AreEqual(delivered + 10, powerPlant.PDelivered);
        }

        [Test]
        public void IncreasePDeliveredBy()
        {
            // arrange
            PowerPlant powerPlant = BuildPowerPlant(efficiency: 1, energySource: EnergySource.Wind, energyMetrics: _energyMetrics, pMax: 100);
            double delivered = powerPlant.PDelivered;

            // act
            powerPlant.IncreasePDeliveredBy(10);

            // assert
            Assert.AreEqual(delivered + 10, powerPlant.PDelivered);
        }

        [Test]
        public void TurnOn()
        {
            // arrange
            PowerPlant powerPlant = BuildPowerPlant(efficiency: 1, energySource: EnergySource.Gas, energyMetrics: _energyMetrics, pMax: 100, pMin: 10);
            double initDelivered = powerPlant.PDelivered;

            // act
            powerPlant.TurnOn();

            // assert
            Assert.AreEqual(0, initDelivered);
            Assert.AreEqual(10, powerPlant.PDelivered);
        }
    }
}
