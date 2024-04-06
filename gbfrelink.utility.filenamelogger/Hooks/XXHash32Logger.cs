using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Reloaded.Hooks.Definitions;

namespace gbfrelink.utility.filenamelogger.Hooks;

public unsafe delegate ulong XXHash32CustomDelegate(byte* name, ulong length);

public class XXHash32Logger : HashLoggerBase<XXHash32CustomDelegate>
{
    public unsafe XXHash32Logger(string name, string fileName, string pattern)
        : base(name, fileName, pattern)
    {
        FuncHook = XXHash32Hasher;
    }

    public unsafe ulong XXHash32Hasher(byte* name, ulong length)
    {
        var buffer = new byte[length];

        for (ulong i = 0; i < length; i++)
            buffer[i] = name[i];

        var filename = buffer.BytesToString();

        uint hash = XXHash32Custom.Hash(name, (int)length);
        AddNotExist($"{hash:X8}|{filename}");

        return Hook.OriginalFunction(name, length);
    }
}
