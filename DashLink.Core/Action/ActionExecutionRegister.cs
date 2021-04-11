using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Core.Action
{
    public class ActionExecutionRegister
    {
        public bool Enabled { get; set; }
        public string Identifier { get; }
        private readonly Dictionary<string, DelegatedAction> actionMap;

        public ActionExecutionRegister(string identifier)
        {
            Enabled = true;
            Identifier = identifier;
            actionMap = new Dictionary<string, DelegatedAction>();
        }

        public void Register(string actionId, DelegatedAction action)
        {
            actionMap.Add(actionId ?? throw new ArgumentNullException(nameof(actionId)), action ?? throw new ArgumentNullException(nameof(action)));
        }

        public void Unregister(string actionId)
        {
            actionMap.Remove(actionId ?? throw new ArgumentNullException(nameof(actionId)));
        }

        public void Clear()
        {
            actionMap.Clear();
        }

        public DelegatedAction TryGetAction(string evt)
        {
            if (actionMap.TryGetValue(evt, out DelegatedAction aer))
            {
                return aer;
            }
            return null;
        }
    }
}
