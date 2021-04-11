using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Core.Action
{
    public enum DebounceMode
    {
        None, // All commands are immediately sent
        IgnoreSubsequent, // All subsequent commands of the same type are ignored from the second time it is seen
        IgnoreSubsequentUntilDelay, // Same as above, but the debounce timer is updated every time until the command isn't seen for awhile
        CountFromFirst, // There is a delay before the command is executed, counting up the occurances
        CountFromSecond // The first command is executed instantly, but subsequent commands are held back for a set duration
    }
}
