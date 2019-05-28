using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeSheet;
using FluentAssertions.Extensions;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void NewVMShouldBeCorrectlyInitialized()
        {
            var vm = new ViewModelData();

            vm.TimeForToday.Should().Be(TimeSpan.Zero);
            vm.TimeForThisWeek.Should().Be(TimeSpan.Zero);
        }

        [TestMethod]
        public async Task OneStartStopShouldSetCorrectValues()
        {
            var vm = new ViewModelData();

            var duration = 10.Seconds();

            vm.Start(duration);

            vm.TimeForToday.Should().Be(10.Seconds(), "Day time incorrect");
            vm.TimeForThisWeek.Should().Be(10.Seconds(), "Week time incorrect");

            vm = new ViewModelData();
            vm.Start();
            await Task.Delay(3.Seconds());
            vm.Stop();

            vm.TimeForToday.Should().BeCloseTo(3.Seconds(), precision:500);
            vm.TimeForThisWeek.Should().BeCloseTo(3.Seconds(), precision: 500);
        }

        [TestMethod]
        public async Task MultipleStartStopShouldSetCorrectValues()
        {
            var vm = new ViewModelData();

            var duration = 10.Seconds(); // Add 10 => 10
            vm.Start(duration);
            vm.TimeForToday.Should().Be(10.Seconds(), "Day time incorrect");
            vm.TimeForThisWeek.Should().Be(10.Seconds(), "Week time incorrect");

            duration = 20.Seconds(); // Add 20 => 30
            vm.Start(duration);
            vm.TimeForToday.Should().Be(30.Seconds(), "Day time incorrect");
            vm.TimeForThisWeek.Should().Be(30.Seconds(), "Week time incorrect");

            vm = new ViewModelData();
            vm.Start();
            await Task.Delay(3.Seconds());
            vm.Stop();
            vm.TimeForToday.Should().BeCloseTo(3.Seconds(), precision: 500);
            vm.TimeForThisWeek.Should().BeCloseTo(3.Seconds(), precision: 500);

            vm.Start();
            await Task.Delay(5.Seconds());
            vm.Stop();
            vm.TimeForToday.Should().BeCloseTo(8.Seconds(), precision: 500);
            vm.TimeForThisWeek.Should().BeCloseTo(8.Seconds(), precision: 500);
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
    }
}
