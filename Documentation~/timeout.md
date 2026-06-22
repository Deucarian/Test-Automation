# Timeout And Watchdog

Pass `-batchTestTimeoutSeconds <seconds>` to control the watchdog. On timeout the runner writes:

- `resultStatus: TimedOut`
- `timedOut: true`
- `callbackCompleted: false`
- `processExitRecommendation: 2`
