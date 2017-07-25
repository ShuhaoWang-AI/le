using Linko.LinkoExchange.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Cache
{
    public interface IApplicationCache
    {
        object Get(string key);
        void Insert(string key, object item, int hours);
    }
}
