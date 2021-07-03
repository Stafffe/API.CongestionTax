using API.CongestionTax.Business.DataObjects;
using API.CongestionTax.Business.Extensions;
using API.CongestionTax.Business.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace API.CongestionTax.Business.Business
{
  public class CongestionTaxCalculator : ICongestionTaxCalculator
  {
    private const int SingleChargeIntervalInMilliseconds = 60000; //1min
    private readonly VehicleType[] tollFreVehiclesTypes = new VehicleType[] {
      VehicleType.Emergency,
      VehicleType.Bus,
      VehicleType.Diplomat,
      VehicleType.Motorcycle,
      VehicleType.Military,
      VehicleType.Foreign
    };
    private readonly DateTime[] holidays = new DateTime[] {
        new DateTime(2013, 1, 1),
        new DateTime(2013, 1, 6),
        new DateTime(2013, 3, 29),
        new DateTime(2013, 3, 31),
        new DateTime(2013, 4, 1),
        new DateTime(2013, 5, 1),
        new DateTime(2013, 5, 9),
        new DateTime(2013, 5, 19),
        new DateTime(2013, 6, 6),
        new DateTime(2013, 6, 22),
        new DateTime(2013, 11, 2),
        new DateTime(2013, 12, 25),
        new DateTime(2013, 12, 26)
      };

    /// <summary>
    /// Calculate the total toll fee for one day
    /// </summary>
    /// <param name="vehicle">The vehicle</param>
    /// <param name="dates">Date and time of all passes on one day</param>
    /// <returns>The total congestion tax for that day</returns>
    public int GetTax(Vehicle vehicle, DateTime[] dates)
    {
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
        if (firstDate.Millisecond - date.Millisecond > SingleChargeIntervalInMilliseconds)
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

      if (time.IsBetween(new TimeSpan(6, 00, 00), new TimeSpan(6, 29, 00)))
        return 8;
      else if (time.IsBetween(new TimeSpan(6, 30, 00), new TimeSpan(6, 59, 00)))
        return 13;
      else if (time.IsBetween(new TimeSpan(7, 00, 00), new TimeSpan(7, 59, 00)))
        return 18;
      else if (time.IsBetween(new TimeSpan(8, 00, 00), new TimeSpan(8, 29, 00)))
        return 13;
      else if (time.IsBetween(new TimeSpan(8, 30, 00), new TimeSpan(14, 59, 00)))
        return 8;
      else if (time.IsBetween(new TimeSpan(15, 00, 00), new TimeSpan(15, 29, 00)))
        return 13;
      else if (time.IsBetween(new TimeSpan(15, 30, 00), new TimeSpan(16, 59, 00)))
        return 18;
      else if (time.IsBetween(new TimeSpan(17, 00, 00), new TimeSpan(17, 59, 00)))
        return 13;
      else if (time.IsBetween(new TimeSpan(18, 00, 00), new TimeSpan(18, 29, 00)))
        return 8;
      else
        return 0;
    }

    private bool IsTollFreeVehicle(Vehicle vehicle)
    {
      return tollFreVehiclesTypes.Contains(vehicle.VehicleType);
    }

    private bool IsTollFreeDate(DateTime date)
    {
      if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
        return true;

      int year = date.Year;
      int month = date.Month;
      int day = date.Day;

      return holidays.Any(h => h.Year == year && h.Month == month && (h.Day == day || h.Day == day + 1));
    }
  }
}