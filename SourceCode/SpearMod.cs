using static CoopTweaks.MainMod;

namespace CoopTweaks;

internal static class SpearMod
{
    //
    // main
    //

    internal static void On_Config_Changed()
    {
        On.Spear.HitSomethingWithoutStopping -= Spear_HitSomethingWithoutStopping;

        if (Option_SlowMotion)
        {
            // share mushroom effect with other players;
            On.Spear.HitSomethingWithoutStopping += Spear_HitSomethingWithoutStopping;
        }
    }

    //
    // private
    //

    private static void Spear_HitSomethingWithoutStopping(On.Spear.orig_HitSomethingWithoutStopping orig, Spear spear, PhysicalObject hit_object, BodyChunk chunk, PhysicalObject.Appendage appendage)
    {
        if (!spear.Spear_NeedleCanFeed() || hit_object is not Mushroom)
        {
            orig(spear, hit_object, chunk, appendage);
            return;
        }

        foreach (AbstractCreature abstractPlayer in spear.abstractPhysicalObject.world.game.Players)
        {
            if (abstractPlayer.realizedCreature is Player player && player != spear.thrownBy)
            {
                player.mushroomCounter += 320;
            }
        }
        orig(spear, hit_object, chunk, appendage);
    }
}