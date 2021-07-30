# Overview

## Content State System

### Usage:

1. Attach `ContentStateManager` on a `GameObject`.
2. Attach any content state (e.g. `PopInContentState`, `BaseContentState`) to the `GameObject`s that are parent to each "scene"/"state" that you want to control.
3. Add all the content states into the `ContentStateManager`'s content state list.
4. Set the default content state index (if needed).
5. Profit!

### Notes:

- There is no need to disable all other content states unless absolutely required. The manager will handle them on `Awake()`.
- To create custom animated transitions, you can inherit `BaseAnimatedContentState`, which is just `BaseContentState` but with some common serialized animation fields (so you don't have to write them again).

## ExplodePolygonContentState

### Usage:

1. Create a material using the `PolygonExplode` shader.
2. Create a 3d model in the scene and apply the material (Icosphere model example is provided in ..\Shaders\ExplodePolygon folder).
3. Add `ExplodingPolygonController` on the object.
4. Add `ExplodePolygonContentState` to all content states that you wish to animate the transition with.
5. Assign the controller field in all `ExplodePolygonContentState` with the  `ExplodingPolygonController` you had created.
6. Add all the content states into the `ContentStateManager`'s content state list.
7. Profit!
