# Fly flight rhythm verification

- Replaced continuously rotating mouse-relative orbit target with fixed short-dart destinations, randomized speed/duration and 0.16–0.4 s hover intervals. A cursor displacement over 100 px interrupts the current hover/dart target.
- Faster steering during nearby darts; turn-rate limited heading and frozen heading at low speed avoid spinning while hovering. Existing Orbit enum remains for compatibility and now represents nearby dart/hover behavior.
- Flying wings integrate five low-opacity exposures on each side per cached sprite phase; body and resting grooming remain anchored. This is an illustrative desktop animation, not a physical flight simulation.
- First regression run failed for the old implementation (no low-speed samples; corner flight escaped the screen). New tests cover mixed fast/slow flight, turns in both directions, corners, continuous panic re-entry and following across display gaps.
- 97 tests passed including existing ten-minute seeded stability and click/landing timer tests. Rendering probes passed at 100/125/150 percent. Native hook and production landing-loop probe passed.
- Independent review found possible offscreen projection jumps and gap pinning; fixed by staying in Approach until inside with a visible route to the cursor and disabling stand-off braking during re-entry. Added both regressions with per-frame displacement/speed bounds.
