using System;
using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.DataSource
{
    public interface IDataSourceService
    {
        int SaveDataSource(DataSourceDto dataSourceDto);

        void DeleteDataSource(int dataSourceId);
        
        List<DataSourceDto> GetDataSources(int organizationRegulatoryProgramId);

        DataSourceDto GetDataSource(int organizationRegulatoryProgramId, string name);

        DataSourceDto GetDataSourceById(int dataSourceId);
    }
}
