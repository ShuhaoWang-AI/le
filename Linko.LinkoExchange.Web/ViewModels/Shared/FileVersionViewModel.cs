using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    public class FileVersionViewModel
    {
        #region public properties

        [ScaffoldColumn(scaffold:false)]
        public int? FileVersionId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        /// <summary>
        /// Authority OrganizationRegulatoryProgramId
        /// </summary>
        [ScaffoldColumn(scaffold:false)]
        public int OrganizationRegulatoryProgramId { get; set; }

        public List<FileVersionFieldViewModel> FileVersionFields { get; set; }

        #endregion

        public class FileVersionFieldViewModel
        {
            #region public properties

            [ScaffoldColumn(scaffold:false)]
            public int? FileVersionFieldId { get; set; }

            /// <summary>
            /// System column name
            /// </summary>
            [Display(Name = "System Field Name")]
            [Editable(allowEdit:false)]
            public SampleImportColumnName SystemFieldName { get; set; }
            /// <summary>
            /// System column name
            /// </summary>
            [Display(Name = "System Field Name")]
            [Editable(allowEdit:false)]
            public string SystemFieldNameDisplayText => SystemFieldName.ToString();

            /// <summary>
            /// Authority's column name
            /// </summary>
            [Display(Name = "Column Header")]
            public string FileVersionFieldName { get; set; }

            [Display(Name = "Is System Required")]
            [Editable(allowEdit:false)]
            public bool IsSystemRequired { get; set; }

            [Display(Name = "Data Optionality")]
            public DataOptionalityName DataOptionalityName { get; set; }

            public IEnumerable<SelectListItem> AvailableResultQualifierValidValues
            {
                get
                {
                    var options = new List<SelectListItem>
                                   {
                                       new SelectListItem
                                       {
                                           Text = DataOptionalityName.Required.ToString(),
                                           Value = ((int) DataOptionalityName.Required).ToString(),
                                           Selected = DataOptionalityName == DataOptionalityName.Required
                                       },
                                       new SelectListItem
                                       {
                                           Text = DataOptionalityName.Recommended.ToString(),
                                           Value = ((int) DataOptionalityName.Recommended).ToString(),
                                           Selected = DataOptionalityName == DataOptionalityName.Recommended
                                       },
                                       new SelectListItem
                                       {
                                           Text = DataOptionalityName.Optional.ToString(),
                                           Value = ((int) DataOptionalityName.Optional).ToString(),
                                           Selected = DataOptionalityName == DataOptionalityName.Optional
                                       }
                                   };

                    return options;
                }
            }

            [Display(Name = "Field Size")]
            [Editable(allowEdit:false)]
            public int? Size { get; set; }

            [Display(Name = "Data Format")]
            [Editable(allowEdit:false)]
            public DataFormatName DataFormatName { get; set; }
            
            [Display(Name = "Data Format")]
            [Editable(allowEdit:false)]
            public string DataFormatDescription { get; set; }

            [Display(Name = "Column Description")]
            [DataType(dataType:DataType.MultilineText)]
            public string Description { get; set; }

            [Display(Name = "Example Data")]
            [DataType(dataType:DataType.MultilineText)]
            public string ExampleData { get; set; }

            [Display(Name = "Additional Comments")]
            [DataType(dataType:DataType.MultilineText)]
            public string AdditionalComments { get; set; }

            /// <summary>
            /// Used in authority portal to select the system field in the File Version
            /// </summary>
            [Display(Name = "Is Included")]
            public bool IsIncluded { get; set; }

            #endregion
        }
    }
}