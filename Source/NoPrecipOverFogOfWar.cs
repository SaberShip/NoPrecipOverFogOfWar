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

        public static void Prefix(IntVec3 c, ref IntVec3 __state)
        {
            __state = c;
            CheckingRainMask = true;
        }

        public static void Postfix(IntVec3 __state, ref bool __result)
        {
            CheckingRainMask = false;
            if (!__result)
            {
                if (IsFoggedForPrecipitation.IsInFog)
                {
                    IsFoggedForPrecipitation.IsInFog = false;
                    __result = true;
                }
                else if (!GridsUtilityRoofedPatch.Roofed)
                {
                    Building b = __state.GetEdifice(GridsUtilityRoofedPatch.TileMap);
                    
                    if (b != null && b.def.holdsRoof && !b.def.MadeFromStuff)
                    {
                        __result = true;
                        GridsUtilityEdificePatch.Building = null;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(FogGrid), nameof(FogGrid.IsFogged), typeof(IntVec3))]
    public class IsFoggedForPrecipitation
    {

        public static bool IsInFog = false;

        public static void Postfix(ref bool __result)
        {
            if (PrecipitationPatch.CheckingRainMask && __result)
            {
                IsInFog = true;
            }
        }
    }

    [HarmonyPatch(typeof(GridsUtility), nameof(GridsUtility.Roofed))]
    public class GridsUtilityRoofedPatch
    {
        public static bool Roofed = false;
        public static Map TileMap = null;

        public static void Postfix(Map map, ref bool __result)
        {
            if (PrecipitationPatch.CheckingRainMask)
            {
                TileMap = map;
                Roofed = __result;
            }
        }
    }

    [HarmonyPatch(typeof(GridsUtility), nameof(GridsUtility.GetEdifice))]
    public class GridsUtilityEdificePatch
    {
        public static Building Building = null;

        public static void Postfix(ref Building __result)
        {
            if (PrecipitationPatch.CheckingRainMask)
            {
                Building = __result;
            }
        }
    }
}
