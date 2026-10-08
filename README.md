# Astra for PEAK

A game integration that makes **Astra** climb [PEAK](https://store.steampowered.com/app/3527290/PEAK/)
with you. It runs on the Astra Unity foundation (`astra-bepinex`), which already draws her inside the
game, lit by its sun and hidden by its rocks. This plugin adds what is specific to PEAK:

- she accompanies **your** scout (`Character.localCharacter`), never another player's;
- she is **twice her own size** here (PEAK's world is built bigger; `Astra.Scale` in the config overrides it);
- she **walks the mountain** on its terrain, **waits at the foot of a wall** while you climb a wall,
  a rope or a vine, and **leaps up to you** in a real arc once you stand on top, instead of
  appearing out of nowhere;
- she tells her animation set the raw fact `climbing`. An animation pack decides what that looks
  like, so no code change is needed when one ships a climbing animation.

Only you see her; your co-op friends do not. PEAK has no anti-cheat; never use BepInEx mods in
games that have one.

## Install with Astra

Paste this repository's address into Astra's **Games** tab:

```
https://github.com/mihailinl/astra-peak
```

Astra fetches the latest release's `astra-gi.zip`, installs it into a profile of its own and
launches PEAK with it when you press Play — steps 1-4 below, done for you, including the Proton
override. Integrations are not reviewed by Astra and are always shown as **Experimental**.

## Install (by hand, until the Astra marketplace installs it)

1. BepInEx 5 (x64) in the PEAK folder, from a mod manager ("BepInExPack" for PEAK) or by hand.
2. The Astra foundation in `PEAK/BepInEx/plugins/Astra/`.
3. This plugin, `AstraPeak.dll` and `astra-item.json`, in `PEAK/BepInEx/plugins/AstraPeak/`.
4. On Linux (Proton), the Steam launch option `WINEDLLOVERRIDES="winhttp=n,b" %command%`.

The Astra app must be running: she leaves your desktop for the game when PEAK starts, and comes
back when it closes.

## Build

No game folder needed: every game type (`Character`, `CharacterData`) is reached by NAME at run
time through the Astra SDK's `Astra.Sdk.GameType`, never compiled against — this builds from
public packages alone (the Astra SDK, `BepInEx.Core`, `UnityEngine.Modules`).

```bash
dotnet build -c Release
```

A tagged push (`v*`) builds and publishes `astra-gi.zip` to a GitHub release through
`.github/workflows/release.yml`. To build the same zip locally (no CI, for testing the layout):

```bash
# once: unpack the pinned BepInEx release beside this repo's own Release build
curl -LO https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.5/BepInEx_win_x64_5.4.23.5.zip
unzip BepInEx_win_x64_5.4.23.5.zip -d bepinex-dist
# build the Astra foundation once (astra-bepinex/tools/pack.sh), then:
tools/make-astra-gi.sh
```

To build against a local, unreleased Astra SDK before it is on nuget.org: add a
`GameDir.props.user` (git-ignored) setting `RestoreAdditionalProjectSources` to the SDK's local
feed path, or uncomment the `astra-local` source in `nuget.config`.

## Contributing

PRs welcome. The rules are the foundation's:

- send raw facts, never animation names for movement;
- never touch rendering;
- keep the plugin small.

`MountainBrain.cs` is where her movement on the mountain lives.

License: MIT.
