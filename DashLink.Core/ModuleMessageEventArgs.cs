using DashLink.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Core
{
    public class ModuleMessageEventArgs : EventArgs
    {
        public string ModuleName { get; }
        public ModuleMessageLevel Level { get; }
        public string Message { get; }
        public Exception ForwardedException { get; }

        public ModuleMessageEventArgs(IModule module, string message) : this(module, ModuleMessageLevel.Info, message, null)
        { }

        public ModuleMessageEventArgs(IModule module, ModuleMessageLevel level, string message) : this(module, level, message, null)
        { }

        public ModuleMessageEventArgs(IModule module, string message, Exception exception) : this(module, ModuleMessageLevel.Error, message, exception)
        { }

        public ModuleMessageEventArgs(IModule module, ModuleMessageLevel level, string message, Exception exception)
        {
            ModuleName = module.Name;
            Level = level;
            Message = message;
            ForwardedException = exception;
        }
    }
}
