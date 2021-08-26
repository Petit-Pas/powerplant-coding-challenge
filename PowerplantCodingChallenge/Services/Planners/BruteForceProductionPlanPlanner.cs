using Microsoft.Extensions.Logging;
using PowerplantCodingChallenge.Models;
using PowerplantCodingChallenge.Models.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Services.Planners
{
    /// <summary>
    ///     The BruteForcePlanner will generate every possibility in term of ON/OFF for power plant 
    ///         => having 6 powerplant will generate 2^6 scenarios
    ///     It will then eliminate the scenarios that could not work, and finetune the remaining to find the optimized cost of each
    ///     
    ///     The idea behind the ON/OFF delimitation is separating the "fixed" (PMin) and "maxed" (Pmax) production (PMin) for each scenario
    ///         You then have a Min and a Max for a complete scenario and not just a given PowerPlant.
    /// </summary>
    public class BruteForceProductionPlanPlanner : IProductionPlanPlanner
    {
        private readonly ILogger<BruteForceProductionPlanPlanner> logger;

        public BruteForceProductionPlanPlanner(ILogger<BruteForceProductionPlanPlanner> logger)
        {
            this.logger = logger;
        }

        public PowerPlantUsageResponse[] ComputeBestPowerUsage(ProductionPlanInput productionPlan)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            // generate scenarios
            productionPlan.PowerPlants.ForEach(x => x.Init(productionPlan.Fuels));
            productionPlan.PowerPlants = productionPlan.PowerPlants.OrderBy(x => x.CostPerMW).ToList();
            List<ProductionPlanScenario> scenarios = GenerateAllPossibilities(productionPlan.PowerPlants);

            // filter useful ones
            RemoveUnusableScenarios(scenarios, productionPlan.RequiredLoad);
            if (scenarios.Count == 0)
                throw new InvalidLoadException("Found no scenario to provide the asked load");

            // finetune them to actually correspond to the given payload
            Stopwatch fineTuneStopwatch = Stopwatch.StartNew();
            scenarios.ForEach(x => x.FineTune(productionPlan.RequiredLoad));
            fineTuneStopwatch.Stop();
            logger.LogInformation($"It took {fineTuneStopwatch.ElapsedMilliseconds}ms to finetune all models");

            // compute each cost
            scenarios.ForEach(x => x.ComputeTotalCost());

            DumpScenarios(scenarios, productionPlan, false);

            // Create response
            scenarios = scenarios.OrderBy(x => x.TotalCost).ToList();
            var response = FormatResponse(productionPlan, scenarios.First());

            stopwatch.Stop();
            logger.LogInformation($"Total process took {stopwatch.ElapsedMilliseconds}ms.");

            return response.ToArray();
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

        public void RemoveUnusableScenarios(List<ProductionPlanScenario> scenarios, double load)
        {
            int amount = scenarios.Count;
            scenarios.RemoveAll(x => x.PMax < load || x.PMin > load);
            logger.LogInformation($"After removing impossible possibilities, {scenarios.Count} of the {amount} initial remain");
        }

        public virtual List<ProductionPlanScenario> GenerateAllPossibilities(List<PowerPlant> powerPlants)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<ProductionPlanScenario> productionPlanScenarios = new List<ProductionPlanScenario>();

            // we generate the 2^powerPlants.Count possible scenarios
            for (int i = 0; i != Math.Pow(2, powerPlants.Count); i += 1)
            {
                ProductionPlanScenario productionPlanScenario = new ProductionPlanScenario()
                {
                    PowerPlants = new List<MinimalistPowerPlant>(),
                };
                // to which we add every powerPlant
                for (int j = 0; j != powerPlants.Count; j += 1)
                {
                    MinimalistPowerPlant minimalistPowerPlant = powerPlants[j].GetMinimalist();
                    // if the bit at index j of i is true, we turn the powerPlant on
                    // since i will range from 0 to 2^powerPlants.Count, it will automatically generate all possibilities
                    if ((i & (1 << j)) != 0)
                    {
                        minimalistPowerPlant.IsTurnedOn = true;
                        minimalistPowerPlant.PDelivered = minimalistPowerPlant.PMin;
                    }
                    productionPlanScenario.PowerPlants.Add(minimalistPowerPlant);
                }
                productionPlanScenario.RefreshPs();
                productionPlanScenarios.Add(productionPlanScenario);
            }

            stopwatch.Stop();
            logger.LogInformation($"Took {stopwatch.ElapsedMilliseconds}ms to generate the {productionPlanScenarios.Count} scenarios.");

            return productionPlanScenarios;
        }

        // for debug purposes
        protected void DumpScenarios(List<ProductionPlanScenario> possibilities, ProductionPlanInput productionPlan, bool details = false)
        {
            string names = String.Join(", ", productionPlan.PowerPlants.Select(x => x.Name));
            logger.LogDebug($"Order of plants: {names}");
            foreach (ProductionPlanScenario scenario in possibilities)
            {
                string states = "";
                foreach (MinimalistPowerPlant powerPlant in scenario.PowerPlants)
                {
                    states += powerPlant.IsTurnedOn ? "1" : "0";
                }
                logger.LogDebug(states + $" could give between {scenario.PMin} and {scenario.PMax} and currently gives {scenario.PDelivered} for a cost of {scenario.TotalCost}");
                if (details)
                {
                    foreach (MinimalistPowerPlant powerPlant in scenario.PowerPlants)
                    {
                        logger.LogDebug($"{powerPlant.PDelivered}");
                    }
                }
            }
        }
    }
}
