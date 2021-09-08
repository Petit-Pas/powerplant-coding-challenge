using PowerplantCodingChallenge.API.Controllers.Dtos;
using PowerplantCodingChallenge.Models;
using System;
using System.Collections.Generic;

namespace PowerplantCodingChallenge.Services.Planners
{
    public interface IProductionPlanPlanner
    {
        public PowerPlantUsageDto[] ComputeBestPowerUsage(PowerPlanDto productionPlan);
    }
}