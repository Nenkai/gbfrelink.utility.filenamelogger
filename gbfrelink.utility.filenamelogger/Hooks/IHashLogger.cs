using Reloaded.Hooks.Definitions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gbfrelink.utility.filenamelogger.Hooks;

public interface IHashLogger
{
    public string FileName { get; set; }
    public string Pattern { get; set; }
    public HashSet<string> Existing { get; set; }
    public string ToAdd { get; set; }

    public void CreateHook(IReloadedHooks hooks, nint address);
    public void Flush(string dir);
}
