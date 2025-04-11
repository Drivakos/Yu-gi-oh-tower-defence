# Yu-gi-oh-tower-defence

A mobile tower defense game set in the Yu-Gi-Oh! universe where players strategically place monsters to defend against waves of enemies.

## Features

- Strategic monster placement and management
- Yu-Gi-Oh! monster types with unique abilities
- Resource management using Duel Points (DP)
- Spell and Trap card system
- Wave-based enemy progression
- Boss battles featuring iconic Yu-Gi-Oh! monsters
- Multiplayer modes (PvP and Co-op)
- Accessibility features for color blindness
- Millennium Items with special abilities
- Card collection and deck building system
- Level editor for custom levels
- Real-time card generation and management
- Status effects and card effects system
- Save and load game functionality
- Pathfinding system for enemy movement
- Object pooling for performance optimization
- UI system for game state feedback
- Targeting system for strategic gameplay
- Special abilities for different monster types
- Visual and audio effects for gameplay feedback

## Development Setup

### Requirements

- Unity 2022.3 LTS or newer
- Visual Studio 2019/2022 or Visual Studio Code
- Android SDK (for mobile development)
- iOS development tools (for iOS builds)

### Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # Core game mechanics
│   ├── Monsters/       # Monster-related scripts
│   ├── Cards/          # Spell and Trap card scripts
│   ├── UI/             # User interface scripts
│   └── Utils/          # Utility scripts
├── Prefabs/            # Game object prefabs
├── Scenes/             # Game scenes
├── Art/                # Visual assets
└── Audio/              # Sound effects and music
```

## Getting Started

1. Clone the repository
2. Open the project in Unity
3. Open the main scene in `Assets/Scenes/MainGame.unity`
4. Press Play to test the game in the editor

## Contributing

Please read CONTRIBUTING.md for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the LICENSE.md file for details.

## Acknowledgments

- Yu-Gi-Oh! is a trademark of Konami
- This is a fan project and is not officially affiliated with Konami

## Accessibility Features

### Color Blind Filter Settings

The game includes a color blind filter to enhance accessibility for players with color vision deficiencies. The filter can simulate different types of color blindness, including Protanopia, Deuteranopia, and Tritanopia.

#### How to Use

1. **Setup the UI Panel**:
   - Create a new UI Panel in your scene.
   - Attach the `ColorBlindSettingsPanel` script to the panel.
   - Assign the necessary UI components (Toggle, Dropdown, Buttons, Image) in the inspector.
   - Save the panel as a prefab in `Assets/Prefabs`.

2. **Configure the Filter**:
   - Use the toggle to enable or disable the color blind filter.
   - Select the desired filter type from the dropdown menu.
   - Use the apply button to save the settings, or the reset button to revert to default settings.

3. **Preview**:
   - The panel includes a preview image that updates in real-time to show the effect of the selected filter.

This feature ensures that the game is more accessible to a wider audience, providing an inclusive gaming experience for all players.

## New Features

### Millennium Items

The game includes special items known as Millennium Items, each with unique abilities and effects. These items can be collected and used to enhance gameplay.

#### Available Items
- **Millennium Ring**: Detects nearby items and provides special abilities.
- **Millennium Puzzle**: Enhances strategic gameplay.
- **Millennium Eye**: Provides vision-related abilities.
- **Millennium Rod**: Offers control over certain game elements.
- **Millennium Key**: Unlocks special areas or features.
- **Millennium Scale**: Balances gameplay mechanics.
- **Millennium Necklace**: Provides protective abilities.

### Card Collection and Deck Building

Players can collect cards and build custom decks to use in the game. The system includes features for managing card quantities, rarity, and special abilities.

#### Features
- **Card Collection**: Track and manage collected cards.
- **Deck Building**: Create and customize decks for gameplay.
- **Card Rarity**: Different rarities affect card availability and power.
- **Special Abilities**: Unique effects based on card type and rarity.

### Level Editor

The game includes a level editor that allows players to create custom levels. This feature enables the community to share and play user-generated content.

#### Features
- **Grid System**: Place monsters, spells, and traps on a grid.
- **Wave Management**: Define enemy waves and spawn points.
- **Resource Settings**: Configure starting resources and difficulty.
- **Save and Load**: Save custom levels and load them for play.

### Status Effects and Card Effects

The game features a comprehensive system for status effects and card effects, enhancing strategic gameplay.

#### Status Effects
- **Poison**: Deals damage over time.
- **Stun**: Prevents actions for a duration.
- **Buff/Debuff**: Modifies stats temporarily.
- **Burn**: Deals fire damage.
- **Freeze**: Slows or stops movement.
- **Heal**: Restores health.
- **Shield**: Provides damage protection.
- **Speed Boost/Slow**: Modifies movement speed.

#### Card Effects
- **Monster Effects**: Special abilities when summoned.
- **Spell Effects**: Various spell types with unique effects.
- **Trap Effects**: Triggered effects for strategic play.

### Save and Load Functionality

The game includes a robust save and load system to ensure player progress is preserved.

#### Features
- **Player Data**: Save player stats, collection, and progress.
- **Settings**: Preserve game settings and preferences.
- **Achievements**: Track and save player achievements.
- **Auto-Save**: Automatic saving at key points in gameplay.

### Pathfinding System

The game includes a pathfinding system for enemy movement, ensuring that enemies can navigate the game world efficiently.

#### Features
- **Waypoint Navigation**: Enemies follow predefined paths.
- **Dynamic Pathfinding**: Adjusts paths based on game state.
- **Performance Optimization**: Uses efficient algorithms for path calculation.

### Object Pooling

The game uses object pooling to optimize performance, reducing the overhead of creating and destroying game objects.

#### Features
- **Efficient Resource Management**: Reuses objects instead of creating new ones.
- **Reduced Garbage Collection**: Minimizes memory allocation and deallocation.
- **Improved Performance**: Enhances game performance during intense gameplay.

### UI System

The game includes a comprehensive UI system for providing feedback on game state and player actions.

#### Features
- **Wave Information**: Displays current wave and timer.
- **Resource Management**: Shows player resources and health.
- **Card Management**: Manages player hand and deck.
- **Game Over Screen**: Provides feedback on game outcome.

### Targeting System

The game features a targeting system for strategic gameplay, allowing players to select and target enemies.

#### Features
- **Target Selection**: Choose enemies to target.
- **Priority Targeting**: Set priorities for targeting enemies.
- **Range Management**: Define targeting ranges for different abilities.

### Special Abilities

The game includes special abilities for different monster types, enhancing strategic gameplay.

#### Features
- **Unique Abilities**: Each monster type has unique abilities.
- **Ability Cooldowns**: Manage ability usage with cooldowns.
- **Visual and Audio Effects**: Provide feedback for ability usage.

### Visual and Audio Effects

The game includes visual and audio effects to enhance the gameplay experience.

#### Features
- **Particle Effects**: Visual feedback for actions and abilities.
- **Sound Effects**: Audio feedback for gameplay events.
- **Animation System**: Smooth animations for game objects.

These features enhance the overall gameplay experience, providing depth and replayability for players. 