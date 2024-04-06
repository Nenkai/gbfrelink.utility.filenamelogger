using Reloaded.Hooks.Definitions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using static gbfrelink.utility.filenamelogger.Hooks.XXHash32Logger;

namespace gbfrelink.utility.filenamelogger.Hooks;

public abstract class HashLoggerBase<T> : IHashLogger
{
    public string FileName { get; set; }
    public string Pattern { get; set; }
    public HashSet<string> Existing { get; set; } = new HashSet<string>();
    public string ToAdd { get; set; } = "";

    public IHook<T> Hook { get; set; }
    public T FuncHook { get; set; }

    public HashLoggerBase(string name, string fileName, string pattern)
    {
        FileName = fileName;
        Pattern = pattern;
    }

    public void CreateHook(IReloadedHooks hooks, nint address)
    {
        Hook = hooks!.CreateHook(FuncHook, address).Activate();
    }

    public void Init()
    {
        string dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        var hashNames = File.Exists(Path.Combine(dir, FileName))
            ? File.ReadAllText(Path.Combine(dir, FileName))
            : "";
        Existing = new HashSet<string>(hashNames.Split(new string[] { Environment.NewLine },
            StringSplitOptions.RemoveEmptyEntries));
    }

    public void Flush(string dir)
    {
        foreach (var path in ToAdd.Split(new[] { Environment.NewLine },
                     StringSplitOptions.RemoveEmptyEntries))
        {
            Existing.Add(path);
        }

        File.AppendAllText(Path.Combine(dir, FileName), ToAdd);
        ToAdd = "";
    }

    public void AddNotExist(string val)
    {
        if (Existing.Contains(val) || ToAdd.Contains(val)) 
            return;

        ToAdd += val + "\n";
    }

    public void Add(string val)
    {
        ToAdd += val + "\n";
    }
}
