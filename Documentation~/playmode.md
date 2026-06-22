# PlayMode Usage

Use `RunPlayMode` for package PlayMode validation. The runner survives domain reloads by re-registering callbacks from `SessionState`. If PlayMode never reports completion, the timeout result is explicit and durable.
