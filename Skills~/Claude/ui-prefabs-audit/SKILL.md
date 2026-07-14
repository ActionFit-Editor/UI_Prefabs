---
name: ui-prefabs-audit
description: Audit UIPrefabsSO discovery, prefab references, missing scripts, duplicate menu paths, sample neutrality, and Foundation dependencies without creating settings, importing samples, or changing scenes and assets.
---

# Audit UI Prefabs

Keep project settings, samples, scenes, prefab assets, selection, and Package Manager state unchanged.

1. Read repository instructions and the package `README.md` and `AI_GUIDE.md`.
2. Resolve the exact absolute project or worktree. Record `git status --short`. When Unity object inspection is needed, require an already-running ready Editor and verify the exact connected path.
3. Discover settings only with direct read-only queries:
   - `AssetDatabase.LoadAssetAtPath<UIPrefabsSO>(UIPrefabsSettingsUtility.DefaultAssetPath)`;
   - `AssetDatabase.FindAssets("t:UIPrefabsSO")`;
   - `AssetDatabase.LoadAssetAtPath` for returned paths.

Do not call `UIPrefabsSettingsUtility.LoadOrCreate()` or the `Setting SO` menu.

4. Audit the canonical/legacy/multiple-settings outcome, the nine base fields, custom entries, blank references, duplicate normalized category/label menu paths, prefab asset paths, missing MonoBehaviours, and broken dependencies. Treat project-specific dependencies as informational for project-owned entries.
5. Inspect package `Samples~/Starter UI Prefabs` in place without importing it. Verify the expected nine prefabs and catalog exist, metadata is present, and serialized references do not point into `Assets/_Project` or other Cat-specific assets. If a sample is already imported, it may be inspected in place but must not be reimported.
6. Report settings selection, missing/duplicate entries, missing-script counts, dependency findings, sample inventory, and Foundation/package version alignment. Re-run `git status --short` and flag any unexpected durable change.

Never call `LoadOrCreate`, import or delete a sample, execute `GameObject/>>>UI_Prefab`, instantiate or save prefabs, change a Canvas or scene, reserialize assets, or run tests that import temporary sample assets from this read-only skill.
