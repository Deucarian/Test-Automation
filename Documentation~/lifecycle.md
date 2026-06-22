# Runner Lifecycle Notes

PlayMode can trigger domain reloads and temporary backup scene loads. One-off validation-project callbacks can be lost during that reload. This package stores pending run configuration in `SessionState` and re-registers callbacks after domain reload through an `InitializeOnLoad` bridge.

The watchdog writes a timed-out result with `callbackCompleted=false` and exits with code `2`.
