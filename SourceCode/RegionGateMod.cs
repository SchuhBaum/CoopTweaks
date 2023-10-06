using static CoopTweaks.MainMod;

namespace CoopTweaks;

internal static class RegionGateMod {
    //
    // main
    //

    internal static void On_Config_Changed() {
        On.RegionGate.PlayersInZone -= RegionGate_PlayersInZone;
        On.RegionGate.PlayersStandingStill -= RegionGate_PlayersStandingStill;

        if (Option_RegionGates) {
            On.RegionGate.PlayersInZone += RegionGate_PlayersInZone;
            On.RegionGate.PlayersStandingStill += RegionGate_PlayersStandingStill; // ignore inputs
        }
    }

    //
    // private
    //

    private static int RegionGate_PlayersInZone(On.RegionGate.orig_PlayersInZone orig, RegionGate region_gate) {
        //
        // this does not work properly in vanilla; if player 1 is not in the room then
        // it would just open for player 2; it relies on the function PlayersStandingStill()
        // to fix it; this mean that this function is bugged and that that function does
        // more than just checking if players are standing still;
        //

        int vanilla_result = orig(region_gate);
        if (vanilla_result == -1) return -1;

        if (ModManager.CoopAvailable) {
            foreach (AbstractCreature abstract_player in region_gate.room.game.PlayersToProgressOrWin) {
                // -1 means not in the gate room;
                if (region_gate.DetectZone(abstract_player) == -1) return -1;
            }
        } else {
            foreach (AbstractCreature abstract_player in region_gate.room.game.Players) {
                if (region_gate.DetectZone(abstract_player) == -1) return -1;
            }
        }
        return vanilla_result;
    }

    private static bool RegionGate_PlayersStandingStill(On.RegionGate.orig_PlayersStandingStill orig, RegionGate region_gate) {
        return true;
    }
}
