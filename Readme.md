# GTA V Bus Mod

A realistic bus transportation mod for Grand Theft Auto V that allows players to run passenger pickup and delivery missions across Los Santos and Blaine County.

## 🚌 Overview

The GTA V Bus Mod transforms your gameplay into a public transportation simulator. Pick up passengers at designated locations, drive them safely to their destinations, and earn money for successful deliveries. Miss your route or endanger your passengers, and face the consequences!

## ✨ Features

- **Custom Mission System**: XML-based mission configuration for easy customization
- **Realistic Passenger AI**: Passengers react to your driving and interact with the vehicle
- **Dynamic Route System**: Visual route indicators guide you to pickup points and destinations
- **Money Rewards/Penalties**: Earn money for successful deliveries, lose money for failures
- **Interactive Menu**: Easy-to-use LemonUI menu system for mission selection
- **Multiple Missions**: Support for unlimited custom missions via XML configuration

## 📋 Requirements

- **Grand Theft Auto V** (PC version)
- **ScriptHookV** - [Download here](http://www.dev-c.com/gtav/scripthookv/)
- **ScriptHookVDotNet v3** - [Download here](https://github.com/scripthookvdotnet/scripthookvdotnet)
- **LemonUI** - [Download here](https://github.com/LemonUIbyLemon/LemonUI)
- **.NET Framework 4.8** or higher

## 🔧 Installation

1. **Install Dependencies**:
   - Install ScriptHookV.
   - Install ScriptHookVDotNet.
   - Install LemonUI.

2. **Install the Mod**:
   - Download the latest release from the [Releases](../../releases) page
   - Extract all files to your `GTA V/scripts/` directory
   - Your folder structure should look like:
     ```
     GTA V/
     ├── scripts/
     │   ├── GtaVBusMod.dll
     │   ├── bus_mod.ini
     │   └── bus_mod_missions.xml
     ```

3. **Configure the Mod**:
   - Open `bus_mod.ini` and set your preferred activation key:
     ```ini
     Key=F7
     ```

## 🎮 How to Play

1. **Launch GTA V** and load into the game
2. **Press your activation key** (default: F7) to open the bus mod menu
3. **Select a mission** from the "Missions List" dropdown
4. **Click "Start"** to begin the mission
5. **Follow the on-screen instructions**:
   - Drive to the bus (marked with a Cab icon)
   - Drive to the passenger pickup location (marked with Friend icons)
   - Stop near passengers and **honk your horn** to pick them up
   - Drive to the destination (marked with a blue blip)
   - Stop at the destination and **honk your horn** to complete delivery

## 🎯 Mission Success/Failure

### ✅ Success Conditions
- Pick up all passengers at the designated location
- Transport them safely to the destination
- No passenger or vehicle deaths

### ❌ Failure Conditions
- Any passenger dies during the mission
- The bus vehicle is destroyed
- Mission is manually canceled

## 🛠️ Creating Custom Missions

Missions are defined in `bus_mod_missions.xml`. Here's the structure:

```xml
<?xml version="1.0" encoding="utf-8"?>
<missions>
  <element>
    <name>Mission Name</name>
    <description>Mission description^Use ^ to separate lines</description>
    <money>500</money>
    
    <!-- Pedestrian(s) -->
    <ped>
      <hash>-408329255</hash> <!-- Ped model hash -->
      <position>
        <x>-1037.0</x>
        <y>-2730.0</y>
        <z>13.7</z>
        <t>240.0</t> <!-- Heading/rotation -->
      </position>
    </ped>
    
    <!-- Add more peds as needed -->
    
    <!-- Vehicle -->
    <vehicle>
      <hash>bus</hash> <!-- Vehicle model name or hash -->
      <position>
        <x>-1050.0</x>
        <y>-2750.0</y>
        <z>13.9</z>
        <t>150.0</t>
      </position>
    </vehicle>
    
    <!-- Destination -->
    <destination>
      <position>
        <x>200.0</x>
        <y>-800.0</y>
        <z>31.0</z>
        <t>0.0</t>
      </position>
    </destination>
  </element>
</missions>
```

### Finding Coordinates
Use a trainer or coordinate display tool to find locations in-game. The `t` value represents the heading (0-360 degrees).

### Finding Model Hashes
- Ped hashes: [GTA V Ped List](https://gist.github.com/SunShineSilver-mdA/77c2307823bac44371e105755e9edbd5) 
  <br><b>Note</b>: This may be changed to hexadecimal later - I wrote this 11 years ago, can't remember where I got the hashes as int back then.
- Vehicle names: Use common vehicle names like "bus", "coach", etc.

## 🏗️ Project Structure

```
GtaVBusMod/
├── BusMod.cs                    # Main script entry point
├── Mission.cs                   # Mission logic and state management
├── Constants.cs                 # Configuration constants
└── Services/
    ├── XmlMissionDataService.cs # XML parsing and data retrieval
    └── PedestrianManager.cs     # Pedestrian lifecycle management
```

## 🔨 Building from Source

### Prerequisites
- Visual Studio 2019 or later
- .NET Framework 4.8 SDK

### Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/gtav-bus-mod.git
   ```

2. Open `GtaVBusMod.sln` in Visual Studio

3. Restore NuGet packages

4. Add references to (via nuget):
   - ScriptHookVDotNet.dll
   - LemonUI.dll

5. Build the solution (Release configuration)

6. Copy the compiled `GtaVBusMod.dll` to your `GTA V/scripts/` folder

## 🐛 Known Issues

- Pedestrians may occasionally clip through the vehicle during pickup
- Route display may flicker when switching between multiple waypoints
- Some vehicle models may not spawn correctly depending on game DLC content

## 📝 To-Do / Future Enhancements

- [ ] Add support for multiple simultaneous pickups/dropoffs
- [ ] Implement a reputation/level system
- [ ] Add time-based challenges (deliver within X minutes)
- [ ] Create passenger dialogue system
- [ ] Add random event spawns during missions
- [ ] Implement a route planning system
- [ ] Add support for custom bus liveries

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

### Development Guidelines
- Follow the existing code style and naming conventions
- Add XML documentation comments to all public methods
- Test your changes thoroughly before submitting
- Update the README.md if you're adding new features

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Credits

- **Original Author**: scorz
- **ScriptHookV**: Alexander Blade
- **ScriptHookVDotNet**: crosire
- **LemonUI**: Lemon

## 📧 Contact

For bug reports and feature requests, please use the [GitHub Issues](../../issues) page.

---

**Disclaimer**: This mod is not affiliated with or endorsed by Rockstar Games or Take-Two Interactive. Grand Theft Auto and GTA are registered trademarks of Take-Two Interactive Software.
