
# PKCS11 Explorer

## What is it?
It's an app that will allow you to see currently plugged-in PKCS11 slots/tokens (providing the middleware needed, of course).
At a later point i would like to allow operations on tokens, but currently we're still in early development.

![screenshot](https://fladnag.net/wp-content/uploads/2019/08/Capture3.png)

## What is it complatible with?

### Desktop operating systems
Currently, we're compatible with:
- Windows x64
- macOS x64
- GNU/Linux x64
*There is also a GNU/Linux ARM build, but I doubt it works well...*

### Readers and Tokens supported
- Open Smardcard compatible cards (OpenSC, Nitrokey HSM 1 and 2)


## What's behind this?
This app is developed in .NET Core using C# and XAML.

We are also using the following libraries:
- Avalonia UI for crossplatform desktop app: https://github.com/AvaloniaUI/Avalonia
- PKCS11Interop for PKCS11 operations: https://github.com/Pkcs11Interop/Pkcs11Interop
- Material icons for in-app icons: https://github.com/google/material-design-icons

## Having a problem? Wants to contribute?
Please open an issue, or ask for PR :)