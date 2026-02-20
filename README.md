# EventLogger — Valheim BepInEx Plugin

Server-side event logger for Valheim dedicated servers. Emits structured `[EventLog] TYPE key=value` log lines that can be parsed by external tools and dashboards.

## Project Structure

```
EventLogger/
  EventLoggerPlugin.cs          Core plugin — Awake, Update, helpers
  PluginConfig.cs               BepInEx config bindings (30 settings)
  StatsAggregator.cs            Per-player stats aggregation + alert system
  RpcSniffer.cs                 Optional RPC-level damage tracking
  Patches/
    ConnectionPatches.cs        PEER_CONNECT, PEER_DISCONNECT
    CombatPatches.cs            PLAYER_DEATH, BOSS_KILL, MOB_KILL
    GatheringPatches.cs         Pickable tracking, HONEY_EXTRACT, SAP_EXTRACT
    TamingPatches.cs            TAME
    CraftingPatches.cs          SMELT_*, FERMENTER_ADD/TAP, COOK_*
    WorldPatches.cs             EVENT_START/STOP, BOSS_SUMMON, CROP_GROWN, SLEEP
    PlayerPatches.cs            EAT, PING
  Scanners/
    FermenterScanner.cs         ZDO-based fermenter state tracking (FERMENTER_DONE)
    PortalScanner.cs            Periodic portal inventory (PORTAL_LIST)
    PlayerTracker.cs            Player distance tracking for stats
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
| **Features** | Toggle RPC sniffer (off by default), stats, alerts, fermenter/portal scanning |
| **Intervals** | Stats flush (60s), fermenter scan (10s), portal scan (60s), distance check (5s), RPC report (300s) |
| **Alerts** | Thresholds/windows for silver_rush, mass_kill, heavy_damage; shared cooldown (15m) |
| **Events** | Toggle entire categories: combat, crafting, gathering, world, player |

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
- Event type names (PLAYER_DEATH, STATS, ALERT, etc.)
- Key names within each event (player=, kills=, damage_dealt=, etc.)
- Value formats (F0 for floats, comma-separated positions, pipe-separated portal data)

When adding new events, use the same pattern. Never rename or remove existing keys.
