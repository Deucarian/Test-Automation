# Deucarian Test Automation Agent Notes

Package ID: `com.deucarian.test-automation`
Repository: `Deucarian/Test-Automation`

Follow the canonical Deucarian governance docs in [Package Registry](https://github.com/Deucarian/Package-Registry/blob/main/ARCHITECTURE.md), especially capability ownership and dependency rules.

## Ownership

This package owns:

- Editor-only Unity batch test runner entry points, durable EditMode/PlayMode result file writing, command-line option parsing for package validation projects, and fallback test execution helpers.

Registered capabilities:
- None.

This package must not own:

- Package metadata governance, Package Registry validation policy, GitHub Actions orchestration, Unity project generation, runtime gameplay tests, generic CI frameworks, package installation, or player-build runtime behavior.

## Dependencies

Allowed dependency shape:

- Dependency-free editor tooling package.
- May reference Unity Test Framework assemblies through Unity's `TestAssemblies` optional reference to drive editor test execution.

Required dependencies and why:

- None.

Optional/version-defined dependencies:

- None.

Architecture exceptions:

- The editor runner asmdef intentionally has `optionalUnityReferences: ["TestAssemblies"]`; the shared validator treats it as test-scoped because it exists only to execute Unity tests from automation.

## Policies

- Keep this package editor-only and out of player builds.
- Do not add runtime assemblies or dependencies without a governance update.
- Do not duplicate Package Registry validation rules here; this package runs Unity tests and writes durable results.
- Logging: Do not introduce direct Unity Debug calls.
- Unity object lifetime: Use Common only if production code directly owns transient Unity object cleanup.
- Testing: Keep command parsing and durable result paths deterministic and shell-friendly.

## Validation

Run the shared validator before committing:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Also run existing repository tests when changing code or asmdefs. Documentation-only updates should still run `git diff --check`.

## Codex Guidance

- Inspect current files before changing anything.
- Work on `develop`; do not edit or merge `main` unless the task is promotion-only.
- Do not edit `Library/PackageCache`.
- Do not guess package versions or dependency versions.
- Do not add package dependencies casually; update asmdefs, `package.json`, `deucarian-package.json`, Package Registry, Package Installer fallback, and Bootstrap fallback together when a dependency is truly required.
- Do not create local copies of shared helpers.
- Keep commits focused and report exactly what changed and what was validated.
