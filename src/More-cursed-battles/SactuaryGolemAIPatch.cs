using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace More_cursed_battles
{
    // makes golems function properly with multiple actions
    // TODO add config option to disable AI modification for potential compatibility issues
    class SactuaryGolemAIPatch
    {
        // acting speed is hardcoded and independent of party speed
        private static int SanctuaryGolemSpeed(int actionCount, int totalActionNumber)
        {
            int result = 99;
            if (actionCount == totalActionNumber)
            {
                result = 99;
            }
            else
            {
                result = 4 + actionCount * 2;
            }
            return result;
        }


        [HarmonyPatch(typeof(AI_S4_Golem))]
        class GreenGolemPatch
        {
            [HarmonyPatch(nameof(AI_S4_Golem.SkillSelect))]
            [HarmonyPrefix]
            static bool SkillSelectPrefix(ref Skill __result, AI_S4_Golem __instance)
            {
                __result = __instance.BChar.Skills[0];
                return false;
            }

            [HarmonyPatch(nameof(AI_S4_Golem.SpeedChange))]
            [HarmonyPrefix]
            static bool SpeedChangetPrefix(ref int __result, Skill skill, int ActionCount, int OriginSpeed, AI_S4_Golem __instance)
            {
                __result = SanctuaryGolemSpeed(ActionCount, __instance.BChar.Info.PlusActCount.Count);
                return false;
            }

        }

        [HarmonyPatch(typeof(AI_S4_Golem2))]
        class YellowGolemPatch
        {
            [HarmonyPatch(nameof(AI_S4_Golem2.SkillSelect))]
            [HarmonyPrefix]
            static bool SkillSelectPrefix(ref Skill __result, AI_S4_Golem2 __instance)
            {
                __result = __instance.BChar.Skills[0];
                return false;
            }
            [HarmonyPatch(nameof(AI_S4_Golem2.SpeedChange))]
            [HarmonyPrefix]
            static bool SpeedChangetPrefix(ref int __result, Skill skill, int ActionCount, int OriginSpeed, AI_S4_Golem2 __instance)
            {
                __result = SanctuaryGolemSpeed(ActionCount, __instance.BChar.Info.PlusActCount.Count);
                return false;
            }
        }
    }
}
