using static CoopTweaks.MainMod;

namespace CoopTweaks;

internal static class RegionGateMod {
    //
    // main
    //

    internal static void On_Config_Changed() {
        On.RegionGate.PlayersStandingStill -= RegionGate_PlayersStandingStill;

        if (Option_RegionGates) {
            On.RegionGate.PlayersStandingStill += RegionGate_PlayersStandingStill; // ignore inputs
        }
    }

    //
    // private
    //

    private static bool RegionGate_PlayersStandingStill(On.RegionGate.orig_PlayersStandingStill orig, RegionGate regionGate) {
        return true;
    }
}
