using PowerplantCodingChallenge.API.Controllers.Dtos;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.API.Services.Notifiers
{
    public class ProductionPlanCalculatedEventArgs
    {
        public ProductionPlanCalculatedEventArgs(PowerPlanDto request, PowerPlantUsageDto[] response)
        {
            Request = request;
            Response = response;
        }

        public PowerPlanDto Request { get; private set; }
        public PowerPlantUsageDto[] Response { get; private set; }
    }

    public delegate void ProductionPlanCalculatedEventHandler(ProductionPlanCalculatedEventArgs e);

    public interface IProductionPlanCalculatedNotifier
    {
        public event ProductionPlanCalculatedEventHandler ProductionPlanCalculated;

        public Task Notify(PowerPlanDto request, PowerPlantUsageDto[] response);
    }
}
