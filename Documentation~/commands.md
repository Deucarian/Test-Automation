# Command API

Entry points:

- `Deucarian.TestAutomation.BatchTestRunner.RunEditMode`
- `Deucarian.TestAutomation.BatchTestRunner.RunPlayMode`
- `Deucarian.TestAutomation.BatchTestRunner.RunAll`

Supported arguments:

- `-batchTestResults <path>`: JSON result path. A sibling `.txt` summary is also written.
- `-batchTestTimeoutSeconds <seconds>`: watchdog timeout. Defaults to 300.
- `-batchTestFilter <optional filter>`: optional test name filter.

Result files include Unity version, project path, platform, UTC start/end, duration, pass/fail/skip/inconclusive totals, status, failure names/messages, log path, timeout flag, callback completion flag, and exit recommendation.

Automation should wait for the `.json` or sibling `.txt` result file instead of assuming the Unity command stub exit means the child editor process has finished.
