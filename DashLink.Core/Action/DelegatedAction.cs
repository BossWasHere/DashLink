using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Core.Action
{
    public abstract class DelegatedAction
    {
        public bool Running { get; internal set; }
        public bool LastResult { get; internal set; }
        public long LastExecution { get; internal set; }
        public int TimesToExecute { get; internal set; }
        public abstract DebounceMode GetDebounceMode();
        public abstract int GetDebounceTime();
        public abstract Task<bool> GetInvokeAction(string data);
    }
}
