using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeSheet;
using FluentAssertions.Extensions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void NewVMShouldBeCorrectlyInitialized()
        {
            var vm = new ViewModelData();

            vm.TodayTime.Should().Be(TimeSpan.Zero);
            vm.ThisWeekTime.Should().Be(TimeSpan.Zero);
            vm.IsStarted.Should().Be(false);
        }

        [TestMethod]
        public async Task OneStartStopShouldSetCorrectValues()
        {
            var vm = new ViewModelData();

            var duration = 10.Seconds();

            vm.Start(duration);

            vm.TodayTime.Should().Be(10.Seconds(), "Day time incorrect");
            vm.ThisWeekTime.Should().Be(10.Seconds(), "Week time incorrect");
            vm.IsStarted.Should().Be(false);

            vm = new ViewModelData();
            vm.Start();
            vm.IsStarted.Should().Be(true);
            await Task.Delay(3.Seconds());
            vm.Stop();
            vm.IsStarted.Should().Be(false);

            vm.TodayTime.Should().BeCloseTo(3.Seconds(), precision:500);
            vm.ThisWeekTime.Should().BeCloseTo(3.Seconds(), precision: 500);
        }

        [TestMethod]
        public async Task MultipleStartStopShouldSetCorrectValues()
        {
            var vm = new ViewModelData();

            var duration = 10.Seconds(); // Add 10 => 10
            vm.Start(duration);
            vm.TodayTime.Should().Be(10.Seconds(), "Day time incorrect");
            vm.ThisWeekTime.Should().Be(10.Seconds(), "Week time incorrect");

            duration = 20.Seconds(); // Add 20 => 30
            vm.Start(duration);
            vm.TodayTime.Should().Be(30.Seconds(), "Day time incorrect");
            vm.ThisWeekTime.Should().Be(30.Seconds(), "Week time incorrect");

            vm = new ViewModelData();
            vm.Start();
            await Task.Delay(3.Seconds());
            vm.Stop();
            vm.TodayTime.Should().BeCloseTo(3.Seconds(), precision: 500);
            vm.ThisWeekTime.Should().BeCloseTo(3.Seconds(), precision: 500);

            vm.Start();
            await Task.Delay(5.Seconds());
            vm.Stop();
            vm.TodayTime.Should().BeCloseTo(8.Seconds(), precision: 500);
            vm.ThisWeekTime.Should().BeCloseTo(8.Seconds(), precision: 500);
        }

        [TestMethod]
        public async Task OneStartStopShouldSetCorrectlyThisDayStats()
        {
            var vm = new ViewModelData();

            var duration = 10.Seconds();
            vm.Start(duration);

            var dow = DateTime.UtcNow.DayOfWeek;
            vm[dow].Should().Be(10.Seconds());

            vm = new ViewModelData();
            vm.Start();
            await Task.Delay(3.Seconds());
            vm.Stop();

            vm[dow].Should().BeCloseTo(3.Seconds(), precision: 500);
        }

        [TestMethod]
        public async Task MultipleStartStopShouldSetCorrectlyThisDayStats()
        {
            var vm = new ViewModelData();

            var duration = 10.Seconds(); // Add 10 => 10
            vm.Start(duration);

            var dow = DateTime.UtcNow.DayOfWeek;
            vm[dow].Should().Be(10.Seconds());

            duration = 20.Seconds(); // Add 20 => 30
            vm.Start(duration);
            vm[dow].Should().Be(30.Seconds());

            vm = new ViewModelData();
            vm.Start();
            await Task.Delay(3.Seconds());
            vm.Stop();
            vm[dow].Should().BeCloseTo(3.Seconds(), precision: 500);

            vm.Start();
            await Task.Delay(5.Seconds());
            vm.Stop();
            vm[dow].Should().BeCloseTo(8.Seconds(), precision: 500);
        }     

        [TestMethod]
        public void GoodCalculationOfDaysStats()
        {
            var vm = new ViewModelData();

            var duration = 5.Hours(); // Add 5h
            vm.Start(duration);
            vm.TodayDelta.Should().Be(-3.Hours());
            vm.ThisWeekDelta.Should().Be(-3.Hours());

            duration = 3.Hours(); // Add 3h => 8h
            vm.Start(duration);
            vm.TodayDelta.Should().Be(TimeSpan.Zero);
            vm.ThisWeekDelta.Should().Be(TimeSpan.Zero);

            duration = 2.Hours(); // Add 2h => 1h
            vm.Start(duration);
            vm.TodayDelta.Should().Be(2.Hours());
            vm.ThisWeekDelta.Should().Be(2.Hours());

            var tdy = DateTime.UtcNow.DayOfWeek;
            var yst = DateTime.UtcNow.AddDays(-1).DayOfWeek;
            vm.SetTime(tdy, TimeSpan.FromHours(5)); // -3
            vm.SetTime(yst, TimeSpan.FromHours(9)); // +1
            vm.TodayDelta.Should().Be(-3.Hours());
            vm.ThisWeekDelta.Should().Be(-2.Hours());

            vm.Start(5.Hours()); // +5 today => 10
            vm.TodayDelta.Should().Be(2.Hours());
            vm.ThisWeekDelta.Should().Be(3.Hours());
        }

        [TestMethod]
        public async Task StartedShouldHaveCorrectStats()
        {
            var persister = new Persister();
            var vm = new ViewModelData(persister:persister);

            vm.Start(5.Hours());
            vm.Start();
            await Task.Delay(TimeSpan.FromSeconds(5));
            vm.TodayTime.Should().BeCloseTo(new TimeSpan(5, 0, 5), 20);
            vm.ThisWeekTime.Should().BeCloseTo(new TimeSpan(5, 0, 5), 20);
        }

        [TestMethod]
        public void TickShouldRaiseEvents()
        {
            var persister = new Persister();
            var vm = new ViewModelData(persister: persister);
            var pnChanged = new List<string>();
            vm.PropertyChanged += (s, e) =>
            {
                pnChanged.Add(e.PropertyName);
            };

            vm.Tick();

            pnChanged.Should().Contain("TodayTime");
            pnChanged.Should().Contain("ThisWeekTime");
            pnChanged.Should().Contain("TodayDelta");
            pnChanged.Should().Contain("ThisWeekDelta");
        }
    }
}
