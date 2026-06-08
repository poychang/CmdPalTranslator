# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net10.0

## Source Control
- **Source Branch**: main
- **Working Branch**: main
- **Commit Strategy**: Single Commit at End
- **Branch Sync**: Auto (Merge)

## Upgrade Options
**Source**: .github/upgrades/scenarios/dotnet-version-upgrade/upgrade-options.md

### Strategy
- Upgrade Strategy: All-at-Once

### Project Structure
- Package Management: Per-Project (defer CPM to post-migration)

### Compatibility
- Unsupported Packages: Resolve Inline (2 incompatible packages)
- Unsupported API Handling: Fix Inline

## Strategy
**Selected**: All-At-Once
**Rationale**: 3 projects, all SDK-style and already on modern .NET (net9/net10), with low migration complexity and shallow dependency graph.

### Execution Constraints
- Single atomic upgrade: update all target projects together before final validation.
- No tiered/phased ordering; avoid bottom-up or top-down sequencing for this solution.
- Complete project/PackageReference/API fixes in one bounded pass, then run full solution validation.
- Defer CPM adoption to post-migration cleanup; keep per-project package versions during this upgrade.
