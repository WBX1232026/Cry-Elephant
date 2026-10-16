# Cry Elephant

A 6-page interactive picture book game for children, built with Unity **2022.3.62f2**.
PROG2006 Assessment 2 — 2026.

## How to run

1. Install Unity 2022.3.62f2 via Unity Hub.
2. In Unity Hub: **Add project from disk** → select the `project` folder.
3. Open the project and press **Play**.

## Story & pages

`0_Home` → `1_Play` (meet Rabbit — easy friend, tap twice) → `2_Choose` (Monkey & the balloon choice) → `3_Cry` (meet Turtle — needs patience, 3 gentle taps) → `4_Calm` (breathe together) → `5_Credits` (friendship summary)

English narration plays on every page; background music automatically ducks while the narrator speaks.

## Folder structure (for reviewers)

| Folder | Content |
|---|---|
| `project/` | The complete runnable Unity project |
| `images/` | All image assets (89) |
| `audio/` | Narration, BGM and SFX (9) |
| `texts/` | Narration script, scene text list and interaction hints |
| `scenes/` | Scene files (7) |
| `scripts/` | C# scripts (26) |
| `fonts/` | Fonts (7) |

## Highlights

- **Separate story objects**: elephant, rabbit, monkey, turtle, balloons, tears — each interactive object is an individual sprite.
- **Friendship difficulty**: Rabbit = easy (2 taps), Turtle = hard (3 patient rounds), Monkey = player's sharing choice.
- **Choices matter**: the sharing decision changes the monkey's reaction, the calm-page bubble text, and the final friend count.
- **Consistent UI**: one canvas system (1080×1920, ScaleWithScreenSize), uniform buttons, banner and hint panels.
