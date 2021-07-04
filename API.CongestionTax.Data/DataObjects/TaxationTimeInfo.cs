using System;

namespace API.CongestionTax.Data.DataObjects
{
  public class TaxationTimeInfo
  {
    public int Taxation { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
  }
}
