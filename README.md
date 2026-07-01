# Deucarian Test Automation

`com.deucarian.test-automation` is an editor-only validation tooling package. It provides reusable Unity batch-mode test entry points that emit durable JSON and text result files for EditMode and PlayMode runs.

This package must not ship in player builds. It is optional tooling for Deucarian package validation projects. Direct Unity `-runTests` may still be used when it is reliable; this package standardizes the fallback path.

## Commands

```text
Unity.exe -batchmode -nographics -projectPath <project> -executeMethod Deucarian.TestAutomation.BatchTestRunner.RunEditMode -batchTestResults <path> -batchTestTimeoutSeconds 300 -logFile <log>
Unity.exe -batchmode -nographics -projectPath <project> -executeMethod Deucarian.TestAutomation.BatchTestRunner.RunPlayMode -batchTestResults <path> -batchTestTimeoutSeconds 300 -logFile <log>
Unity.exe -batchmode -nographics -projectPath <project> -executeMethod Deucarian.TestAutomation.BatchTestRunner.RunAll -batchTestResults <path> -batchTestTimeoutSeconds 300 -logFile <log>
```

Optional:

```text
-batchTestFilter <test name>
```

When running from automation, poll for the requested `.json` or sibling `.txt` result file. On this Unity version the command stub can return before the child editor process has finished writing the durable result.

## Install

Stable:

```json
"com.deucarian.test-automation": "https://github.com/Deucarian/Test-Automation.git#main"
```

Development:

```json
"com.deucarian.test-automation": "https://github.com/Deucarian/Test-Automation.git#develop"
```

Use `#main` for stable package consumption and `#develop` when testing active package work.

## When To Use This

Use this package when you need Editor-only Unity batch test runner helpers for durable EditMode and PlayMode validation results.

Do not use this package to take ownership of capabilities outside its `AGENTS.md` boundary. Reusable behavior should stay with the package that owns that capability in the Package Registry governance docs.

## Quick Start

1. Install the package through Deucarian Package Installer or Unity Package Manager using the URL above.
2. Let Unity finish resolving packages and compiling assemblies.
3. Start from the package README sections above and the public runtime/editor APIs in this repository.

## Integrations

This package has no direct Deucarian package dependencies.

Install optional companion packages only when their owned capability is needed by production code, samples, or tests.

## Troubleshooting

- Package does not resolve: confirm the stable or development Git URL matches the Package Registry entry and that required Deucarian dependencies are installed.
- Unity compile errors after install: let Package Manager finish resolving dependencies, then check asmdef references against `package.json` dependencies.
- Behavior appears to belong in another package: consult `AGENTS.md` and the Package Registry governance docs before moving or duplicating code.
