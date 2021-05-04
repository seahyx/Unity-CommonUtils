# Overview

## Saving System

### Usage:

1. Modify `SaveData` to contain the serializable values that you want to save.
2. Attach `SaveDataManager` to a persistent `GameObject` in the scene.
3. Call upon `SaveGame()` and `LoadSave()` from the manager to save/load data.

### Notes:

Currently only supports a single save. Save file name and file location are constants. They can be found in `SaveDataManager`.
