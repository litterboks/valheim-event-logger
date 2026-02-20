# Changelog

## 4.0.0

### Added
- **PICKUP event** — new `PICKUP player=X item=Y amount=N` event for all pickable items (berries, mushrooms, ore, etc.)

### Removed
- **Stats aggregation** — removed periodic per-player stats (kills, damage, distance, pickups, etc.). Aggregation is better handled by external tools parsing the event logs.
- **Alert system** — removed silver rush, mass kill, and heavy damage alerts. Same reasoning — alerting belongs in the consuming application.
- **RPC sniffer** — removed network-level damage tracking (`MOB_DMG`, `RPC_SNIFFER`, `ZDO_RPC` events)
- **Distance tracking** — removed player distance measurement that fed into stats

### Changed
- Config file simplified: removed all stats, alert, RPC, and distance-related settings
- Plugin is now purely an event logger — no aggregation or analysis

## 3.2.0

### Added
- **Biome on player death** — `PLAYER_DEATH` now includes `biome=` field (Meadows, BlackForest, etc.)
- **Tombstone events** — `TOMBSTONE_CREATE` logged on death with biome and position, `TOMBSTONE_PICKUP` logged when items are recovered
- **Stats flush on disconnect** — player stats are immediately flushed when they disconnect, so the last session's data is never lost

## 3.1.0

### Added
- **BOSS_KILL player attribution** — `killed_by=` field now shows who landed the killing blow
- **Portal events** — new `PORTAL_BUILD`, `PORTAL_DESTROY`, `PORTAL_RENAME` events with player/tag/position
- **Configurable MOB_KILL threshold** — `MobKillMinStars` setting (0=all kills, 1=1-star+ default, 2=2-star+)

### Changed
- **Event-driven portal scanning** — replaces 60-second polling; scans on startup and portal build/destroy/rename with 2s debounce
- **Optimized fermenter scanner** — initial full scan once, then only checks tracked fermenters instead of iterating all ZDOs

## 3.0.0

### Added
- **BepInEx config system** — all features, intervals, thresholds, and event categories are now configurable via `BepInEx/config/games.blockfactory.eventlogger.cfg`
- Feature toggles: disable fermenter scanning or portal scanning independently
- Event category toggles: disable combat, crafting, gathering, world, or player events independently
- Thunderstore packaging support

### Fixed
- Duplicate `FERMENTER_ADD` log (scanner and patch both fired — now only the patch logs, with correct player attribution)
- `KnownPlayers` dictionary leak on player disconnect (entries are now cleaned up)

### Changed
- Codebase restructured from single 938-line file into focused modules under `Patches/` and `Scanners/`
- Reference paths in `.csproj` are now portable via `$(ValheimRefs)` environment variable
