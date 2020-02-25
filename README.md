# com.pixelwizards.shotassembly

[![openupm](https://img.shields.io/npm/v/com.pixelwizards.shotassembly?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.pixelwizards.shotassembly/)

## About this Project

Shot Assembly Wizard for procedurally generating Timeline sequences from a set of animations, 
and optionally to export the resulting scene as a .unitypackage

Can use an existing Timeline in a scene or optionally generate a new one.

You can find the Wizard via Window->Sequencing->Shot Assembly Wizard

![Main Window](Documentation~/images/ShotAssemblyWindow.png)

## Future Tasks

* API to allow for pipeline integration / CLI operation
* optionally create the camera / cinemachine brain 

## Installation

### Install via OpenUPM

The package is available on the [openupm registry](https://openupm.com). It's recommended to install it via [openupm-cli](https://github.com/openupm/openupm-cli).

```
openupm add com.pixelwizards.shotassembly
```

### Install via git URL

To install this package, you need to edit your Unity project's `Packages/manifest.json` and add this repository as a dependency. You can also specify the commit hash or tag like this:

```json
{
  "dependencies": {
    "com.pixelwizards.shotassembly": "https://github.com/PixelWizards/com.pixelwizards.shotassembly.git",
   }
}
```

## Prerequistes
---------------
* This has been tested for `>= 2018.3`

## Content
----------------

### Tools

* Window/Sequencing/Shot Assembly Wizard

### Required dependencies
---------------
* Timeline
* Cinemachine

