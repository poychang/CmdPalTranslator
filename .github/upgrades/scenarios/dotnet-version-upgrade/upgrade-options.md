# Upgrade Options — CmdPalTranslator

Assessment: 3 projects (all SDK-style, modern .NET), 2 projects need TFM upgrade, 2 incompatible packages, 2 source-incompatible API changes.

## Strategy

### Upgrade Strategy
Solution is small (3 projects) and all projects are already on modern .NET, so a single coordinated upgrade pass is the default.

| Value | Description |
|-------|-------------|
| **All-at-Once** (selected) | Upgrade all projects in one atomic pass for fastest completion. |
| Top-Down | Upgrade app entry points first and temporarily multi-target shared libraries. |

## Project Structure

### Package Management
This solution has multiple projects but limited scale (3 projects, low package spread), so central package management can be deferred.

| Value | Description |
|-------|-------------|
| Central Package Management (CPM) | Create `Directory.Packages.props` and centralize package versions. |
| **Per-Project (defer CPM to post-migration)** (selected) | Keep package versions in each project during migration and revisit CPM after stabilization. |

## Compatibility

### Unsupported Packages
Assessment found 2 incompatible packages, which is a small set suitable for direct fix during the upgrade tasks.

| Value | Description |
|-------|-------------|
| **Resolve Inline** (selected) | Research and resolve each incompatible package in the same task without deferring work. |
| Defer Resolution | Keep buildability with temporary stubs and create follow-up resolution tasks. |
| Compatibility Mode | Keep framework reference compatibility temporarily for limited scenarios. |

### Unsupported API Handling
Assessment found source-incompatible API changes with limited count, so direct fixes in the same task are preferred.

| Value | Description |
|-------|-------------|
| **Fix Inline** (selected) | Resolve API changes directly in upgrade tasks, including complex replacements if needed. |
| Defer Complex Changes | Apply simple fixes now and defer complex API replacements with temporary stubs. |
