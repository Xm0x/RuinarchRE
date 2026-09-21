# Assets

The code track recovers the game's C#. This track recovers the game's *content*:
prefabs, sprites, audio, and ScriptableObject data. That content is copyrighted
and is **never committed**; like `decompile.sh`, the extraction runs on your own
install and writes to a gitignored folder.

## Where assets live in the game

Ruinarch ships content in the usual Unity containers under `Ruinarch_Data/`:

- `sharedassets*.assets` / `.resS`, `resources.assets` / `.resS`: serialized
  objects (textures, meshes, audio clips, ScriptableObjects) and their raw data
  streams.
- `globalgamemanagers*`: the object/type registry and build settings.
- `level*`: serialized scenes.
- `StreamingAssets/`: files shipped verbatim (read at runtime by path).

Enum-driven content in the code (`TILE_OBJECT_TYPE`, `STRUCTURE_TYPE`, and so on)
is bound to these assets by name/id at load; reading the decompiled `src/`
alongside the extracted assets is how you map a type to its art and data.

## Extracting

```
tools/extract-assets.sh primary      # assets + decompiled Scripts (content tree)
tools/extract-assets.sh project      # a reconstructable Unity project
```

Output goes to `reference-assets/` (gitignored). Set `ASSETRIPPER` to your
[AssetRipper](https://assetripper.github.io/) executable if it is not at the
default path; the script drives its headless web API (load the install, export,
shut down).

- `primary` gives a raw content tree: a `Scripts/` dump plus assets laid out by
  source container. Good for browsing and pulling individual assets.
- `project` reconstructs a Unity project you can open in the editor, useful for
  total-conversion work.

A full export is a few hundred MB and takes a few minutes. Do not commit it.
