using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

using static CoopTweaks.MainMod;

namespace CoopTweaks;

internal static class PlayerMod
{
    //
    // variables
    //

    private static bool isEnabled = false;

    //
    //
    //

    internal static void OnToggle()
    {
        isEnabled = !isEnabled;
        if (Option_ArtificerStun)
        {
            if (isEnabled)
            {
                // remove friendly fire stuns;
                // they added this in v1.9.06;
                // the only real difference is that this works in Arena as well;
                // and you can have spear friendly fire enabled;
                // also I don't remove the knock-back;
                IL.Player.ClassMechanicsArtificer += IL_Player_ClassMechanicsArtificer;
            }
            else
            {
                IL.Player.ClassMechanicsArtificer -= IL_Player_ClassMechanicsArtificer;
            }
        }

        if (Option_DeafBeep || Option_ReleaseGrasp || Option_SlowMotion || Option_SlugOnBack)
        {
            if (isEnabled)
            {
                // skip deaf beep;
                // release grasp when pressing jump;
                // sync mushroom counter between player;
                // only drop player when holding down + grab;
                On.Player.Update += Player_Update;
            }
            else
            {
                On.Player.Update -= Player_Update;
            }
        }

        if (Option_ItemBlinking)
        {
            if (isEnabled)
            {
                On.Player.CanIPickThisUp += Player_CanIPickThisUp; // remove blinking when you cannot pickup items;
            }
            else
            {
                On.Player.CanIPickThisUp -= Player_CanIPickThisUp;
            }
        }

        if (Option_SlowMotion)
        {
            if (isEnabled)
            {
                On.Player.BiteEdibleObject += Player_BiteEdibleObject; // adds eat sound to mushrooms;
            }
            else
            {
                On.Player.BiteEdibleObject -= Player_BiteEdibleObject;
            }
        }

        if (Option_SlugOnBack)
        {
            if (isEnabled)
            {
                IL.Player.GrabUpdate += IL_Player_GrabUpdate; // remove ability to throw slugcats from back;
            }
            else
            {
                IL.Player.GrabUpdate -= IL_Player_GrabUpdate;
            }
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

        // skip the stun and knock-back
        // if (cursor.TryGotoNext(instruction => instruction.MatchCallvirt<Room>("VisualContact")))
        // {
        //     Debug.Log("CoopTweaks: IL_Player_ClassMechanicsArtificer: Index " + cursor.Index); // 907

        //     cursor.Goto(cursor.Index - 3); // 904
        //     object creature = cursor.Next.Operand;

        //     cursor.Goto(cursor.Index + 5); // 909
        //     object label = cursor.Prev.Operand;

        //     // change instead of Emit such that the label target to 909 is preserved;
        //     cursor.Next.OpCode = OpCodes.Ldloc_S;
        //     cursor.Next.Operand = creature;

        //     // skip stun and push back when creature is player;
        //     cursor.Goto(cursor.Index + 1);
        //     cursor.EmitDelegate<Func<Creature, bool>>(creature => creature is Player);
        //     cursor.Emit(OpCodes.Brtrue, label);

        //     // restore vanilla since this was changed to creature before;
        //     cursor.Emit(OpCodes.Ldarg_0);
        // }
        // else
        // {
        //     Debug.LogException(new Exception("CoopTweaks: IL_Player_ClassMechanicsArtificer failed."));
        // }

        // ignore the coop friendly fire setting;
        // otherwise the knock back might be skipped as well;
        if (cursor.TryGotoNext(instruction => instruction.MatchLdsfld<ModManager>("CoopAvailable")))
        {
            Debug.Log("CoopTweaks: IL_Player_ClassMechanicsArtificer: Index " + cursor.Index); // 861

            cursor.Goto(cursor.Index + 1);

            cursor.Prev.OpCode = OpCodes.Br;
            cursor.Prev.Operand = cursor.Next.Operand; // label
            cursor.RemoveRange(1);
        }
        else
        {
            Debug.LogException(new Exception("CoopTweaks: IL_Player_ClassMechanicsArtificer failed."));
        }

        // skip only the stun but not the knock-back;
        if (cursor.TryGotoNext(instruction => instruction.MatchIsinst("Scavenger")))
        {
            Debug.Log("CoopTweaks: IL_Player_ClassMechanicsArtificer: Index " + cursor.Index); // 922

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
            Debug.LogException(new Exception("CoopTweaks: IL_Player_ClassMechanicsArtificer failed."));
        }
        // LogAllInstructions(context);
    }

    private static void IL_Player_GrabUpdate(ILContext context)
    {
        // LogAllInstructions(context);

        ILCursor cursor = new(context);
        if (cursor.TryGotoNext(instruction => instruction.MatchCallvirt<Player.SlugOnBack>("SlugToHand")))
        {
            Debug.Log("CoopTweaks: IL_Player_GrabUpdate: Index " + cursor.Index); // 2497

            cursor.Goto(cursor.Index - 14); // 2483
            cursor.Emit(OpCodes.Br, cursor.Prev.Operand); // skip whole if statement;
        }
        else
        {
            Debug.LogException(new Exception("CoopTweaks: IL_Player_GrabUpdate failed."));
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

    private static bool Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player player, PhysicalObject physicalObject)
    {
        Player.ObjectGrabability objectGrabability = player.Grabability(physicalObject);
        bool canGrabOneHanded = player.grasps[0] == null || player.grasps[1] == null;

        if (objectGrabability == Player.ObjectGrabability.OneHand && !canGrabOneHanded) return false;
        else if (objectGrabability == Player.ObjectGrabability.BigOneHand && !player.CanPutSpearToBack && (!canGrabOneHanded || player.grasps[0]?.grabbed is Spear || player.grasps[1]?.grabbed is Spear)) return false;
        return orig(player, physicalObject);
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