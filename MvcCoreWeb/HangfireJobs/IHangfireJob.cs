using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcCoreWeb.HangfireJobs
{
    public interface IHangfireJob
    {
        void DaliyWork();
    }
}
