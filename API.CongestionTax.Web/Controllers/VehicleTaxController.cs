using API.CongestionTax.Business.DataObjects;
using API.CongestionTax.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace API.CongestionTax.Web.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class VehicleTaxController : ControllerBase
  {
    private readonly ILogger<VehicleTaxController> _logger;
    private readonly ICongestionTaxCalculator _congestionTaxCalculator;

    public VehicleTaxController(ILogger<VehicleTaxController> logger, ICongestionTaxCalculator congestionTaxCalculator)
    {
      _logger = logger;
      _congestionTaxCalculator = congestionTaxCalculator;
    }

    [HttpGet]
    public int Get(VehicleType vehicleType, DateTime[] datesForTaxations)
    {
      try
      {
        return _congestionTaxCalculator.GetTax(new Vehicle { VehicleType = vehicleType }, datesForTaxations);
      }
      catch   (Exception ex)
      {
        _logger.LogError("Something went wrong trying to calucalte congestion tax.", ex);
        throw;
      }
    }
  }
}
