using DashLink.Core.Action;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Core
{
    public delegate void ModuleMessageHandler(object sender, ModuleMessageEventArgs args);

    public interface IModule
    {
        event ModuleMessageHandler ModuleMessageEvent;

        string Name { get; }
        void Start(DashLinkHost host);
        void Stop();
        void RegisterActions(ActionExecutionRegister aer);
    }
}
