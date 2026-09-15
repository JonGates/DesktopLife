# Localization and global shortcuts verification

2026-09-15

## Behavior

Simplified Chinese / English picker at the upper right updates dynamic WPF resources, validation/status text, display labels and tray entries immediately, without replacing pending quantity edits. Language is persisted immediately. Population and shortcuts have separately labeled save buttons.

Start/resume defaults to Ctrl+Alt+S; pause defaults to Ctrl+Alt+P. Record a modified letter, digit or F1–F11 in the fields; Backspace clears a binding. Both actions are idempotent. Registrations belong to an application-lifetime message-only window, remain active when settings close, and are released on exit. Capturing an existing registration updates the focused field without executing the action; deactivation clears capture state. Shortcuts cannot launch a terminated process.

New combinations are reserved before saving. Conflict, duplicate bindings, invalid syntax and failed persistence preserve the old registrations. Swapping the two bindings reuses registrations. Preferences use a separate atomic JSON file and do not modify existing creature counts.

## Evidence

- Release solution build: zero errors and warnings.
- Existing 93 xUnit tests passed.
- `--controls`: population controls, invalid input, persistence, pause/resume, failed writes and reopening passed after localization.
- `--preferences`: parsing and canonical round trips; actual RegisterHotKey success/conflict; reservation rollback after injected write failure; native WM_HOTKEY dispatch to the owned message window; action swapping, idempotence, capture suppression, disabling and registration release passed.
- English title/static strings, tray updates, language persistence, preservation of unsaved counts, shortcut controls/persistence, reopening, corrupt preferences fallback and failed language writes passed.
- Inspected English settings and Chinese/English shortcut screenshots; long labels wrap within the window and the body scrolls while action buttons remain visible.
- Independent review found no confirmed important defect. Added explicit deactivation handling for the noted capture-focus risk and exercised its event handler.

The native-message diagnostic sends messages only to its own test window; it does not synthesize a physical keyboard shortcut or Alt+Tab to another application. Physical keyboard/foreground transitions still warrant user confirmation on the target desktop.

Reference: [Microsoft RegisterHotKey documentation](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey).

Published build: actual two-monitor bounds, transparent click-through, no activation and single-instance settings opening passed. Updated app left running with settings open.
