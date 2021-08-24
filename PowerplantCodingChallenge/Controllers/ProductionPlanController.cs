using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult<PowerPlantUsage[]> CalculateProductionPlan([FromBody] ProductionPlan input) 
        {
            input.PowerPlants.ForEach(x => x.Init(input.Fuels));

            List<PowerPlantUsage> Response = new List<PowerPlantUsage>();


            foreach (PowerPlant powerPlant in input.PowerPlants)
            {
                Response.Add(new PowerPlantUsage()
                {
                    Name = powerPlant.Name,
                    Power = 0.1f,
                });
            }
            return Ok(Response.ToArray());
        }
    }
}
