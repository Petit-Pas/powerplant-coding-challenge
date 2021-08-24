using Microsoft.Extensions.Logging;
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

            RemoveUnusableScenarios(scenarios, productionPlan.Load);

            DumpPossibilities(scenarios, productionPlan);

            List<PowerPlantUsageResponse> Response = new List<PowerPlantUsageResponse>();
            foreach (PowerPlant powerPlant in productionPlan.PowerPlants)
            {
                Response.Add(new PowerPlantUsageResponse()
                {
                    Name = powerPlant.Name,
                    Power = 0.1f,
                });
            }

            stopwatch.Stop();
            logger.LogInformation($"Total process took {stopwatch.ElapsedMilliseconds}ms.");
            return Response.ToArray();
        }

        private void RemoveUnusableScenarios(List<ProductionPlanScenario> possibilities, int load)
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

        private void DumpPossibilities(List<ProductionPlanScenario> possibilities, ProductionPlanInput productionPlan)
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
                logger.LogDebug(states + $" could give between {scenario.PMin} and {scenario.PMax}");
            }
        }
    }
}
