using CRM.Application.Core.Enums;
using CRM.Application.Core.ViewModels;
using CRM.DAL;
using CRM.DAL.Repositories;
using CRM.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

using X.PagedList;

namespace CRM.Application.Core.Services
{
    public class TableFilterSortPagingService<T, T2> where T : class
                                                     where T2 : class
    {
        public GenericViewModel<T2, T> FilterTable(GenericViewModel<T2, T> genericViewModel, Object viewModel)
        {

            UnitofWork _uow = new UnitofWork();
            Expression<Func<T, bool>> query = null;
            Expression<Func<T, bool>> temp = null;
            QueryOperatorComparer queryOperator = (QueryOperatorComparer)Enum.Parse(typeof(QueryOperatorComparer), genericViewModel.QueryOperatorComparer);
            QueryOperatorComparer specialQueryOperator = 0;
            if (!string.IsNullOrEmpty(genericViewModel.SpecialConditionQueryOperatorComparer))
                specialQueryOperator = (QueryOperatorComparer)Enum.Parse(typeof(QueryOperatorComparer), genericViewModel.SpecialConditionQueryOperatorComparer);
            DynamicTableQueryResult<T> dynamicTableQueryResult = new DynamicTableQueryResult<T>();
            TableRepo<T> tableRepo = new TableRepo<T>(new CRMContext());
            if (genericViewModel.QueryParameters != null)
            {

                for (int i = 0; i < genericViewModel.QueryParameters.Count(); i++)
                {
                    var queryItem = genericViewModel.QueryParameters[i];
                    if (!string.IsNullOrEmpty(queryItem.SearchKey) && !string.IsNullOrEmpty(queryItem.Value))
                    {
                        Object value = null;
                        var isNumeric = int.TryParse(queryItem.Value, out int n);
                        System.Reflection.PropertyInfo p = typeof(T).GetProperty(queryItem.SearchKey);
                        Type propertyType = p.PropertyType;
                        if (isNumeric && propertyType.Name.ToLower() == "Int32".ToLower())
                            value = Convert.ToInt32(queryItem.Value);
                        else if (propertyType.Name.ToLower() == "Boolean".ToLower())
                            value = Convert.ToBoolean(queryItem.Value);
                        else
                            value = queryItem.Value;

                        query = LinqExpressionBuilder.BuildPredicate<T>(value, (OperatorComparer)Enum.Parse(typeof(OperatorComparer), queryItem.Operator), queryItem.SearchKey);
                        if (temp != null)
                        {
                            if (queryItem.HasSpecialConditionQueryOperatorComparer)
                                query = LinqExpressionBuilder.AddOperatorBetweenTwoExpression<T>(query, temp, specialQueryOperator);
                            else
                                query = LinqExpressionBuilder.AddOperatorBetweenTwoExpression<T>(query, temp, queryOperator);

                        }
                        temp = query;
                    }
                }
                dynamicTableQueryResult = tableRepo.DynamicTable(
                genericViewModel.PageSize,
                genericViewModel.PageNumber - 1,
                genericViewModel.OrderBy,
                genericViewModel.DefaultOrderBy,
                genericViewModel.Direction ?? genericViewModel.DefaultDirection,
                query);
            }
            else
                dynamicTableQueryResult = tableRepo.DynamicTable(
                     genericViewModel.PageSize,
                     genericViewModel.PageNumber - 1,
                     genericViewModel.OrderBy,
                     genericViewModel.DefaultOrderBy,
                     genericViewModel.Direction ?? genericViewModel.DefaultDirection,
                     null);
            var res = new StaticPagedList<T>(dynamicTableQueryResult.QueryResultList,
                genericViewModel.PageNumber,
                genericViewModel.PageSize,
                genericViewModel.QueryParameters != null ? dynamicTableQueryResult.QueryCount : dynamicTableQueryResult.TableCount);
            genericViewModel.ResultList = res;
            genericViewModel.TableCount = dynamicTableQueryResult.TableCount;
            genericViewModel.QueryCount = dynamicTableQueryResult.QueryCount;

            return genericViewModel;
        }
    }
}
