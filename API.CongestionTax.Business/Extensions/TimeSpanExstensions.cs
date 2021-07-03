using System;

namespace API.CongestionTax.Business.Extensions
{
  public static class TimeSpanExstensions
  {
    public static bool IsBetween(this TimeSpan dateToCompare, TimeSpan start, TimeSpan end) {
      return dateToCompare >= start && dateToCompare <= end;
    }
  }
}
