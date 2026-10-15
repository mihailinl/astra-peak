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
  like, so no code change is needed when one ships a climbing animation;
- **PEAK's own climb, for her** (opt-in): switch capture on for PEAK in Astra (Parameters › Games),
  then climb. Each climb of 1.5 to 60 seconds (at most three per game run) is recorded from your scout
  — from a moment after you grab on, so the jump at the wall is not in it, and without your looking
  around — and offered to Astra, which asks you whether to use it for `climbing` ([Use for climbing]
  / [Keep] / [Discard]). With capture off nothing is recorded. An Astra that tells a running game
  (`capture-live`) starts or stops capturing the moment you flip the switch, and switching it off
  mid-climb keeps nothing of that climb; with an older Astra the switch takes effect from the
  game's next start. A captured clip is PEAK's animation: it never leaves your machine, and Astra
  will not export it. Needs the Astra foundation 0.8.0 or newer (beside an older one this plugin is
  not loaded; BepInEx says so in its log).

Only you see her; your co-op friends do not. PEAK has no anti-cheat; never use BepInEx mods in
games that have one.

## Parameters

| name | type | meaning |
|---|---|---|
| `climbing` | bool | your scout is on a wall, a rope or a vine |

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

### Build against a local SDK

The capture (`CaptureWhile`) needs the Astra SDK **0.8.0** (the project pins `[0.8.0,1)`), which
is not released yet: the release workflow's SDK and foundation lines are TODOs until it is, and
`packages.lock.json` was made against a local pre-release build of 0.8.0 — regenerate it from the
released package (`dotnet restore --force-evaluate`) when the workflow moves. Until then, build
locally against astra-bepinex's own pack, in a NuGet cache of its own (a `-dev` package must never
meet a cached one of the same version) and without touching `packages.lock.json`:

```bash
# in astra-bepinex: build the foundation and the SDK package (artifacts/nuget/Astra.Unity.Sdk.0.8.0-dev.nupkg)
tools/pack.sh
# here:
NUGET_PACKAGES=$PWD/.nuget-local-cache dotnet build -c Release \
  -p:AstraSdkVersion=0.8.0-dev \
  -p:RestoreAdditionalProjectSources=<astra-bepinex>/artifacts/nuget \
  -p:NuGetLockFilePath=$PWD/obj/local.lock.json
```

The game needs the same foundation: copy `artifacts/foundation-mono/BepInEx/plugins/Astra/` from
astra-bepinex over the one in your BepInEx profile.

## Contributing

PRs welcome. The rules are the foundation's:

- send raw facts, never animation names for movement;
- never touch rendering;
- keep the plugin small.

`MountainBrain.cs` is where her movement on the mountain lives.

License: MIT.
