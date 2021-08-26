using Microsoft.AspNetCore.Mvc;
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

        public ProductionPlanController(IProductionPlanPlanner planner)
        {
            this.planner = planner;
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
                return Ok(planner.ComputeBestPowerUsage(input));
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
