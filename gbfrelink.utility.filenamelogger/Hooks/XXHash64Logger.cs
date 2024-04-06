using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Reloaded.Hooks.Definitions;

namespace gbfrelink.utility.filenamelogger.Hooks;

public unsafe delegate ulong XXHash64Delegate(byte* name, ulong length, ulong key);

public class XXHash64Logger : HashLoggerBase<XXHash64Delegate>
{
    public unsafe XXHash64Logger(string name, string fileName, string pattern)
        : base(name, fileName, pattern)
    {
        FuncHook = FileHash;
    }

    public unsafe ulong FileHash(byte* name, ulong length, ulong key)
    {
        if (length > 260) 
            return Hook.OriginalFunction(name, length, key);

        try
        {
            var buffer = new byte[length];

            for (ulong i = 0; i < length; i++)
            {
                buffer[i] = name[i];
            }

            var filename = buffer.BytesToString();

            if (!filename.All(char.IsAscii)) return Hook.OriginalFunction(name, length, key);
            AddNotExist(filename);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Hook.OriginalFunction(name, length, key);
        }

        return Hook.OriginalFunction(name, length, key);
    }
}
