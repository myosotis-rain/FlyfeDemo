# FlyfeDemo Context

This project is a 2D platformer game developed in Unity. The core gameplay revolves around platforming, utilizing different skills, and a unique "shadow recording" puzzle mechanic where the player can record their actions and replay them as a ghost to interact with the world.

## Directory Structure & Components

### `Camera/`
Handles camera movement and visual effects.
- **`CameraManager.cs`**: Manages the camera, including dynamically switching the follow target between the player and the active recording shadow.
- **`ParallaxLayer.cs`**: Creates a depth effect for background layers during movement.

### `Core/`
Core game management systems.
- **`GameStateManager.cs`**: Manages distinct world states (`Present`, `Memory`, `Replay`). These states are crucial for the recording mechanic, allowing the game to switch between the normal world, the recording phase, and the playback phase.
- **`Tags.cs`**: Constant string definitions for standard game tags.

### `Dialogue/`
System for in-game conversations and cutscenes.
- Contains scripts for triggering dialogues (`DialogueTrigger.cs`), managing cutscenes (`CutsceneController.cs`), and defining conversation data structures.

### `Gameplay/`
Interactable puzzle elements and environment objects.
- Contains various switches (`SwitchController.cs`, `LeverController.cs`, `RotarySwitchController.cs`), doors, and moving platforms.
- **`IInteractable.cs`**: Interface for objects the player or shadow can interact with.
- **`IResettable.cs`**: Interface for puzzle elements that must reset to their default state when the world state changes (e.g., when a recording ends).
- **`VineController.cs`**: Logic for climbable vines.

### `Player/`
Player character controls, input, and state management.
- **`PlayerController.cs`**: Handles core 2D movement, jumping, slope detection, and vine climbing utilizing `Rigidbody2D`.
- **`PlayerInputController.cs`**: Interfaces with Unity's Input System (`PlayerInputActions.inputactions`).
- **`Checkpoint.cs` & `PlayerRespawn.cs`**: Manages player death and respawning.
- **`Meter/`**: Contains scripts likely related to resource or UI meters.

### `Recording/`
The core "shadow" puzzle mechanic of the game.
- **`RecordingService.cs`**: Records the player's movements and interactions frame-by-frame. When recording starts, it shifts the world state to 'Memory' and spawns a 'Shadow'.
- **`ShadowReplay.cs`**: Plays back the previously recorded frames as a ghost in the 'Replay' world state.
- **`ReplayLimits.cs`**: Likely defines constraints on the recording functionality (e.g., duration, area).

### `Skills/`
Abilities the player can equip and utilize.
- **`SkillManager.cs`**: Manages the currently active skill for the player.
- **`HoverSkill.cs` & `NoSkill.cs`**: Implementations of player abilities. The active skill can influence which specific shadow prefab is spawned during recording.

### `UI/`
User Interface components and managers.
- Manages dialogue UI (`DialogueUI.cs`), screen transitions (`ScreenFader.cs`), interaction prompts (`InstructionPromptUI.cs`), and skill selection menus (`SkillSelectorUI.cs`).

## Key Gameplay Mechanics
- **Shadow Recording:** The player can activate a recording mode to log their movement and interactions (up to a time limit). Upon finishing, a "Shadow Replay" entity is created to repeat those exact actions. This allows the player to collaborate with their past self to solve puzzles.
- **World State Shifting:** The game actively resets and shifts environmental states depending on whether the player is in the present, currently recording a memory, or watching a replay.
- **Interchangeable Skills:** The player can cycle through different skills (like Hovering), which modifies movement capabilities and interacts with the shadow recording system.