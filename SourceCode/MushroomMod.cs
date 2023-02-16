namespace CoopTweaks
{
    internal static class MushroomMod
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
            if (MainMod.Option_SlowMotion)
            {
                if (isEnabled)
                {
                    On.Mushroom.BitByPlayer += Mushroom_BitByPlayer; // share mushroom effect;
                }
                else
                {
                    On.Mushroom.BitByPlayer -= Mushroom_BitByPlayer;
                }
            }
        }

        //
        // private
        //

        private static void Mushroom_BitByPlayer(On.Mushroom.orig_BitByPlayer orig, Mushroom mushroom, Creature.Grasp? grasp, bool eu) // MainMod.Option_SlowMotion
        {
            orig(mushroom, grasp, eu);
            foreach (AbstractCreature abstractPlayer in mushroom.abstractPhysicalObject.world.game.Players) // mushroom.room is null when in room transition // doesn't matter in this case
            {
                if (abstractPlayer.realizedCreature is Player player && player != grasp?.grabber)
                {
                    player.mushroomCounter += 320;
                }
            }
        }
    }
}