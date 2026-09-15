# Realistic fly atlas

Generated with the built-in image_gen tool on 2026-09-15. This is a generated photorealistic asset, not a documentary photograph.

- File: `fly-atlas.png` (1536×1024 RGBA PNG).
- Left half: resting, wings folded. Right half: flight, wings spread with motion blur.
- Top-down view, head points +X. Runtime anchor near (500,512) in each half.
- Genuine alpha verified: corner, top padding and gap samples have alpha 0. The original alpha is preserved; no background-removal or pixel editing was applied.
- Embedded in DesktopLife.Rendering, decoded once and reused. Approximate 40 px leg span; source padding is not part of the visible sprite size.

## Final generation prompt

Create a production-ready photorealistic housefly sprite atlas PNG with a genuinely TRANSPARENT alpha background, no solid background, NO checkerboard baked in. Canvas landscape 1536x1024, exactly TWO equal-width 768x1024 cells side by side. In each cell show ONE identical adult common housefly seen from directly overhead (orthographic dorsal view, no perspective), head pointing RIGHT (+X). LEFT cell: fly landed and resting, wings folded naturally over the abdomen, all six thin black segmented legs touching an imaginary surface, crisp realistic fine hairs, gray thorax with four dark stripes, realistic dark segmented abdomen, small muted brick-red compound eyes, translucent veined wings. RIGHT cell: same insect same body center/size/orientation with two anatomically accurate transparent wings spread in flight, subtle wing-motion blur but crisp unchanged head and thorax, legs tucked slightly. Both insect body centers precisely at respective cell centers (384,512) and (1152,512). Keep both complete insects including legs and wings inside their cells with generous transparent padding, approximately 540 pixels total horizontal span per insect. Real macro-photo visual texture, anatomically credible Musca domestica; NOT cartoon, NOT diagram, NOT vector, NOT painted illustration, not green blowfly. No lettering, labels, decorations, borders, rulers, detached parts, environment, cast background shadow, extra insects. Intended to display at 38-48 physical pixels in a desktop pet application; maintain readable natural silhouette.


# Realistic crawler body atlas · v0.3.1

`insect-bodies.png`: 1536×1024 RGBA, generated and alpha-refined using the built-in image_gen tool on 2026-09-16. This is generated photorealistic artwork, not a photograph. Original alpha is preserved. Runtime reads the three horizontal rows (cockroach, ant, caterpillar) and bounds the visible body; legs and antennae are separately animated in WPF. Body padding is fully transparent at tested samples. Body interior alpha is approximately 253/255 in this output.

Generation prompt: one production game texture atlas, three equal horizontal rows, directly overhead insect bodies facing right, no legs/antennae (animated separately), genuine transparent background, no cast shadows or text. Row 1: brown cockroach with tapered veined forewings and shield-shaped pronotum. Row 2: dark reddish-brown worker ant with narrow thorax/petiole and glossy segmented abdomen. Row 3: continuous green caterpillar body with granular skin, pale markings and small lateral spiracles. Neutral diffuse illumination; no cartoon outlines or cute eyes.

Final edit prompt: preserve the three bodies and positions; remove all background/glow/halos and cast shadows. Outside body silhouettes alpha must be zero, with only narrow edge antialiasing and fine hairs; body interiors opaque. Keep 1536×1024 and separate rows, no added legs or antennae.
