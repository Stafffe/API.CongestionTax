using API.CongestionTax.Business.DataObjects;
using API.CongestionTax.Business.Interfaces;
using API.CongestionTax.Data.Enums;
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

    /// <summary>
    /// Gets the sum of taxation for a number of dates
    /// </summary>
    /// <param name="vehicleType">The type of vehicle taxed. Motorcycle = 1, Tractor = 2, Emergency = 3, Diplomat = 4, Foreign = 5, Military = 6, Bus = 7, Car = 8, Other = 9</param>
    /// <param name="datesForTaxations">The occurences of passages for the vehicle. Format YYYY-MM-DD HH:mm:ss</param>
    /// <returns></returns>
    [HttpGet]
    public int Get(VehicleType vehicleType, [FromQuery] DateTime[] datesForTaxations)
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
