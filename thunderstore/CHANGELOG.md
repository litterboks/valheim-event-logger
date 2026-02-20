# Changelog

## 3.0.0

### Added
- **BepInEx config system** — all features, intervals, thresholds, and event categories are now configurable via `BepInEx/config/games.blockfactory.eventlogger.cfg`
- Feature toggles: disable RPC sniffer, stats aggregation, alerts, fermenter scanning, or portal scanning independently
- Event category toggles: disable combat, crafting, gathering, world, or player events independently
- Configurable intervals for stats flush, fermenter scan, portal scan, distance check, and RPC report
- Configurable alert thresholds and windows for silver rush, mass kill, and heavy damage alerts
- Thunderstore packaging support

### Fixed
- Duplicate `FERMENTER_ADD` log (scanner and patch both fired — now only the patch logs, with correct player attribution)
- Empty catch blocks in RPC sniffer now log warnings instead of silently swallowing errors
- `KnownPlayers` dictionary leak on player disconnect (entries are now cleaned up)
- `lastPositions` tracking leak on player disconnect

### Changed
- RPC sniffer is now **off by default** (enable via config if needed)
- Codebase restructured from single 938-line file into focused modules under `Patches/` and `Scanners/`
- Reference paths in `.csproj` are now portable via `$(ValheimRefs)` environment variable
- Version bumped to 3.0.0
