# osu! PP tools [![Build status](https://ci.appveyor.com/api/projects/status/9wh7855bvj2dcyq7?svg=true)](https://ci.appveyor.com/project/HoLLy-HaCKeR/osu-pp-tools/build/artifacts)

A tool providing a live PP display for both players and mappers.

## Usage

### For players
Just have the tool open when in game. It will show you the PP values for the currently playing song (for various mod combinations and accuracies).

### For mappers
Drag either your .osu file or the folder containing your .osu file on the program to launch it. It will update the display every time you save your map.

## Building
Recursively clone, place an osu!.exe in the References folder, restore nuget packages, and build using Visual Studio 2017+.
