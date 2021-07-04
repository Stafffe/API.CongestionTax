using API.CongestionTax.Data.DataObjects;
using API.CongestionTax.Data.Enums;
using System;

namespace API.CongestionTax.Data.Interfaces
{
  public interface ITaxationInfoProvider
  {
    int GetTaxationIntervalLength(Cities gothenburg);
    VehicleType[] GetTollFreeVehicleTypes(Cities gothenburg);
    DateTime[] GettHolidays();
    TaxationTimeInfo[] GetTaxationTimeInfos(Cities gothenburg);
  }
}