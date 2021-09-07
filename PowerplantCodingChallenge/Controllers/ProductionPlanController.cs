using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PowerplantCodingChallenge.API.Services.Notifiers;
using PowerplantCodingChallenge.Models;
using PowerplantCodingChallenge.Models.Exceptions;
using PowerplantCodingChallenge.Services.Planners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductionPlanController : Controller
    {
        private readonly IProductionPlanPlanner planner;
        private readonly IProductionPlanCalculatedNotifier notifier;
        private readonly ILogger<ProductionPlanController> logger;

        public ProductionPlanController(IProductionPlanPlanner planner, IProductionPlanCalculatedNotifier notifier, ILogger<ProductionPlanController> logger)
        {
            this.planner = planner;
            this.notifier = notifier;
            this.logger = logger;
        }

        [HttpPost]
        public ActionResult<PowerPlantUsageResponse[]> CalculateProductionPlan([FromBody] ProductionPlanInput input) 
        {
            if (input.RequiredLoad < 0)
            {
                logger.LogWarning("A negative load has been requested.");
                return BadRequest("Load cannot be negative");
            }

            string errorMessage;
            string errorType;

            try
            {
                PowerPlantUsageResponse[] response = planner.ComputeBestPowerUsage(input);
                notifier.Notify(input, response);
                return Ok(response);
            }
            catch (InvalidLoadException e)
            {
                errorMessage = e.Message;
                errorType = e.GetType().ToString();
            }
            catch (InvalidEnergyTypeException e)
            {
                errorMessage = e.Message;
                errorType = e.GetType().ToString();
            }
            catch (ArgumentNullException e)
            {
                errorMessage = e.Message;
                errorType = e.GetType().ToString();
            }
            logger.LogWarning($"An exception of type {errorType} has been thrown: {errorMessage}");
            return BadRequest(errorMessage);
        }
    }
}
