# Sprite assets

## Spider body · 0.4.2-preview.2

`spider-body.png` was generated with the built-in image_gen tool on 2026-09-16. The original transparent PNG is embedded without pixel edits; the renderer bounds its alpha silhouette at load time. Legs and pedipalps are animated separately by `SpiderSprite`; the cute appearance is code-native. This is a generated representative body texture, not a specimen photograph.

Final generation prompt:

Create ONE production game sprite texture, transparent PNG with real alpha. Photorealistic natural brown wolf-spider BODY ONLY for an articulated desktop pet. Strict orthographic dorsal view looking straight down, long axis horizontal, facing RIGHT. Exactly two connected body masses: oval softly hairy mottled brown abdomen on LEFT, smaller brown cephalothorax on RIGHT joined by very short narrow pedicel. Subtle pale median stripe and natural dark markings, fine short hairs, realistic muted brown, soft neutral lighting. Absolutely NO legs, NO pedipalps, NO antennae, NO wings: all appendages are separately animated in code, this is only the intact central body texture, not an injured animal depiction. No ground, no cast shadow, no text, no checkerboard pixels. Body aspect ratio approximately 1.85:1, centered with generous transparent margin, fills 75% of image width. This is a biological game asset, not a cartoon or logo.

## Realistic fly atlas

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

# Eight additional insect bodies · v0.4.0

Generated with the built-in image_gen tool on 2026-09-16. Two original 1024×1536 RGBA files, no pixel edits or background removal. Alpha range 0–254; four separated nontransparent row bands verified per file. The visible RGB glow in a raw viewer has zero alpha outside the bodies. Runtime bounds each occupied row and preserves the alpha. WPF diagnostic checks render all eight species on transparent, light and dark backgrounds.

- `small-insect-bodies.png`: ladybug, ground beetle, earwig, silverfish; occupied rows 51–409, 478–768, 855–1065, 1179–1419.
- `long-insect-bodies.png`: cricket, grasshopper, mantis, stick insect; occupied rows 161–379, 540–733, 902–1076, 1279–1342.

## Prompt set

Common: production 2D game BODY texture atlas, genuinely transparent RGBA background, portrait 1024×1536, exactly four separated horizontal rows, one isolated body per row, dorsal top-down orthographic macro photorealistic view. Head right, tail left. Omit all legs, antennae, tail filaments and pincers because they will be animated separately. Neutral diffuse illumination, no shadow, text, scenery, cartoon outlines or painted checkerboard. Leave transparent gutters and outer margins.

Small atlas: row 1 red seven-spotted ladybug, domed elytra and black/white pronotum, body about 420×320 px; row 2 black-bronze ground beetle, elongated striated elytra and narrow pronotum, about 650×220; row 3 dark reddish-brown European earwig, segmented narrow abdomen and short wing covers, no pincers, about 720×130; row 4 metallic silverfish with scaled tapered segmented abdomen, tiny head and no tail filaments, about 720×140. Requested row centers y=192,576,960,1344; actual transparent gutters are detected rather than assuming exact row placement.

Long atlas: row 1 dark brown/black field cricket, folded leathery forewings and rounded pronotum, about 650×190 px; row 2 green grasshopper, tapered folded wings with subtle brown dorsal stripe, pronounced pronotum and rounded head, about 720×160; row 3 adult green mantis, folded wings over abdomen, very long thin prothorax, triangular head with two eyes on right, no raptorial arms, about 780×110; row 4 brown walking-stick body, very long thin segmented twig with subtle bark texture, tiny head, about 800×40. Requested row centers y=192,576,960,1344.

These assets convey representative insect groups and are not scientific specimen photographs. Legs, antennae and characteristic rear appendages are drawn in `AdditionalInsectSprite` with per-group proportions.
