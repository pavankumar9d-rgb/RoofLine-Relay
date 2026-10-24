# Roofline Relay — Workspace Agent Rules & Guidelines

## 1. Project Root & Identity
- **Project Root:** `C:\Users\saipa\OneDrive\Desktop\Roofline Relay`
- **Remote Origin:** `https://github.com/pavankumar9d-rgb/RoofLine-Relay.git`
- **Default Branch:** `main`

## 2. Version Control Protocols
All development tasks in this workspace must adhere to `GIT_WORKFLOW.md`:
1. Always check `git status` before beginning work.
2. Read existing files and preserve existing assets before making modifications.
3. Keep atomic, descriptive local commits on `main` after each verified task.
4. Never stage credentials, secrets, IDE caches, or temporary build artifacts.
5. Never execute `git reset --hard` or destructive operations on user code.
6. **STRICT RULE: NEVER automatically run `git push` or remote-modifying commands.** Only output the exact push command when requested by the user.

## 3. Asset & Directory Integrity
- Never delete, overwrite, or move existing verified 3D assets (`.glb`) or reference images (`.png`) without explicit user sign-off.
- All 3D assets must be logged in `docs/asset-audit/MODEL_INVENTORY.csv` and `docs/asset-audit/MODEL_INVENTORY.md`.
- Reference concepts and prompts must be cataloged in `docs/art/3d-reference-library/08_manifest_and_prompts/`.
