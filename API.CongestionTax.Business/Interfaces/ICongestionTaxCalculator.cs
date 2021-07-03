using API.CongestionTax.Business.DataObjects;
using System;

namespace API.CongestionTax.Business.Interfaces
{
  public interface ICongestionTaxCalculator
  {
    int GetTax(Vehicle vehicle, DateTime[] dates);
  }
}