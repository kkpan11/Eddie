## Build

Use script in /repository to build installer and packages.

## Projects

- App.CLI.Linux, App.CLI.MacOS, App.CLI.Windows
Main Application, CLI edition. 
Language: C#, net7
Editor: Visual Studio Code, Visual Studio

- Lib.Core
Main library
Language: C#, net4 & net7

- Lib.Platform.Linux and Lib.Platform.Linux.Native
Libraries for Linux-specific code. Referenced by Lib.Core library.
Language: C# the base, C++ the native.

- Lib.Platform.MacOS and Lib.Platform.MacOS.Native
Libraries for MacOS-specific code. Referenced by Lib.Core library.
Language: C# the base, C++ the native.

- Lib.Platform.Windows and Lib.Platform.Windows.Native
Libraries for Windows-specific code. Referenced by Lib.Core library.
Language: C# the base, C++ the native.

- App.CLI.Linux.Elevated, App.CLI.MacOS.Elevated, App.CLI.Windows.Elevated, App.Service.Windows.Elevated
Application with specific code that need to run with elevated privileges.
App.Service.Windows.Elevated is the service edition of App.CLI.Windows.Elevated.
Language: C++

- Lib.CLI.Elevated
Library, base for App.*.Elevated projects
Language: C++
Editor: files only

# Projects: UI 2.x, based on net4

- App.Cocoa.MacOS
Main Application for MacOS
Language: C#, Mono, Xamarin.Mac
Editor: Visual Studio for Mac (deprecated in 2024)

- App.Forms.Windows, App.Forms.Linux, App.Forms.Linux.Tray
Main Application for Linux and Windows. App.Forms.Linux.Tray is a small helper (written in C++) for tray under Linux.
Language: C#, Mono
Editor: Visual Studio

- Lib.Forms, Lib.Forms.Skin
Library for the UI written in Net 4.8 / Mono, used by App.Forms.Linux and App.Forms.Windows
Language: C#
Editor: Visual Studio

# Projects: UI 3.x, under development, not released yet on public GitHub.

- App.UI.Linux
App UI for MacOS.
Language: C++, GTK
Editor: Visual Studio Code

- App.UI.MacOS
App UI for MacOS.
Language: C++, Objective-C, Cocoa
Editor: Visual Studio Code

- App.UI.Windows
App UI for Windows.
Language: C++, WinForms
Editor: Visual Studio Code and Visual Studio

- Lib.UI
Library, base for App.UI.* projects
Language: C++
Editor: files only

# Projects: Utils

- App.Checking      
Utils, App for development checking and adapt code
Language: C#
Editor: Visual Studio Code

# Other files in src directory

- linux_clean.sh, macos_clean.sh, win_clean.bat
Script to clean obj/bin directory, to avoid messup between net4 and net7.

- .editorconfig
net7 C# ruleset for code checking and style.

- ruleset/*
net4 C# ruleset, not maintained anymore.



## Tags used in sources codes:

// TOTRANSLATE
  Need a message in Messages.cs

// TODO
  Need an implementation

// TOOPTIMIZE
  May need optimization

// TOCLEAN
  Deprecated code that will be deleted

// TOFIX
  Knows issues that will be addressed in the next releases

// TOCHECK
  Need an investigation

// TOTEST
  Need a detailed test

// TOCONTINUE
  Under implementation

// WIP
  Missing implementation that will be addressed in the next releases

// EDDIE_DOTNET preprocessor directive
  Used to differentiate between net4 and net7 code

