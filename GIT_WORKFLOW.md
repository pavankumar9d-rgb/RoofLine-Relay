# Roofline Relay — Git & Version Control Workflow

This document establishes the mandatory version-control protocols for all contributors and automated agents working on **Roofline Relay**. These rules must be followed for every session and development milestone.

---

## Repository Specifications

- **Project Root Directory:** `C:\Users\saipa\OneDrive\Desktop\Roofline Relay`
- **Remote Origin:** `https://github.com/pavankumar9d-rgb/RoofLine-Relay.git`
- **Default Branch:** `main`

---

## 14 Mandatory Workflow Rules

1. **Root Directory Constraint:** Always operate strictly within `C:\Users\saipa\OneDrive\Desktop\Roofline Relay` as the project root. Never write project files to temporary directories, Desktop root, or external user paths.
2. **Reconnaissance First:** Always check `git status` and verify existing tracking state before beginning any task or creating/modifying files.
3. **Preserve Existing Work:** Read and understand existing files, structures, and assets before making changes. Never overwrite, rename, or delete existing verified assets without explicit instructions.
4. **Commit After Validated Tasks:** After each meaningful, completed, and validated unit of work, review the changes and create a clean local commit.
5. **Standardized Commit Messages:** Use concise, descriptive conventional commit messages:
   - `feat: <description>` (new gameplay features, mechanics, or systems)
   - `docs: <description>` (documentation, inventories, style guides)
   - `assets: <description>` (3D models, textures, audio additions)
   - `refactor: <description>` (code/structure cleanups without behavior changes)
   - `fix: <description>` (bug fixes and corrections)
   - `chore: <description>` (repository setup, config, tooling)
6. **Zero Leakage / Clean Staging:** Never stage or commit secrets, credentials, API keys, private tokens, OS artifacts (`Thumbs.db`), IDE caches, engine temporary caches (`Library/`, `.godot/`), or build executables.
7. **Diff Inspection:** Review `git status`, staged filenames, and `git diff --cached` thoroughly prior to finalizing every commit.
8. **Validation Before Committing:** Run relevant tests, linters, or asset verifications before committing. Explicitly report any tests that could not be run.
9. **Never Discard User Changes:** Never execute `git reset --hard`, destructive rebases, unexplained file deletions, or force operations that risk data loss.
10. **Zero Automatic Remote Pushes:** **NEVER execute `git push`, `git push -u origin main`, or any remote-writing command automatically.** All remote writes require explicit user review and authorization.
11. **Immutable Commit History:** Never amend (`git commit --amend`) or rewrite historical commits without explicit user permission.
12. **Main Branch Continuity:** Keep completed task commits on the existing `main` branch unless the user explicitly requests or approves a feature-branch workflow.
13. **Milestone Reporting:** At the completion of each milestone or session, provide a summary including the commit hash, changed files, and validation results.
14. **User-Gated Remote Upload:** When the user indicates that work is ready to upload, provide the exact PowerShell command required for the user to push accumulated local commits to GitHub.

---

## Standard Push Command (When Ready)

When authorized by the user to publish local commits to the remote GitHub repository, run the following command in PowerShell from the project root:

```powershell
git push -u origin main
```
