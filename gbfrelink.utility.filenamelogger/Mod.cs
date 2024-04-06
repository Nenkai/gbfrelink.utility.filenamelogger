using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using gbfrelink.utility.filenamelogger.Template;
using gbfrelink.utility.filenamelogger.Configuration;

using Reloaded.Mod.Interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;
using gbfrelink.utility.filenamelogger.Hooks;

namespace gbfrelink.utility.filenamelogger;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    private static IStartupScanner? _startupScanner = null!;

    private unsafe delegate ulong XXHash32CustomDelegate(byte* name, ulong length);

    private static void SigScan(string pattern, string name, Action<nint> action)
    {
        var baseAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;
        _startupScanner?.AddMainModuleScan(pattern, result =>
        {
            if (!result.Found)
            {
                return;
            }
            action(result.Offset + baseAddress);
        });
    }

    public List<IHashLogger> _loggers;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        _loggers = new()
        {
            new XXHash64Logger("FileHasher", "filelist.txt", "41 57 41 56 41 55 41 54 56 57 55 53 49 BB 4F EB D4 27 3D AE B2 C2"),
            new XXHash32Logger("XXHash32Hasher", "hashlist.txt", "48 85 C9 0F 84 1B 02 00 00 4C 8D 04 11 B8 A4 54"),
            new CRC32HashLogger("CRC32NameHasher", "crclist.txt", "56 57 53 48 81 EC ?? ?? ?? ?? 48 89 CB E8"),
            new CRC32HashLogger("CRC32NameHasher2", "crclist2.txt", "B8 ?? ?? ?? ?? 48 83 FA ?? 0F 82 ?? ?? ?? ?? B8", toLower: false),
            new ReflectionAttributeLogger("ReflectionAttributeLogger", "attributes.txt", "55 41 57 41 56 41 54 56 57 53 48 83 EC ?? 48 8D 6C 24 ?? 48 C7 45 ?? ?? ?? ?? ?? 49 89 CF 48 8D 0D")
        };

        var startupScannerController = _modLoader.GetController<IStartupScanner>();
        if (startupScannerController == null || !startupScannerController.TryGetTarget(out _startupScanner))
        {
            return;
        }

        foreach (var logger in _loggers)
        {
            SigScan(logger.Pattern, "", address =>
            {
                _logger.WriteLine($"OK: {logger.FileName}");
                logger.CreateHook(_hooks, address);
            });
        }

        var thread = new Thread(WriteStuff);
        thread.Start();
    }

    private void WriteStuff()
    {
        DirectoryInfo dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
        while (true)
        {
            foreach (var logger in _loggers)
                logger.Flush(dir.FullName);

            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }


    #region Standard Overrides

    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
    }

    #endregion

    #region For Exports, Serialization etc.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod()
    {
    }
#pragma warning restore CS8618

    #endregion
}

