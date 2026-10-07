using Astra.Sdk;
using BepInEx;

namespace AstraPeak
{
    /// <summary>
    /// Astra for PEAK. The Astra foundation already makes her work here; this plugin makes her a
    /// climbing companion:
    /// <list type="bullet">
    /// <item>she accompanies YOUR scout (<c>Character.localCharacter</c>), never another player's;</item>
    /// <item>she is sized to your scout (she looked small at her own height);</item>
    /// <item>she walks the mountain on its terrain, waits at the foot of a wall while you climb, and
    /// LEAPS up to you once you stand on top, instead of appearing out of nowhere
    /// (<see cref="MountainBrain"/>);</item>
    /// <item>she tells her animation set the raw fact <c>climbing</c>; an animation pack decides what
    /// that looks like.</item>
    /// </list>
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(AstraSdk.Guid, AstraSdk.Dependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        void Awake()
        {
            var astra = AstraSdk.Register(MyPluginInfo.PLUGIN_GUID, "PEAK");
            astra.Defaults.MatchPlayerHeight = 0.95f;
            // PEAK's walkable world: its terrain, its map pieces and props; never the scouts or ropes.
            astra.Defaults.GroundLayers = "Terrain,Map,Default";
            // A mountain is tall: she leaps after you rather than appear, so let her be far first.
            astra.Defaults.TeleportDistance = 60f;
            astra.Defaults.FollowStart = 4f;
            astra.UsePlayer(_ => PeakPlayer.Locate());
            astra.UseBrain(new MountainBrain());
            astra.OnFrame(f => f.Params.Set("climbing", PeakPlayer.Climbing));
            Logger.LogInfo("Astra will climb PEAK with you");
        }
    }
}
