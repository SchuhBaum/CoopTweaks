using BepInEx;
using MonoMod.Cil;
using System.Security.Permissions;
using UnityEngine;

// allows access to private members;
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace CoopTweaks;

[BepInPlugin("SchuhBaum.CoopTweaks", "CoopTweaks", "0.1.6")]
public class MainMod : BaseUnityPlugin {
    //
    // meta data
    //

    public static readonly string MOD_ID = "CoopTweaks";
    public static readonly string author = "SchuhBaum";
    public static readonly string version = "0.1.6";

    //
    // options
    //

    public static bool Option_ArtificerStun => MainModOptions.artificerStun.Value;
    public static bool Option_DeafBeep => MainModOptions.deafBeep.Value;
    public static bool Option_ItemBlinking => MainModOptions.itemBlinking.Value;
    public static bool Option_ReleaseGrasp => MainModOptions.releaseGrasp.Value;

    public static bool Option_RegionGates => MainModOptions.regionGates.Value;
    public static bool Option_SlowMotion => MainModOptions.slowMotion.Value;
    public static bool Option_SlugcatCollision => MainModOptions.slugcatCollision.Value;
    public static bool Option_SlugOnBack => MainModOptions.slugOnBack.Value;

    //
    // other mods
    //

    public static bool is_sb_camera_scroll_enabled = false;

    //
    // variables
    //

    public static bool is_initialized = false;
    public static bool can_log_il_hooks = false;

    //
    // main
    //

    public MainMod() { }
    public void OnEnable() => On.RainWorld.OnModsInit += RainWorld_OnModsInit; // look for dependencies and initialize hooks

    //
    // public
    //

    public static void LogAllInstructions(ILContext context, int indexStringLength = 9, int opCodeStringLength = 14) {
        if (context == null) return;

        Debug.Log("-----------------------------------------------------------------");
        Debug.Log("Log all IL-instructions.");
        Debug.Log("Index:" + new string(' ', indexStringLength - 6) + "OpCode:" + new string(' ', opCodeStringLength - 7) + "Operand:");

        ILCursor cursor = new(context);
        ILCursor labelCursor = cursor.Clone();

        string cursorIndexString;
        string opCodeString;
        string operandString;

        while (true) {
            // this might return too early;
            // if (cursor.Next.MatchRet()) break;

            // should always break at some point;
            // only TryGotoNext() doesn't seem to be enough;
            // it still throws an exception;
            try {
                if (cursor.TryGotoNext(MoveType.Before)) {
                    cursorIndexString = cursor.Index.ToString();
                    cursorIndexString = cursorIndexString.Length < indexStringLength ? cursorIndexString + new string(' ', indexStringLength - cursorIndexString.Length) : cursorIndexString;
                    opCodeString = cursor.Next.OpCode.ToString();

                    if (cursor.Next.Operand is ILLabel label) {
                        labelCursor.GotoLabel(label);
                        operandString = "Label >>> " + labelCursor.Index;
                    } else {
                        operandString = cursor.Next.Operand?.ToString() ?? "";
                    }

                    if (operandString == "") {
                        Debug.Log(cursorIndexString + opCodeString);
                    } else {
                        opCodeString = opCodeString.Length < opCodeStringLength ? opCodeString + new string(' ', opCodeStringLength - opCodeString.Length) : opCodeString;
                        Debug.Log(cursorIndexString + opCodeString + operandString);
                    }
                } else {
                    break;
                }
            } catch {
                break;
            }
        }
        Debug.Log("-----------------------------------------------------------------");
    }

    //
    // private
    //

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld rainWorld) {
        orig(rainWorld);
        MachineConnector.SetRegisteredOI(MOD_ID, MainModOptions.main_mod_options);

        if (is_initialized) return;
        is_initialized = true;
        Debug.Log("CoopTweaks: version " + version);

        foreach (ModManager.Mod mod in ModManager.ActiveMods) {
            if (mod.id == "SBCameraScroll") {
                is_sb_camera_scroll_enabled = true;
                break;
            }
        }

        if (!is_sb_camera_scroll_enabled) {
            Debug.Log("CoopTweaks: SBCameraScroll not found.");
        } else {
            Debug.Log("CoopTweaks: SBCameraScroll found. Synchronize shortcut position updates when mushroom effect is active.");
        }

        ArtificialIntelligenceMod.OnEnable();
        ProcessManagerMod.OnEnable();
    }
}
