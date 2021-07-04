using API.CongestionTax.Data.DataObjects;
using API.CongestionTax.Data.Enums;
using API.CongestionTax.Data.Interfaces;
using System;

namespace API.CongestionTax.Data.Providers
{
  public class DatabaseProvider : ITaxationInfoProvider
  {
    public int GetTaxationIntervalLength(Cities gothenburg)
    {
      return 60;
    }

    public DataObjects.TaxationTimeInfo[] GetTaxationTimeInfos(Cities gothenburg)
    {
      return new TaxationTimeInfo[] {
        new TaxationTimeInfo {
          Taxation = 8,
          StartTime = new TimeSpan(6, 00, 00),
          EndTime = new TimeSpan(6, 29, 00)
        },
        new TaxationTimeInfo {
          Taxation = 13,
          StartTime = new TimeSpan(6, 30, 00),
          EndTime = new TimeSpan(6,59,00)
        },
        new TaxationTimeInfo {
          Taxation = 18,
          StartTime = new TimeSpan(7,00,00),
          EndTime =  new TimeSpan(7,59,00)
        },
        new TaxationTimeInfo {
          Taxation = 13,
          StartTime = new TimeSpan(8, 00, 00),
          EndTime = new TimeSpan(8, 29, 00)
        },
        new TaxationTimeInfo {
          Taxation = 8,
          StartTime = new TimeSpan(8,30,00),
          EndTime =  new TimeSpan(14,59,00)
        },
        new TaxationTimeInfo {
          Taxation = 13,
          StartTime = new TimeSpan(15,00,00),
          EndTime =  new TimeSpan(15,29,00)
        }  ,
        new TaxationTimeInfo {
          Taxation = 18,
          StartTime = new TimeSpan(15,30,00),
          EndTime =  new TimeSpan(16,59,00)
        },
        new TaxationTimeInfo {
          Taxation = 13,
          StartTime = new TimeSpan(17,00,00),
          EndTime =  new TimeSpan(17,59,00)
        },
        new TaxationTimeInfo {
          Taxation = 8,
          StartTime = new TimeSpan(18,00,00),
          EndTime =  new TimeSpan(18,29,00)
        }
      };
    }

    public DateTime[] GettHolidays()
    {
      return new DateTime[] {
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
    }

    public VehicleType[] GetTollFreeVehicleTypes(Cities gothenburg)
    {
      return new VehicleType[] {
        VehicleType.Emergency,
        VehicleType.Bus,
        VehicleType.Diplomat,
        VehicleType.Motorcycle,
        VehicleType.Military,
        VehicleType.Foreign
      };
    }
  }
}
