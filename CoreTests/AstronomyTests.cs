using Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tests
{
    [TestFixture()]
    public class AstronomyTests
    {


        [TestCase("2025-01-01")]
        [TestCase("2003-06-15")]
        [TestCase("2026-12-31")]
        [TestCase("1987-04-03")]
        public void GetSynodicAge_GivenDate_ReturnsInRange(string dateTime)
        {

            //Arrange
            var date = DateTime.Parse(dateTime);

            //Act
            var age = Astronomy.GetSynodicAge(date);

            //Assert
            Assert.That(age, Is.InRange(0, Astronomy.SynodicMonth));
        }

        [TestCase("2024-09-18 02:34")]
        [TestCase("2024-10-17 11:26")]
        public void GetMoonPhase_GivenDate_IdentifiesFullMoon(string dateTime)
        {

            //Arrange
            var date = DateTime.ParseExact(dateTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

            //Act
            var moonPhase = Astronomy.GetMoonPhase(date, 0.5);

            //Assert
            Assert.That(moonPhase, Is.EqualTo(MoonPhases.FullMoon));
        }

        [TestCase("2024-09-03 05:55")]
        [TestCase("2024-10-02 18:49")]
        public void GetMoonPhase_GivenDate_IdentifiesNewMoon(string dateTime)
        {

            //Arrange
            var date = DateTime.ParseExact(dateTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

            //Act
            var moonPhase = Astronomy.GetMoonPhase(date, 0.5);

            //Assert
            Assert.That(moonPhase, Is.EqualTo(MoonPhases.NewMoon));
        }
    }
}