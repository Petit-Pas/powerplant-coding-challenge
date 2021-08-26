using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using PowerplantCodingChallenge.Models;
using PowerplantCodingChallenge.Models.Exceptions;
using PowerplantCodingChallenge.Services.Planners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Test.Services.Planners
{
    public class BruteForceProductionPlanPlannerScenarios
    {
        private BruteForceProductionPlanPlanner _planner;
        private BruteForceProductionPlanPlanner _plannerCO2Enabled;
        private EnergyMetrics _baseEnergyMetrics;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Mock<ILogger<BruteForceProductionPlanPlanner>> logger = new Mock<ILogger<BruteForceProductionPlanPlanner>>();
            Mock<IConfigurationSection> configurationSection = new Mock<IConfigurationSection>();
            configurationSection.SetupGet(x => x.Value).Returns("false");
            Mock<IConfiguration> configuration = new Mock<IConfiguration>();
            configuration.Setup(x => x.GetSection(It.IsAny<string>()))
                    .Returns(configurationSection.Object);

            _planner = new BruteForceProductionPlanPlanner(logger.Object, configuration.Object);
            _baseEnergyMetrics = new EnergyMetrics() { Co2 = 20, KersosineCost = 50, GasCost = 15, WindEfficiency = 50 };

            Mock<IConfigurationSection> configurationSectionCO2Enabled = new Mock<IConfigurationSection>();
            configurationSectionCO2Enabled.SetupGet(x => x.Value).Returns("true");
            Mock<IConfiguration> configurationCO2Enabled = new Mock<IConfiguration>();
            configurationCO2Enabled.Setup(x => x.GetSection(It.IsAny<string>()))
                    .Returns(configurationSectionCO2Enabled.Object);
            _plannerCO2Enabled = new BruteForceProductionPlanPlanner(logger.Object, configurationCO2Enabled.Object);

        }

        [Test]
        public void ComputeBestPowerUsage_CannotProvideLoad_NotEnough()
        {
            ProductionPlanInput productionPlan = new ProductionPlanInput (500, _baseEnergyMetrics, new List<PowerPlant>()
            {
                new("Gas1", EnergySource.Gas, 0.5, 50, 100),
                new("Gas2", EnergySource.Gas, 0.5, 50, 100),
            });

            Assert.Throws(typeof(InvalidLoadException), () => _planner.ComputeBestPowerUsage(productionPlan));
        }

        [Test]
        public void ComputeBestPowerUsage_CannotProvideLoad_TooMuch()
        {
            ProductionPlanInput productionPlan = new ProductionPlanInput(20, _baseEnergyMetrics, new List<PowerPlant>()
            {
                new("Gas1", EnergySource.Gas, 0.5, 50, 100),
                new("Wind1", EnergySource.Wind, 1, 0, 50),
            });

            Assert.Throws(typeof(InvalidLoadException), () => _planner.ComputeBestPowerUsage(productionPlan));
        }

        [Test]
        public void ComputeBestPowerUsage_Wind_Enough()
        {
            ProductionPlanInput productionPlan = new ProductionPlanInput(25, _baseEnergyMetrics, new List<PowerPlant>()
            {
                new("Gas1", EnergySource.Gas, 0.5, 10, 100),
                new("Wind1", EnergySource.Wind, 1, 0, 50),
            });

            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            Assert.AreEqual(25, result.First(x => x.Name == "Wind1").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "Gas1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_Wind_NotEnough()
        {
            ProductionPlanInput productionPlan = new ProductionPlanInput(50, _baseEnergyMetrics, new List<PowerPlant>()
            {
                new("Gas1", EnergySource.Gas, 0.5, 10, 100),
                new("Wind1", EnergySource.Wind, 1, 0, 50),
            });

            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            Assert.AreEqual(25, result.First(x => x.Name == "Wind1").Power);
            Assert.AreEqual(25, result.First(x => x.Name == "Gas1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_Wind_TooMuch()
        {
            ProductionPlanInput productionPlan = new ProductionPlanInput(20, _baseEnergyMetrics, new List<PowerPlant>()
            {
                new("Gas1", EnergySource.Gas, 0.5, 10, 100),
                new("Wind1", EnergySource.Wind, 1, 0, 50),
            });

            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            Assert.AreEqual(0, result.First(x => x.Name == "Wind1").Power);
            Assert.AreEqual(20, result.First(x => x.Name == "Gas1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_Gas_Efficiency()
        {
            ProductionPlanInput productionPlan = new ProductionPlanInput(20, _baseEnergyMetrics, new List<PowerPlant>()
            {
                new("Gas1", EnergySource.Gas, 0.5, 10, 100),
                new("Gas2", EnergySource.Gas, 0.6, 10, 100),
                new("Gas3", EnergySource.Gas, 0.8, 10, 100),
                new("Gas4", EnergySource.Gas, 0.3, 10, 100),
                new("Gas5", EnergySource.Gas, 0.45, 10, 100),
            });

            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            Assert.AreEqual(20, result.First(x => x.Name == "Gas3").Power);
            Assert.AreEqual(0, result.Where(x => x.Name != "Gas3").Select(x => x.Power).Sum());
        }

        [Test]
        public void ComputeBestPowerUsage_Gas_Pmin()
        {
            ProductionPlanInput productionPlan = new ProductionPlanInput(125, _baseEnergyMetrics, new List<PowerPlant>()
            {
                new("Wind1", EnergySource.Wind, 1, 0, 50),
                new("Gas1", EnergySource.Gas, 0.5, 110, 200),
                new("Gas2", EnergySource.Gas, 0.8, 80, 150),
            });

            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            Assert.AreEqual(100, result.First(x => x.Name == "Gas2").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "Gas1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_Kerosine()
        {
            ProductionPlanInput productionPlan = new ProductionPlanInput(100, _baseEnergyMetrics, new List<PowerPlant>()
            {
                new("Wind1", EnergySource.Wind, 1, 0, 150),
                new("Gas1", EnergySource.Gas, 0.5, 100, 200),
                new("Kerosine1", EnergySource.Kerosine, 0.5, 0, 200),
            });

            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            Assert.AreEqual(0, result.First(x => x.Name == "Gas1").Power);
            Assert.AreEqual(25, result.First(x => x.Name == "Kerosine1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_CO2Impact()
        {
            ProductionPlanInput productionPlan = new ProductionPlanInput(150, _baseEnergyMetrics, new List<PowerPlant>()
            {
                new("Gas1", EnergySource.Gas, 0.3, 100, 200),
                new("Kerosine1", EnergySource.Kerosine, 1, 0, 200),
            });

            var resultNoCO2 = _planner.ComputeBestPowerUsage(productionPlan);
            var resultCO2 = _plannerCO2Enabled.ComputeBestPowerUsage(productionPlan);

            Assert.AreEqual(150, resultNoCO2.First(x => x.Name == "Gas1").Power);
            Assert.AreEqual(150, resultCO2.First(x => x.Name == "Kerosine1").Power);
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
