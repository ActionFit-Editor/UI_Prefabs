---
name: ui-prefabs-help
description: Explain UI Prefabs, its installed skills, project-owned UIPrefabsSO settings, Foundation dependency, starter samples, authoring menus, and read-only audit boundaries.
---

# UI Prefabs Help

Answer in the user's language. Explain workflows without creating settings, importing samples, instantiating prefabs, changing scenes, or editing serialized assets unless the user separately requests those operations.

1. Read `PACKAGE_SKILLS.md` first. Treat its generated package identity, complete related-skill table, `$skill-name` invocations, descriptions, and access values as authoritative.
2. Read `Packages/com.actionfit.ui.prefabs/README.md` and `AI_GUIDE.md` when available. If downloaded, resolve `Library/PackageCache/com.actionfit.ui.prefabs@*` without editing it.
3. Explain the one-way `com.actionfit.ui.foundation` dependency, project-owned `UIPrefabsSO`, canonical and legacy lookup behavior, base/custom entries, prefab-link-preserving authoring menus, and neutral starter samples.
4. Separate the read-only audit from `Setting SO`, Package Manager sample import, and `GameObject > >>>UI_Prefab` creation commands.
5. List `Setting SO` and `README` under `Tools > Package > UI Prefabs`, plus the `GameObject > >>>UI_Prefab` authoring menu.

State that the audit never creates or moves a settings asset, imports samples, instantiates a prefab, saves a scene, or changes serialized fields.
