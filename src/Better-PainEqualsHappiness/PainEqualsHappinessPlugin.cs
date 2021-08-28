using BepInEx;
using GameDataEditor;
using HarmonyLib;
using System;
using UnityEngine;

namespace Better_PainEqualsHappiness
{
    [BepInPlugin(GUID, "Better Pain Equals Happiness", version)]
    [BepInProcess("ChronoArk.exe")]
    public class PainEqualsHappinessPlugin : BaseUnityPlugin
    {
        public const string GUID = "org.neo.chronoark.cardmod.painequalshappiness";
        public const string version = "1.0.0";

        private static readonly Harmony harmony = new Harmony(GUID);
        void Awake()
        {
            harmony.PatchAll();
        }
        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchAll(GUID);
        }

        // B_Queen_10_T does not directly override the following methods therefore base class has to patched
        [HarmonyPatch(typeof(Buff))]
        class PEH_Buff_Patch
        {
            [HarmonyPatch(nameof(Buff.Init))]
            [HarmonyPostfix]
            static void InitPostfix(Buff __instance)
            {
                if (__instance is B_Queen_10_T)
                {
                    __instance.StackInfo[0].RemainTime = 3;
                }
            }

            [HarmonyPatch(nameof(Buff.DescExtended), new Type[] { })]
            [HarmonyPostfix]
            static void DescExtendedPostfix(ref string __result, Buff __instance)
            {
                if (__instance is B_Queen_10_T)
                {
                    __result = "When you take <color=purple>Pain damage</color>, instead of taking damage heal for <b>half</b> amount or for <b>full</b> amount if the damage was <b>directly</b> caused by an ally.\nKindly note: at the end of the turn this buff expires <b>before</b> <sprite=1> debuff tick.";
                }
            }
        }

        [HarmonyPatch(typeof(B_Queen_10_T))]
        class TransferPain_effect_Patch
        {
            [HarmonyPatch(nameof(B_Queen_10_T.DamageTake))]
            [HarmonyPrefix]
            static bool DamageTake(B_Queen_10_T __instance, BattleChar User, int Dmg, bool Cri, ref bool resist, bool NODEF = false, bool NOEFFECT = false, BattleChar Target = null)
            {
                if (NODEF)
                {
                    resist = true;
                    // copied from B_ShadowPriest_7_S (Soul Stigma)
                    if (__instance.BChar.Info.Ally == User.Info.Ally && User != BattleSystem.instance.DummyChar)
                        __instance.BChar.Heal(User, (float)(Dmg), false, false, null);
                    else
                        __instance.BChar.Heal(User, (float)(Dmg/2), false, false, null);
                }
                return false;
            }
        }

    }
}
