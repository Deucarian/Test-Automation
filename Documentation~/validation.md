# Package Validation Notes

Validation target: Unity `6000.3.5f1`.

This package is editor-only and should not be referenced by runtime package assemblies.

## Package Import

Validation project: `C:\Repositories\Deucarian\TestAutomation-TestProject`.

Command:

```text
Unity.exe -batchmode -nographics -quit -projectPath C:\Repositories\Deucarian\TestAutomation-TestProject -logFile C:\Repositories\Deucarian\TestAutomation-TestProject-phase1p-import-final.log
```

Result: passed with exit code `0`. `Deucarian.TestAutomation.Editor.dll` compiled in `Library\ScriptAssemblies`.

## Runner Validation

The Unity command stub can return before the child editor process has finished. Validation scripts must poll for the requested `.txt` or `.json` result file before reading results.

EditMode command entry point:

```text
Deucarian.TestAutomation.BatchTestRunner.RunEditMode
```

Result file: `C:\Repositories\Deucarian\TestAutomation-TestProject-phase1p-edit-polled.txt`.

- Result: passed.
- Passed: 1.
- Failed: 0.
- Skipped: 1 explicit disabled failure sample.
- Inconclusive: 0.
- Duration: 1.143 seconds.
- `timedOut=False`.
- `callbackCompleted=True`.
- Exit code: 0.

PlayMode command entry point:

```text
Deucarian.TestAutomation.BatchTestRunner.RunPlayMode
```

Result file: `C:\Repositories\Deucarian\TestAutomation-TestProject-phase1p-play-polled.txt`.

- Result: passed.
- Passed: 2.
- Failed: 0.
- Skipped: 0.
- Inconclusive: 0.
- Duration: 2.803 seconds.
- `timedOut=False`.
- `callbackCompleted=True`.
- Exit code: 0.

## Auto Defense Closeout

Validation project: `C:\Repositories\Deucarian\AutoDefense-TestProject`.

The Auto Defense validation project consumes this package with a local file reference:

```json
"com.deucarian.test-automation": "file:C:/Repositories/Deucarian/Test-Automation"
```

Root cause of the Phase 1O PlayMode caveat: the old project-local `AutoDefenseBatchTestRunner` registered callbacks only in the pre-PlayMode editor domain. The PlayMode run loaded `Temp/__Backupscenes/0.backup` and reloaded assemblies; the old callback did not survive to write a durable summary or exit recommendation.

Replacement result: `Deucarian.TestAutomation.BatchTestRunner.RunPlayMode` produced durable summaries twice:

- Pass 1: 1 passed, 0 failed, 0 skipped, duration 1.923 seconds, `callbackCompleted=True`, exit code 0.
- Pass 2: 1 passed, 0 failed, 0 skipped, duration 1.976 seconds, `callbackCompleted=True`, exit code 0.

No Auto Defense runtime assembly references `com.deucarian.test-automation`; the dependency is validation-project only.
