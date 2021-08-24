using PowerplantCodingChallenge.Energy.Types;

namespace PowerplantCodingChallenge.Energy.Tools
{
    public interface IProductionPlanPlanner
    {
        public PowerPlantUsageResponse[] ComputerBestPowerUsage(ProductionPlanInput productionPlan);
    }
}