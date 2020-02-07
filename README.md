# com.pixelwizards.shotassembly

## About this Project

Shot Assembly Wizard for procedurally generating Timeline sequences from a set of animations, 
and optionally to export the resulting scene as a .unitypackage

You can find the Wizard via Window->Sequencing->Shot Assembly Wizard

![Main Window](Documentation~/MainWindow.png)

## Future Tasks

* API to allow for pipeline integration / CLI operation
* optionally use an existing open scene 
* optionally use an existing Playable Director / Timeline sequence to generate the animation tracks
* optionally create the camera / cinemachine brain 

## Installation

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

