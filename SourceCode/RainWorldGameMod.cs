using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace CoopTweaks
{
    public static class RainWorldGameMod
    {
        //
        // variables
        //

        private static bool isEnabled = false;

        //
        //
        //

        internal static void OnEnable()
        {
            On.RainWorldGame.ctor += RainWorldGame_ctor;
            On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;
        }

        internal static void OnToggle()
        {
            isEnabled = !isEnabled;
            if (MainMod.Option_SlowMotion)
            {
                if (isEnabled)
                {
                    IL.RainWorldGame.RawUpdate += IL_RainWorldGame_RawUpdate;
                }
                else
                {
                    IL.RainWorldGame.RawUpdate -= IL_RainWorldGame_RawUpdate;
                }
            }
        }

        //
        // public
        //

        public static void SetMaxUpdateShortcut(int framesPerSecond)
        {
            SBCameraScroll.RoomCameraMod.maxUpdateShortcut = framesPerSecond > 33 ? 3f : 2f; // framesPerSecond: 40f/1.21f ~ 33 & updateShortCut: 3f/1.21f < 2.5f rounded to 2
        }

        //
        // private
        //

        private static void IL_RainWorldGame_RawUpdate(ILContext context) // MainMod.Option_ModEnabled
        {
            // MainMod.LogAllInstructions(context);

            //
            // put code before MainLoopProcess.RawUpdate() is called;
            // this way I can override the changes made to framesPerSecond
            // which are only used in MainLoopProcess.RawUpdate();
            // I can also use local variables like isChatLogActive;
            //

            ILCursor cursor = new(context);
            if (cursor.TryGotoNext(instruction => instruction.MatchCall<MainLoopProcess>("RawUpdate")))
            {
                Debug.Log("CoopTweaks: IL_RainWorldGame_RawUpdate: Index " + cursor.Index); // 583

                // only go one back such that the labels are preserved;
                // emit Ldarg_0 after the function call;
                cursor.Goto(cursor.Index - 1);
                cursor.Emit(OpCodes.Ldloc_1);

                cursor.EmitDelegate<Action<RainWorldGame, bool>>((game, isChatLogActive) =>
                {
                    // skip when you collect chat log tokens;
                    if (isChatLogActive) return;

                    game.framesPerSecond = 40;
                    foreach (AbstractCreature abstractPlayer in game.Players)
                    {
                        if (abstractPlayer.state.alive && abstractPlayer.realizedCreature is Player player && player.Adrenaline > 0.0f)
                        {
                            game.framesPerSecond = Mathf.RoundToInt(40f / Mathf.Lerp(1f, 1.5f, player.Adrenaline));
                            if (game.updateShortCut == Mathf.RoundToInt(3f / Mathf.Lerp(1f, 1.5f, player.Adrenaline)) - 1)
                            {
                                game.updateShortCut = 2;
                            }
                            break;
                        }
                    }

                    if (MainMod.isSBCameraScrollEnabled)
                    {
                        SetMaxUpdateShortcut(game.framesPerSecond);
                    }
                });
                cursor.Emit(OpCodes.Ldarg_0);
            }
            else
            {
                Debug.LogException(new Exception("CoopTweaks: IL_RainWorldGame_RawUpdate failed."));
            }
            // MainMod.LogAllInstructions(context);
        }

        //
        // private
        //

        private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame game, ProcessManager manager)
        {
            Debug.Log("CoopTweaks: Add option specific hooks.");
            MainModOptions.instance.MainModOptions_OnConfigChanged();

            MushroomMod.OnToggle();
            PlayerMod.OnToggle();
            RegionGateMod.OnToggle();
            RainWorldGameMod.OnToggle();
            RoomMod.OnToggle();

            orig(game, manager);
        }

        private static void RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame game)
        {
            Debug.Log("CoopTweaks: Remove option specific hooks.");
            orig(game);

            MushroomMod.OnToggle();
            PlayerMod.OnToggle();
            RegionGateMod.OnToggle();
            RainWorldGameMod.OnToggle();
            RoomMod.OnToggle();
        }
    }
}