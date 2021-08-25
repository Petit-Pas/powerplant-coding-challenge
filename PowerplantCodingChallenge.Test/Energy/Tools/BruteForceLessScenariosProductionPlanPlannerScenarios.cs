using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using PowerplantCodingChallenge.Energy.Tools;
using PowerplantCodingChallenge.Energy.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Test.Energy.Tools
{
    public class BruteForceLessScenariosProductionPlanPlannerScenarios
    {
        private BruteForceLessScenariosProductionPlanPlanner _planner;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _planner = new BruteForceLessScenariosProductionPlanPlanner(new Mock<ILogger<BruteForceLessScenariosProductionPlanPlanner>>().Object);
        }

        [Test]
        public void ComputeBestPowerUsage_ExamplePayload1_NoCO2()
        {
            EnergyMetrics energyMetrics = new EnergyMetrics() { Co2 = 0, KersosineCost = 50.8, GasCost = 13.4, WindEfficiency = 60 };
            ProductionPlanInput productionPlan = new ProductionPlanInput(480, energyMetrics, new List<PowerPlant>()
            {
                new("gasfiredbig1", EnergySource.Gas, 0.53, 100, 460),
                new("gasfiredbig2", EnergySource.Gas, 0.53, 100, 460),
                new("gasfiredsomewhatsmaller", EnergySource.Gas, 0.37, 40, 210),
                new("tj1", EnergySource.Kerosine, 0.3, 0, 16),
                new("windpark1", EnergySource.Wind, 1, 0, 150),
                new("windpark2", EnergySource.Wind, 1, 0, 36),
            });

            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            Assert.AreEqual(480, result.Select(x => x.Power).Sum());

            double cost = 0;
            productionPlan.PowerPlants.Where(x => x.EnergySource == EnergySource.Gas || x.EnergySource == EnergySource.Kerosine).ToList()
                .ForEach(x => cost += x.CostPerMW * result.First(y => x.Name == y.Name).Power);
            Assert.AreEqual(9314.264150943396, cost);
        }

        [Test]
        public void ComputeBestPowerUsage_ExamplePayload2_NoCO2()
        {
            EnergyMetrics energyMetrics = new EnergyMetrics() { Co2 = 0, KersosineCost = 50.8, GasCost = 13.4, WindEfficiency = 0 };
            ProductionPlanInput productionPlan = new ProductionPlanInput(480, energyMetrics, new List<PowerPlant>()
            {
                new("gasfiredbig1", EnergySource.Gas, 0.53, 100, 460),
                new("gasfiredbig2", EnergySource.Gas, 0.53, 100, 460),
                new("gasfiredsomewhatsmaller", EnergySource.Gas, 0.37, 40, 210),
                new("tj1", EnergySource.Kerosine, 0.3, 0, 16),
                new("windpark1", EnergySource.Wind, 1, 0, 150),
                new("windpark2", EnergySource.Wind, 1, 0, 36),
            });

            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            Assert.AreEqual(480, result.Select(x => x.Power).Sum());

            double cost = 0;
            productionPlan.PowerPlants.Where(x => x.EnergySource == EnergySource.Gas || x.EnergySource == EnergySource.Kerosine).ToList()
                .ForEach(x => cost += x.CostPerMW * result.First(y => x.Name == y.Name).Power);
            Assert.AreEqual(12135.849056603774, cost);
        }

        [Test]
        public void ComputeBestPowerUsage_ExamplePayload3_NoCO2()
        {
            EnergyMetrics energyMetrics = new EnergyMetrics() { Co2 = 0, KersosineCost = 50.8, GasCost = 13.4, WindEfficiency = 60 };
            ProductionPlanInput productionPlan = new ProductionPlanInput(910, energyMetrics, new List<PowerPlant>()
            {
                new("gasfiredbig1", EnergySource.Gas, 0.53, 100, 460),
                new("gasfiredbig2", EnergySource.Gas, 0.53, 100, 460),
                new("gasfiredsomewhatsmaller", EnergySource.Gas, 0.37, 40, 210),
                new("tj1", EnergySource.Kerosine, 0.3, 0, 16),
                new("windpark1", EnergySource.Wind, 1, 0, 150),
                new("windpark2", EnergySource.Wind, 1, 0, 36),
            });

            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            Assert.AreEqual(910, result.Select(x => x.Power).Sum());

            double cost = 0;
            productionPlan.PowerPlants.Where(x => x.EnergySource == EnergySource.Gas || x.EnergySource == EnergySource.Kerosine).ToList()
                .ForEach(x => cost += x.CostPerMW * result.First(y => x.Name == y.Name).Power);
            Assert.AreEqual(20185.96226415094, cost);
        }
    }
}
