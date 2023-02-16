using System.Security.Permissions;
using BepInEx;
using UnityEngine;

//TODO
// don't throw slugcat from you back when Option_SlugOnBack is enabled;



// temporary fix // should be added automatically //TODO
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace CoopTweaks
{
    [BepInPlugin("SchuhBaum.CoopTweaks", "CoopTweaks", "0.0.1")]
    public class MainMod : BaseUnityPlugin
    {
        //
        // meta data
        //

        public static readonly string MOD_ID = "CoopTweaks";
        public static readonly string author = "SchuhBaum";
        public static readonly string version = "0.0.1";

        //
        // options
        //

        public static bool Option_DeafBeep => MainModOptions.deafBeep.Value;
        public static bool Option_ItemBlinking => MainModOptions.itemBlinking.Value;
        public static bool Option_ReleaseGrasp => MainModOptions.releaseGrasp.Value;

        public static bool Option_RegionGates => MainModOptions.regionGates.Value;
        public static bool Option_SlugcatCollision => MainModOptions.slugcatCollision.Value;
        public static bool Option_SlugOnBack => MainModOptions.slugOnBack.Value;

        //
        // other mods
        //

        public static bool isSBCameraScrollEnabled = false;

        //
        // variables
        //

        public static bool isInitialized = false;

        //
        // main
        //

        public MainMod() { }
        public void OnEnable() => On.RainWorld.OnModsInit += RainWorld_OnModsInit; // look for dependencies and initialize hooks

        //
        // private
        //

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld rainWorld)
        {
            orig(rainWorld);

            MachineConnector.SetRegisteredOI(MOD_ID, MainModOptions.instance);

            if (isInitialized) return;
            isInitialized = true;

            Debug.Log("CoopTweaks: version " + version);

            ArtificialIntelligenceMod.OnEnable();
            RainWorldGameMod.OnEnable();
        }
    }
}