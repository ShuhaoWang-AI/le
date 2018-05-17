﻿using System.Collections.Generic;
using Linko.LinkoExchange.Core.Domain;
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

        List<DataSourceTranslationDto> GetDataSourceTranslations(int dataSourceId, DataSourceTranslationType translationType);

        Dictionary<string, DataSourceTranslationItemDto> GetDataSourceTranslationDict(int dataSourceId, DataSourceTranslationType translationType);
    }
}