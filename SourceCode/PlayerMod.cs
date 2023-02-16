using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace CoopTweaks
{
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
            if (MainMod.Option_DeafBeep || MainMod.Option_ReleaseGrasp || MainMod.Option_SlugOnBack)
            {
                if (isEnabled)
                {
                    On.Player.Update += Player_Update;
                }
                else
                {
                    On.Player.Update -= Player_Update;
                }
            }

            if (MainMod.Option_ItemBlinking)
            {
                if (isEnabled)
                {
                    On.Player.CanIPickThisUp += Player_CanIPickThisUp; // remove blinking when you cannot pickup items
                }
                else
                {
                    On.Player.CanIPickThisUp -= Player_CanIPickThisUp;
                }
            }

            if (MainMod.Option_SlugOnBack)
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
        // private
        //

        private static void IL_Player_GrabUpdate(ILContext context)
        {
            // MainMod.LogAllInstructions(context);

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
            // MainMod.LogAllInstructions(context);
        }

        //
        //
        //

        private static bool Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player player, PhysicalObject physicalObject)
        {
            Player.ObjectGrabability objectGrabability = player.Grabability(physicalObject);
            bool canGrabOneHanded = player.grasps[0] == null || player.grasps[1] == null;

            if (objectGrabability == Player.ObjectGrabability.OneHand && !canGrabOneHanded) return false;
            else if (objectGrabability == Player.ObjectGrabability.BigOneHand && !player.CanPutSpearToBack && (!canGrabOneHanded || player.grasps[0]?.grabbed is Spear || player.grasps[1]?.grabbed is Spear)) return false;
            return orig(player, physicalObject);
        }

        private static void Player_Update(On.Player.orig_Update orig, Player player, bool eu) // MainMod.Option_DeafBeep // MainMod.Option_ReleaseGrasp // MainMod.Option_SlugOnBack
        {
            if (MainMod.Option_DeafBeep)
            {
                player.deaf = 0; // this sound loop can get stuck // disable for now
            }
            orig(player, eu);

            if (player.slugOnBack.HasASlug && player.input[0].y != -1 && MainMod.Option_SlugOnBack)
            {
                player.slugOnBack.increment = false;
            }

            if (player.input[0].jmp && !player.input[1].jmp && player.grabbedBy?.Count > 0 && MainMod.Option_ReleaseGrasp)
            {
                for (int graspIndex = player.grabbedBy.Count - 1; graspIndex >= 0; graspIndex--)
                {
                    if (player.grabbedBy[graspIndex] is Creature.Grasp grasp && grasp.grabber is Player player_)
                    {
                        player_.ReleaseGrasp(grasp.graspUsed); // list is modified
                    }
                }
            }
        }
    }
}