# OculusExplore
OculusExplore is virtual reality based project that allows users to explore VR in 3 differing modes using. The project leverages several technologies including the Oculus Rift, the Microsoft Kinect, the Unity game engine, as well as Google Streetview and its respective public API. The first mode allows users to load existing VR content such as video or images, in both 2D and 3D content, and visualize it. The second mode allows users to view 2D streetview images using the Oculus Rift and navigate throughout them. The third mode allows users to load a mesh from the Microsoft Kinect and visualize it within a game world, all in VR.

## Description

Implements three different modes:

* VR player capable of loading and playing VR videos, images. Currently supports OGV format in Over/Under style.

* Streetview viewer

* Kinect viewer (requires Microsoft Kinect and Kinect for Windows SDK 2.0)

## Setup

To run the project, some software is required:
* Oculus SDK
* Microsoft Kinect SDK (for mode #2)
* Unity v5.6 or higher
* Google API account and Google Streetview API key is required for full navigational StreetView functionality (mode #3).

A config file can be provided to give access the data required. It should be placed in : `OculusExplore/Config/config.ini`

With the following format:

```
[streetview]
API_KEY=<api_key_here>
LAT=<starting_latitude_here>
LNG=<starting_longitude_here>
[video]
VIDEO=<video_path_here> *
IMAGES=<image_path_here>*
MONO=<run in mono mode, true or false>
```

*Note: this is a relative path starting from OculusExplore/Assets/Resources ! Files MUST be in this directory if they are to be loaded using the config (this is a Unity limitation ). Likewise, paths should ONLY use slash ‘ / ‘, NOT backslash ‘ \ ‘ , and also omit any file extensions. Only provide either a IMAGE path or a VIDEO path. Flag mono if applicable. Currently, only Over Under format is supported for 3D VR.*

The Main project scene can subsequently be run by executing: `OculusExplore/Assets/Scenes/MainMenu.scene`
