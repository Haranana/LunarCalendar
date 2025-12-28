using NUnit.Framework;
using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tests
{
    [TestFixture]
    public class CacheStorageTests
    {
        [Test]
        public void InvalidateInstant_SetsInstantToNull()
        {
            //Arrange
            var cache = new CacheStorage();
            cache.SetInstant(new InstantCacheData { LastUpdateTime = DateTimeOffset.UtcNow });

            //Act
            cache.InvalidateInstant();

            //Asser
            Assert.That(cache.InstantCacheData, Is.Null);
        }

        [Test]
        public void IsInstantFresh_WhenNull_ReturnsFalse()
        {
            //Arrange
            var cache = new CacheStorage();

            //Act
            cache.InvalidateInstant();

            //Assert
            Assert.That(cache.IsInstantFresh(TimeSpan.FromMinutes(5)), Is.False);
        }

        [Test]
        public void IsInstantFresh_JustUpdated_ReturnsTrue()
        {
            //Arrange
            var cache = new CacheStorage();

            //Act
            cache.SetInstant(new InstantCacheData { LastUpdateTime = DateTimeOffset.UtcNow });

            //Assert
            Assert.That(cache.IsInstantFresh(TimeSpan.FromMinutes(30)), Is.True);
        }

        [Test]
        public void IsInstantFresh_WhenExpired_ReturnsFalse()
        {
            //Arrange
            var cache = new CacheStorage();

            //Act
            cache.SetInstant(new InstantCacheData { LastUpdateTime = DateTimeOffset.UtcNow - TimeSpan.FromHours(2) });

            //Assert
            Assert.That(cache.IsInstantFresh(TimeSpan.FromMinutes(10)), Is.False);
        }

        [Test]
        public void IsWeeklyFresh_OnDayOfLastUpdate_ReturnsTrue()
        {
            //Arrange
            var cache = new CacheStorage();

            //Act
            cache.SetWeekly(new WeeklyCacheData { LastUpdateTime = DateTimeOffset.UtcNow });

            //Assert
            Assert.That(cache.IsWeeklyFresh(), Is.True);
        }

        [Test]
        public void IsWeeklyFresh_DayAfterLastUpdate_ReturnsFalse()
        {
            //Arrange
            var cache = new CacheStorage();

            //Act
            cache.SetWeekly(new WeeklyCacheData { LastUpdateTime = DateTimeOffset.UtcNow - TimeSpan.FromDays(1) });

            //Assert
            Assert.That(cache.IsWeeklyFresh(), Is.False);
        }

        [Test]
        public void RefreshInstantData_WhenNull_ThrowsException()
        {
            var cache = new CacheStorage();
            Assert.Throws<Exception>(() => cache.RefreshInstantData(null));
        }
    }
}