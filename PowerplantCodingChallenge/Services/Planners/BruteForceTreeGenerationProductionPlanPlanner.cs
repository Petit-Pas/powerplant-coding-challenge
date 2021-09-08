using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PowerplantCodingChallenge.API.Controllers.Dtos;
using PowerplantCodingChallenge.Models;
using PowerplantCodingChallenge.Models.Exceptions;
using PowerplantCodingChallenge.Services.Planners;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.API.Services.Planners
{
    // ITERATION 3

    public class BruteForceTreeGenerationProductionPlanPlanner : IProductionPlanPlanner
    {
        private readonly ILogger<BruteForceTreeGenerationProductionPlanPlanner> logger;
        private readonly IConfiguration configuration;

        public BruteForceTreeGenerationProductionPlanPlanner(ILogger<BruteForceTreeGenerationProductionPlanPlanner> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        // will be used in recursive but will not change for each call, so we store them as fields in order to prevent the stack to grow too fast
        private List<PowerPlant> powerPlants;
        private double requiredLoad;
        private ProductionPlanScenario currentBestScenario = null;

        public PowerPlantUsageDto[] ComputeBestPowerUsage(PowerPlanDto productionPlan)
        {
            if (productionPlan?.Fuels == null || productionPlan?.PowerPlants == null)
            {
                throw new ArgumentNullException("production plan could not be correctly parsed");
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            // generate scenarios
            bool co2Enabled = bool.Parse(configuration.GetSection("PowerPlantCodingChallenge:CO2Enabled").Value);
            powerPlants = productionPlan.PowerPlants.ConvertAll(x => new PowerPlant(x.Name, x.Type.ConvertToEnergySource(), x.Efficiency, x.PMin, x.PMax, productionPlan.Fuels, co2Enabled))
                                                    .OrderBy(x => x.CostPerMW).ToList();
            requiredLoad = productionPlan.RequiredLoad;

            bool[] turnedOn = powerPlants.Select(x => x.IsTurnedOn).ToArray();
            double PMax = powerPlants.Select(x => x.PMax).Sum();

            buildPossibilityTree(turnedOn, 0, 0, PMax);

            if (currentBestScenario == null)
                throw new InvalidLoadException("Found no scenario to provide the asked load");
            var response = currentBestScenario.PowerPlants.ConvertAll(x => new PowerPlantUsageDto(x.Name, x.PDelivered)).ToArray();

            stopwatch.Stop();
            logger.LogInformation($"Total process took {stopwatch.ElapsedMilliseconds}ms.");

            return response;
        }

        /// <summary>
        ///     will generate all possible scenarios in a recursive tree-like way, ignoring branches that would lead to impossible scenarios
        /// </summary>
        /// <param name="turnedOn"> an array describing which powerplant is Turned on, we use this array instead of PowerPlant.IsTurnedOn because it will be duplicated a lot through the recursive </param>
        /// <param name="currentIndex"> the index of the turnedOn array that the recursive is currently at </param>
        /// <param name="currentPMin"> The current minimum possible load for the branch 
        ///                             will increase each time a 0 is set to 1 in turnedOn </param>
        /// <param name="currentPMax"> The current maximum possible load for the given branch, powerplants that haven't been processed in the recursive are considered on for that calculation
        ///                             will decrease for the branch everytime we go to the next recursive level while keeping a 0 in turnedOn </param>
        public void buildPossibilityTree(bool[] turnedOn, int currentIndex, double currentPMin, double currentPMax)
        {
            // all branch starting from here will always be above the required load
            if (currentPMin > requiredLoad)
                return;
            // all branch starting from here will always be below the required load
            if (currentPMax < requiredLoad)
                return;

            // We reached the end of the branch, we can now evaluate the scenario
            if (currentIndex == turnedOn.Length)
            {
                CheckPossibleScenario(powerPlants, turnedOn, requiredLoad);
                return;
            }

            // if the PowerPlant is already considered "On", it means it had a PMin of 0, hence not generating 2 branch.
            if (turnedOn[currentIndex] == true)
            {
                buildPossibilityTree(turnedOn, currentIndex + 1, currentPMin, currentPMax);
                return;
            }

            // 1 branch where we turn the current powerPlant On
            bool[] nextTurnedOn = (bool[])(turnedOn.Clone());
            nextTurnedOn[currentIndex] = true;
            buildPossibilityTree(nextTurnedOn, currentIndex + 1, currentPMin + powerPlants[currentIndex].PMin, currentPMax);

            // 1 branch where we keep the current powerPlant Off
            nextTurnedOn = (bool[])(turnedOn.Clone());
            buildPossibilityTree(nextTurnedOn, currentIndex + 1, currentPMin, currentPMax - powerPlants[currentIndex].PMax);

        }

        private void CheckPossibleScenario(List<PowerPlant> powerPlants, bool[] turnedOn, double requiredLoad)
        {
            ProductionPlanScenario scenario = new (powerPlants.ConvertAll(x => new PowerPlant(x)));

            for (int i = 0; i != scenario.PowerPlants.Count; i += 1)
            {
                if (turnedOn[i])
                    scenario.PowerPlants[i].TurnOn();
            }

            scenario.ComputePs();
            scenario.FineTune(requiredLoad);
            scenario.ComputeTotalCost();
            if (currentBestScenario == null || currentBestScenario.TotalCost > scenario.TotalCost)
                currentBestScenario = scenario;
        }

        // debug purpose
        private void display_array(bool[] array, double currentPmax, double currentPmin, double totalCost)
        {
            string output = "[";
            foreach (bool boolean in array)
            {
                if (boolean)
                    output += "1";
                else
                    output += "0";
            }
            output += "]";

            output += $" will output between {currentPmin} and {currentPmax} for a price of {totalCost}";
            logger.LogDebug(output);
        }
    }
}
