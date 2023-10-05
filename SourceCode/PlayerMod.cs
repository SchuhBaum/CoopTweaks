using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;
using static CoopTweaks.MainMod;

namespace CoopTweaks;

public static class PlayerMod {
    //
    // variables
    //

    public static bool has_encountered_not_a_number_bug = false;

    //
    // main
    //

    internal static void On_Config_Changed() {
        IL.Player.ClassMechanicsArtificer -= IL_Player_ClassMechanicsArtificer;
        IL.Player.GrabUpdate -= IL_Player_GrabUpdate;

        On.Player.BiteEdibleObject -= Player_BiteEdibleObject;
        On.Player.CanIPickThisUp -= Player_CanIPickThisUp;
        On.Player.Update -= Player_Update;

        // add bugfix for missing body parts;
        // 
        // skip deaf beep;
        // release grasp when pressing jump;
        // sync mushroom counter between player;
        // only drop player when holding down + grab;
        On.Player.Update += Player_Update;

        if (Option_ArtificerStun) {
            // remove friendly fire stuns;
            // they added this in v1.9.06;
            // the only real difference is that this works in Arena as well;
            // and you can have spear friendly fire enabled;
            // also I don't remove the knock-back;
            IL.Player.ClassMechanicsArtificer += IL_Player_ClassMechanicsArtificer;
        }

        if (Option_ItemBlinking) {
            // remove blinking when you cannot pickup items;
            On.Player.CanIPickThisUp += Player_CanIPickThisUp;
        }

        if (Option_ItemBlinking || Option_SlugOnBack) {
            // prevent items from blinking if you are on the back of another player;
            // remove the ability to throw slugcats from back;
            IL.Player.GrabUpdate += IL_Player_GrabUpdate;
        }

        if (Option_SlowMotion) {
            // adds eat sound to mushrooms;
            On.Player.BiteEdibleObject += Player_BiteEdibleObject;
        }
    }

    //
    // public
    //

    public static void SynchronizeMushroomCounter(Player player) {
        if (player.inShortcut) return;

        // synchronize with other player;
        // player in shortcuts don't update the mushroom counter on their own,
        // i.e. the update function is not called;
        foreach (AbstractCreature abstract_player in player.abstractCreature.world.game.Players) {
            if (abstract_player.realizedCreature is Player player_ && player_.inShortcut) {
                player_.mushroomCounter = player.mushroomCounter;
            }
        }
    }

    //
    // private
    //

    private static void IL_Player_ClassMechanicsArtificer(ILContext context) { // Option_ArtificerStun
        // LogAllInstructions(context);
        ILCursor cursor = new(context);

        // ignore the coop friendly fire setting;
        // otherwise the knock back might be skipped as well;
        if (cursor.TryGotoNext(instruction => instruction.MatchLdsfld<ModManager>("CoopAvailable"))) {
            if (can_log_il_hooks) {
                Debug.Log("CoopTweaks: IL_Player_ClassMechanicsArtificer: Index " + cursor.Index); // 861
            }

            cursor.Goto(cursor.Index + 1);
            cursor.Prev.OpCode = OpCodes.Br;
            cursor.Prev.Operand = cursor.Next.Operand; // label
            cursor.RemoveRange(1);
        } else {
            if (can_log_il_hooks) {
                Debug.Log("CoopTweaks: IL_Player_ClassMechanicsArtificer failed.");
            }
            return;
        }

        // skip only the stun but not the knock-back;
        if (cursor.TryGotoNext(instruction => instruction.MatchIsinst("Scavenger"))) {
            if (can_log_il_hooks) {
                Debug.Log("CoopTweaks: IL_Player_ClassMechanicsArtificer: Index " + cursor.Index); // 922
            }

            cursor.Goto(cursor.Index + 7); // 929
            object label = cursor.Prev.Operand;
            object creature = cursor.Next.Operand;

            cursor.Goto(cursor.Index + 1);
            cursor.Emit(OpCodes.Isinst, typeof(Player));
            cursor.Emit(OpCodes.Brtrue, label);

            // restore vanilla since this was re-used for the type check;
            cursor.Emit(OpCodes.Ldloc_S, creature);
        } else {
            if (can_log_il_hooks) {
                Debug.Log("CoopTweaks: IL_Player_ClassMechanicsArtificer failed.");
            }
            return;
        }
        // LogAllInstructions(context);
    }

    private static void IL_Player_GrabUpdate(ILContext context) { // Option_ItemBlinking // Option_SlugOnBack
        // LogAllInstructions(context);
        ILCursor cursor = new(context);

        if (cursor.TryGotoNext(
            instruction => instruction.MatchBrfalse(out _),
            instruction => instruction.MatchLdloc(out _),
            instruction => instruction.MatchIsinst<PlayerCarryableItem>(),
            instruction => instruction.MatchCallvirt<PlayerCarryableItem>("Blink")
        )) {
            if (Option_ItemBlinking) {
                if (can_log_il_hooks) {
                    Debug.Log("CoopTweaks: IL_Player_GrabUpdate: Index " + cursor.Index); // 2340
                }

                // I am not sure why I can't get the label from the MatchBrFalse(out ILLabel label);
                // maybe it is because I am using lambda expressions;
                ILLabel label = (ILLabel)cursor.Next.Operand;
                cursor.Goto(cursor.Index + 1);

                // let items not blink when you are a on the back of another player or a npc;
                cursor.Emit(OpCodes.Ldarg_0); // player
                cursor.EmitDelegate<Func<Player, bool>>(player => player.onBack != null || player.isNPC);
                cursor.Emit(OpCodes.Brtrue, label);
            }
        } else {
            if (can_log_il_hooks) {
                Debug.Log("CoopTweaks: IL_Player_GrabUpdate failed.");
            }
            return;
        }

        if (cursor.TryGotoNext(instruction => instruction.MatchCallvirt<Player.SlugOnBack>("SlugToHand")) &&
            cursor.TryGotoPrev(instruction => instruction.MatchBle(out ILLabel _))) {
            if (Option_SlugOnBack) {
                if (can_log_il_hooks) {
                    Debug.Log("CoopTweaks: IL_Player_GrabUpdate: Index " + cursor.Index); // 2482
                }

                // don't throw slugpups that are carried on your back; skip the if block
                // after the wantToThrow check is done;
                cursor.Goto(cursor.Index + 1); // 2483
                cursor.Emit(OpCodes.Br, cursor.Prev.Operand);
            }
        } else {
            if (can_log_il_hooks) {
                Debug.Log("CoopTweaks: IL_Player_GrabUpdate failed.");
            }
            return;
        }
        // LogAllInstructions(context);
    }

    //
    //
    //

    private static void Player_BiteEdibleObject(On.Player.orig_BiteEdibleObject orig, Player player, bool eu) { // Option_SlowMotion
        foreach (Creature.Grasp? grasp in player.grasps) {
            if (grasp?.grabbed is Mushroom) {
                player.room?.PlaySound(SoundID.Slugcat_Bite_Dangle_Fruit, player.mainBodyChunk);
                break;
            }
        }
        orig(player, eu);
    }

    private static bool Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player player, PhysicalObject physical_object) { // Option_ItemBlinking
        Player.ObjectGrabability object_grabability = player.Grabability(physical_object);
        bool both_hands_are_full = player.grasps[0] != null && player.grasps[1] != null;

        if (object_grabability == Player.ObjectGrabability.OneHand && both_hands_are_full) {
            // spearmaster can grab spears with one hand;
            // this is a perk in Expedition as well;
            if (physical_object is Spear && player.CanPutSpearToBack) return orig(player, physical_object);
            return false;
        }

        if (object_grabability == Player.ObjectGrabability.BigOneHand && !player.CanPutSpearToBack && (both_hands_are_full || player.grasps[0]?.grabbed is Spear || player.grasps[1]?.grabbed is Spear)) return false;
        return orig(player, physical_object);
    }

    private static void Player_Update(On.Player.orig_Update orig, Player player, bool eu) { // Option_DeafBeep // Option_ReleaseGrasp // Option_SlowMotion // Option_SlugOnBack
        // bug is reported // temporary
        if (Mathf.Max(1f - player.airInLungs, player.aerobicLevel - (player.slugcatStats.malnourished ? 1.2f : 1f) / (((player.input[0].x == 0 && player.input[0].y == 0) ? 400f : 1100f) * (1f + 3f * Mathf.InverseLerp(0.9f, 1f, player.aerobicLevel)))) is float.NaN && !has_encountered_not_a_number_bug) {
            Debug.Log("CoopTweaks: The variable aerobicLevel might be NaN. Some body parts might be missing. Let's hope for the best. This message will only be logged once.");
            has_encountered_not_a_number_bug = true;
        }

        if (!Option_DeafBeep && !Option_ReleaseGrasp && !Option_SlowMotion && !Option_SlugOnBack) {
            orig(player, eu);
            return;
        }

        if (player.isNPC) {
            orig(player, eu);
            return;
        }

        if (Option_DeafBeep) {
            player.deaf = 0; // this sound loop can get stuck // disable for now
        }
        orig(player, eu);

        if (player.input[0].jmp && !player.input[1].jmp && player.grabbedBy?.Count > 0 && Option_ReleaseGrasp) {
            for (int grasp_index = player.grabbedBy.Count - 1; grasp_index >= 0; grasp_index--) {
                if (player.grabbedBy[grasp_index] is Creature.Grasp grasp && grasp.grabber is Player player_) {
                    player_.ReleaseGrasp(grasp.graspUsed); // list is modified
                }
            }
        }

        if (Option_SlowMotion) {
            SynchronizeMushroomCounter(player);
        }

        // dropping slugcats when holding up is helpful when using the mod Slugpup
        // Safari; holding down + grab will drop all slugpups at once;
        if (Option_SlugOnBack && player.slugOnBack != null && !(player.input[0].y == 0 && (player.grasps[0]?.grabbed is Player || player.grasps[1]?.grabbed is Player) || player.input[0].y != 0 && player.grasps[0]?.grabbed is not Player && player.grasps[1]?.grabbed is not Player)) {
            player.slugOnBack.increment = false;
        }
    }
}
