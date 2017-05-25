using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services
{
    abstract public class BaseService
    {
        public BaseService()
        {

        }

        abstract public bool CanUserExecuteAPI(string apiName, params int[] id);

    }
}
