using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Core.Data
{
    public enum CommandType
    {
        Native,
        NativePrivileged,
        PowerShell,

        //Disabled: Could cause problems i.e. infinitely calling the same event
        //Internal,
        //EmulateInput
    }
}
