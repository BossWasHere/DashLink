using DashLink.Core.Action;
using DashLink.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Core.Builtin
{
    public sealed class ProfileModule : DelegatedAction, IModule
    {
        public string Name => "profile";

        private DashLinkHost host;

        public event ModuleMessageHandler ModuleMessageEvent;

        public void RegisterActions(ActionExecutionRegister aer)
        {
            aer.Register(Name + ".switch", this);
        }

        public void Start(DashLinkHost host)
        {
            this.host = host;
        }

        public void Stop()
        { }

        public override DebounceMode GetDebounceMode()
        {
            return DebounceMode.None;
        }

        public override int GetDebounceTime()
        {
            return 0;
        }

        public override Task<bool> GetInvokeAction(string data)
        {
            return new Task<bool>(() =>
            {
                ModuleMessageEvent?.Invoke(this, new ModuleMessageEventArgs(this, ModuleMessageLevel.Info, "Switching profile to " + (string.IsNullOrEmpty(data) ? "[Default]" : data)));
                return (string.IsNullOrEmpty(data) ? host.SwitchDefaultProfile() : host.SwitchProfile(data)) != null;
            });
        }
    }
}
