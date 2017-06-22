using System;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc;
using Kendo.Mvc.UI;

namespace Linko.LinkoExchange.Web.Mvc
{
    // URL: http://www.crowbarsolutions.com/ignoring-time-when-filtering-dates-in-telerik-kendo-grids/
    public class CustomDataSourceRequestAttribute : DataSourceRequestAttribute
    {
        public override IModelBinder GetBinder()
        {
            return new CustomDataSourceRequestModelBinder();
        }
    }

    /// <summary>
    ///     DateTime filtering is horribly unintuitive in Kendo Grids when a non-default (00:00:00) time is attached
    ///     to the grid's datetime data. We use this custom model binder to transform the grid filters to return
    ///     results that ignore the attached time, leading to intuitive results that make users happy.
    ///     To use the code, substitute the [DataSourceRequest] attribute for [CustomDataSourceRequest] in your MVC controller
    /// </summary>
    public class CustomDataSourceRequestModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            // Get an instance of the original kendo model binder and call the binding method
            var baseBinder = new DataSourceRequestModelBinder();
            var request = (DataSourceRequest) baseBinder.BindModel(controllerContext: controllerContext, bindingContext: bindingContext);

            if (request.Filters != null && request.Filters.Count > 0)
            {
                var transformedFilters = request.Filters.Select(selector: TransformFilterDescriptors).ToList();
                request.Filters = transformedFilters;
            }

            return request;
        }

        private IFilterDescriptor TransformFilterDescriptors(IFilterDescriptor filter)
        {
            if (filter is CompositeFilterDescriptor)
            {
                var compositeFilterDescriptor = filter as CompositeFilterDescriptor;
                var transformedCompositeFilterDescriptor = new CompositeFilterDescriptor {LogicalOperator = compositeFilterDescriptor.LogicalOperator};
                foreach (var filterDescriptor in compositeFilterDescriptor.FilterDescriptors)
                {
                    transformedCompositeFilterDescriptor.FilterDescriptors.Add(item: TransformFilterDescriptors(filter: filterDescriptor));
                }

                return transformedCompositeFilterDescriptor;
            }

            if (filter is FilterDescriptor)
            {
                var filterDescriptor = filter as FilterDescriptor;
                if (filterDescriptor.Value is DateTime)
                {
                    var value = (DateTime) filterDescriptor.Value;
                    switch (filterDescriptor.Operator)
                    {
                        case FilterOperator.IsEqualTo:

                            //convert the "is equal to <date><time>" filter to a "is greater than or equal to <date> 00:00:00" AND "is less than or equal to <date> 23:59:59"
                            var isEqualCompositeFilterDescriptor = new CompositeFilterDescriptor {LogicalOperator = FilterCompositionLogicalOperator.And};
                            isEqualCompositeFilterDescriptor.FilterDescriptors.Add(item: new FilterDescriptor(member: filterDescriptor.Member,
                                                                                                              filterOperator: FilterOperator.IsGreaterThanOrEqualTo,
                                                                                                              filterValue: new DateTime(year: value.Year, month: value.Month, day: value.Day, hour: 0,
                                                                                                                                        minute: 0, second: 0)));
                            isEqualCompositeFilterDescriptor.FilterDescriptors.Add(item: new FilterDescriptor(member: filterDescriptor.Member,
                                                                                                              filterOperator: FilterOperator.IsLessThanOrEqualTo,
                                                                                                              filterValue: new DateTime(year: value.Year, month: value.Month, day: value.Day, hour: 23,
                                                                                                                                        minute: 59, second: 59)));
                            return isEqualCompositeFilterDescriptor;

                        case FilterOperator.IsNotEqualTo:

                            //convert the "is not equal to <date><time>" filter to a "is less than <date> 00:00:00" OR "is greater than <date> 23:59:59"
                            var notEqualCompositeFilterDescriptor = new CompositeFilterDescriptor {LogicalOperator = FilterCompositionLogicalOperator.Or};
                            notEqualCompositeFilterDescriptor.FilterDescriptors.Add(item: new FilterDescriptor(member: filterDescriptor.Member,
                                                                                                               filterOperator: FilterOperator.IsLessThan,
                                                                                                               filterValue: new DateTime(year: value.Year, month: value.Month, day: value.Day, hour: 0,
                                                                                                                                         minute: 0, second: 0)));
                            notEqualCompositeFilterDescriptor.FilterDescriptors.Add(item: new FilterDescriptor(member: filterDescriptor.Member,
                                                                                                               filterOperator: FilterOperator.IsGreaterThan,
                                                                                                               filterValue: new DateTime(year: value.Year, month: value.Month, day: value.Day, hour: 23,
                                                                                                                                         minute: 59, second: 59)));
                            return notEqualCompositeFilterDescriptor;

                        case FilterOperator.IsGreaterThanOrEqualTo:

                            //convert the "is greater than or equal to <date><time>" filter to a "is greater than or equal to <date> 00:00:00"
                            filterDescriptor.Value = new DateTime(year: value.Year, month: value.Month, day: value.Day, hour: 0, minute: 0, second: 0);
                            return filterDescriptor;

                        case FilterOperator.IsGreaterThan:

                            //convert the "is greater than <date><time>" filter to a "is greater than <date> 23:59:59"
                            filterDescriptor.Value = new DateTime(year: value.Year, month: value.Month, day: value.Day, hour: 23, minute: 59, second: 59);
                            return filterDescriptor;

                        case FilterOperator.IsLessThanOrEqualTo:

                            //convert the "is less than or equal to <date><time>" filter to a "is less than or equal to <date> 23:59:59"
                            filterDescriptor.Value = new DateTime(year: value.Year, month: value.Month, day: value.Day, hour: 23, minute: 59, second: 59);
                            return filterDescriptor;

                        case FilterOperator.IsLessThan:

                            //convert the "is less than <date><time>" filter to a "is less than <date> 00:00:00"
                            filterDescriptor.Value = new DateTime(year: value.Year, month: value.Month, day: value.Day, hour: 0, minute: 0, second: 0);
                            return filterDescriptor;

                        default:
                            throw new Exception(message: string.Format(format: "Filter operator '{0}' is not supported for DateTime member '{1}'", arg0: filterDescriptor.Operator,
                                                                       arg1: filterDescriptor.Member));
                    }
                }
            }

            return filter;
        }
    }
}