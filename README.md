# Detour

A Unity + .NET Core framework for asynchronous multiplayer games over WebSockets.

## Features

* A fast WebSockets server designed directly for gaming in .NET Core, with or without ASP.NET
* A killer Unity client library designed to work with the Detour server, on any build target (even WebGL!)
* Share code (like message types and gameplay logic) between the server and the client

## FAQs

* Is Detour suitable for realtime games?
  * Realtime multiplayer games are more suited to binary streams than passing JSON back and forth for various reasons, and the founder of this project contributes to [Mirror](https://github.com/vis2k/Mirror), a Unity framework designed for this task that is much better suited to it.
  
  That being said, I'd be psyched if someone would try!
* Can it be used (*with/without*) ASP.NET Core?
  * Yes! Detour can be used standalone in a .NET Core console app or with ASP.NET Core. Our goal with the ASP.NET Core middleware version of the library is to eventually provide a killer front-end for checking server status, managing load, etc, but the standalone library will be provided and supported for as long as it makes sense.
* Does Detour work on (*insert Unity build target*)?
  * Yes! While I do not currently have access to console SDKs, so far the only build target that provides any resistance is WebGL, which cannot use System.Net.Sockets. Detour provides its own JavaScript library (*JSLIB* in Unity parlance) to "bridge the gap" and connect to a Detour server using a WebGL client.
* Does Detour scale?
  * We'll find out together, but there's no reason it shouldn't.
* How should I report and document Detour issues?
  * Fill out an Issue on GitHub with your OS version, Unity version, Detour version and .NET Core version. If you can provide one or more reproduction projects to view the bug, that is all the better.