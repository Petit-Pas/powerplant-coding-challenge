using PowerplantCodingChallenge.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.API.Services.Notifiers
{
    public class ProductionPlanCalculatedEventArgs
    {
        public ProductionPlanCalculatedEventArgs(ProductionPlanInput request, PowerPlantUsageResponse[] response)
        {
            Request = request;
            Response = response;
        }

        public ProductionPlanInput Request { get; set; }
        public PowerPlantUsageResponse[] Response { get; set; }
    }

    public delegate void ProductionPlanCalculatedEventHandler(ProductionPlanCalculatedEventArgs e);

    public interface IProductionPlanCalculatedNotifier
    {
        public event ProductionPlanCalculatedEventHandler ProductionPlanCalculated;

        public Task Notify(ProductionPlanInput request, PowerPlantUsageResponse[] response);
    }
}
