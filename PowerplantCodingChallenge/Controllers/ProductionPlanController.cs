using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerplantCodingChallenge.API.Services.Planners;
using PowerPlantCodingChallenge.API.Controllers.Dtos;
using PowerPlantCodingChallenge.API.Models.Exceptions;
using PowerPlantCodingChallenge.API.Services.Notifiers;
using System;
using System.Linq;

namespace PowerPlantCodingChallenge.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductionPlanController : Controller
    {
        private readonly IProductionPlanPlanner _planner;
        private readonly IProductionPlanCalculatedNotifier _notifier;
        private readonly ILogger<ProductionPlanController> _logger;

        public ProductionPlanController(IProductionPlanPlanner planner, IProductionPlanCalculatedNotifier notifier, ILogger<ProductionPlanController> logger)
        {
            this._planner = planner;
            this._notifier = notifier;
            this._logger = logger;
        }

        [HttpPost]
        public ActionResult<PowerPlantUsageDto[]> CalculateProductionPlan([FromBody] PowerPlanDto input) 
        {
            PowerPlanDtoValidator validator = new();

            var result = validator.Validate(input);

            if (result.Errors.Count != 0)
            {
                var messages = result.Errors.Select(x => x.ErrorMessage);
                _logger.LogWarning($"a request generated {result.Errors.Count} errors: " + String.Join(", ", messages));
                return BadRequest(JsonConvert.SerializeObject(new { errors = messages }));
            }

            string errorMessage;
            string errorType;
            try
            {
                PowerPlantUsageDto[] response = _planner.ComputeBestPowerUsage(input);
                _notifier.Notify(input, response);
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
            _logger.LogWarning($"An exception of type {errorType} has been thrown: {errorMessage}");
            return BadRequest(errorMessage);
        }
    }
}
