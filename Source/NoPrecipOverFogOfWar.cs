using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NoPrecipOverFogOfWar
{
    public class NoPrecipOverFogOfWar : Mod
    {

        public NoPrecipOverFogOfWar(ModContentPack content) : base(content)
        {
#if DEBUG
            Harmony.DEBUG = true;
#endif

            Harmony harmony = new Harmony("rimworld.noprecipoverfogofwar");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch]
    public static class PrecipitationPatch
    {
        public static bool CheckingRainMask = false;

        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("SectionLayer_IndoorMask"), "HideRainPrimary");
        }

        public static void Prefix(IntVec3 c)
        {
            CheckingRainMask = true;
        }

        public static void Postfix(ref bool __result)
        {
            CheckingRainMask = false;
            if (IsFoggedForPrecipitation.MaskRain)
            {
                IsFoggedForPrecipitation.MaskRain = false;
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(FogGrid), nameof(FogGrid.IsFogged), typeof(IntVec3))]
    public static class IsFoggedForPrecipitation
    {

        public static bool MaskRain = false;

        public static void Postfix(ref bool __result)
        {
            if (PrecipitationPatch.CheckingRainMask && __result)
            {
                MaskRain = true;
            }
        }
    }
}
