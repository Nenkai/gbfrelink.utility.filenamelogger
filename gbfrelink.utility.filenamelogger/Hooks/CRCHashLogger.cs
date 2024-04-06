using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Reloaded.Hooks.Definitions;

namespace gbfrelink.utility.filenamelogger.Hooks;

public unsafe delegate ulong CRCHashDelegate(byte* name, ulong length);

public class CRC32HashLogger : HashLoggerBase<CRCHashDelegate>
{
    private bool _toLower;

    public unsafe CRC32HashLogger(string name, string fileName, string pattern, bool toLower = true)
        : base(name, fileName, pattern)
    {
        _toLower = toLower;
        FuncHook = CRC32NameHasher;
    }

    private unsafe ulong CRC32NameHasher(byte* name, ulong length)
    {
        var buffer = new byte[0x200];

        int i = 0;
        while (true)
        {
            if (name[i] == 0)
                break;

            buffer[i] = name[i];
            i++;
        }

        var str = Encoding.UTF8.GetString(buffer).TrimEnd('\0');
        uint hash = CRC32.crc32_0x77073096(str);

        AddNotExist($"{hash:X8}|{str}");

        return Hook.OriginalFunction(name, length);
    }
}
