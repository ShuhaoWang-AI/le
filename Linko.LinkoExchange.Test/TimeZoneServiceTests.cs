using System.Configuration;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services.Permission;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.TimeZone;
using System;
using Moq;
using NLog;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Mapping;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class TimeZoneServiceTests
    {
        private TimeZoneService _tZservice;
        Mock<ISettingService> _settings;

        public TimeZoneServiceTests()
        {
        }

        [TestInitialize]
        public void Initialize()
        {
            _settings = new Mock<ISettingService>();
            _settings.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), 1, It.IsAny<SettingType>())).Returns("1");
            _settings.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), 2, It.IsAny<SettingType>())).Returns("4");
            _settings.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<SettingType>())).Returns("4");

            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            _tZservice = new TimeZoneService(new LinkoExchangeContext(connectionString), _settings.Object, new MapHelper());
        }

        [TestMethod]
        public void GetTimeZones_Test()
        {
            var dtos = _tZservice.GetTimeZones();
        }

        [TestMethod]
        public void GetCurrentUtcDateTime_Test()
        {
            DateTime now = DateTime.UtcNow;
            DateTimeOffset? registrationDate = now;
            DateTime regDatePST = _tZservice.GetLocalizedDateTimeUsingSettingForThisOrg(registrationDate.Value.UtcDateTime, 1000, 1);
            DateTime regDateEST = _tZservice.GetLocalizedDateTimeUsingSettingForThisOrg(registrationDate.Value.UtcDateTime, 1000, 2);
            Assert.AreEqual(regDateEST, regDatePST.AddHours(3));


            DateTimeOffset now2 = DateTimeOffset.UtcNow;
            registrationDate = now2;
            regDatePST = _tZservice.GetLocalizedDateTimeUsingSettingForThisOrg(registrationDate.Value.UtcDateTime, 1000, 1);
            regDateEST = _tZservice.GetLocalizedDateTimeUsingSettingForThisOrg(registrationDate.Value.UtcDateTime, 1000, 2);
            Assert.AreEqual(regDateEST, regDatePST.AddHours(3));

        }

        [TestMethod]
        public void Convert_Current_UTC_To_Local_And_Back_To_UTC_Test()
        {
            DateTime now = DateTime.UtcNow;

            var localTime = _tZservice.GetLocalizedDateTimeUsingSettingForThisOrg(now, 1);
            var convertedBackToUTC = _tZservice.GetUTCDateTimeUsingSettingForThisOrg(localTime, 1);

            Assert.AreEqual(now, convertedBackToUTC);
        }

        [TestMethod]
        public void GetLocalizedDateTimeOffsetUsingSettingForThisOrg()
        {
            DateTime dateTimeUtcNow = DateTime.UtcNow;

            var localizedDateTimeOffset = _tZservice.GetLocalizedDateTimeOffsetUsingSettingForThisOrg(dateTimeUtcNow, 1);
            var convertedBackToDateTimeUtc = localizedDateTimeOffset.UtcDateTime;

            Assert.AreEqual(dateTimeUtcNow, convertedBackToDateTimeUtc);
        }

        [TestMethod]
        public void GetLocalizedDateTimeOffsetUsingSettingForThisOrg_WinterTime()
        {
            // return Pacific time;
            _settings.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<SettingType>())).Returns("3");

            DateTime dateTimeUtcNow = new DateTime(2016, 12, 9, 16, 5, 7, 123);

            var localizedDateTimeOffset = _tZservice.GetLocalizedDateTimeOffsetUsingSettingForThisOrg(dateTimeUtcNow, 1);

            //offset is UTC-8
            Assert.AreEqual(-8, localizedDateTimeOffset.Offset.Hours);
        }

        [TestMethod]
        public void GetLocalizedDateTimeOffsetUsingSettingForThisOrg_SummarTime()
        {
            // return Pacific time;
            _settings.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<SettingType>())).Returns("3");
            DateTime dateTimeUtcNow = new DateTime(2016, 6, 9, 16, 5, 7, 123);

            var localizedDateTimeOffset = _tZservice.GetLocalizedDateTimeOffsetUsingSettingForThisOrg(dateTimeUtcNow, 1);
            var ft = localizedDateTimeOffset.ToString("MM/dd/yyyyThh:mm:ss zzzz");

            // DaylingSaving, offset is UTC-7
            Assert.AreEqual(-7, localizedDateTimeOffset.Offset.Hours);

        }
    }
}
