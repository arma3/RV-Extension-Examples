# What is this?
A template project for writing Arma 3 extensions in .NET Core, using ILCompiler provided by CoreRT

# Author
CoreRT https://github.com/dotnet/corert
Dardo https://github.com/DardoTheMaster

## How to Setup
1) Install .NET Core SDK and dependencies from Microsoft website ( https://dotnet.microsoft.com/download )

2) Open a terminal in the project directory

3) Install the native compiler
```bash
dotnet add package Microsoft.DotNet.ILCompiler -v 1.0.0-alpha-* 
```
4) Compile using

  ```bash
> dotnet publish /p:NativeLib=Shared -r win-x64|linux-x64
```
5) Grab your native compiled library
```bash
/bin/Debug/netstandard2.0/linux-x64/native/dotnet_a3_x64.so
/bin/Debug/netstandard2.0/win-x64/native/dotnet_a3_x64.dll
```
Now the library is ready to be loaded from Arma servers / clients.

## Why to use?
Obviously this won't never run faster than native c/c++ code. The only thing you'll really get, is that you'll be able to compile an extension for both windows and linux without touching in any ways your code.

## Notes
- using '-c Release' flag while compiling will drastically reduce dll/so size
- sometimes compiling will take a lot of time
