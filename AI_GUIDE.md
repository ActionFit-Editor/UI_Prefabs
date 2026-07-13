# AI Guide - UI Prefabs

## Package Identity

- Package ID: `com.actionfit.ui.prefabs`
- Display name: UI Prefabs
- Repository: `https://github.com/ActionFit-Editor/UI_Prefabs.git`
- Repository visibility metadata: `Public`
- Current package version at generation time: `1.0.0`
- Unity version: `6000.2`

## Purpose And Boundary

This optional Editor package adds prefab authoring menus, a project-owned `UIPrefabsSO`, and neutral starter samples on top of `com.actionfit.ui.foundation`. It must depend on Foundation in one direction; Foundation must never reference this package.

The package does not own Cat Merge Cafe art, fonts, reward extensions, button service adapters, or the nine project-specific example prefabs. Those remain under project `Assets`.

## Project Router Registration

Requested router entry:

- `Packages/com.actionfit.ui.prefabs/AI_GUIDE.md` - UI Prefabs owns Foundation-based prefab authoring menus, project-owned `UIPrefabsSO` settings, and neutral starter samples. Read when changing those menus, settings lookup, samples, package metadata, tests, or release behavior.

Read this guide before changing files under `Packages/com.actionfit.ui.prefabs/`, diagnosing its behavior, or preparing a release.

## Architecture Contracts

- `package.json` must depend on `com.actionfit.ui.foundation` and expose `Samples~/Starter UI Prefabs` through its `samples` array.
- `com.actionfit.ui.prefabs.Editor` is Editor-only and references `com.actionfit.ui.foundation`.
- `UIPrefabsSO`, `NewUIPrefabObject`, and `UIPrefabsSettingsUtility` remain global types. Do not add a namespace without an explicit consumer migration.
- Preserve the existing `UIPrefabsSO` and `NewUIPrefabObject` script GUIDs and serialized field names.
- Settings are owned by the consuming project at `Assets/Editor/ActionFit/UI Prefabs/UIPrefabsSO.asset`, never inside the immutable package directory or `Resources`.
- The canonical settings path wins. A single legacy `UIPrefabsSO` may be used as fallback; multiple noncanonical assets must return no selection and warn.
- Prefab creation uses `PrefabUtility.InstantiatePrefab`, requires a selected Canvas descendant, preserves the prefab link, and registers Undo.

## Sample Rules

- Keep samples neutral: only Unity built-ins, UGUI/TMP, Foundation components, and files within this package may be dependencies.
- Do not reference `Assets/_Project`, Cat Merge Cafe adapters, addressables, game fonts, sprites, audio, or data types.
- Keep Image, Text, Button, Input, InputBtn, Scroll, Mask, Mask2D, Fill prefabs and the sample catalog internally consistent.
- Generate or edit prefab YAML through Unity serialization APIs. Preserve `.meta` GUIDs after creation.

## Package Menus

- `Tools/Package/UI Prefabs/README`
- `Tools/Package/UI Prefabs/Setting SO`
- `GameObject/>>>UI_Prefab/...`

## Validation

- Run the package contract validator for `com.actionfit.ui.prefabs`.
- Run `com.actionfit.ui.prefabs.Editor.Tests` in Unity.
- Verify sample prefabs have no missing scripts and no Cat project dependencies.
- Compile in an isolated fixture containing this package and Foundation.

## Metadata And Release

- `package.json` is the source for ID, version, Unity version, dependencies, and samples.
- `Editor/PackageInfo/ActionFitPackageInfo_SO.asset` is the source for catalog metadata; `_repositoryVisibility: 0` means Public.
- Release notes describe only the current version and are written in Korean.
- Publishing is manual through Custom Package Manager. Public metadata does not authorize remote repository creation, tagging, catalog updates, or publishing.
- Published tags are immutable; bump to the next unused version after a tag exists.
