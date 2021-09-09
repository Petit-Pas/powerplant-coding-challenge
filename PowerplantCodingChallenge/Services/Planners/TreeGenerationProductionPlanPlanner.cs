using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PowerplantCodingChallenge.API.Models;
using PowerplantCodingChallenge.API.Services.Planners;
using PowerPlantCodingChallenge.API.Controllers.Dtos;
using PowerPlantCodingChallenge.API.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PowerPlantCodingChallenge.API.Services.Planners
{
    public class TreeGenerationProductionPlanPlanner : IProductionPlanPlanner
    {
        private readonly ILogger<TreeGenerationProductionPlanPlanner> _logger;
        private readonly IConfiguration _configuration;

        public TreeGenerationProductionPlanPlanner(ILogger<TreeGenerationProductionPlanPlanner> logger, IConfiguration configuration)
        {
            this._logger = logger;
            this._configuration = configuration;
        }

        // will be used in recursive but will not change for each call, so we store them as fields in order to prevent the stack to grow too fast
        private List<PowerPlant> _powerPlants;
        private double _requiredLoad;
        private ProductionPlanScenario _currentBestScenario = null;

        public PowerPlantUsageDto[] ComputeBestPowerUsage(PowerPlanDto productionPlan)
        {
            if (productionPlan?.Fuels == null || productionPlan?.PowerPlants == null)
            {
                throw new ArgumentNullException("invalid production plan");
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            
            // prepare recursive
            bool co2Enabled = bool.Parse(_configuration.GetSection("PowerPlantCodingChallenge:CO2Enabled").Value);
            _powerPlants = productionPlan.PowerPlants.ConvertAll(x => new PowerPlant(x.Name, x.Type.ConvertToEnergySource(), x.Efficiency, x.PMin, x.PMax, productionPlan.Fuels, co2Enabled))
                                                    .OrderBy(x => x.CostPerMW).ToList();
            _requiredLoad = productionPlan.RequiredLoad;

            // look for the best scenario
            BuildPossibilityTree(_powerPlants.Select(x => x.IsTurnedOn).ToArray(), 0, 0, _powerPlants.Select(x => x.PMax).Sum(), 0);

            if (_currentBestScenario == null)
                throw new InvalidLoadException("Found no scenario to provide the asked load");

            // creates response
            var response = _currentBestScenario.PowerPlants.ConvertAll(x => new PowerPlantUsageDto(x.Name, Math.Round(x.PDelivered, 1))).ToArray();

            stopwatch.Stop();
            _logger.LogInformation($"Total process took {stopwatch.ElapsedMilliseconds}ms.");

            return response;
        }

        /// <summary>
        ///     will generate all possible scenarios in a recursive tree-like way, ignoring branches that would lead to impossible scenarios
        /// </summary>
        /// <param name="turnedOn"> an array describing which powerplant is Turned on, we use this array instead of PowerPlant.IsTurnedOn because it will be duplicated a lot through the recursive </param>
        /// <param name="currentIndex"> the index of the turnedOn array that the recursive is currently at </param>
        /// <param name="currentPMin"> The current minimum possible load for the branch 
        ///                             will increase each time a 0 is set to 1 in turnedOn </param>
        /// <param name="absolutePMax"> The absolute maximum possible load for the given branch, powerplants that haven't been processed in the recursive are considered on for that calculation
        ///                             will decrease for the branch everytime we go to the next recursive level while keeping a 0 in turnedOn </param>
        /// <param name="currentPMax">  The current possible load for the given branch, only the powerplants that haven been processed (we went trough them with recursive) are considered on for that calculation 
        ///                             It will increase for the branch everytime we set a plant to ON </param>
        public void BuildPossibilityTree(bool[] turnedOn, int currentIndex, double currentPMin, double absolutePMax, double currentPMax)
        {
            // all branch starting from here will always be above the required load
            if (currentPMin > _requiredLoad)
                return;
            // all branch starting from here will always be below the required load
            if (absolutePMax < _requiredLoad)
                return;

            // the current branch can already provide the required load so we can evaluate the scenario
            // since power plants are ordered by cost efficiency, turning any later power plant ON would be more costly, so we stop the recursive
            if (currentPMin <= _requiredLoad && _requiredLoad <= currentPMax)
            {
                CheckPossibleScenario(turnedOn);
                return;
            }

            // We reached the end of the branch, the scenario is evaluated and we do not proceed further in the tree
            if (currentIndex == turnedOn.Length)
            {
                CheckPossibleScenario(turnedOn);
                return;
            }

            // if the PowerPlant is already considered "On", it means it had a PMin of 0, hence not generating 2 branch.
            if (turnedOn[currentIndex])
            {
                BuildPossibilityTree(turnedOn, currentIndex + 1, currentPMin, absolutePMax, currentPMax + _powerPlants[currentIndex].PMax);
                return;
            }

            // 1 branch where we turn the current powerPlant On
            bool[] nextTurnedOn = (bool[])(turnedOn.Clone());
            nextTurnedOn[currentIndex] = true;
            BuildPossibilityTree(nextTurnedOn, currentIndex + 1, currentPMin + _powerPlants[currentIndex].PMin, absolutePMax, currentPMax + _powerPlants[currentIndex].PMax);

            // 1 branch where we keep the current powerPlant Off
            nextTurnedOn = (bool[])(turnedOn.Clone());
            BuildPossibilityTree(nextTurnedOn, currentIndex + 1, currentPMin, absolutePMax - _powerPlants[currentIndex].PMax, currentPMax);

        }

        private void CheckPossibleScenario(bool[] turnedOn)
        {
            ProductionPlanScenario scenario = new (_powerPlants.ConvertAll(x => new PowerPlant(x)));

            for (int i = 0; i != scenario.PowerPlants.Count; i += 1)
            {
                if (turnedOn[i])
                    scenario.PowerPlants[i].TurnOn();
            }

            scenario.FineTune(_requiredLoad);
            if (_currentBestScenario == null || _currentBestScenario.TotalCost > scenario.TotalCost)
                _currentBestScenario = scenario;
        }
    }
}
