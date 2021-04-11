using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Core
{
    public interface IDashLinkModuleLoader
    {
        void LoadModules(DashLinkHost host, HashSet<string> moduleIds);
    }
}
