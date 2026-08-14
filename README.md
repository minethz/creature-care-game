# Creature Care 🐾

A small 2D creature-care game made in **Unity 6 (6000.0.32f1)** for the Game Developer Challenge.

The idea is simple: you have a pet that needs to be looked after. Its hunger, happiness, and energy change over time, and the player needs to manage these stats to keep the pet alive.

The goal is to take care of the pet for **5 in-game days**.

## How to Run

1. Open the project in **Unity Hub**.
2. Use **Unity 6 (6000.0.32f1)** or a newer version.
3. Open `Assets/Scenes/SampleScene.unity`.
4. Press **Play**.

The UI and game systems are created automatically when the game starts, so there shouldn't be any extra setup needed.

## Controls

| Key | Action |
| --- | ------ |
| `F` | Feed   |
| `P` | Play   |
| `S` | Sleep  |

The same actions can also be used through the buttons on screen.

## Gameplay

The pet has 3 main stats:

* **Hunger** – goes down over time and can be restored by feeding.
* **Happiness** – decreases over time and can be increased by playing.
* **Energy** – decreases over time and can be restored by sleeping.

All stats are between **0 and 100**.

The player can't simply keep pressing the same button because actions have cooldowns and some actions have a cost. This means you have to pay attention to all three stats.

### Current values

* Hunger decay: `1.4 / second`
* Happiness decay: `0.8 / second`
* Energy decay: `0.45 / second`

## Pet States

The pet changes its state depending on how it is being looked after.

Current states include:

* Happy
* Content
* Hungry
* Sad
* Tired
* Sick
* Sleeping
* Playing
* Gone

The state is shown through changes to the pet's sprite/animation, tint, movement and sound effects.

For example, if the pet is getting too hungry, it will change into a hungry state rather than just changing a number on the UI.

## Day & Night

The game has a simple day/night cycle.

One in-game day lasts **60 seconds**.

During the night, the screen becomes darker and the pet's stats decrease slightly faster. This gives the player another thing to keep in mind while taking care of the pet.

## Win / Game Over

The main goal is to survive for **5 days**.

If any of the important stats reaches `0`, the pet dies and the game shows a game-over state.

If the pet survives all 5 days, the player reaches the win screen.

## Save System

The game automatically saves the pet's progress using Unity's `PlayerPrefs`.

This allows the game state to be loaded again when the project is opened later.

## Audio

The sound effects are generated at runtime, so there aren't any external audio files that need to be included in the project.

Sounds are used for actions and different pet states.

## Project Structure

```text
Assets/
├── Scripts/
│   ├── Pet.cs
│   ├── PetVisual.cs
│   ├── PetUI.cs
│   ├── PetAudio.cs
│   └── PetSave.cs
│
├── Resources/
│   └── Pet/
│       └── Sprite sheets
│
└── Scenes/
    └── SampleScene.unity
```

### Main scripts

**Pet.cs**

Handles the main game logic including stats, stat decay, actions, cooldowns, pet states, day/night cycle, winning and game over.

**PetVisual.cs**

Handles the pet's animations and visual changes. The sprite sheets are processed in code and the appropriate animation is selected based on the current pet state.

**PetUI.cs**

Creates and manages the game UI, including the stat bars, buttons, cooldowns and win/game-over screens.

**PetAudio.cs**

Creates the sound effects at runtime.

**PetSave.cs**

Handles saving and loading the game state using `PlayerPrefs`.

## Tuning

Most of the gameplay values can be changed directly from the **Pet** component in the Unity Inspector.

This includes:

* Stat decay rates
* Night multiplier
* State thresholds
* Feed amount
* Play amount
* Energy cost
* Action cooldowns
* Number of days required to win
* Length of each day

This made it easier to test and adjust the difficulty without changing the core code.

## Challenge Requirements

The main requirements from the challenge are covered:

* ✅ 3 stats
* ✅ Automatic stat decay
* ✅ 3 player actions
* ✅ Pet visible on screen
* ✅ Multiple pet states
* ✅ Visual/audio feedback
* ✅ Game-over condition
* ✅ Always-visible stat indicators
* ✅ Real-time gameplay
* ✅ Action cooldowns/costs
* ✅ Clear survival goal

Additional features:

* ✅ Save/load
* ✅ Day/night cycle
* ✅ Sound effects
* ✅ Procedural audio
* ✅ Multiple pet animations

## If I Had More Time

There are a few things I would like to add with more development time:

* Add more actions such as bathing or giving medicine
* Add more animations and reactions
* Add more environments
* Add random events to make each playthrough slightly different
* Add a proper start/menu screen
* Add more visual polish and transitions
* Make more animated pet rather than just Sprites


For this version, I focused mainly on getting the core gameplay loop working properly and keeping the systems easy to modify.

## AI Tools

AI tools were used during development for things like debugging, brainstorming and getting help with implementation.

I still reviewed, tested and modified the code and systems to fit the game and understand how the different parts work.

---

**Built with Unity 6 and C#**

**Developer:** Mineth Perera
