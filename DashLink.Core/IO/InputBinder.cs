using DashLink.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Core.IO
{
    public class InputBinder
    {
        private Dictionary<string, List<string>> bindingPairs;

        public InputBinder()
        {
            bindingPairs = new Dictionary<string, List<string>>();
        }

        public void Populate(ICollection<Binding> bindings)
        {
            foreach (Binding binding in bindings)
            {
                bindingPairs.Add(binding.Event, binding.Commands);
            }
        }

        public IEnumerable<string> GetBindings(string eventId)
        {
            return bindingPairs.TryGetValue(eventId, out List<string> v) ? v : Enumerable.Empty<string>();
        }

        public void Reset()
        {
            bindingPairs.Clear();
        }
    }
}
