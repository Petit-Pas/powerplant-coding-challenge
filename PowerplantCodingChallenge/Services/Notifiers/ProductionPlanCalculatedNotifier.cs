using PowerplantCodingChallenge.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.API.Services.Notifiers
{
    public class ProductionPlanCalculatedNotifier : IProductionPlanCalculatedNotifier
    {
        public event ProductionPlanCalculatedEventHandler ProductionPlanCalculated;

        public async Task Notify(ProductionPlanInput request, PowerPlantUsageResponse[] response)
        {
            await Task.Run(() => ProductionPlanCalculated?.Invoke(new ProductionPlanCalculatedEventArgs(request, response)));
        }
    }
}
