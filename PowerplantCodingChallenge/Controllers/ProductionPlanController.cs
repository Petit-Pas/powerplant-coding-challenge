using Microsoft.AspNetCore.Mvc;
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
            return Ok(planner.ComputerBestPowerUsage(input));
        }
    }
}
