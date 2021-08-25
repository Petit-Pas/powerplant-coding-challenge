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
    public class BruteForceProductionPlanPlannerTest
    {
        private BruteForceProductionPlanPlanner _planner;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _planner = new BruteForceProductionPlanPlanner(new Mock<ILogger<BruteForceProductionPlanPlanner>>().Object);
        }

        [Test]
        public void RemoveUnusableScenarios()
        {
            List<ProductionPlanScenario> scenarios = new List<ProductionPlanScenario>()
            {
                new ProductionPlanScenario() { PMin = 100, PMax = 250 },
                new ProductionPlanScenario() { PMin = 210, PMax = 250 },
                new ProductionPlanScenario() { PMin = 100, PMax = 150 },
            };

            _planner.RemoveUnusableScenarios(scenarios, 200);

            Assert.AreEqual(1, scenarios.Count);
        }

        [Test]
        public void GenerateAllPossibilities()
        {
            List<PowerPlant> powerPlants = new List<PowerPlant>()
            {
                new PowerPlant() { },
                new PowerPlant() { },
                new PowerPlant() { },
                new PowerPlant() { },
                new PowerPlant() { },
            };
            List<ProductionPlanScenario> result = _planner.GenerateAllPossibilities(powerPlants);

            Assert.AreEqual(Math.Pow(2, powerPlants.Count), result.Count);
            int amountON = 0;
            result.ForEach(x => amountON += x.PowerPlants.Where(x => x.IsTurnedOn).Count());
            // there should be (scenarios * powerplant/2) power plant ON through all the scenarios
            Assert.AreEqual(result.Count * (powerPlants.Count / 2.0d), amountON);
        }

        [Test]
        public void FormatResponse()
        {
            ProductionPlanScenario productionPlanScenario = new ProductionPlanScenario()
            {
                PowerPlants = new List<MinimalistPowerPlant>()
                {
                    new MinimalistPowerPlant() { PDelivered = 25 },
                    new MinimalistPowerPlant() { PDelivered = 50 },
                    new MinimalistPowerPlant() { PDelivered = 75 },
                },
            };
            ProductionPlanInput productionPlanInput = new ProductionPlanInput()
            {
                PowerPlants = new List<PowerPlant>()
                {
                    new PowerPlant() { Name="0" },
                    new PowerPlant() { Name="1" },
                    new PowerPlant() { Name="2" },
                },
            };

            PowerPlantUsageResponse[] result = _planner.FormatResponse(productionPlanInput, productionPlanScenario);

            Assert.AreEqual(result[0].Name, "0");
            Assert.AreEqual(result[1].Name, "1");
            Assert.AreEqual(result[2].Name, "2");
            Assert.AreEqual(result[0].Power, 25);
            Assert.AreEqual(result[1].Power, 50);
            Assert.AreEqual(result[2].Power, 75);
        }
    }
}
