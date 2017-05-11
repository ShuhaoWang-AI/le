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
            DateTime dateTimeEST = new DateTime(2016, 11, 6, 20, 0, 0, 0, DateTimeKind.Unspecified);
            var dateTimeOffsetLocalizedToServer = _tZservice.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(dateTimeEST, localTimeZoneId);

            //8pm EST = 5pm (17:00) PST (server time)
            Assert.AreEqual(17, dateTimeOffsetLocalizedToServer.Hour);
            Assert.AreEqual(0, dateTimeOffsetLocalizedToServer.Minute);
            Assert.AreEqual(0, dateTimeOffsetLocalizedToServer.Second);
            Assert.AreEqual(-8, dateTimeOffsetLocalizedToServer.Offset.Hours);

        }

        [TestMethod]
        public void DateTimeOffset_Now()
        {
            var now = DateTimeOffset.Now;
            Assert.AreEqual(-7, now.Offset.Hours); //PDT
        }
    }
}
