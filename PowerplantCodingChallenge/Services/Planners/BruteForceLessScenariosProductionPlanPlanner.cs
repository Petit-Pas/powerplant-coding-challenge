using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PowerplantCodingChallenge.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Services.Planners
{
    public class BruteForceLessScenariosProductionPlanPlanner : BruteForceProductionPlanPlanner
    {
        private readonly ILogger<BruteForceLessScenariosProductionPlanPlanner> logger;
        private readonly IConfiguration configuration;

        public BruteForceLessScenariosProductionPlanPlanner(ILogger<BruteForceLessScenariosProductionPlanPlanner> logger, IConfiguration configuration) 
            : base(logger, configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        // this overriden method will generate less scenarios since it will use the same ON/OFF mechanic than the original BruteForceProductionPlanPlanner,
        //      but it won't be generating scenarios in which powerPlants with a PMin of 0 are turned off, since it has no 'cost' to turn it on
        public override List<ProductionPlanScenario> GenerateAllPossibilities(List<PowerPlant> powerPlants)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            // we only take into account the powerPlants with a PMin != 0, others will be added as ON later
            List<PowerPlant> nonIgnored = powerPlants.Where(x => x.PMin != 0).ToList();
            List<ProductionPlanScenario> productionPlanScenarios = new List<ProductionPlanScenario>();

            // we generate the 2^nonIgnored.Count possible scenarios
            for (int i = 0; i != Math.Pow(2, nonIgnored.Count); i += 1)
            {
                ProductionPlanScenario productionPlanScenario = new ProductionPlanScenario()
                {
                    PowerPlants = new List<MinimalistPowerPlant>(),
                };
                // to which we add every non ignored powerPlant
                for (int j = 0; j != nonIgnored.Count; j += 1)
                {
                    MinimalistPowerPlant minimalistPowerPlant = nonIgnored[j].GetMinimalist();
                    // if the bit at index j of i is true, we turn the powerPlant on
                    // since i will range from 0 to 2^nonIgnored.Count, it will automatically generate all possibilities
                    if ((i & (1 << j)) != 0)
                    {
                        minimalistPowerPlant.IsTurnedOn = true;
                        minimalistPowerPlant.PDelivered = minimalistPowerPlant.PMin;
                    }
                    productionPlanScenario.PowerPlants.Add(minimalistPowerPlant);
                }

                // we repopulate the ignored plants, AKA those with a PMin of 0
                for (int k = 0; k != powerPlants.Count; k += 1)
                {
                    if (powerPlants[k].PMin == 0)
                    {
                        MinimalistPowerPlant minimalistPowerPlant = powerPlants[k].GetMinimalist();
                        minimalistPowerPlant.IsTurnedOn = true;
                        productionPlanScenario.PowerPlants.Insert(k, minimalistPowerPlant);
                    }
                }

                productionPlanScenario.RefreshPs();
                productionPlanScenarios.Add(productionPlanScenario);
            }

            stopwatch.Stop();
            logger.LogInformation($"Took {stopwatch.ElapsedMilliseconds}ms to generate the {productionPlanScenarios.Count} scenarios.");

            return productionPlanScenarios;

        }

    }
}
