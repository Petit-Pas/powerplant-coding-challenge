using PowerplantCodingChallenge.API.Controllers.Dtos;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.API.Services.Notifiers
{
    public class ProductionPlanCalculatedNotifier : IProductionPlanCalculatedNotifier
    {
        public event ProductionPlanCalculatedEventHandler ProductionPlanCalculated;

        public async Task Notify(PowerPlanDto request, PowerPlantUsageDto[] response)
        {
            await Task.Run(() => ProductionPlanCalculated?.Invoke(new ProductionPlanCalculatedEventArgs(request, response)));
        }
    }
}
