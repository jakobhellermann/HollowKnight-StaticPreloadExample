# Static Preload Example Mod

This is a mod demonstrating the usage of [unity-scene-repacker](https://github.com/jakobhellermann/unity-scene-repacker) with hollow knight.

The idea is - instead of doing the regular preloading process at runtime - to repack only the objects from the game you're interested in into an [`AssetBundle`](https://docs.unity3d.com/6000.1/Documentation/Manual/AssetBundlesIntro.html) that you can then load at runtime at a fraction of the cost.

## Usage

In [Resources/bundle.objects.jsonc](./Resources/bundle.objects.jsonc) you can find the definition of objects you want to load:

```jsonc
{
 "Fungus1_01": [
  "Moss Walker (3)",
  "Plant Trap",
  "Mossman_Shaker (1)",
  "Pigeon (1)",
  "Mossman_Runner"
 ],
 "Dream_Final_Boss": [
  "Boss Control/Radiance"
 ],
}
```

To generate the assetbundle in [Resources/bundle.unity3d](./Resources/bundle.unity3d), simply run

```sh
uvx unity-scene-repacker \
    --steam-game "Hollow Knight"
    --objects "Resources/bundle.objects.jsonc" \
    --output Resources/bundle.unity3d \
    --bundlename examplemod
```
or `just` to run from the [`justfile`](./just) if you have [just](https://github.com/casey/just) installed.

If you don't have [uv](https://astral.sh/blog/uv), you can also compile and install the tool from source using `cargo install --git https://github.com/jakobhellermann/unity-scene-repacker`.

```sh
ℹ Detected game 'Hollow Knight' at '/home/jakob/.local/share/Steam/steamapps/common/Hollow Knight'
ℹ Repacking 325 objects in 158 scenes
ℹ Pruned 761244 -> 16364 objects
ℹ 362.52 MiB -> 20.70 MiB raw size

✔ Repacked into Resources/bundle.unity3d (5.88 MiB) in 491.13ms
```

The bundle is embedded into the mod dll using
```xml
<ItemGroup>
    <EmbeddedResource Include="../Resources/bundle.unity3d" />
</ItemGroup>
```

The [`BundleSpawner`](./Source/BundleSpawner.cs) file shows an example of loading the object and consecutively spawning every object contained in it, but you're flexible in when and how you use the bundle.