using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.UI.Fluent;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Web.Controllers;
using Linko.LinkoExchange.Web.Shared;
using Linko.LinkoExchange.Web.ViewModels.Industry;
using Linko.LinkoExchange.Web.ViewModels.Shared;

namespace Linko.LinkoExchange.Web.Extensions
{
    public static class HtmlExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="htmlHelper"> </param>
        /// <param name="controllers"> </param>
        /// <param name="actions"> </param>
        /// <param name="routeValueTag"> </param>
        /// <param name="cssClass"> </param>
        /// <returns> </returns>
        public static string IsActive(this HtmlHelper htmlHelper, string controllers = "", string actions = "", string routeValueTag = null, string cssClass = "active")
        {
            return DoesLocationMatch(htmlHelper:htmlHelper, controllers:controllers, actions:actions, routeValueTag:routeValueTag) ? cssClass : string.Empty;
        }

        /// <summary>
        /// </summary>
        /// <param name="htmlHelper"> </param>
        /// <param name="controllers"> </param>
        /// <param name="actions"> </param>
        /// <param name="routeValueTag"> </param>
        /// <returns> </returns>
        public static bool DoesLocationMatch(this HtmlHelper htmlHelper, string controllers = "", string actions = "", string routeValueTag = null)
        {
            var viewContext = htmlHelper.ViewContext;
            var isChildAction = viewContext.Controller.ControllerContext.IsChildAction;

            if (isChildAction)
            {
                viewContext = htmlHelper.ViewContext.ParentActionViewContext;
            }

            var routeValues = viewContext.RouteData.Values;
            var currentAction = routeValues[key:"action"].ToString();
            var currentController = routeValues[key:"controller"].ToString();

            if (string.IsNullOrEmpty(value:actions))
            {
                actions = currentAction;
            }

            if (string.IsNullOrEmpty(value:controllers))
            {
                controllers = currentController;
            }

            var acceptedActions = actions.Trim().Split(',').Distinct().ToArray();
            var acceptedControllers = controllers.Trim().Split(',').Distinct().ToArray();

            if (string.IsNullOrEmpty(value:routeValueTag))
            {
                return acceptedActions.Contains(value:currentAction) && acceptedControllers.Contains(value:currentController);
            }

            var routeKeyValuePair = routeValueTag.Split(':').ToArray();
            var routeValue = routeValues[key:routeKeyValuePair[0]]?.ToString();

            if (string.IsNullOrEmpty(value:routeValue))
            {
                return acceptedActions.Contains(value:currentAction) && acceptedControllers.Contains(value:currentController);
            }

            return acceptedActions.Contains(value:currentAction) && acceptedControllers.Contains(value:currentController) && routeValue.Equals(value:routeKeyValuePair[1]);
        }

        public static GridBuilder<T> KendoSampleImportDataTranslationGrid<T>(
            this HtmlHelper helper,
            List<T> dataTranslationViewModels,
            string memberName,
            DataSourceTranslationType translationType,
            bool isEditable,
            bool isUntranslatedSwitchOn)
            where T : DataSourceTranslationViewModel
        {
            var grid = helper.Kendo().Grid(dataSource:dataTranslationViewModels);

            grid.Name(componentName:$"grid{translationType}")
                .Columns(columns =>
                         {
                             columns.Bound(c => c.IsTranslated)
                                    .ClientTemplate(
                                                    value:"# if (!IsTranslated) { #" + "<i class='fa fa-lg fa-exclamation-triangle fg-yellow'/> " + "# } #"
                                                   ).Title(text:"")
                                    .HtmlAttributes(attributes:new
                                                               {
                                                                   align = "center"
                                                               })
                                    .Hidden(!isUntranslatedSwitchOn)
                                    .Width(pixelWidth:40);
                             columns.Bound(c => c.DataSourceTerm)
                                    .HtmlAttributes(attributes:new {@class = "col-sm-2"})
                                    .HeaderHtmlAttributes(attributes:new {@class = "col-sm-2"})
                                 ;
                             columns.Bound(memberType:typeof(DropdownOptionViewModel), memberName:memberName)
                                    .ClientTemplate(value:"#= (TranslatedItem==null)? '' : TranslatedItem.DisplayName #")
                                    .HtmlAttributes(attributes:new {@class = isEditable ? "col-sm-4" : ""})
                                    .HeaderHtmlAttributes(attributes:new {@class = isEditable ? "col-sm-4" : ""})
                                 ;
                             if (isEditable)
                             {
                                 columns.Command(command => { command.Edit().UpdateText(text:"Save"); });
                             }
                         });
            if (isEditable)
            {
                grid.Editable(editable => editable.Mode(mode:GridEditMode.InLine));
            }

            var defaultDropdownOptionKey = SampleImportHelpers.GetDefaultDropdownOptionKey(translationType:translationType);
            var defaultValue = helper.ViewData[key:defaultDropdownOptionKey] as DropdownOptionViewModel;
            var actionName = GetUpdateSampleImportDataTranslationActionName(translationType:translationType);

            grid.Pageable(pager => pager.Enabled(value:false))
                .Scrollable(s => s.Enabled(value:false))
                .Sortable(sortable => { sortable.SortMode(value:GridSortMode.MultipleColumn); })
                .NoRecords(text: GetNoDataTranslationsDescription(translationType:translationType, isUntranslated:isUntranslatedSwitchOn))
                .DataSource(configurator:GetSampleImportDataTranslationDataSource<T>(memberName:memberName,
                                                                                     actionName:actionName,
                                                                                     translationType: translationType,
                                                                                     defaultValue:defaultValue,
                                                                                     isUntranslatedSwitchOn:isUntranslatedSwitchOn));

            return grid;
        }

        public static string GetNoDataTranslationsDescription(DataSourceTranslationType translationType, bool isUntranslated)
        {
            var pattern = isUntranslated ? "There are no untranslated {0}." : "There are no translated {0}.";
            var domainName = IndustryController.GetTranslatedTypeDomainName(translationType:translationType, plural:true);

            return string.Format(format:pattern, arg0:domainName);
        }

        private static Action<DataSourceBuilder<T>> GetSampleImportDataTranslationDataSource<T>(string memberName,
                                                                                                string actionName,
                                                                                                DataSourceTranslationType translationType,
                                                                                                DropdownOptionViewModel defaultValue,
                                                                                                bool isUntranslatedSwitchOn)
            where T : DataSourceTranslationViewModel
        {
            return dataSource =>
                   {
                       var dataSourceBuilder = dataSource.Ajax();
                       dataSourceBuilder.ServerOperation(enabled:false)
                                        .Batch(enabled:false)
                                        .Update(update => update.Action(actionName:actionName, controllerName:"Industry"))
                                        .Sort(sort =>
                                              {
                                                  sort.Add(memberName:"IsTranslated").Ascending();
                                                  sort.Add(memberName:"DataSourceTerm").Ascending();
                                              })
                                        .PageSize(pageSize:100)
                                        .Filter(f => f.Add(p => p.IsTranslated).IsEqualTo(value:!isUntranslatedSwitchOn))
                                        .Events(events => events.Error(handler:string.Format(format:"dataSourceManipulatingErrorHandler.bind({{gridId: '#grid{0}', errorDivId: '#validationSummary{0}'}})", arg0:translationType.ToString())))
                                        .Model(model =>
                                               {
                                                   model.Id(p => p.DataSourceTerm);
                                                   model.Field(p => p.IsTranslated).Editable(enabled:false);
                                                   model.Field(p => p.DataSourceTerm).Editable(enabled:false);
                                                   model.Field(p => p.Id).Editable(enabled:false);
                                                   model.Field(p => p.DataSourceId).Editable(enabled:false);
                                                   model.Field(p => p.TranslationType).Editable(enabled:false);
                                                   model.Field(memberName:memberName, memberType:typeof(DropdownOptionViewModel)).DefaultValue(value:defaultValue);
                                               });
                   };
        }

        private static string GetUpdateSampleImportDataTranslationActionName(DataSourceTranslationType translationType)
        {
            switch (translationType)
            {
                case DataSourceTranslationType.MonitoringPoint: return "SampleImportSaveMonitoringPointDataTranslation";
                case DataSourceTranslationType.SampleType: return "SampleImportSaveSampleTypeDataTranslation";
                case DataSourceTranslationType.CollectionMethod: return "SampleImportSaveCollectionMethodDataTranslation";
                case DataSourceTranslationType.Parameter: return "SampleImportSaveParameterDataTranslation";
                case DataSourceTranslationType.Unit: return "SampleImportSaveUnitDataTranslation";
                default: throw new ArgumentOutOfRangeException(paramName:nameof(translationType), actualValue:translationType, message:null);
            }
        }
    }
}