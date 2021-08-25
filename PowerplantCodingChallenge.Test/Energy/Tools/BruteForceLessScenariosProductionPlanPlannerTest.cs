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
    public class BruteForceLessScenariosProductionPlanPlannerTest
    {
        private BruteForceLessScenariosProductionPlanPlanner _planner;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _planner = new BruteForceLessScenariosProductionPlanPlanner(new Mock<ILogger<BruteForceLessScenariosProductionPlanPlanner>>().Object);
        }

        [Test]
        public void GenerateAllPossibilities()
        {
            List<PowerPlant> powerPlants = new List<PowerPlant>()
            {
                new PowerPlant() { PMin = 150 },
                new PowerPlant() { PMin = 150 },
                new PowerPlant() { PMin = 150 },
                new PowerPlant() { PMin = 150 },
                new PowerPlant() { PMin = 0 },
            };
            List<ProductionPlanScenario> result = _planner.GenerateAllPossibilities(powerPlants);

            Assert.AreEqual(Math.Pow(2, powerPlants.Count - 1), result.Count);
            int amountON = 0;
            result.ForEach(x => amountON += x.PowerPlants.Where(x => x.IsTurnedOn).Count());
            // there should be (scenarios * (powerplant with pmin != 0) /2) + result * (powerplant with PMin = 0) powerPlant ON through all the scenarios
            Assert.AreEqual(result.Count * ((powerPlants.Count - 1) / 2.0d) + result.Count, amountON);
        }
    }
}
