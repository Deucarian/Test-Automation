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
