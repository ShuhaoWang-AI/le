using System;
using System.Configuration;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;

// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleStringLiteral

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class TimeZoneServiceTests
    {
        #region fields

        private Mock<ISettingService> _settings;
        private TimeZoneService _tZservice;
        private Mock<ILogger> _logger;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            _logger = new Mock<ILogger>();
            _settings = new Mock<ISettingService>();
            _settings.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), 1, It.IsAny<SettingType>())).Returns(value:"1");
            _settings.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), 2, It.IsAny<SettingType>())).Returns(value:"4");
            _settings.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<SettingType>())).Returns(value:"4");

            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;
            _tZservice = new TimeZoneService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString), settings:_settings.Object, mapHelper:new MapHelper(),
                                             appCache:new Mock<IApplicationCache>().Object, logger:_logger.Object);
        }

        [TestMethod]
        public void GetTimeZones_Test()
        {
            var dtos = _tZservice.GetTimeZones();
        }

        [TestMethod]
        public void GetCurrentUtcDateTime_Test()
        {
            var now = DateTime.UtcNow;
            DateTimeOffset? registrationDate = now;
            var regDatePST = _tZservice.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:registrationDate.Value.UtcDateTime, orgId:1000, regProgramId:1);
            var regDateEST = _tZservice.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:registrationDate.Value.UtcDateTime, orgId:1000, regProgramId:2);
            Assert.AreEqual(expected:regDateEST, actual:regDatePST.AddHours(value:3));

            var now2 = DateTimeOffset.UtcNow;
            registrationDate = now2;
            regDatePST = _tZservice.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:registrationDate.Value.UtcDateTime, orgId:1000, regProgramId:1);
            regDateEST = _tZservice.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:registrationDate.Value.UtcDateTime, orgId:1000, regProgramId:2);
            Assert.AreEqual(expected:regDateEST, actual:regDatePST.AddHours(value:3));
        }

        //=========================================================================
        //
        //  Daylight saving time 2016 in Canada (PST) began at:
        //    2:00 AM on Sunday, March 13 (10am UTC [converted from PST])
        //  and ended at:
        //    2:00 AM on Sunday, November 6 (9am UTC [converted from PST])
        //
        //=========================================================================

        //[TestMethod]
        //public void GetLocalizedDateTimeOffsetUsingSettingForThisOrg_Daylight_Savings_Start_Cusp()
        //{
        //    //  Daylight saving time 2016 in Canada (PST) began at:
        //    //    2:00am (PST) on Sunday, March 13 (10:00am UTC)

        //    //Return "Pacific Standard Time", "(GMT-08:00) Pacific Time (US & Canada)"
        //    _settings.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<SettingType>())).Returns("3");

        //    //Normal / Non-daylight saving time (PST)
        //    DateTime dateTimeUtcNow = new DateTime(2016, 3, 13, 9, 59, 59, 999, DateTimeKind.Utc);
        //    var localizedDateTimeOffset = _tZservice.GetLocalDateTimeOffsetFromUtcUsingSettingForThisOrg(dateTimeUtcNow, 1);
        //    Assert.AreEqual(-8, localizedDateTimeOffset.Offset.Hours);

        //    //Start Daylight saving time ("Spring ahead" and add 1 hour to our local time) PST -> PDT

        //    //PDT
        //    dateTimeUtcNow = new DateTime(2016, 3, 13, 10, 0, 0, 1, DateTimeKind.Utc);
        //    localizedDateTimeOffset = _tZservice.GetLocalDateTimeOffsetFromUtcUsingSettingForThisOrg(dateTimeUtcNow, 1);
        //    Assert.AreEqual(-7, localizedDateTimeOffset.Offset.Hours);
        //}

        //[TestMethod]
        //public void GetLocalizedDateTimeOffsetUsingSettingForThisOrg_Daylight_Savings_End_Cusp()
        //{
        //    //  Daylight saving time 2016 in Canada (PST) ended at:
        //    //    2:00am (PDT) on Sunday, November 6 (9:00am UTC)

        //    //Return "Pacific Standard Time", "(GMT-08:00) Pacific Time (US & Canada)"
        //    _settings.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<SettingType>())).Returns("3");

        //    //Daylight saving time (PDT)
        //    DateTime dateTimeUtcNow = new DateTime(2016, 11, 6, 8, 59, 59, 999, DateTimeKind.Utc);
        //    var localizedDateTimeOffset = _tZservice.GetLocalDateTimeOffsetFromUtcUsingSettingForThisOrg(dateTimeUtcNow, 1);
        //    Assert.AreEqual(-7, localizedDateTimeOffset.Offset.Hours);

        //    //End Daylight saving time ("Fall back" and subtract 1 hour to our local time) PDT -> PST

        //    //PST
        //    dateTimeUtcNow = new DateTime(2016, 11, 6, 9, 0, 0, 1, DateTimeKind.Utc);
        //    localizedDateTimeOffset = _tZservice.GetLocalDateTimeOffsetFromUtcUsingSettingForThisOrg(dateTimeUtcNow, 1);
        //    Assert.AreEqual(-8, localizedDateTimeOffset.Offset.Hours);

        //}

        ////======== Test methods without converting from UTC

        //[TestMethod]
        //public void GetDateTimeOffsetFromLocalUsingSettingForThisOrg_Daylight_Savings_Start_Cusp_LOCAL()
        //{
        //    //  Daylight saving time 2016 in Canada (PST) began at:
        //    //    2:00am (PST) on Sunday, March 13 (10:00am UTC)

        //    //Return "Pacific Standard Time", "(GMT-08:00) Pacific Time (US & Canada)"
        //    _settings.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<SettingType>())).Returns("3");

        //    //Normal / Non-daylight saving time (PST)
        //    DateTime dateTimeNow = new DateTime(2016, 3, 13, 1, 59, 59, 999, DateTimeKind.Unspecified);
        //    var localizedDateTimeOffset = _tZservice.GetLocalDateTimeOffsetFromLocalUsingSettingForThisOrg(dateTimeNow, 1);
        //    Assert.AreEqual(-8, localizedDateTimeOffset.Offset.Hours);

        //    //Start Daylight saving time ("Spring ahead" and add 1 hour to our local time) PST -> PDT

        //    //PDT
        //    dateTimeNow = new DateTime(2016, 3, 13, 2, 0, 0, 1, DateTimeKind.Unspecified);
        //    localizedDateTimeOffset = _tZservice.GetLocalDateTimeOffsetFromLocalUsingSettingForThisOrg(dateTimeNow, 1);
        //    Assert.AreEqual(-7, localizedDateTimeOffset.Offset.Hours);
        //}

        //[TestMethod]
        //public void GetDateTimeOffsetFromLocalUsingSettingForThisOrg_Daylight_Savings_End_Cusp_LOCAL()
        //{
        //    //  Daylight saving time 2016 in Canada (PST) ended at:
        //    //    2:00am (PDT) on Sunday, November 6 (9:00am UTC)

        //    //Return "Pacific Standard Time", "(GMT-08:00) Pacific Time (US & Canada)"
        //    _settings.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<SettingType>())).Returns("3");

        //    //Daylight saving time (PDT)
        //    DateTime dateTimeUtcNow = new DateTime(2016, 11, 6, 1, 59, 59, 999, DateTimeKind.Unspecified);
        //    var localizedDateTimeOffset = _tZservice.GetLocalDateTimeOffsetFromLocalUsingSettingForThisOrg(dateTimeUtcNow, 1);
        //    Assert.AreEqual(-7, localizedDateTimeOffset.Offset.Hours);

        //    //End Daylight saving time ("Fall back" and subtract 1 hour to our local time) PDT -> PST

        //    //PST
        //    dateTimeUtcNow = new DateTime(2016, 11, 6, 2, 0, 0, 1, DateTimeKind.Unspecified);
        //    localizedDateTimeOffset = _tZservice.GetLocalDateTimeOffsetFromLocalUsingSettingForThisOrg(dateTimeUtcNow, 1);
        //    Assert.AreEqual(-8, localizedDateTimeOffset.Offset.Hours);

        //}

        [TestMethod]
        public void GetDateTimeOffsetFromLocalUsingServerTimeZone()
        {
            //Return Eastern Standard Time
            var localTimeZoneId = 6;

            //8pm EST
            var dateTimeEST = new DateTime(year:2016, month:11, day:6, hour:20, minute:0, second:0, millisecond:0, kind:DateTimeKind.Unspecified);
            var dateTimeOffsetLocalizedToServer = _tZservice.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(localDateTime:dateTimeEST, timeZoneId:localTimeZoneId);

            //8pm EST = 5pm (17:00) PST (server time)
            Assert.AreEqual(expected:17, actual:dateTimeOffsetLocalizedToServer.Hour);
            Assert.AreEqual(expected:0, actual:dateTimeOffsetLocalizedToServer.Minute);
            Assert.AreEqual(expected:0, actual:dateTimeOffsetLocalizedToServer.Second);
            Assert.AreEqual(expected:-8, actual:dateTimeOffsetLocalizedToServer.Offset.Hours);
        }

        [TestMethod]
        public void DateTimeOffset_Now()
        {
            var now = DateTimeOffset.Now;
            Assert.AreEqual(expected:-7, actual:now.Offset.Hours); //PDT
        }
    }
}