using BepInEx;
using MonoMod.Cil;
using System.Security.Permissions;
using UnityEngine;

// allows access to private members;
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace CoopTweaks;

[BepInPlugin("SchuhBaum.CoopTweaks", "CoopTweaks", "0.1.9")]
public class MainMod : BaseUnityPlugin {
    //
    // meta data
    //

    public static readonly string mod_id = "CoopTweaks";
    public static readonly string author = "SchuhBaum";
    public static readonly string version = "0.1.9";

    //
    // options
    //

    public static bool Option_ArtificerStun => MainModOptions.artificer_stun.Value;
    public static bool Option_DeafBeep => MainModOptions.deaf_beep.Value;
    public static bool Option_ItemBlinking => MainModOptions.item_blinking.Value;
    public static bool Option_ReleaseGrasp => MainModOptions.release_grasp.Value;

    public static bool Option_RegionGates => MainModOptions.region_gates.Value;
    public static bool Option_SlowMotion => MainModOptions.slow_motion.Value;
    public static bool Option_SlugcatCollision => MainModOptions.slugcat_collision.Value;
    public static bool Option_SlugOnBack => MainModOptions.slug_on_back.Value && !is_slugpup_safari_enabled;

    //
    // other mods
    //

    public static bool is_sb_camera_scroll_enabled = false;
    public static bool is_slugpup_safari_enabled = false;

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

    public static void LogAllInstructions(ILContext context, int index_string_length = 9, int op_code_string_length = 14) {
        if (context == null) return;

        Debug.Log("-----------------------------------------------------------------");
        Debug.Log("Log all IL-instructions.");
        Debug.Log("Index:" + new string(' ', index_string_length - 6) + "OpCode:" + new string(' ', op_code_string_length - 7) + "Operand:");

        ILCursor cursor = new(context);
        ILCursor label_cursor = cursor.Clone();

        string cursor_index_string;
        string op_code_string;
        string operand_string;

        while (true) {
            // this might return too early;
            // if (cursor.Next.MatchRet()) break;

            // should always break at some point;
            // only TryGotoNext() doesn't seem to be enough;
            // it still throws an exception;
            try {
                if (cursor.TryGotoNext(MoveType.Before)) {
                    cursor_index_string = cursor.Index.ToString();
                    cursor_index_string = cursor_index_string.Length < index_string_length ? cursor_index_string + new string(' ', index_string_length - cursor_index_string.Length) : cursor_index_string;
                    op_code_string = cursor.Next.OpCode.ToString();

                    if (cursor.Next.Operand is ILLabel label) {
                        label_cursor.GotoLabel(label);
                        operand_string = "Label >>> " + label_cursor.Index;
                    } else {
                        operand_string = cursor.Next.Operand?.ToString() ?? "";
                    }

                    if (operand_string == "") {
                        Debug.Log(cursor_index_string + op_code_string);
                    } else {
                        op_code_string = op_code_string.Length < op_code_string_length ? op_code_string + new string(' ', op_code_string_length - op_code_string.Length) : op_code_string;
                        Debug.Log(cursor_index_string + op_code_string + operand_string);
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

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld rain_world) {
        orig(rain_world);
        MachineConnector.SetRegisteredOI(mod_id, MainModOptions.main_mod_options);

        if (is_initialized) return;
        is_initialized = true;
        Debug.Log("CoopTweaks: version " + version);

        foreach (ModManager.Mod mod in ModManager.ActiveMods) {
            if (mod.id == "SBCameraScroll") {
                is_sb_camera_scroll_enabled = true;
                continue;
            }

            if (mod.id == "yeliah.slugpupFieldtrip") {
                is_slugpup_safari_enabled = true;
                continue;
            }
        }

        if (!is_sb_camera_scroll_enabled) {
            Debug.Log("CoopTweaks: SBCameraScroll not found.");
        } else {
            Debug.Log("CoopTweaks: SBCameraScroll found. Synchronize shortcut position updates when mushroom effect is active.");
        }

        if (!is_slugpup_safari_enabled) {
            Debug.Log("CoopTweaks: Slugpup Safari not found.");
        } else {
            Debug.Log("CoopTweaks: Slugpup Safari found. Disable Option_SlugOnBack. Otherwise slugpups don't stack as intended.");
        }

        ArtificialIntelligenceMod.OnEnable();
        ProcessManagerMod.OnEnable();
    }
}
