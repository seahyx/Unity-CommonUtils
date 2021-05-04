# Common Utility Scripts for Unity

A unity library for many useful scripts that is targeted towards development of content for the Hololens 2 with MRTK.

> The _Core folder includes scripts that used as a dependency for other scripts. Be sure to import that folder in first.

Most scripts are plug and play, just drop them in and they should work. ;-)

## Overview

- ### _Core

	- Contains the base networking class that some network-enabled scripts will use.
	- Includes `NetworkMonoBehaviour`, a useful modified version of the `MonoBehaviour` to make implementing synchronised networking features easy.

- ### Content State System

	- A simple content state management system for scene-like transitions and flow. When you don't need something complex.
	- Network-capable, easily extensible and customizable.
	- Will require _Core scripts.

- ### Debugging

	- Various debugging tools.

- ### General

	- Various useful scripts to accomplish common tasks such as object scaling, transform resetting, etc.

- ### MRTK Interactable Sprite Theme Engine

	- A workaround interactable sprite theme engine for when you prefer to use sprite icons that can be changed using interactable profiles.
	- As this is a workaround, there may be visual bugs.

- ### Networking

	- Various useful scripts to accomplish common networking tasks.

- ### Pie Chart

	- Simple pie chart generator.

- ### Saving

	- Save/loading system.

## Dependencies

Some scripts require Odin Serializer for the inspector tools.

Some scripts require Photon for networking capabilities.

## Bug-reporting

Please report any bugs to the issue tracker. Any code contributions will be welcome! :DD

## License

MIT License
