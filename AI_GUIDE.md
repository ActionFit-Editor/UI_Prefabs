# AI Guide - UI Prefabs

## Package Identity

- Package ID: `com.actionfit.ui.prefabs`
- Display name: UI Prefabs
- Repository: `https://github.com/ActionFit-Editor/UI_Prefabs.git`
- Repository visibility metadata: `Public`
- Current package version at generation time: `2.0.2`
- Unity version: `6000.2`

## Purpose And Boundary

### Settings SO Lifecycle

- `UIPrefabsSO` is registered as `EditorOnly` with canonical project path `Assets/_Data/_UI Prefabs/UIPrefabsSO.asset`.
- `Assets/Editor/ActionFit/UI Prefabs/UIPrefabsSO.asset` remains a declared legacy candidate. Existing prefab references and asset GUIDs are preserved.
- The shared provider replaces the package's repeated type search and caches resolution; duplicate candidates block automatic selection or creation.

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
- Settings are owned by the consuming project at `Assets/_Data/_UI Prefabs/UIPrefabsSO.asset`; the old `Assets/Editor/ActionFit/UI Prefabs/UIPrefabsSO.asset` path remains a legacy candidate, and neither path is inside the immutable package directory or `Resources`.
- The canonical settings path wins. A single legacy `UIPrefabsSO` may be used as fallback; multiple noncanonical assets must return no selection and warn.
- Prefab creation uses `PrefabUtility.InstantiatePrefab`, requires a selected Canvas descendant, preserves the prefab link, and registers Undo.
- `GameObject/>>>UI_Prefab/Add UI Prefab...` always opens the authoring window. The window may save an explicitly selected Canvas-descendant scene object as a new prefab asset or register an existing prefab asset, then appends one Custom entry without replacing existing settings.
- `UIPrefabsSO.Custom` entries generate direct `GameObject/>>>UI_Prefab/<Category>/<Label>` commands in the project-owned `Assets/Editor/ActionFitUIPrefabsGeneratedMenuItems.cs`. Registration, Inspector edits, settings asset import, and Editor reload schedule deterministic synchronization; never hand-edit the generated file.
- Generated commands resolve prefabs by asset GUID, call `PrefabUtility.InstantiatePrefab` through `NewUIPrefabObject`, preserve Undo, and remain disabled unless the current selection is a Canvas or its descendant.
- When the selected settings has no Custom entries, an existing generated file is rewritten to an empty menu class so stale commands disappear. Do not create a generated file when no project-owned settings or generated output exists.
- Canceling the prefab save dialog must leave `UIPrefabsSO` unchanged. Registering a duplicate Category/Label menu path must fail with visible guidance.

## Sample Rules

- Keep samples neutral: only Unity built-ins, UGUI/TMP, Foundation components, and files within this package may be dependencies.
- Do not reference `Assets/_Project`, Cat Merge Cafe adapters, addressables, game fonts, sprites, audio, or data types.
- Keep Image, Text, Button, Input, InputBtn, Scroll, Mask, Mask2D, Fill prefabs and the sample catalog internally consistent.
- Every sample `UI_Button` must use Foundation 2.0 directly and must not carry a native `UnityEngine.UI.Button` or same-GameObject `UIButtonPressEffect`.
- Generate or edit prefab YAML through Unity serialization APIs. Preserve `.meta` GUIDs after creation.
- Version `2.0.2` enables TMP Extra Padding in every Starter UI Prefabs sample while preserving sample hierarchy, references, text, materials, and GUIDs.

## Package Menus

- `Tools/Package/UI Prefabs/README`
- `Tools/Package/UI Prefabs/Setting SO`
- `GameObject/>>>UI_Prefab/...`

## Agent Skills

- `Skills~/manifest.json` uses schema v2 with the unique `ui-prefabs` prefix.
- `ui-prefabs-help` and `ui-prefabs-audit` are read-only for Codex and Claude.
- Audits must discover existing settings through direct `AssetDatabase` queries and must not call `LoadOrCreate`, import samples, execute prefab creation menus, save assets, or reserialize.
- Package samples may be inspected in place for inventory, missing scripts, metadata, and neutral dependencies; do not import them during a read-only audit.
- The installed help skill must read generated `PACKAGE_SKILLS.md`; do not author that reserved file in package sources.

## Validation

- Run the package contract validator for `com.actionfit.ui.prefabs`.
- Run `com.actionfit.ui.prefabs.Editor.Tests` in Unity.
- Verify generated Custom menu source is deterministic, skips invalid or duplicate paths, and compiles in the consuming project Editor assembly.
- Verify sample prefabs have no missing scripts, no Cat project dependencies, and no legacy native Button/press-effect pair on `UI_Button` objects.
- Compile in an isolated fixture containing this package and Foundation.

## Metadata And Release

- `package.json` is the source for ID, version, Unity version, dependencies, and samples.
- `Editor/PackageInfo/ActionFitPackageInfo_SO.asset` is the source for catalog metadata; `_repositoryVisibility: 0` means Public.
- Release notes describe only the current version and are written in Korean.
- Publishing is manual through Custom Package Manager. Public metadata does not authorize remote repository creation, tagging, catalog updates, or publishing.
- Published tags are immutable; bump to the next unused version after a tag exists.
