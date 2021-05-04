# Overview

## NetworkMonoBehaviour

### Usage:

Inherit this class as you would a normal MonoBehaviour.

Photon's room properties are used for synchronisation of state.

This class will provide additional functions to send and receive Photon room properties. Synchronised properties are per-object, i.e. multiple instanced objects of the same class will synchronise separately.

Uses PhotonView to get a custom object instance designation. (Note: may be prone to bugs when instantiation, have not yet tested it)

This works in both singleplayer and multiplayer.
