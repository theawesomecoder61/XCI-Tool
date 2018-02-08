# XCI-Tool
A Windows program that can view and extract the contents of a Nintendo Switch Cartridge Image (XCI)

## What this does
This tool can view information and the files within a Nintendo Switch Cartridge Image (XCI). The files are displayed in a tree view and each file can be exported. The first 10 MB of the selected file is shown in a hex viewer.

## Building
You will need:
* Visual Studio 2015 or later
* .NET Framework 4.5 or later

1. Clone the repo
2. Open the `.sln` file in Visual Studio
3. `Build > Build Solution`

## Unfortunately...
XCI Tool **can not decrypt** anything. You will need the decryption key to do so.

## Note
Some files in the XCI files can be ***gigabytes*** in size. I implemented (this hacky solution (hey, it works))[] for exporting them.

Also, as of now, the code is messy. In a future release, I will organize the code.

## Thanks to
* [Falo (on GBATemp)](https://pastebin.com/RMv2CW2H)
* [SwitchBrew](http://switchbrew.org/index.php?title=Gamecard_Format)
