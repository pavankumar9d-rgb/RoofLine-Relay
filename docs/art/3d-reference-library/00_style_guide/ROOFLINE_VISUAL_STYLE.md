# Roofline Relay — Visual Style Guide & Asset Creation Rules

## 1. Aesthetic Identity & World Concept
*Roofline Relay* is a third-person, high-speed futuristic rooftop parkour game set in a near-future vertical metropolis. The art direction emphasizes **grounded, industrial, and high-performance functional design** rather than high-fantasy cyberpunk or excessive neon clutter. Every element—from runner apparel to modular HVAC ducting—feels engineered, purposeful, and mechanically plausible.

---

## 2. Core Color Palette

| Tone | Hex Code | Purpose & Application |
| :--- | :--- | :--- |
| **Graphite Grey** | `#26292B` / `#363B3E` | Base primary tone for architectural frameworks, softshell technical fabrics, and industrial equipment casing. |
| **Dark Teal** | `#1C3B3C` / `#224D4E` | Secondary brand tone for weather-resistant ripstop apparel panels, conduit sleeves, and specialized high-rise modules. |
| **Restrained Amber** | `#E58A24` / `#D46E15` | High-visibility accent color used for functional parkour callouts, utility zipper pull-cords, shoe tread accents, warning markers, and active beacon energy cores. |
| **Galvanized Steel / Bare Metal**| `#8A9196` / `#A3AAAF` | Mechanical structural beams, safety railings, catwalks, ladders, hinges, and fasteners. |
| **Aged Concrete / Rooftop Tar** | `#52565A` / `#3F4346` | Parapet coping stones, rooftop deck slabs, gravel aggregate, and roof surface coatings. |

---

## 3. Material & Textile Language

1. **Apparel & Runner Gear:**
   - **Matte Graphite Softshell:** Flexible, 4-way stretch, abrasion-resistant fabric used on major jacket sleeves and cargo trousers.
   - **Dark Teal Ripstop Nylon:** Micro-grid reinforced weatherproof weave utilized on chest plates, back panels, and knee/elbow reinforcements.
   - **High-Friction Vibram/Approach Rubber:** Molded multidirectional grip soles on footwear and silicone grip pads on climbing palms.
   - **Machined Hardware:** Anodized matte black or brushed graphite magnetic buckles (Fidlock style), aluminum D-rings, and heavy-duty taped water-repellent zippers.

2. **Architecture & Rooftop Traversal:**
   - **Poured & Pre-cast Concrete:** Heavy aggregate texture with light atmospheric weathering, expansion seams, and drainage scuppers.
   - **Galvanized / Powder-coated Metal:** Catwalk gratings, safety ladders, and handrails with realistic edge wear and weld beads.
   - **Dark Architectural Glass:** Reflective low-emissivity glass on facade modules and skylights with discrete framing mullions.

3. **Gameplay Devices & Beacons:**
   - **Composite Enclosures:** Heavy-duty carbon-fiber and magnesium chassis protecting internal telemetry hardware.
   - **Restrained Amber Emissive:** Subdued amber luminescent status rings, waveguide indicators, and interactive target elements (diffuse, not blinding neon bloom).

---

## 4. Silhouette & Shape Language
- **Silhouettes:** Crisp, directional, readable instantly from third-person camera distances (5–20 meters).
- **Proportions:** Athletic, biomechanically plausible human proportions (7.5 to 8 heads tall). No exaggerated cartoon anatomy or bulky mecha armor.
- **Modularity:** Straight edges, 90-degree alignment corners, standardized snapping increments (e.g. 1m, 2m, 4m, 8m grid compatibility for parkour ledges and bridges).

---

## 5. Image-to-3D Generation Standards (Critical)
To ensure reliable 3D mesh reconstruction via image-to-3D tools:
1. **Isolated Single Objects:** One clear object per primary generation. Never generate multiple floating items in an input image.
2. **Neutral Studio Setting:** Plain solid light-grey (`#D6D8DA` to `#E2E4E6`) studio backdrop without ground shadows cutting into the geometry.
3. **No Decorative Clutter:** Zero typography, text labels, dimension watermarks, or user interface overlays.
4. **Complete Framing:** The entire asset must be enclosed within the frame, ensuring zero cropped feet, antennas, or edges.
5. **Orthographic & Consistent Views:** For characters and major hero devices, preserve matching scale, gear, and lighting across Front, Profile (Side), and Rear views.
