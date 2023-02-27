using static CoopTweaks.MainMod;

namespace CoopTweaks;

internal static class RegionGateMod
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
        if (Option_RegionGates)
        {
            if (isEnabled)
            {
                On.RegionGate.PlayersStandingStill += RegionGate_PlayersStandingStill; // ignore inputs
            }
            else
            {
                On.RegionGate.PlayersStandingStill -= RegionGate_PlayersStandingStill;
            }
        }
    }

    //
    // private
    //

    private static bool RegionGate_PlayersStandingStill(On.RegionGate.orig_PlayersStandingStill orig, RegionGate regionGate)
    {
        return true;
    }
}