using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

namespace CMDLets
{

    [Cmdlet(VerbsCommon.Get,"Launcher",RemotingCapability=RemotingCapability.PowerShell)]
    public class Launcher : PSCmdlet
    {
    }
}
