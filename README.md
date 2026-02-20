# EventLogger — Valheim BepInEx Plugin

Server-side event logger for Valheim dedicated servers. Emits structured `[EventLog] TYPE key=value` log lines that can be parsed by external tools and dashboards.

## Project Structure

```
EventLogger/
  EventLoggerPlugin.cs          Core plugin — Awake, Update, helpers
  PluginConfig.cs               BepInEx config bindings
  Patches/
    ConnectionPatches.cs        PEER_CONNECT, PEER_DISCONNECT
    CombatPatches.cs            PLAYER_DEATH, BOSS_KILL, MOB_KILL
    GatheringPatches.cs         PICKUP, HONEY_EXTRACT, SAP_EXTRACT
    TamingPatches.cs            TAME
    CraftingPatches.cs          SMELT_*, FERMENTER_ADD/TAP, COOK_*
    WorldPatches.cs             EVENT_START/STOP, BOSS_SUMMON, CROP_GROWN, SLEEP
    PlayerPatches.cs            EAT, PING
    PortalPatches.cs            PORTAL_BUILD, PORTAL_DESTROY, PORTAL_RENAME
    TombstonePatches.cs         TOMBSTONE_CREATE, TOMBSTONE_PICKUP
  Scanners/
    FermenterScanner.cs         ZDO-based fermenter state tracking (FERMENTER_DONE)
    PortalScanner.cs            Event-driven portal inventory (PORTAL_LIST)
  thunderstore/                 Thunderstore/Nexus packaging files
```

## Building

### Prerequisites

Copy the required Valheim + BepInEx reference DLLs into a folder and set `ValheimRefs`:

```bash
# Default fallback (Windows):
#   %LOCALAPPDATA%\Temp\valheim-refs\

# Or set explicitly:
export ValheimRefs=/path/to/valheim-refs
```

Required DLLs: `BepInEx.dll`, `0Harmony.dll`, `Assembly-CSharp.dll`, `assembly_valheim.dll`, `assembly_utils.dll`, `UnityEngine.dll`, `UnityEngine.CoreModule.dll`, `UnityEngine.PhysicsModule.dll`

### Build

```bash
dotnet build -c Release
```

Output: `bin/Release/netstandard2.1/EventLogger.dll`
Release builds also copy the DLL to `thunderstore/plugins/`.

## Deploying

Copy `EventLogger.dll` to `BepInEx/plugins/` on the dedicated server and restart.

On first run, a config file is generated at `BepInEx/config/games.blockfactory.eventlogger.cfg` with all default settings.

## Configuration

All settings are hot-reloadable via BepInEx config. Categories:

| Section | Key settings |
|---------|-------------|
| **Features** | Toggle fermenter scanning, portal scanning |
| **Intervals** | Fermenter scan interval (10s) |
| **Events** | Toggle entire categories: combat, crafting, gathering, world, player; configurable MOB_KILL star threshold |

See `thunderstore/README.md` for the full config reference table.

## Event Format

Every event is a single log line:

```
[Info   :EventLogger] [EventLog] TYPE key1=value1 key2=value2 ...
```

The `[EventLog]` prefix and key=value format is a stable contract — downstream parsers depend on the exact format.

## Log Format Contract

These log lines may be parsed by external tools. Changing any of the following is a breaking change:

- The `[EventLog]` prefix on every event line
- Event type names (PLAYER_DEATH, PICKUP, BOSS_KILL, etc.)
- Key names within each event (player=, item=, amount=, etc.)
- Value formats (F0 for floats, comma-separated positions, pipe-separated portal data)

When adding new events, use the same pattern. Never rename or remove existing keys.
