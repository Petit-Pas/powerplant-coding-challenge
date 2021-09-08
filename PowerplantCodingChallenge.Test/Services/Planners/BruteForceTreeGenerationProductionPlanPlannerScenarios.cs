using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using PowerplantCodingChallenge.API.Controllers.Dtos;
using PowerplantCodingChallenge.API.Services.Planners;
using PowerplantCodingChallenge.Models;
using PowerplantCodingChallenge.Models.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace PowerplantCodingChallenge.Test.Services.Planners
{
    public class BruteForceTreeGenerationProductionPlanPlannerScenarios
    {
        private BruteForceTreeGenerationProductionPlanPlanner _planner;

        private BruteForceTreeGenerationProductionPlanPlanner _plannerCo2Enabled;

        private EnergyMetricsDto _baseEnergyMetrics;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _baseEnergyMetrics = new EnergyMetricsDto() { Co2 = 20, KersosineCost = 50, GasCost = 15, WindEfficiency = 50 };
        }

        [SetUp]
        public void Setup()
        {
            Mock<ILogger<BruteForceTreeGenerationProductionPlanPlanner>> logger = new();
            Mock<IConfigurationSection> configurationSection = new();
            configurationSection.SetupGet(x => x.Value).Returns("false");
            Mock<IConfiguration> configuration = new();
            configuration.Setup(x => x.GetSection(It.IsAny<string>()))
                    .Returns(configurationSection.Object);
            _planner = new BruteForceTreeGenerationProductionPlanPlanner(logger.Object, configuration.Object);

            Mock<IConfigurationSection> configurationSectionCO2Enabled = new Mock<IConfigurationSection>();
            configurationSectionCO2Enabled.SetupGet(x => x.Value).Returns("true");
            Mock<IConfiguration> configurationCO2Enabled = new Mock<IConfiguration>();
            configurationCO2Enabled.Setup(x => x.GetSection(It.IsAny<string>()))
                    .Returns(configurationSectionCO2Enabled.Object);
            _plannerCo2Enabled = new BruteForceTreeGenerationProductionPlanPlanner(logger.Object, configurationCO2Enabled.Object);
        }

        [Test]
        public void ComputeBestPowerUsage_CannotProvideLoad_NotEnough()
        {
            PowerPlanDto productionPlan = new (500, _baseEnergyMetrics, new List<PowerPlantDto>()
            {
                new("Gas1", EnergySource.Gas.ConvertToString(), 0.5, 50, 100),
                new("Gas2", EnergySource.Gas.ConvertToString(), 0.5, 50, 100),
            });

            Assert.Throws(typeof(InvalidLoadException), () => _planner.ComputeBestPowerUsage(productionPlan));
        }

        [Test]
        public void ComputeBestPowerUsage_CannotProvideLoad_TooMuch()
        {
            PowerPlanDto productionPlan = new (20, _baseEnergyMetrics, new List<PowerPlantDto>()
            {
                new("Gas1", EnergySource.Gas.ConvertToString(), 0.5, 50, 100),
                new("Wind1", EnergySource.Wind.ConvertToString(), 1, 0, 50),
            });

            Assert.Throws(typeof(InvalidLoadException), () => _planner.ComputeBestPowerUsage(productionPlan));
        }

        [Test]
        public void ComputeBestPowerUsage_Wind_Enough()
        {
            PowerPlanDto productionPlan = new (25, _baseEnergyMetrics, new List<PowerPlantDto>()
            {
                new("Gas1", EnergySource.Gas.ConvertToString(), 0.5, 10, 100),
                new("Wind1", EnergySource.Wind.ConvertToString(), 1, 0, 50),
            });

            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            Assert.AreEqual(25, result.First(x => x.Name == "Wind1").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "Gas1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_Wind_NotEnough()
        {
            PowerPlanDto productionPlan = new (50, _baseEnergyMetrics, new List<PowerPlantDto>()
            {
                new("Gas1", EnergySource.Gas.ConvertToString(), 0.5, 10, 100),
                new("Wind1", EnergySource.Wind.ConvertToString(), 1, 0, 50),
            });

            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            Assert.AreEqual(25, result.First(x => x.Name == "Wind1").Power);
            Assert.AreEqual(25, result.First(x => x.Name == "Gas1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_Wind_TooMuch()
        {
            PowerPlanDto productionPlan = new (20, _baseEnergyMetrics, new List<PowerPlantDto>()
            {
                new("Gas1", EnergySource.Gas.ConvertToString(), 0.5, 10, 100),
                new("Wind1", EnergySource.Wind.ConvertToString(), 1, 0, 50),
            });

            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            Assert.AreEqual(0, result.First(x => x.Name == "Wind1").Power);
            Assert.AreEqual(20, result.First(x => x.Name == "Gas1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_Gas_Efficiency()
        {
            PowerPlanDto productionPlan = new (20, _baseEnergyMetrics, new List<PowerPlantDto>()
            {
                new("Gas1", EnergySource.Gas.ConvertToString(), 0.5, 10, 100),
                new("Gas2", EnergySource.Gas.ConvertToString(), 0.6, 10, 100),
                new("Gas3", EnergySource.Gas.ConvertToString(), 0.8, 10, 100),
                new("Gas4", EnergySource.Gas.ConvertToString(), 0.3, 10, 100),
                new("Gas5", EnergySource.Gas.ConvertToString(), 0.45, 10, 100),
            });

            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            Assert.AreEqual(20, result.First(x => x.Name == "Gas3").Power);
            Assert.AreEqual(0, result.Where(x => x.Name != "Gas3").Select(x => x.Power).Sum());
        }

        [Test]
        public void ComputeBestPowerUsage_Gas_Pmin()
        {
            PowerPlanDto productionPlan = new (125, _baseEnergyMetrics, new List<PowerPlantDto>()
            {
                new("Wind1", EnergySource.Wind.ConvertToString(), 1, 0, 50),
                new("Gas1", EnergySource.Gas.ConvertToString(), 0.5, 110, 200),
                new("Gas2", EnergySource.Gas.ConvertToString(), 0.8, 80, 150),
            });

            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            Assert.AreEqual(100, result.First(x => x.Name == "Gas2").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "Gas1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_Kerosine()
        {
            PowerPlanDto productionPlan = new (100, _baseEnergyMetrics, new List<PowerPlantDto>()
            {
                new("Wind1", EnergySource.Wind.ConvertToString(), 1, 0, 150),
                new("Gas1", EnergySource.Gas.ConvertToString(), 0.5, 100, 200),
                new("Kerosine1", EnergySource.Kerosine.ConvertToString(), 0.5, 0, 200),
            });

            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            Assert.AreEqual(0, result.First(x => x.Name == "Gas1").Power);
            Assert.AreEqual(25, result.First(x => x.Name == "Kerosine1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_CO2Impact()
        {
            PowerPlanDto productionPlan = new (150, _baseEnergyMetrics, new List<PowerPlantDto>()
            {
                new("Gas1", EnergySource.Gas.ConvertToString(), 0.3, 100, 200),
                new("Kerosine1", EnergySource.Kerosine.ConvertToString(), 1, 0, 200),
            });

            var resultNoCO2 = _planner.ComputeBestPowerUsage(productionPlan);
            var resultCO2 = _plannerCo2Enabled.ComputeBestPowerUsage(productionPlan);

            Assert.AreEqual(150, resultNoCO2.First(x => x.Name == "Gas1").Power);
            Assert.AreEqual(150, resultCO2.First(x => x.Name == "Kerosine1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_TrickyTest1()
        {
            // arrange
            EnergyMetricsDto energyMetrics = new () { Co2 = 0, KersosineCost = 50.8, GasCost = 20, WindEfficiency = 100 };
            PowerPlanDto productionPlan = new (60, energyMetrics, new List<PowerPlantDto> {
                new("windpark1", EnergySource.Wind.ConvertToString(), 1, 0, 20),
                new("gasfired", EnergySource.Gas.ConvertToString(), 0.9, 50, 100),
                new("gasfiredinefficient", EnergySource.Gas.ConvertToString(), 0.1, 0, 100),
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
            EnergyMetricsDto energyMetrics = new () { Co2 = 0, KersosineCost = 50.8, GasCost = 20, WindEfficiency = 100 };
            PowerPlanDto productionPlan = new (80, energyMetrics, new List<PowerPlantDto> {
                new("windpark1", EnergySource.Wind.ConvertToString(), 1, 0, 60),
                new("gasfired", EnergySource.Gas.ConvertToString(), 0.9, 50, 100),
                new("gasfiredinefficient", EnergySource.Gas.ConvertToString(), 0.1, 0, 200),
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
            EnergyMetricsDto energyMetrics = new () { Co2 = 0, KersosineCost = 50.8, GasCost = 13.4, WindEfficiency = 60 };
            PowerPlanDto productionPlan = new (480, energyMetrics, new List<PowerPlantDto> {
                new("gasfiredbig1", EnergySource.Gas.ConvertToString(), 0.53, 100, 460),
                new("gasfiredbig2", EnergySource.Gas.ConvertToString(), 0.53, 100, 460),
                new("gasfiredsomewhatsmaller", EnergySource.Gas.ConvertToString(), 0.37, 40, 210),
                new("tj1", EnergySource.Kerosine.ConvertToString(), 0.3, 0, 16),
                new("windpark1", EnergySource.Wind.ConvertToString(), 1, 0, 150),
                new("windpark2", EnergySource.Wind.ConvertToString(), 1, 0, 36),
            });

            // act
            var result = _planner.ComputeBestPowerUsage(productionPlan).ToList();

            // assert
            Assert.AreEqual(480, result.Select(x => x.Power).Sum());
            Assert.AreEqual(90, result.First(x => x.Name == "windpark1").Power);
            Assert.AreEqual(21.6, result.First(x => x.Name == "windpark2").Power);
            Assert.AreEqual(268.4, result.First(x => x.Name == "gasfiredbig1").Power);
            Assert.AreEqual(100, result.First(x => x.Name == "gasfiredbig2").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "gasfiredsomewhatsmaller").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "tj1").Power);
        }

        [Test]
        public void ComputeBestPowerUsage_ExamplePayload2_NoCO2()
        {
            // arrange
            EnergyMetricsDto energyMetrics = new () { Co2 = 0, KersosineCost = 50.8, GasCost = 13.4, WindEfficiency = 0 };
            PowerPlanDto productionPlan = new (480, energyMetrics, new List<PowerPlantDto> {
                new("gasfiredbig1", EnergySource.Gas.ConvertToString(), 0.53, 100, 460),
                new("gasfiredbig2", EnergySource.Gas.ConvertToString(), 0.53, 100, 460),
                new("gasfiredsomewhatsmaller", EnergySource.Gas.ConvertToString(), 0.37, 40, 210),
                new("tj1", EnergySource.Kerosine.ConvertToString(), 0.3, 0, 16),
                new("windpark1", EnergySource.Wind.ConvertToString(), 1, 0, 150),
                new("windpark2", EnergySource.Wind.ConvertToString(), 1, 0, 36),
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
            EnergyMetricsDto energyMetrics = new () { Co2 = 0, KersosineCost = 50.8, GasCost = 13.4, WindEfficiency = 60 };
            PowerPlanDto productionPlan = new (910, energyMetrics, new List<PowerPlantDto> {
                new("gasfiredbig1", EnergySource.Gas.ConvertToString(), 0.53, 100, 460),
                new("gasfiredbig2", EnergySource.Gas.ConvertToString(), 0.53, 100, 460),
                new("gasfiredsomewhatsmaller", EnergySource.Gas.ConvertToString(), 0.37, 40, 210),
                new("tj1", EnergySource.Kerosine.ConvertToString(), 0.3, 0, 16),
                new("windpark1", EnergySource.Wind.ConvertToString(), 1, 0, 150),
                new("windpark2", EnergySource.Wind.ConvertToString(), 1, 0, 36),
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
