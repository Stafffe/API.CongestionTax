using API.CongestionTax.Business.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace API.CongestionTax.Business.Business
{

  public class CongestionTaxCalculator
  {
    /**
         * Calculate the total toll fee for one day
         *
         * @param vehicle - the vehicle
         * @param dates   - date and time of all passes on one day
         * @return - the total congestion tax for that day
         */

    private const int SingleChargeIntervalInMilliseconds = 60000; //1min
    private readonly VehicleType[] TollFreVehiclesTypes = new VehicleType[] {
      VehicleType.Motorcycle,
      VehicleType.Tractor,
      VehicleType.Emergency,
      VehicleType.Diplomat,
      VehicleType.Foreign,
      VehicleType.Military
    };

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

      int hour = date.Hour;
      int minute = date.Minute;

      if (hour == 6 && minute >= 0 && minute <= 29) return 8;
      else if (hour == 6 && minute >= 30 && minute <= 59) return 13;
      else if (hour == 7 && minute >= 0 && minute <= 59) return 18;
      else if (hour == 8 && minute >= 0 && minute <= 29) return 13;
      else if (hour >= 8 && hour <= 14 && minute >= 30 && minute <= 59) return 8;
      else if (hour == 15 && minute >= 0 && minute <= 29) return 13;
      else if (hour == 15 && minute >= 0 || hour == 16 && minute <= 59) return 18;
      else if (hour == 17 && minute >= 0 && minute <= 59) return 13;
      else if (hour == 18 && minute >= 0 && minute <= 29) return 8;
      else return 0;
    }

    private bool IsTollFreeVehicle(Vehicle vehicle)
    {
      return TollFreVehiclesTypes.Contains(vehicle.VehicleType);
    }

    private bool IsTollFreeDate(DateTime date)
    {
      int year = date.Year;
      int month = date.Month;
      int day = date.Day;

      if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

      if (year == 2013)
      {
        if (month == 1 && day == 1 ||
            month == 3 && (day == 28 || day == 29) ||
            month == 4 && (day == 1 || day == 30) ||
            month == 5 && (day == 1 || day == 8 || day == 9) ||
            month == 6 && (day == 5 || day == 6 || day == 21) ||
            month == 7 ||
            month == 11 && day == 1 ||
            month == 12 && (day == 24 || day == 25 || day == 26 || day == 31))
        {
          return true;
        }
      }
      return false;
    }
  }
}