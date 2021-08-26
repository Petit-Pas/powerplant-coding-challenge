using PowerplantCodingChallenge.Models;
using System;
using System.Collections.Generic;

namespace PowerplantCodingChallenge.Services.Planners
{
    public interface IProductionPlanPlanner
    {
        public PowerPlantUsageResponse[] ComputeBestPowerUsage(ProductionPlanInput productionPlan);
    }
}