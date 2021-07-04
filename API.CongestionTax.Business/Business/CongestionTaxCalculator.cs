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
    private readonly ITaxationInfoProvider _taxationInfoProvider;
    private bool _initialized = false;
    static readonly object _lockObject = new();
    private int _singleChargeIntervalInMinutes;
    private VehicleType[] _tollFreVehiclesTypes;
    private DateTime[] _holidays;
    private TaxationTimeInfo[] _taxationTimeInfos;

    public CongestionTaxCalculator(ITaxationInfoProvider taxationInfoProvider)
    {
      _taxationInfoProvider = taxationInfoProvider;
    }

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

      if (dates.Length == 0)
        return 0;

      dates = dates.OrderBy(d => d).ToArray();

      var totalFee = GetTaxRecursive(dates, vehicle);
      return totalFee > 60 ? 60 : totalFee;
    }

    private void InitializeTaxationParams()
    {
      lock (_lockObject)
      {
        if (_initialized)
          return;

        _singleChargeIntervalInMinutes = _taxationInfoProvider.GetTaxationIntervalLength(Cities.Gothenburg);
        _tollFreVehiclesTypes = _taxationInfoProvider.GetTollFreeVehicleTypes(Cities.Gothenburg);
        _holidays = _taxationInfoProvider.GettHolidays();
        _taxationTimeInfos = _taxationInfoProvider.GetTaxationTimeInfos(Cities.Gothenburg);

        _initialized = true;
      }
    }

    private int GetTaxRecursive(DateTime[] dates, Vehicle vehicle)
    {
      int totalFee = 0;

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
        totalFee += GetTaxRecursive(nextDatesToCheck, vehicle);
      }

      return totalFee;
    }

    private DateTime[] GetDatesWithinInterval(DateTime[] dates)
    {
      var maxDate = dates[0].AddMinutes(_singleChargeIntervalInMinutes);
      var dateList = new List<DateTime>();

      foreach (var date in dates)
      {
        if (date < maxDate)
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

      return _holidays.Any(h => h.Year == year && h.Month == month && (h.Day == day || h.Day - 1 == day));   //Free on holidays + the day before
    }
  }
}