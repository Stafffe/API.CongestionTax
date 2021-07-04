using API.CongestionTax.Business.DataObjects;
using API.CongestionTax.Business.Extensions;
using API.CongestionTax.Business.Interfaces;
using API.CongestionTax.Data.DataObjects;
using API.CongestionTax.Data.Enums;
using API.CongestionTax.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace API.CongestionTax.Business.Business
{
  public class CongestionTaxCalculator : ICongestionTaxCalculator
  {

    public CongestionTaxCalculator(ITaxationInfoProvider taxationInfoProvider)
    {
      _taxationInfoProvider = taxationInfoProvider;
    }

    private readonly ITaxationInfoProvider _taxationInfoProvider;

    private bool _initialized = false;
    static readonly object _lockObject = new();

    private int _singleChargeIntervalInMilliseconds;
    private VehicleType[] _tollFreVehiclesTypes;
    private DateTime[] _holidays;
    private TaxationTimeInfo[] _taxationTimeInfos;

    /// <summary>
    /// Calculate the total toll fee for one day
    /// </summary>
    /// <param name="vehicle">The vehicle</param>
    /// <param name="dates">Date and time of all passes on one day</param>
    /// <returns>The total congestion tax for that day</returns>
    public int GetTax(Vehicle vehicle, DateTime[] dates)
    {
      if (!_initialized)
        InitializeTaxationParams();

      dates = dates.OrderBy(d => d).ToArray();

      var totalFee = GetTaxRecursive(dates, 0, vehicle);

      return totalFee > 60 ? 60 : totalFee;
    }

    private int GetTaxRecursive(DateTime[] dates, int totalFee, Vehicle vehicle)
    {
      var datesWithin60Mins = GetDatesWithinInterval(dates);
      var nextDatesToCheck = dates.Skip(datesWithin60Mins.Length).ToArray();

      var highestFeeInInterval = 0;
      foreach (var date in datesWithin60Mins)
      {
        var fee = GetTollFee(date, vehicle);
        if (fee > highestFeeInInterval)
          highestFeeInInterval = fee;
      }

      totalFee += highestFeeInInterval;

      if (nextDatesToCheck.Length > 0)
      {
        totalFee += GetTaxRecursive(nextDatesToCheck, totalFee, vehicle);
      }

      return totalFee;
    }

    private DateTime[] GetDatesWithinInterval(DateTime[] dates)
    {
      var firstDate = dates[0];
      var dateList = new List<DateTime> { firstDate };

      foreach (var date in dates.Skip(1))
      {
        if (firstDate.Millisecond - date.Millisecond > _singleChargeIntervalInMilliseconds)
          dateList.Add(date);
        else
          break;
      }

      return dateList.ToArray();
    }

    private int GetTollFee(DateTime date, Vehicle vehicle)
    {
      if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle))
        return 0;

      var time = date.TimeOfDay;
      var taxationTimeInfo = _taxationTimeInfos.FirstOrDefault(tti => time.IsBetween(tti.StartTime, tti.EndTime));

      return taxationTimeInfo == null ? 0 : taxationTimeInfo.Taxation;
    }

    private bool IsTollFreeVehicle(Vehicle vehicle)
    {
      return _tollFreVehiclesTypes.Contains(vehicle.VehicleType);
    }

    private bool IsTollFreeDate(DateTime date)
    {
      if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
        return true;

      int year = date.Year;
      int month = date.Month;
      int day = date.Day;

      return _holidays.Any(h => h.Year == year && h.Month == month && (h.Day == day || h.Day == day + 1));
    }

    private void InitializeTaxationParams()
    {
      lock (_lockObject)
      {
        if (_initialized)
          return;

        _singleChargeIntervalInMilliseconds = _taxationInfoProvider.GetTaxationIntervalLength(Cities.Gothenburg) * 1000;
        _tollFreVehiclesTypes = _taxationInfoProvider.GetTollFreeVehicleTypes(Cities.Gothenburg);
        _holidays = _taxationInfoProvider.GettHolidays();
        _taxationTimeInfos = _taxationInfoProvider.GetTaxationTimeInfos(Cities.Gothenburg);

        _initialized = true;
      }
    }
  }
}