# Pixel art assets v1

Generated with the built-in image_gen tool and saved inside the Unity project. Original explorer concept sheet was the reference for the hero and melee weapons. Source RGBA PNGs are preserved; Unity imports them as Texture2D / Sprite / FullRect, Point filtering, no mipmaps, uncompressed, maximum texture size 512.

| File under Assets/_Game/Art | Use |
|---|---|
| Characters/Explorer.png | Player prefab; teal scarf, ivory shirt, leather equipment |
| Weapons/Dagger.png | Dagger held sprite |
| Weapons/Sword.png | Sword held sprite |
| Weapons/Greatsword.png | Greatsword held sprite |
| Weapons/Bow.png | Bow held sprite; generation prompt in bow-pixel-v1.md |
| Weapons/Arrow.png | Original code-generated arrow sprite |
| Enemies/Slime.png | Chaser prefab; red-orange melee slime |
| Enemies/Goblin.png | Ranged prefab; purple-hooded goblin archer |
| Enemies/Boar.png | Charger prefab; russet charging boar |

These are single idle images, not completed directional animation sheets. Held weapons rotate with aim; evolved Dagger gains a cyan tint and the existing piercing wave. Collider sizes, enemy behavior and damage rules remain independent of artwork. SpriteHitFlash material preserves visible white hit feedback on colored artwork.

## Generation prompts

### Explorer
Create ONE production-use standalone player character sprite matching the EXPLORER in the reference sheet. TRANSPARENT RGBA background. No labels, sheet, palette, scenery, shadows, floor or glow. One full-body unarmed idle character, centered, facing DOWN towards viewer in a 2D top-down game, slightly see top of hair and shoulders. Same short tousled dark hair, visible face, teal scarf and short cape, ivory shirt, practical brown leather armor/belt/boots, dark pants, copper clasp. Compact chibi proportions with readable hands and separated feet. True low-resolution PIXEL ART, visible square pixels, limited palette, dark outline, NO antialiasing or smooth shading. About 32x48 logical pixels enlarged with crisp pixel edges. Keep the actual figure generously large within image, full body uncropped, small transparent margin. No weapon; hands ready for separately held weapon sprite. Original fantasy adventurer, consistent with reference.

### Dagger
ONE standalone DAGGER sprite matching the reference weapon design. Transparent RGBA background, no text, no sheet, no shadows, no floor or surrounding glow. Horizontal orientation, HANDLE on LEFT, BLADE POINTING RIGHT. Short curved asymmetric silver-steel blade, small copper guard, teal wrapped handle and copper pommel. Authentic chunky low-resolution pixel art with dark outline, limited palette, flat square pixels and no smooth antialiasing. Readable at 32x16 logical pixels. Full weapon uncropped, centered with modest transparent margins. Original fantasy equipment. This is a usable isolated weapon image, not a design board.

### Sword
ONE standalone SWORD sprite matching the reference weapon set. Transparent RGBA background, no text, no sheet, no shadows or glow, no character. Horizontal orientation, HANDLE LEFT, BLADE POINTING RIGHT. Medium-length straight double-edged steel blade with sharp point and simple central ridge, copper crossguard, teal wrapped grip, copper pommel. Balanced classic fantasy sword, longer and slimmer than the dagger. Authentic chunky low-resolution pixel art, dark outline, limited 8-color palette, crisp square pixels, NO smooth antialiasing. Readable at approximately 48x16 logical pixels. Full weapon centered and uncropped with modest transparent margin. Original design.

### Greatsword
ONE standalone GREATSWORD sprite matching the reference set. Transparent RGBA background, no sheet, no labels, no glow or floor shadow. Horizontal orientation, HANDLE LEFT, BLADE POINTING RIGHT. Very broad heavy rectangular steel blade with a clipped angular tip, reinforced dark spine, simple copper guard, long two-handed teal wrapped handle with copper pommel. Unmistakably massive and heavy compared with the sword. Clean chunky low-resolution pixel art, flat square pixel clusters, dark outline, limited steel-gray/ivory/copper/teal palette, NO smooth gradients or antialiasing. Readable at 56x24 logical pixels. Full weapon isolated, centered, uncropped. Original practical fantasy design.

### Slime
ONE standalone enemy SLIME sprite for an original 2D top-down pixel-art action roguelite. Transparent RGBA background, no text, no sheet, no floor or cast shadow, no glow backdrop. A small squat reddish-orange jelly monster with a rounded triangular blob silhouette, two dark determined eyes, tiny mouth, one restrained ivory pixel highlight and darker red lower rim. Cute but clearly an enemy, simple shape, no accessories. Slight top-down three-quarter front view, full body centered. Authentic chunky 32x24 logical-pixel art enlarged crisply, limited 6-color palette, dark outline, flat pixel clusters, no smooth gradients or antialiasing. Large clear silhouette with modest transparent margin, highly readable on a dark dungeon floor.

### Goblin
ONE standalone GOBLIN ARCHER enemy sprite for an original 2D top-down pixel-art action roguelite. Transparent RGBA background, no sheet, labels, floor, shadow or glow. Short green-skinned goblin with pointed ears, yellow eyes, large expressive head, a simple purple hood/short vest, brown belt and boots, a tiny wooden shortbow held close to its side and small quiver on back. Clearly readable ranged enemy, mischievous not gruesome. Full body, front three-quarter slightly top-down view, feet visible. Authentic chunky low-resolution 32x40 logical-pixel game art enlarged crisply, dark outline, limited 10-color palette with violet and leaf-green contrasted against brown. Square pixel clusters, no antialiasing or smooth rendering. Centered with modest transparent margins. Original simple design, consistent with a teal-cloaked pixel adventurer.

### Boar
ONE standalone charging BOAR enemy sprite for an original 2D top-down pixel-art action roguelite. Transparent RGBA background, no text, no sheet, no ground shadow, no glow or scenery. A compact stocky wild boar with warm russet-brown fur, dark spiky mane, broad orange-brown snout, short ivory tusks, determined eyes, four short sturdy legs. Full body, slightly top-down three-quarter front view, head lowered ready to charge. Rounded heavy silhouette clearly different from a slime and goblin. Cute fierce monster, no blood or gore, no accessories. Authentic chunky low-resolution 40x32 logical-pixel game art enlarged crisply, dark outline, limited 8-color palette, square pixel clusters, NO smooth gradients or antialiasing. Centered with modest transparent margins, original design.

Weapon-grip follow-up: artwork is anchored by per-weapon heldGrip coordinates; Bow's left-facing source is mirrored at rendering time. Small hands, melee swing and Bow drawing/recoil are procedural visual poses synchronized to the existing attack. No source image or Input Contract was changed.
