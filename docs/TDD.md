# Test-Driven Development Notes

This sprint introduced two behavior-focused changes:

- Standardised the Stronghold null-checks in `WaveManager` so we have a single pattern that is easy to reason about.
- Implemented a player feedback loop via the new damage indicator UI (controller, pooling, fallbacks, and system hooks).

Because the damage indicator contains the most new logic, the TDD work emphasised proving the maths that keeps the indicator stable and reusable across systems.

## Test Matrix

| Test | Location | Purpose |
| ---- | -------- | ------- |
| `SignedAngle_IsZero_WhenSourceIsDirectlyAhead` | `Assets/Tests/EditMode/DamageIndicatorMathTests.cs` | Guards the baseline direction math so the indicator never drifts when the threat is in front of the player. |
| `SignedAngle_IsPositiveOnRightAndNegativeOnLeft` | same as above | Validates left/right awareness so we can confidently rotate the UI element toward attackers. |
| `NormalizeDistance_ClampsBetweenZeroAndOne` | same as above | Ensures the scaling curve always receives safe input, preventing indicator jitter at extreme distances. |
| `Blend_InterpolatesColors` | same as above | Verifies our helper used for future visual themes blends colors predictably. |

Each test was written first, then the production code in `DamageIndicatorMath` was implemented/minimised until the tests passed, before hooking the math into `DamageIndicatorController`.

## Running The Tests

1. Open the project in Unity.
2. Window → General → Test Runner (Edit Mode tab).
3. In the Edit Mode list (under `Assembly-CSharp-Editor`), click *Run All*.

Unity will compile the `Assets/Tests/EditMode` assembly (defined by `EditModeTests.asmdef`) and execute the NUnit tests listed above. Because the tests only depend on `UnityEngine` structs and our helper class, they run quickly and deterministically.

## Additional Notes

- The damage indicator controller and element scripts rely on the same math helpers that the tests cover, so regressions in rotation/distance math will be caught early.
- Future UI polish (sprites, gradients, animations) can extend `DamageIndicatorElement` without touching the math class, keeping tests fast.
- If more observable behaviors need testing (e.g., pooling, lifetime fade), create additional pure C# helpers or wrap the logic so it can be unit-tested without invoking full `MonoBehaviour` lifecycles.
