using Microsoft.AspNetCore.Mvc;
using PowerplantCodingChallenge.Energy.Exceptions;
using PowerplantCodingChallenge.Energy.Tools;
using PowerplantCodingChallenge.Energy.Types;
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
                return Ok(planner.ComputerBestPowerUsage(input));
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
