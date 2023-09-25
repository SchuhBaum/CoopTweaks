using static CoopTweaks.MainMod;

namespace CoopTweaks;

internal static class MushroomMod {
    //
    // main
    //

    internal static void On_Config_Changed() {
        On.Mushroom.BitByPlayer -= Mushroom_BitByPlayer;

        if (Option_SlowMotion) {
            // share mushroom effect;
            On.Mushroom.BitByPlayer += Mushroom_BitByPlayer;
        }
    }

    //
    // private
    //

    private static void Mushroom_BitByPlayer(On.Mushroom.orig_BitByPlayer orig, Mushroom mushroom, Creature.Grasp? grasp, bool eu) { // Option_SlowMotion
        orig(mushroom, grasp, eu);
        foreach (AbstractCreature abstract_player in mushroom.abstractPhysicalObject.world.game.Players) { // mushroom.room is null when in room transition // doesn't matter in this case
            if (abstract_player.realizedCreature is Player player && player != grasp?.grabber) {
                player.mushroomCounter += 320;
            }
        }
    }
}
