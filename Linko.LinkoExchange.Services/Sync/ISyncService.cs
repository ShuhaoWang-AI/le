
namespace Linko.LinkoExchange.Services.Sync
{
    public interface ISyncService
    {
        /// <summary>
        /// 1. Only send report packages, elements and samples to CTS where an event type in the RP or Sample exists.
        ///    a. A report package must have an event type to be sent to CTS.
        ///    b. Samples and results must also have an event type. 
        ///       Samples and results that are part of a report package are not sent to CTS unless the report package also has an event type.
        /// 2. Only send Report Element data where the element has an Event Type AND 
        ///    the report package also has an Event Type and the Element was actually included in the Report Package by the user.
        /// </summary>
        /// <param name="reportPackageId">Report package Id.</param>
        void SendSubmittedReportPackageToCts(int reportPackageId);
    }
}
