# EventLogger

Server-side event logger for Valheim dedicated servers. Logs 30+ event types in a structured `[EventLog] TYPE key=value` format designed for parsing by external tools and dashboards.

## Features

- **Connection tracking** — player connect/disconnect with UID
- **Combat events** — player deaths (with killer attribution), boss kills, starred mob kills
- **Gathering** — pickable item tracking, honey extraction, sap extraction
- **Taming** — creature tame events with player attribution
- **Crafting** — smelter fuel/ore/done, fermenter add/done/tap, cooking add/done
- **World events** — raid start/stop, boss summoning, crop growth, sleep cycles
- **Player activity** — food consumption, map pings
- **Portal scanning** — periodic portal inventory with tags and positions
- **Fermenter scanning** — ZDO-based fermenter state tracking with player attribution
- **Stats aggregation** — periodic per-player stats (kills, damage, distance, pickups, etc.)
- **Alert system** — configurable alerts for silver rushes, mass kills, heavy damage
- **RPC sniffer** — optional network-level damage tracking (off by default)

## Installation

### Manual
1. Copy `EventLogger.dll` into `BepInEx/plugins/` on your dedicated server
2. Restart the server
3. (Optional) Edit `BepInEx/config/games.blockfactory.eventlogger.cfg` to customize settings

### Thunderstore
Install via Thunderstore mod manager or `r2modman`.

## Event Reference

| Event | Format |
|-------|--------|
| `PEER_CONNECT` | `player=X uid=N` |
| `PEER_DISCONNECT` | `player=X uid=N` |
| `PLAYER_DEATH` | `player=X killed_by=Y biome=Z pos=X,Y,Z` |
| `BOSS_KILL` | `boss=X killed_by=Y pos=X,Y,Z` |
| `MOB_KILL` | `player=X mob=Y stars=N pos=X,Y,Z` |
| `TOMBSTONE_CREATE` | `player=X biome=Y pos=X,Y,Z` |
| `TOMBSTONE_PICKUP` | `player=X pos=X,Y,Z` |
| `TAME` | `player=X creature=Y` |
| `EVENT_START` | `event=X` |
| `EVENT_STOP` | *(no params)* |
| `BOSS_SUMMON` | `boss=X player=Y pos=X,Y,Z` |
| `CROP_GROWN` | `plant=X [planted_by=Y]` |
| `SLEEP_START` | *(no params)* |
| `SLEEP_STOP` | *(no params)* |
| `EAT` | `player=X food=Y` |
| `PING` | `player=X x=N z=N` |
| `SMELT_FUEL` | `player=X station=Y item=Z` |
| `SMELT_ORE` | `player=X station=Y item=Z` |
| `SMELT_DONE` | `station=X item=Y count=N` |
| `FERMENTER_ADD` | `player=X item=Y` |
| `FERMENTER_DONE` | `player=X item=Y` |
| `FERMENTER_TAP` | `player=X item=Y` |
| `COOK_ADD` | `player=X item=Y` |
| `COOK_DONE` | `player=X item=Y` |
| `HONEY_EXTRACT` | `player=X` |
| `SAP_EXTRACT` | `player=X` |
| `PORTAL_BUILD` | `player=X pos=X,Z` |
| `PORTAL_DESTROY` | `tag=X pos=X,Z` |
| `PORTAL_RENAME` | `player=X tag=Y pos=X,Z` |
| `PORTAL_LIST` | `portals=tag\|x\|z,...` |
| `STATS` | `player=X kills=N damage_dealt=N ...` |
| `MOB_DMG` | `player=X mob=Y damage=N` |
| `ALERT` | `player=X type=Y detail=Z count=N window=Nm` |
| `RPC_SNIFFER` | `damage_rpcs=N total_rpcs=N` |
| `ZDO_RPC` | `hash=N count=N` |

## Configuration

After first run, edit `BepInEx/config/games.blockfactory.eventlogger.cfg`:

### Features
| Setting | Default | Description |
|---------|---------|-------------|
| `EnableRpcSniffer` | `false` | Enable RPC sniffer for damage tracking via network packets |
| `EnableStatsAggregation` | `true` | Enable periodic stats aggregation and flush |
| `EnableAlerts` | `true` | Enable alert system for unusual activity |
| `EnableFermenterScanning` | `true` | Enable periodic fermenter ZDO scanning |
| `EnablePortalScanning` | `true` | Enable periodic portal ZDO scanning |

### Intervals
| Setting | Default | Description |
|---------|---------|-------------|
| `StatsFlushInterval` | `60` | Seconds between stats flushes |
| `FermenterScanInterval` | `10` | Seconds between fermenter scans |
| `PortalScanInterval` | `60` | Seconds between portal scans |
| `DistanceCheckInterval` | `5` | Seconds between player distance checks |
| `RpcReportInterval` | `300` | Seconds between RPC sniffer reports |

### Alerts
| Setting | Default | Description |
|---------|---------|-------------|
| `SilverRushThreshold` | `10` | SilverOre pickups to trigger alert |
| `SilverRushWindowMinutes` | `10` | Time window for silver rush detection |
| `MassKillThreshold` | `20` | Kills to trigger alert |
| `MassKillWindowMinutes` | `5` | Time window for mass kill detection |
| `HeavyDamageThreshold` | `500` | Cumulative damage to trigger alert |
| `HeavyDamageWindowMinutes` | `2` | Time window for heavy damage detection |
| `CooldownMinutes` | `15` | Shared cooldown between alerts |

### Events
| Setting | Default | Description |
|---------|---------|-------------|
| `MobKillMinStars` | `1` | Min star level for MOB_KILL (0=all, 1=1-star+, 2=2-star+) |
| `EnableCombatEvents` | `true` | PLAYER_DEATH, BOSS_KILL, MOB_KILL |
| `EnableCraftingEvents` | `true` | SMELT_*, FERMENTER_*, COOK_* |
| `EnableGatheringEvents` | `true` | Pickups, HONEY_EXTRACT, SAP_EXTRACT |
| `EnableWorldEvents` | `true` | EVENT_START/STOP, BOSS_SUMMON, CROP_GROWN, SLEEP, TAME |
| `EnablePlayerEvents` | `true` | EAT, PING |
