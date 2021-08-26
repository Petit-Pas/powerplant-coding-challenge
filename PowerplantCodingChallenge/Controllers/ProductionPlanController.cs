using Microsoft.AspNetCore.Mvc;
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

        public ProductionPlanController(IProductionPlanPlanner planner, IProductionPlanCalculatedNotifier notifier)
        {
            this.planner = planner;
            this.notifier = notifier;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult<PowerPlantUsageResponse[]> CalculateProductionPlan([FromBody] ProductionPlanInput input) 
        {
            try
            {
                PowerPlantUsageResponse[] response = planner.ComputeBestPowerUsage(input);
                notifier.Notify(input, response);
                return Ok(response);
            }
            catch (InvalidLoadException e)
            {
                return BadRequest(e.Message);
            }
            catch (InvalidEnergyTypeException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
