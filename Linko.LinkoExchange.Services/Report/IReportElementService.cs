using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Report
{
    public interface IReportElementService
    {
        /// <summary>
        /// Gets a collection of Report Element Types associated with a category.
        /// </summary>
        /// <param name="categoryName">The name of the report element category</param>
        /// <returns>Collection of dto's that map to the Report Element Type objects associated with the passed in category </returns>
        IEnumerable<ReportElementTypeDto> GetReportElementTypes(ReportElementCategoryName categoryName);

        /// <summary>
        /// Gets the details of a Report Element Type from the database.
        /// </summary>
        /// <param name="reportElementTypeId">ReportElementTypeId in the tReportElementType table</param>
        /// <returns>Dto that maps to the Report Element Type object associated with the passed in Id</returns>
        ReportElementTypeDto GetReportElementType(int reportElementTypeId);

        /// <summary>
        /// Creates a new Report Element Type in the database or updates an existing one (if an Id is provided)
        /// </summary>
        /// <param name="reportElementType">The Dto that gets mapped to a Report Element Type and saved to the DB.
        /// If and Id is not provided, it is assumed a new object gets created in the database.</param>
        /// <returns>Existing Id or newly created Id</returns>
        int SaveReportElementType(ReportElementTypeDto reportElementType);

        /// <summary>
        /// Removes a Report Element Type from the database
        /// </summary>
        /// <param name="reportElementTypeId">Id of the Report Element Type to remove from the database</param>
        void DeleteReportElementType(int reportElementTypeId);

        /// <summary>
        /// Checks to see if a Report Element Type is used in any Report Package Template
        /// </summary>
        /// <param name="reportElementTypeId">The Id of the Report Element Type to check.</param>
        /// <returns>True = Report Element Type is included in at least 1 Report Package Template, False otherwise.</returns>
        bool IsReportElementTypeInUse(int reportElementTypeId);
    }
}
