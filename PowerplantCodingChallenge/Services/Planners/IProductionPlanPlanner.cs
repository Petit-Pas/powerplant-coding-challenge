using PowerPlantCodingChallenge.API.Controllers.Dtos;

namespace PowerplantCodingChallenge.API.Services.Planners
{
    public interface IProductionPlanPlanner
    {
        public PowerPlantUsageDto[] ComputeBestPowerUsage(PowerPlanDto productionPlan);
    }
}