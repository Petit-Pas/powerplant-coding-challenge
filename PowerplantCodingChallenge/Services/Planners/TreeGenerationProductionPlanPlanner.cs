using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    public class TreeGenerationProductionPlanPlanner : IProductionPlanPlanner
    {
        private readonly ILogger<TreeGenerationProductionPlanPlanner> logger;
        private readonly IConfiguration configuration;

        public TreeGenerationProductionPlanPlanner(ILogger<TreeGenerationProductionPlanPlanner> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        public PowerPlantUsageResponse[] ComputeBestPowerUsage(ProductionPlanInput productionPlan)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            // generate scenarios
            bool co2Enabled = bool.Parse(configuration.GetSection("PowerPlantCodingChallenge:CO2Enabled").Value);
            productionPlan.PowerPlants.ForEach(x => x.Init(productionPlan.Fuels, co2Enabled));
            productionPlan.PowerPlants = productionPlan.PowerPlants.OrderBy(x => x.CostPerMW).ToList();

            bool[] turnedOn = productionPlan.PowerPlants.Select(x => x.PMin == 0).ToArray();

            double PMax = productionPlan.PowerPlants.Select(x => x.PMax).Sum();
            buildPossibilityTree(productionPlan.PowerPlants, turnedOn, 0, 0, PMax, productionPlan.RequiredLoad);

            if (_currentBestScenario == null)
                throw new InvalidLoadException("Found no scenario to provide the asked load");

            var response = FormatResponse(productionPlan, _currentBestScenario);

            stopwatch.Stop();
            logger.LogInformation($"Total process took {stopwatch.ElapsedMilliseconds}ms.");

            return response;
        }

        /// <summary>
        /// </summary>
        /// <param name="powerPlants"> The powerplants that are linked to the turnedOn array </param>
        /// <param name="turnedOn"> an array describing which powerplant is Turned on, we use this array instead of PowerPlant.IsTurnedOn because it will be duplicated a lot through the recursive </param>
        /// <param name="currentIndex"> the index of the turnedOn array that the recursive is currently at </param>
        /// <param name="currentPMin"> The current minimum possible load for the branch 
        ///                             will increase each time a 0 is set to 1 in turnedOn </param>
        /// <param name="currentPMax"> The current maximum possible load for the given branch, powerplants that haven't been processed in the recursive are considered on for that calculation
        ///                             will decrease for the branch everytime we go to the next recursive level while keeping a 0 in turnedOn </param>
        /// <param name="requiredLoad"> The required load of the PowerPlan </param>
        public void buildPossibilityTree(List<PowerPlant> powerPlants, bool[] turnedOn, int currentIndex, double currentPMin, double currentPMax, double requiredLoad)
        {
            // all branch starting from here will always be over the required load
            if (currentPMin > requiredLoad)
                return;
            // all branch starting from here will always be below the required load
            if (currentPMax < requiredLoad)
                return;

            //end condition
            if (currentIndex == turnedOn.Length)
            {
                checkPossibleScenario(powerPlants, turnedOn, requiredLoad);
                return;
            }

            // if the PowerPlant is already considered "On", it means it had a PMin of 0, hence not generating 2 branch.
            if (turnedOn[currentIndex] == true)
            {
                buildPossibilityTree(powerPlants, turnedOn, currentIndex + 1, currentPMin, currentPMax, requiredLoad);
                return;
            }

            // 1 branch where we keep the current powerPlant Off
            bool[] nextTurnedOn = (bool[])(turnedOn.Clone());
            buildPossibilityTree(powerPlants, nextTurnedOn, currentIndex + 1, currentPMin, currentPMax - powerPlants[currentIndex].PMax, requiredLoad);

            // 1 branch where we turn the current powerPlant On
            nextTurnedOn = (bool[])(turnedOn.Clone());
            nextTurnedOn[currentIndex] = true;
            buildPossibilityTree(powerPlants, nextTurnedOn, currentIndex + 1, currentPMin + powerPlants[currentIndex].PMin, currentPMax, requiredLoad);
        }

        private ProductionPlanScenario _currentBestScenario = null;

        private void checkPossibleScenario(List<PowerPlant> powerPlants, bool[] turnedOn, double requiredLoad)
        {
            ProductionPlanScenario scenario = new ProductionPlanScenario();
            for (int i = 0; i != powerPlants.Count; i += 1)
            {
                MinimalistPowerPlant minimalist = powerPlants[i].GetMinimalist();
                minimalist.IsTurnedOn = turnedOn[i];
                if (turnedOn[i])
                    minimalist.PDelivered = minimalist.PMin;
                scenario.PowerPlants.Add(minimalist);
            }
            scenario.RefreshPs();
            scenario.FineTune(requiredLoad);
            scenario.ComputeTotalCost();
            if (_currentBestScenario == null || _currentBestScenario.TotalCost > scenario.TotalCost)
                _currentBestScenario = scenario;
        }

        public PowerPlantUsageResponse[] FormatResponse(ProductionPlanInput productionPlan, ProductionPlanScenario productionPlanScenario)
        {
            List<PowerPlantUsageResponse> result = new List<PowerPlantUsageResponse>();
            for (int i = 0; i != productionPlan.PowerPlants.Count; i += 1)
            {
                result.Add(new PowerPlantUsageResponse()
                {
                    Name = productionPlan.PowerPlants[i].Name,
                    Power = Math.Round(productionPlanScenario.PowerPlants[i].PDelivered, 1),
                });
            }
            return result.ToArray();
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
