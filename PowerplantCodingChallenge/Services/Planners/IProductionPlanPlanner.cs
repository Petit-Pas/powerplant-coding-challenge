using PowerplantCodingChallenge.API.Controllers.Dtos;

namespace PowerplantCodingChallenge.Services.Planners
{
    public interface IProductionPlanPlanner
    {
        public PowerPlantUsageDto[] ComputeBestPowerUsage(PowerPlanDto productionPlan);
    }
}