using Linko.LinkoExchange.Services.Dto;
using System;
using System.Web;
using Linko.LinkoExchange.Data;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.AuditLog
{ 
    public class CrommerAuditLogService : IAuditLogService
    {
        /// <summary>
        /// Write CrommerAuditLog
        /// </summary>
        /// <param name="logEntry">CrommerAuditLog object</param>
        public void Log(IAuditLogEntry logEntry)
        {
//            var crommerLogEntry = logEntry as CrommerAuditLogEntry;
//
//            var userInfo = (UserDto) HttpContext.Current.Session["userInfo"]; 
//
//            var linkoExchangeDbContext = new LinkoExchangeEntities();
//            var crommerLog = new CrommerAuditLog {AuthorityId = userInfo.AuthorityId};
//
//            if (crommerLogEntry != null)
//            {
//                crommerLog.EventName = crommerLogEntry.EventName;
//                crommerLog.CreatedAt = DateTime.Now;
//
//                var comments = String.Format ("UserName:{0}, Email:{1}, Authority name: {2},  Authority Id: {3} Industry name:{4}, Industry Id{5} ",
//
//                    userInfo.FirstName + " " + userInfo.LastName,
//                    crommerLogEntry.Email,
//                    "",
//                    userInfo.AuthorityId,
//                    userInfo.IndustryName,
//                    userInfo.IndustryId
//                );
//                
//                crommerLog.Comments = comments;
//            }
//
//            linkoExchangeDbContext.CrommerAuditLogs.Add (crommerLog);
//
//            try
//            {
//                linkoExchangeDbContext.SaveChanges ();
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine (e);
//            }
//
//            Console.WriteLine ("calling  CrommerAuditLog Service......");
        }
    }
}
