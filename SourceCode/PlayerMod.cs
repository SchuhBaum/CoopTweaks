using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

using static CoopTweaks.MainMod;

namespace CoopTweaks;

public static class PlayerMod
{
    //
    // main
    //

    internal static void On_Config_Changed()
    {
        IL.Player.ClassMechanicsArtificer -= IL_Player_ClassMechanicsArtificer;
        IL.Player.GrabUpdate -= IL_Player_GrabUpdate;

        On.Player.BiteEdibleObject -= Player_BiteEdibleObject;
        On.Player.CanIPickThisUp -= Player_CanIPickThisUp;
        On.Player.Update -= Player_Update;

        if (Option_ArtificerStun)
        {
            // remove friendly fire stuns;
            // they added this in v1.9.06;
            // the only real difference is that this works in Arena as well;
            // and you can have spear friendly fire enabled;
            // also I don't remove the knock-back;
            IL.Player.ClassMechanicsArtificer += IL_Player_ClassMechanicsArtificer;
        }

        if (Option_DeafBeep || Option_ReleaseGrasp || Option_SlowMotion || Option_SlugOnBack)
        {
            // skip deaf beep;
            // release grasp when pressing jump;
            // sync mushroom counter between player;
            // only drop player when holding down + grab;
            On.Player.Update += Player_Update;
        }

        if (Option_ItemBlinking)
        {
            On.Player.CanIPickThisUp += Player_CanIPickThisUp; // remove blinking when you cannot pickup items;
        }

        if (Option_SlowMotion)
        {
            On.Player.BiteEdibleObject += Player_BiteEdibleObject; // adds eat sound to mushrooms;
        }

        if (Option_SlugOnBack)
        {
            IL.Player.GrabUpdate += IL_Player_GrabUpdate; // remove ability to throw slugcats from back;
        }
    }

    //
    // public
    //

    public static void SynchronizeMushroomCounter(Player player)
    {
        if (player.inShortcut) return;

        // synchronize with other player;
        // player in shortcuts don't update the mushroom counter on their own,
        // i.e. the update function is not called;
        foreach (AbstractCreature abstractPlayer in player.abstractCreature.world.game.Players)
        {
            if (abstractPlayer.realizedCreature is Player player_ && player_.inShortcut)
            {
                player_.mushroomCounter = player.mushroomCounter;
            }
        }
    }

    //
    // private
    //

    private static void IL_Player_ClassMechanicsArtificer(ILContext context)
    {
        // LogAllInstructions(context);
        ILCursor cursor = new(context);

        // ignore the coop friendly fire setting;
        // otherwise the knock back might be skipped as well;
        if (cursor.TryGotoNext(instruction => instruction.MatchLdsfld<ModManager>("CoopAvailable")))
        {
            if (can_log_il_hooks)
            {
                Debug.Log("CoopTweaks: IL_Player_ClassMechanicsArtificer: Index " + cursor.Index); // 861
            }

            cursor.Goto(cursor.Index + 1);
            cursor.Prev.OpCode = OpCodes.Br;
            cursor.Prev.Operand = cursor.Next.Operand; // label
            cursor.RemoveRange(1);
        }
        else
        {
            if (can_log_il_hooks)
            {
                Debug.Log("CoopTweaks: IL_Player_ClassMechanicsArtificer failed.");
            }
            return;
        }

        // skip only the stun but not the knock-back;
        if (cursor.TryGotoNext(instruction => instruction.MatchIsinst("Scavenger")))
        {
            if (can_log_il_hooks)
            {
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
        }
        else
        {
            if (can_log_il_hooks)
            {
                Debug.Log("CoopTweaks: IL_Player_ClassMechanicsArtificer failed.");
            }
            return;
        }
        // LogAllInstructions(context);
    }

    private static void IL_Player_GrabUpdate(ILContext context)
    {
        // LogAllInstructions(context);
        ILCursor cursor = new(context);

        if (cursor.TryGotoNext(instruction => instruction.MatchCallvirt<Player.SlugOnBack>("SlugToHand")))
        {
            if (can_log_il_hooks)
            {
                Debug.Log("CoopTweaks: IL_Player_GrabUpdate: Index " + cursor.Index); // 2497
            }

            cursor.Goto(cursor.Index - 14); // 2483
            cursor.Emit(OpCodes.Br, cursor.Prev.Operand); // skip whole if statement;
        }
        else
        {
            if (can_log_il_hooks)
            {
                Debug.Log("CoopTweaks: IL_Player_GrabUpdate failed.");
            }
            return;
        }
        // LogAllInstructions(context);
    }

    //
    //
    //

    private static void Player_BiteEdibleObject(On.Player.orig_BiteEdibleObject orig, Player player, bool eu) // Option_SlowMotion
    {
        foreach (Creature.Grasp? grasp in player.grasps)
        {
            if (grasp?.grabbed is Mushroom)
            {
                player.room?.PlaySound(SoundID.Slugcat_Bite_Dangle_Fruit, player.mainBodyChunk);
                break;
            }
        }
        orig(player, eu);
    }

    private static bool Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player player, PhysicalObject physical_object)
    {
        Player.ObjectGrabability object_grabability = player.Grabability(physical_object);
        bool both_hands_are_full = player.grasps[0] != null && player.grasps[1] != null;

        if (object_grabability == Player.ObjectGrabability.OneHand && both_hands_are_full)
        {
            // spearmaster can grab spears with one hand;
            // this is a perk in Expedition as well;
            if (physical_object is Spear && player.CanPutSpearToBack) return orig(player, physical_object);
            return false;
        }

        if (object_grabability == Player.ObjectGrabability.BigOneHand && !player.CanPutSpearToBack && (both_hands_are_full || player.grasps[0]?.grabbed is Spear || player.grasps[1]?.grabbed is Spear)) return false;
        return orig(player, physical_object);
    }

    private static void Player_Update(On.Player.orig_Update orig, Player player, bool eu) // Option_DeafBeep // Option_ReleaseGrasp // Option_SlowMotion // Option_SlugOnBack
    {
        if (player.isNPC)
        {
            orig(player, eu);
            return;
        }

        if (Option_DeafBeep)
        {
            player.deaf = 0; // this sound loop can get stuck // disable for now
        }
        orig(player, eu);

        if (player.input[0].jmp && !player.input[1].jmp && player.grabbedBy?.Count > 0 && Option_ReleaseGrasp)
        {
            for (int graspIndex = player.grabbedBy.Count - 1; graspIndex >= 0; graspIndex--)
            {
                if (player.grabbedBy[graspIndex] is Creature.Grasp grasp && grasp.grabber is Player player_)
                {
                    player_.ReleaseGrasp(grasp.graspUsed); // list is modified
                }
            }
        }

        if (Option_SlowMotion)
        {
            SynchronizeMushroomCounter(player);
        }

        if (player.slugOnBack.HasASlug && player.input[0].y != -1 && Option_SlugOnBack)
        {
            player.slugOnBack.increment = false;
        }
    }
}