# Forest and Ocean Modes Implementation Plan

## Approved behavior

User requested a Forest/Ocean tabbed configuration, and confirmed that switching tabs switches the active scene while preserving independent settings. Forest keeps its current creatures. Ocean has one cursor-following green turtle and twelve configurable fish species. Desktop and screensaver both support the scene choice, existing bilingual UI and realistic/cute styles.

## Shared contracts

- Append enum values; preserve existing serialized creature numbers.
- Engine Habitat enum Forest/Ocean. Append Habitat and Ocean fields to PopulationSettings, with GetOcean and validation. Retain forest fields for backward compatibility.
- OceanCatalog.Fish reuses InsectDefinition for names, sprite dimensions, speed, stride and caps; GreenTurtle is fixed at one and excluded from editable counts.
- Forest and ocean have independent populations; only selected habitat is instantiated. Defaults for new ocean fish: two per species, 80–120% size.
- Fish: clownfish, blue tang, yellow tang, butterflyfish, marine angelfish, lionfish, pufferfish, seahorse, mandarin fish, royal gramma, Moorish idol and wrasse.

## Work units

1. Engine and simulation: introduce catalog, backward-compatible settings and swimming behaviors. Add failing tests, implement and verify migration, scene switching, size/count validation, multi-screen movement and turtle click/dwell/resume.
2. Configuration: Forest/Ocean tabs, preserve drafts, immediate atomic desktop scene switching, saver scene persistence, bilingual labels. Test switching, saving, invalid input and old settings with offscreen WPF diagnostics.
3. Rendering/assets: transparent realistic fish atlas textures and turtle body, code-driven tail/fin/flipper motion, distinct cute variants. Inspect real renders on light/dark backgrounds; preserve transparency, pause stability, orientation and DPI scaling.
4. Integration: run complete tests and relevant diagnostics; inspect UI screenshots; document limitations and build desktop/saver self-contained previews with the same version/commit. Keep released v0.4.2 intact until a new release is requested.

## Verification

Use dotnet tests, OceanSettingsProbe and ocean rendering diagnostics. Verify exactly one turtle, twelve editable fish, no forest creatures in ocean, preservation of both configurations, multi-monitor gaps, no forced settings changes and no interruption of an already running user process.
