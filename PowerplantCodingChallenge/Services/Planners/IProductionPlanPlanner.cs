using PowerplantCodingChallenge.Models;

namespace PowerplantCodingChallenge.Services.Planners
{
    public interface IProductionPlanPlanner
    {
        public PowerPlantUsageResponse[] ComputeBestPowerUsage(ProductionPlanInput productionPlan);
    }
}