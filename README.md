# Astra for PEAK

A game integration that makes **Astra** climb [PEAK](https://store.steampowered.com/app/3527290/PEAK/)
with you. It runs on the Astra Unity foundation (`astra-bepinex`), which already draws her inside the
game, lit by its sun and hidden by its rocks. This plugin adds what is specific to PEAK:

- she accompanies **your** scout (`Character.localCharacter`), never another player's;
- she is **sized to your scout** (95% of its height) instead of her own, which looked small here;
- she **walks the mountain** on its terrain, **waits at the foot of a wall** while you climb a wall,
  a rope or a vine, and **leaps up to you** in a real arc once you stand on top, instead of
  appearing out of nowhere;
- she tells her animation set the raw fact `climbing`. An animation pack decides what that looks
  like, so no code change is needed when one ships a climbing animation.

Only you see her; your co-op friends do not. PEAK has no anti-cheat; never use BepInEx mods in
games that have one.

## Install (by hand, until the Astra marketplace installs it)

1. BepInEx 5 (x64) in the PEAK folder, from a mod manager ("BepInExPack" for PEAK) or by hand.
2. The Astra foundation in `PEAK/BepInEx/plugins/Astra/`.
3. This plugin, `AstraPeak.dll` and `astra-item.json`, in `PEAK/BepInEx/plugins/AstraPeak/`.
4. On Linux (Proton), the Steam launch option `WINEDLLOVERRIDES="winhttp=n,b" %command%`.

The Astra app must be running: she leaves your desktop for the game when PEAK starts, and comes
back when it closes.

## Build

```bash
dotnet build -c Release -p:GameDir="/path/to/PEAK"
```

You can also put the path in `GameDir.props.user` (git ignores it). The game's assemblies are
referenced to compile and are never shipped.

## Contributing

PRs welcome. The rules are the foundation's:

- send raw facts, never animation names for movement;
- never touch rendering;
- keep the plugin small.

`MountainBrain.cs` is where her movement on the mountain lives.

License: MIT.
