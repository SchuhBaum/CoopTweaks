using UnityEngine;

namespace CoopTweaks
{
    public static class RainWorldGameMod
    {
        internal static void OnEnable()
        {
            On.RainWorldGame.ctor += RainWorldGame_ctor;
            On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;
        }

        //
        // private
        //

        private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame game, ProcessManager manager)
        {
            Debug.Log("ReducedSlowMotion: Add option specific hooks.");
            MainModOptions.instance.MainModOptions_OnConfigChanged();

            PlayerMod.OnToggle();
            RegionGateMod.OnToggle();
            RoomMod.OnToggle();

            orig(game, manager);
        }

        private static void RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame game)
        {
            Debug.Log("ReducedSlowMotion: Remove option specific hooks.");
            orig(game);

            PlayerMod.OnToggle();
            RegionGateMod.OnToggle();
            RoomMod.OnToggle();
        }
    }
}