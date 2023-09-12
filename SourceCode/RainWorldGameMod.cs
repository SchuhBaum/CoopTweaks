using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;
using static CoopTweaks.MainMod;

namespace CoopTweaks;

public static class RainWorldGameMod {
    //
    // main
    //

    internal static void On_Config_Changed() {
        IL.RainWorldGame.RawUpdate -= IL_RainWorldGame_RawUpdate;

        if (Option_SlowMotion) {
            IL.RainWorldGame.RawUpdate += IL_RainWorldGame_RawUpdate;
        }
    }

    //
    // public
    //

    public static void Set_Number_Of_Frames_Per_Shortcut_Update(int frames_per_second) {
        // framesPerSecond: 40f/1.21f ~ 33 & updateShortCut: 3f/1.21f < 2.5f rounded to 2;
        SBCameraScroll.RoomCameraMod.number_of_frames_per_shortcut_udpate = frames_per_second > 33 ? 3f : 2f;
    }

    //
    // private
    //

    private static void IL_RainWorldGame_RawUpdate(ILContext context) // Option_SlowMotion
    {
        // LogAllInstructions(context);

        //
        // put code before MainLoopProcess.RawUpdate() is called;
        // this way I can override the changes made to framesPerSecond
        // which are only used in MainLoopProcess.RawUpdate();
        // I can also use local variables like isChatLogActive;
        //

        ILCursor cursor = new(context);
        if (cursor.TryGotoNext(instruction => instruction.MatchCall<MainLoopProcess>("RawUpdate"))) {
            if (can_log_il_hooks) {
                Debug.Log("CoopTweaks: IL_RainWorldGame_RawUpdate: Index " + cursor.Index); // 583
            }

            // only go one back such that the labels are preserved;
            // emit Ldarg_0 after the function call;
            cursor.Goto(cursor.Index - 1);
            cursor.Emit(OpCodes.Ldloc_1);

            cursor.EmitDelegate<Action<RainWorldGame, bool>>((game, is_chat_log_active) => {
                // skip when you collect chat log tokens;
                if (is_chat_log_active) return;

                game.framesPerSecond = 40;
                foreach (AbstractCreature abstract_player in game.Players) {
                    if (abstract_player.state.alive && abstract_player.realizedCreature is Player player && player.Adrenaline > 0.0f) {
                        game.framesPerSecond = Mathf.RoundToInt(40f / Mathf.Lerp(1f, 1.5f, player.Adrenaline));
                        if (game.updateShortCut == Mathf.RoundToInt(3f / Mathf.Lerp(1f, 1.5f, player.Adrenaline)) - 1) {
                            game.updateShortCut = 2;
                        }
                        break;
                    }
                }

                if (is_sb_camera_scroll_enabled) {
                    Set_Number_Of_Frames_Per_Shortcut_Update(game.framesPerSecond);
                }
            });
            cursor.Emit(OpCodes.Ldarg_0);
        } else {
            if (can_log_il_hooks) {
                Debug.Log("CoopTweaks: IL_RainWorldGame_RawUpdate failed.");
            }
            return;
        }
        // LogAllInstructions(context);
    }
}
