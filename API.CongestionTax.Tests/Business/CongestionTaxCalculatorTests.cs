using API.CongestionTax.Business.Business;
using API.CongestionTax.Business.DataObjects;
using API.CongestionTax.Data.DataObjects;
using API.CongestionTax.Data.Enums;
using API.CongestionTax.Data.Interfaces;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace API.CongestionTax.Tests.Business
{
  public class CongestionTaxCalculatorTests
  {
    private readonly CongestionTaxCalculator _sut;

    private readonly Mock<ITaxationInfoProvider> _taxationInfoProviderMock;

    private readonly Vehicle _car = new Vehicle { VehicleType = VehicleType.Car };
    private readonly Vehicle _emergency = new Vehicle { VehicleType = VehicleType.Emergency };

    public CongestionTaxCalculatorTests()
    {
      _taxationInfoProviderMock = new Mock<ITaxationInfoProvider>();
      _sut = new CongestionTaxCalculator(_taxationInfoProviderMock.Object);
    }

    [Fact]
    public void GetTax_WithNoDates_ShouldReturn0Tax()
    {
      SetupFullTaxAllDAys();

      var tax = _sut.GetTax(_car, new DateTime[0]);

      Assert.Equal(0, tax);
    }

    [Fact]
    public void GetTax_WithMultipleParalellCalls_ShouldOnlyInitializeOnce()
    {
      SetupFullTaxAllDAys();
      _taxationInfoProviderMock.Setup(mock => mock.GetTaxationTimeInfos(Cities.Gothenburg))
      .Callback(() => Thread.Sleep(1000))
      .Returns(new TaxationTimeInfo[0]);

      var getTax1 = Task.Run(() => _sut.GetTax(_car, new DateTime[0]));
      var getTax2 = Task.Run(() => _sut.GetTax(_car, new DateTime[0]));
      Task.WaitAll(new[] { getTax1, getTax2 });

      _taxationInfoProviderMock.Verify(mock => mock.GetTaxationIntervalLength(It.IsAny<Cities>()), Times.Once);
      _taxationInfoProviderMock.Verify(mock => mock.GetTaxationTimeInfos(It.IsAny<Cities>()), Times.Once);
      _taxationInfoProviderMock.Verify(mock => mock.GettHolidays(), Times.Once);
      _taxationInfoProviderMock.Verify(mock => mock.GetTollFreeVehicleTypes(It.IsAny<Cities>()), Times.Once);
    }

    [Fact]
    public void GetTax_OnSaturday_ShouldNotTax()
    {
      SetupFullTaxAllDAys();
      var saturday = new DateTime[] { new DateTime(2021, 07, 03) };

      var tax = _sut.GetTax(_car, saturday);

      Assert.Equal(0, tax);
    }

    [Fact]
    public void GetTax_OnSunday_ShouldNotTax()
    {
      SetupFullTaxAllDAys();
      var saturday = new DateTime[] { new DateTime(2021, 07, 04) };

      var tax = _sut.GetTax(_car, saturday);

      Assert.Equal(0, tax);
    }

    [Fact]
    public void GetTax_OnMonday_ShouldTax()
    {
      SetupFullTaxAllDAys();
      var monday = new DateTime[] { new DateTime(2021, 07, 05) };

      var tax = _sut.GetTax(_car, monday);

      Assert.Equal(10, tax);
    }

    [Fact]
    public void GetTax_MultipleTimesWithinSameInterval_ShouldTaxOnlyOnce()
    {
      SetupFullTaxAllDAys();

      var dates = new DateTime[] {
        new DateTime(2021, 07, 05, 1, 0, 0),
        new DateTime(2021, 07, 05, 1, 20, 0),
        new DateTime(2021, 07, 05, 1, 40, 0),
      };
      var tax = _sut.GetTax(_car, dates);

      Assert.Equal(10, tax);
    }

    [Fact]
    public void GetTax_MultipleTimesWithinTwoIntervals_ShouldTaxTwice()
    {
      SetupFullTaxAllDAys();

      var dates = new DateTime[] {
        new DateTime(2021, 07, 05, 1, 0, 0),
        new DateTime(2021, 07, 05, 1, 20, 0),
        new DateTime(2021, 07, 05, 1, 40, 0),
        new DateTime(2021, 07, 05, 2, 00, 1),
      };
      var tax = _sut.GetTax(_car, dates);

      Assert.Equal(20, tax);
    }

    [Fact]
    public void GetTax_MultipleTimesWithinThreeIntervalsAndRandomOrder_ShouldTaxThrice()
    {
      SetupFullTaxAllDAys();

      var dates = new DateTime[] {
        new DateTime(2021, 07, 05, 1, 0, 0),
        new DateTime(2021, 07, 05, 1, 20, 0),
        new DateTime(2021, 07, 05, 1, 40, 0),
        new DateTime(2021, 07, 05, 2, 00, 1),
        new DateTime(2021, 07, 05, 2, 55, 0),
        new DateTime(2021, 07, 06, 2, 55, 0),
      };

      var random = new Random();
      dates.OrderBy(d => random.Next()); //Set random order

      var tax = _sut.GetTax(_car, dates);

      Assert.Equal(30, tax);
    }

    [Fact]
    public void GetTax_WithTotalTaxOver60_ShouldTax60()
    {
      SetupFullTaxAllDAys();

      var dates = new DateTime[] {
        new DateTime(2021, 7, 05, 1, 0, 0),
        new DateTime(2021, 7, 06, 1, 20, 0),
        new DateTime(2021, 7, 07, 1, 40, 0),
        new DateTime(2021, 7, 08, 2, 00, 1),
        new DateTime(2021, 7, 09, 2, 00, 1),
        new DateTime(2021, 7, 10, 2, 00, 1),
        new DateTime(2021, 7, 11, 2, 00, 1),
        new DateTime(2021, 7, 12, 2, 00, 1),
      };
      var tax = _sut.GetTax(_car, dates);

      Assert.Equal(60, tax);
    }

    [Fact]
    public void GetTax_WithTaxFreeVehicle_ShouldNotTax()
    {
      SetupFullTaxAllDAys();

      var dates = new DateTime[] {
        new DateTime(2021, 7, 05, 1, 0, 0),
      };
      var tax = _sut.GetTax(_emergency, dates);

      Assert.Equal(0, tax);
    }

    [Fact]
    public void GetTax_OnHoliday_ShouldNotTax()
    {
      SetupFullTaxAllDAys();
      var holiday = new DateTime(2021, 07, 07);
      _taxationInfoProviderMock.Setup(mock => mock.GettHolidays())
        .Returns(new DateTime[] { holiday });

      var dates = new DateTime[] {
        holiday.AddHours(4).AddMinutes(5).AddSeconds(15)
      };
      var tax = _sut.GetTax(_car, dates);

      Assert.Equal(0, tax);
    }

    [Fact]
    public void GetTax_OnDayBeforeHoliday_ShouldNotTax()
    {
      SetupFullTaxAllDAys();
      var holiday = new DateTime(2021, 07, 07);
      _taxationInfoProviderMock.Setup(mock => mock.GettHolidays())
        .Returns(new DateTime[] { holiday });

      var dates = new DateTime[] {
        holiday.AddHours(4).AddMinutes(5).AddSeconds(15).AddDays(-1)
      };
      var tax = _sut.GetTax(_car, dates);

      Assert.Equal(0, tax);
    }

    [Fact]
    public void GetTax_TwoDaysBeforeHoliday_ShouldTax()
    {
      SetupFullTaxAllDAys();
      var holiday = new DateTime(2021, 07, 07);
      _taxationInfoProviderMock.Setup(mock => mock.GettHolidays())
        .Returns(new DateTime[] { holiday });

      var dates = new DateTime[] {
        holiday.AddHours(4).AddMinutes(5).AddSeconds(15).AddDays(-2)
      };
      var tax = _sut.GetTax(_car, dates);

      Assert.Equal(10, tax);
    }

    [Fact]
    public void GetTax_WithMultipleAmountsWithinSameInterval_ShouldTaxTheHighest()
    {
      SetupFullTaxAllDAys();
      var taxationTimes = new TaxationTimeInfo[]{
        new TaxationTimeInfo { StartTime = new TimeSpan(1, 0, 0), EndTime = new TimeSpan(1, 10, 0), Taxation = 10 },
        new TaxationTimeInfo { StartTime = new TimeSpan(1, 11, 0), EndTime = new TimeSpan(1, 30, 0), Taxation = 30 }
      };
      _taxationInfoProviderMock.Setup(mock => mock.GetTaxationTimeInfos(Cities.Gothenburg))
        .Returns(taxationTimes);

      var dates = new DateTime[] {
        new DateTime(2021, 7, 5, 0, 50, 0), //0 tax
        new DateTime(2021, 7, 5, 1, 1, 0), //10 tax
        new DateTime(2021, 7, 5, 1, 25, 0), //30 tax
      };
      var tax = _sut.GetTax(_car, dates);

      Assert.Equal(30, tax);
    }

    private void SetupFullTaxAllDAys()
    {
      _taxationInfoProviderMock.Setup(mock => mock.GetTaxationIntervalLength(Cities.Gothenburg))
        .Returns(60);

      _taxationInfoProviderMock.Setup(mock => mock.GetTaxationTimeInfos(Cities.Gothenburg))
        .Returns(new TaxationTimeInfo[] { new TaxationTimeInfo { StartTime = new TimeSpan(0, 0, 0), EndTime = new TimeSpan(24, 59, 59), Taxation = 10 } });

      _taxationInfoProviderMock.Setup(mock => mock.GetTollFreeVehicleTypes(Cities.Gothenburg))
        .Returns(new VehicleType[] { VehicleType.Emergency });

      _taxationInfoProviderMock.Setup(mock => mock.GettHolidays())
        .Returns(new DateTime[0]);
    }
  }
}
