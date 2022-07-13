using BepInEx;
using GameDataEditor;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
                    //__instance.StackInfo[0].RemainTime = 99;

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
                    {
                        __instance.BChar.Heal(User, (float)(Dmg), false, false, null);
                    }
                   /* else if (User == BattleSystem.instance.DummyChar)
                    {
                        var st = new StackTrace();
                        *//*                        Debug.Log(st.GetFrame(4).GetMethod().Name);
                                                (new List<StackFrame>(st.GetFrames())).ForEach(sf => Debug.Log(sf.GetMethod()));*//*
                        if (st.FrameCount >= 5 && st.GetFrame(4).GetMethod().Equals(AccessTools.Method(typeof(BattleChar), nameof(BattleChar.TickUpdate))))
                        {
                            var heal = 0f;
                            foreach (var dot in Target.GetBuffs(BattleChar.GETBUFFTYPE.DOT, false, false))
                            {
                                //Debug.Log(dot.Usestate_L.Info.Ally);
                                if (!dot.DestroyBuff)
                                {
                                    Debug.Log(dot.Usestate_L == null);
                                    if (dot.Usestate_L.Info.Ally)
                                    {
                                        UnityEngine.Debug.Log("Ally pain");
                                        heal += (float)Math.Max(0, dot.Tick());
                                    }
                                    else
                                    {
                                        UnityEngine.Debug.Log("enemy pain");

                                        heal += (float)Math.Max(0f, dot.Tick() / 2f);
                                    }
                                }
                            }
                            __instance.BChar.Heal(User, heal, false, false, null);

                        }
                    }*/
                    else
                    {
                        __instance.BChar.Heal(User, (float)(Dmg / 2), false, false, null);
                    }
                }
                return false;
            }
        }

/*        [HarmonyPatch(typeof(ArkCode), "Start")]
        class TimeScale2xPatch
        {
            static void Postfix()
            {
                Time.timeScale = 2f;
                Debug.Log("Sonic Speed");
            }
        }

        [HarmonyPatch(typeof(BattleSystem), "CheatChack")]
        class dd
        {
            static void Postfix(BattleSystem __instance)
            {
                if (__instance.CheatChat == "paine")
                {
                    __instance.CheatEnabled();
                    __instance.AllyList[0].BuffAdd(GDEItemKeys.Buff_B_Maid_T_1, __instance.EnemyList[0], false, 300, false, -1, false);
                }
            }
        }*/

    }
}
