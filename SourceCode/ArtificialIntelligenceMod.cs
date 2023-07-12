namespace CoopTweaks;

internal static class ArtificialIntelligenceMod {
    internal static void OnEnable() {
        On.ArtificialIntelligence.TrackerToDiscardDeadCreature += ArtificialIntelligence_TrackerToDiscardDeadCreature; // don't ignore dead slugcats // this can happen when a slugcat dies before a dynamic relationship was initialized
    }

    //
    // private
    //

    // bug is reported // temporary
    private static bool ArtificialIntelligence_TrackerToDiscardDeadCreature(On.ArtificialIntelligence.orig_TrackerToDiscardDeadCreature orig, ArtificialIntelligence artificialIntelligence, AbstractCreature abstractCreature) {
        if (abstractCreature.state.dead && abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat) return false;
        return orig(artificialIntelligence, abstractCreature);
    }
}
