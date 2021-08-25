using Microsoft.Extensions.Logging;
using PowerplantCodingChallenge.Energy.Exceptions;
using PowerplantCodingChallenge.Energy.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Energy.Tools
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

        public PowerPlantUsageResponse[] ComputerBestPowerUsage(ProductionPlanInput productionPlan)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            productionPlan.PowerPlants.ForEach(x => x.Init(productionPlan.Fuels));
            productionPlan.PowerPlants = productionPlan.PowerPlants.OrderBy(x => x.CostPerMW).ToList();
            List<ProductionPlanScenario> scenarios = GenerateAllPossibilities(productionPlan.PowerPlants);

            //DumpPossibilities(scenarios, productionPlan);

            RemoveUnusableScenarios(scenarios, productionPlan.Load);
            if (scenarios.Count == 0)
                throw new InvalidLoadException("Found no scenario to provide the asked load");

            //DumpPossibilities(scenarios, productionPlan);

            Stopwatch fineTuneStopwatch = Stopwatch.StartNew();
            scenarios.ForEach(x => x.FineTune(productionPlan.Load));
            fineTuneStopwatch.Stop();
            logger.LogInformation($"It took {fineTuneStopwatch.ElapsedMilliseconds}ms to finetune all models");

            //DumpPossibilities(scenarios, productionPlan);

            scenarios.ForEach(x => x.ComputeTotalCost());

            //DumpPossibilities(scenarios, productionPlan, true);

            scenarios = scenarios.OrderBy(x => x.TotalCost).ToList();

            List<PowerPlantUsageResponse> Response = new List<PowerPlantUsageResponse>();
            for (int i = 0; i != productionPlan.PowerPlants.Count; i += 1)
            {
                Response.Add(new PowerPlantUsageResponse()
                {
                    Name = productionPlan.PowerPlants[i].Name,
                    Power = Math.Round(scenarios[0].PowerPlants[i].PDelivered, 1),
                });
            }

            stopwatch.Stop();
            logger.LogInformation($"Total process took {stopwatch.ElapsedMilliseconds}ms.");
            return Response.ToArray();
        }

        private void RemoveUnusableScenarios(List<ProductionPlanScenario> possibilities, double load)
        {
            int amount = possibilities.Count;
            possibilities.RemoveAll(x => x.PMax < load || x.PMin > load);
            logger.LogInformation($"After removing impossible possibilities, {possibilities.Count} of the {amount} initial remain");
        }

        private List<ProductionPlanScenario> GenerateAllPossibilities(List<PowerPlant> powerPlants)
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
        private void DumpPossibilities(List<ProductionPlanScenario> possibilities, ProductionPlanInput productionPlan, bool details = false)
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
