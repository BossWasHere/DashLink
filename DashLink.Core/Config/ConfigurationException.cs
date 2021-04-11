using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Core.Config
{
    public class ConfigurationException : Exception
    {
        public DashLinkConfig Configuration { get; }

        public ConfigurationException(DashLinkConfig config) : base()
        {
            Configuration = config;
        }

        public ConfigurationException(DashLinkConfig config, string message) : base(message)
        {
            Configuration = config;
        }
    }
}
