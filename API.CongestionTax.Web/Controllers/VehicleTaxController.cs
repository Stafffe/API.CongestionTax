using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.CongestionTax.Web.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class VehicleTaxController : ControllerBase
  {
    private readonly ILogger<VehicleTaxController> _logger;

    public VehicleTaxController(ILogger<VehicleTaxController> logger)
    {
      _logger = logger;
    }

    [HttpGet]
    public void Get()
    {

    }
  }
}
