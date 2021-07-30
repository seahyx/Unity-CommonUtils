# Overview

## PolygonExplodeShader

### Usage:

1. Apply the material to any 3d model.
2. Adjust the object height field on the material to fit the object.

### Notes:

- Can be used with `ExplodingPolygonController` in "..\Content State System" to facilitate animating the animation progress via code.
- Is used by `ExplodePolygonContentState` in "..\Content State System" to animate content state transitions.
- Iridescence and environment colouring effects only apply on the wire portion of the shader.

### Dependencies:

- `Easings.cginc` for interpolation functions.
