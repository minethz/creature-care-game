# Creature Care — Design & Implementation Notes

*Game Developer Challenge submission — short written document.*

## Engine choice

**Unity 6 (6000.0.32f1) with the Universal Render Pipeline (2D).**

Unity is the preferred engine for this challenge and was an easy choice: I already had a
2D Unity project with shiba-pet sprite sheets, and Unity's 2D tooling, legacy UI and
`PlayerPrefs` cover everything this game needs with zero dependencies. The project stays
self-contained — no plugins, no external assets, and no network.

## Design decisions

### The concept
A pet that lives on screen and is cared for in real time. To keep the scope tight and the
game polished (the brief explicitly values "a small, polished game over a broken big one"),
I focused on three stats, three actions and one clear goal: **keep the pet alive for 5 days**.

### Stats and decay
- **Hunger, Happiness, Energy** — each 0–100, decaying automatically each frame in
  `Pet.Update`. Different decay rates make each stat demand attention on a different
  cadence, which keeps the player juggling rather than memorising a button order.
- **Night cycle (bonus):** decay is 1.3× faster at night, signalled by a screen darken.
  This adds a small time-of-day decision without complicating the design.

### Actions with cooldowns
Feed/Play/Sleep all have cooldowns (or an energy cost, for Play). This satisfies the
"no spamming" requirement and makes the game a continuous attention task instead of a
one-button idle. Sleep trades off time (other stats keep decaying slowly) for a big
Energy recovery — a deliberate risk/reward.

### Pet states and feedback
States are computed from the *lowest* stat so they feel truthful: Hungry, Sad, Tired,
Sick, and Happy are all reachable. Feedback is layered so it is always readable:
1. **Sprite** — each state plays a different sprite-sheet animation,
2. **Tint** — sad/tired/sick/sleeping shift the pet's colour,
3. **Movement** — bob speed and height change, the pet falls over when it dies,
4. **Sound** — procedural beeps on actions, sad tones when it drops into a bad state.

### Game over
Any stat reaching 0 kills the pet. The death is explicit: the pet greys out, falls over,
a "GAME OVER" panel explains the cause and shows the stats that led to it.

### UI
Built entirely at runtime in `PetUI.cs` (UnityEngine.UI, legacy) so there are no font or
layout assets to import and nothing to wire up manually. Stat bars are always on screen,
buttons show live cooldown numbers, and keyboard shortcuts (F/P/S) are included as a bonus.

### Bonus features included
- **Name your pet** — start screen with a name input.
- **Save / load** — auto-saves every 5s and on quit; returning players resume, with stats
  decaying gently while away.
- **Day / night cycle** — affects decay rate with visual feedback.
- **Procedural audio** — no asset files required.
- **Pet age/lifecycle** — not implemented; see below.

## Code structure

Scripts are split by responsibility so the game logic is engine-light and testable in
principle:

| Script | Responsibility |
|---|---|
| `Pet.cs` | Pure-ish game state: stats, decay, actions, states, day/night, events |
| `PetVisual.cs` | Presentation: sprite animation, tint, bob, death pose |
| `PetUI.cs` | Presentation: all UI built at runtime, wires button → pet actions |
| `PetAudio.cs` | Presentation: procedural sound effects reacting to pet events |
| `PetSave.cs` | Persistence: PlayerPrefs save/load, offline decay |

Components communicate through C# events (`StateChanged`, `Message`, `GameOver`, …), so
the visuals and UI don't reach into the game logic and new feedback systems can be added
by subscribing.

## What I would add or improve with more time

- **Pet lifecycle (age)** — the pet grows between life stages based on days survived, with
  a sprite sheet swap per stage. The event architecture is already in place for this.
- **More actions & stats** — e.g. a Hygiene stat with a "Clean" action, and medicine when
  sick, to widen the decision space.
- **More expressive animation** — proper transitions between states (blink-in/out) and a
  room the pet can move around in, instead of the anchored pose.
- **Audio** — replace the procedural beeps with a small set of real sound effects and a
  gentle background track; volume slider.
- **Save polish** — migrate save data to a file in `Application.persistentDataPath` and
  add a "reset pet" option in the menu.
- **Accessibility** — colour-blind-safe state indicators (icons beside tints) and a pause
  option so the game respects player time.
- **Build & share** — WebGL build on itch.io/GitHub Pages so it can be played in a browser.

## Time budget

Built in a single focused pass: core logic first, then visuals, then UI, then persistence
and audio, verifying each compile step as it went. The priority throughout was making the
core loop complete and readable before layering on bonus features.
