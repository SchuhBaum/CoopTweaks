using UnityEngine;

using static CoopTweaks.MainMod;
using static CoopTweaks.MainModOptions;
using static ProcessManager;

namespace CoopTweaks;

public static class ProcessManagerMod {
    //
    // main
    //

    internal static void OnEnable() {
        On.ProcessManager.RequestMainProcessSwitch_ProcessID += ProcessManager_RequestMainProcessSwitch;
    }

    //
    // public
    //

    public static void Initialize_Option_Specific_Hooks() {
        // without can_log_il_hooks the logs are repeated
        // for every other mod adding the corresponding IL hook;

        main_mod_options.Log_All_Options();
        Debug.Log("CoopTweaks: Initialize option specific hooks.");

        can_log_il_hooks = true;
        LeechMod.On_Config_Changed();
        MushroomMod.On_Config_Changed();
        PlayerMod.On_Config_Changed();

        RainWorldGameMod.On_Config_Changed();
        RegionGateMod.On_Config_Changed();
        RoomMod.On_Config_Changed();

        SpearMod.On_Config_Changed();
        can_log_il_hooks = false;
    }

    //
    // private
    //

    private static void ProcessManager_RequestMainProcessSwitch(On.ProcessManager.orig_RequestMainProcessSwitch_ProcessID orig, ProcessManager process_manager, ProcessID next_process_id) {
        // I want to use the event OnConfigChanged in MainModOptions;
        // but I had cases from other users where logging was not triggered
        // when starting the game;
        // it seems to be inconsistent otherwise;
        // maybe I can do both and use this as a backup;
        // from what I tested this is triggered after the event OnConfigChanged;

        ProcessID current_process_id = process_manager.currentMainLoop.ID;
        orig(process_manager, next_process_id);
        if (current_process_id != ProcessID.Initialization) return;
        Initialize_Option_Specific_Hooks();
    }
}
