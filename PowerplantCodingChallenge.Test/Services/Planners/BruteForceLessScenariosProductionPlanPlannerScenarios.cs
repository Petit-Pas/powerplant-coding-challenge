using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using PowerplantCodingChallenge.Models;
using PowerplantCodingChallenge.Services.Planners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Test.Services.Planners
{
    public class BruteForceLessScenariosProductionPlanPlannerScenarios
    {
        private BruteForceLessScenariosProductionPlanPlanner _planner;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Mock<ILogger<BruteForceLessScenariosProductionPlanPlanner>> logger = new Mock<ILogger<BruteForceLessScenariosProductionPlanPlanner>>();
            Mock<IConfigurationSection> configurationSection = new Mock<IConfigurationSection>();
            configurationSection.SetupGet(x => x.Value).Returns("false");
            Mock<IConfiguration> configuration = new Mock<IConfiguration>();
            configuration.Setup(x => x.GetSection(It.IsAny<string>()))
                    .Returns(configurationSection.Object);
            _planner = new BruteForceLessScenariosProductionPlanPlanner(logger.Object, configuration.Object);
        }

        [Test]
        public void ComputeBestPowerUsage_TrickyTest1()
        {
            // arrange
            EnergyMetrics energyMetrics = new EnergyMetrics() { Co2 = 0, KersosineCost = 50.8, GasCost = 20, WindEfficiency = 100 };
            ProductionPlanInput productionPlan = new ProductionPlanInput(60, energyMetrics, new List<PowerPlant>()
            {
                new("windpark1", EnergySource.Wind, 1, 0, 20),
                new("gasfired", EnergySource.Gas, 0.9, 50, 100),
                new("gasfiredinefficient", EnergySource.Gas, 0.1, 0, 100),
            });

            // act
            var result = _planner.ComputeBestPowerUsage(productionPlan);

            // assert
            Assert.AreEqual(60, result.Select(x => x.Power).Sum());
            Assert.AreEqual(0, result.First(x => x.Name == "windpark1").Power);
            Assert.AreEqual(60, result.First(x => x.Name == "gasfired").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "gasfiredinefficient").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_TrickyTest2()
        {
            // arrange
            EnergyMetrics energyMetrics = new EnergyMetrics() { Co2 = 0, KersosineCost = 50.8, GasCost = 20, WindEfficiency = 100 };
            ProductionPlanInput productionPlan = new ProductionPlanInput(80, energyMetrics, new List<PowerPlant>()
            {
                new("windpark1", EnergySource.Wind, 1, 0, 60),
                new("gasfired", EnergySource.Gas, 0.9, 50, 100),
                new("gasfiredinefficient", EnergySource.Gas, 0.1, 0, 200),
            });

            // act
            var result = _planner.ComputeBestPowerUsage(productionPlan);

            // assert
            Assert.AreEqual(80, result.Select(x => x.Power).Sum());
            Assert.AreEqual(0, result.First(x => x.Name == "windpark1").Power);
            Assert.AreEqual(80, result.First(x => x.Name == "gasfired").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "gasfiredinefficient").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_ExamplePayload1_NoCO2()
        {
            // arrange
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

            // act
            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            // assert
            Assert.AreEqual(480, result.Select(x => x.Power).Sum());
            Assert.AreEqual(90, result.First(x => x.Name == "windpark1").Power);
            Assert.AreEqual(21.6, result.First(x => x.Name == "windpark2").Power);
            Assert.AreEqual(368.4, result.First(x => x.Name == "gasfiredbig1").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "gasfiredbig2").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "gasfiredsomewhatsmaller").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "tj1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_ExamplePayload2_NoCO2()
        {
            // arrange
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

            // act
            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            // assert
            Assert.AreEqual(480, result.Select(x => x.Power).Sum());
            Assert.AreEqual(0, result.First(x => x.Name == "windpark1").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "windpark2").Power);
            Assert.AreEqual(380, result.First(x => x.Name == "gasfiredbig1").Power);
            Assert.AreEqual(100, result.First(x => x.Name == "gasfiredbig2").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "gasfiredsomewhatsmaller").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "tj1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_ExamplePayload3_NoCO2()
        {
            // arrange
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

            // act
            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            // assert
            Assert.AreEqual(910, result.Select(x => x.Power).Sum());
            Assert.AreEqual(90, result.First(x => x.Name == "windpark1").Power);
            Assert.AreEqual(21.6, result.First(x => x.Name == "windpark2").Power);
            Assert.AreEqual(460, result.First(x => x.Name == "gasfiredbig1").Power);
            Assert.AreEqual(338.4, result.First(x => x.Name == "gasfiredbig2").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "gasfiredsomewhatsmaller").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "tj1").Power);
        }
    }
}
