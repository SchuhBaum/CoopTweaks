using MonoMod.Cil;
using System;
using UnityEngine;
using static CoopTweaks.MainMod;

namespace CoopTweaks;

internal static class LeechMod {
    //
    // main
    //

    internal static void On_Config_Changed() {
        IL.Leech.Swim -= IL_Leech_Swim;

        if (Option_SlugOnBack) {
            IL.Leech.Swim += IL_Leech_Swim;
        }
    }

    //
    // private
    //

    private static void IL_Leech_Swim(ILContext context) {
        // LogAllInstructions(context);
        ILCursor cursor = new(context);

        if (cursor.TryGotoNext(instruction => instruction.MatchLdfld<Creature>("leechedOut"))) {
            if (can_log_il_hooks) {
                Debug.Log("CoopTweaks: IL_Leech_Swim: Index " + cursor.Index);
            }

            cursor.RemoveRange(1);
            cursor.EmitDelegate<Func<Creature, bool>>(creature => {
                if (creature.leechedOut) return true; // vanilla case;

                // don't hunt slugcats that are carried on the back; if a leech grabs a slugcat 
                // that is carried then it will get dropped; SlugOnBack.GraphicsModuleUpdated()
                // checks for that; in most cases this is fine; leeches can be annoying though;
                return creature is Player player && player.onBack != null;
            });
        } else {
            if (can_log_il_hooks) {
                Debug.Log("CoopTweaks: IL_Leech_Swim failed.");
            }
            return;
        }
        // LogAllInstructions(context);
    }
}
