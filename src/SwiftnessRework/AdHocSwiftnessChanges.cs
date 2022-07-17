using BepInEx;
using BepInEx.Configuration;
using GameDataEditor;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using System.Linq;
using Debug = UnityEngine.Debug;
using DarkTonic.MasterAudio;
using System.Reflection.Emit;
using System;

namespace SwiftnessRework
{
    // for sources (mostly skill extends) which applied swiftness
    class AdHocSwiftnessChanges
    {

        static QuickManager quickManager = SwiftnessReworkPlugin.quickManager;

        //2do adjust descriptions

        [HarmonyPatch(typeof(Extended_Azar_7_0), nameof(Extended_Azar_7_0.Init))]
        class FantasySkillBuff_Patch
        {
            static void Postfix(Extended_Azar_7_0 __instance)
            {
                quickManager.SetVal(__instance, true);
            }
        }


        [HarmonyPatch(typeof(Extended_Lucy_0_1), nameof(Extended_Lucy_0_1.Init))]
        class AccelerateBattleBuff_Patch
        {
            static void Postfix(Extended_Lucy_0_1 __instance)
            {
                quickManager.SetVal(__instance, true);
            }
        }


        [HarmonyPatch(typeof(Extended_Lucy_14_0), nameof(Extended_Lucy_14_0.Init))]
        class FlagOfCombatBuff_Patch
        {
            static void Postfix(Extended_Lucy_14_0 __instance)
            {
                quickManager.SetVal(__instance, true);
            }
        }


        [HarmonyPatch]
        class Bazum_AccCircleFixedUpdatePatch
        {

            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(Extended_MissChain_2), nameof(Extended_MissChain_2.FixedUpdate));
                yield return AccessTools.Method(typeof(S_Lucy_15_0), nameof(S_Lucy_15_0.FixedUpdate));


            }

            // sets quick to false or true under certain conditions
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                CodeInstruction prevCi = null;
                foreach (var ci in instructions)
                {
                    if (ci.Is(OpCodes.Stfld, AccessTools.Field(typeof(Skill_Extended), nameof(Skill_Extended.NotCount))) && 
                        (prevCi != null && prevCi.opcode == OpCodes.Ldc_I4_0 || prevCi.opcode == OpCodes.Ldc_I4_1))
                    {
                        yield return ci;
                        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(AdHocSwiftnessChanges), nameof(quickManager)));
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return prevCi;
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(QuickManager), nameof(QuickManager.SetVal)));
                    }
                    else
                    {
                        yield return ci;
                    }
                    prevCi = ci;
                }
            }

        }


        [HarmonyPatch(typeof(S_Lucy_15_0), nameof(S_Lucy_15_0.SkillUseHand))]
        class AccCircleUseHand_Patch
        {

            static bool CheckNotCountAndQuick(Skill_Extended se)
            {
                return se.NotCount && quickManager.ExtendedGetQuick(se);
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var ci in instructions)
                {
                    if (ci.Is(OpCodes.Ldfld, AccessTools.Field(typeof(Skill_Extended), nameof(Skill_Extended.NotCount))))
                    {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AccCircleUseHand_Patch), nameof(AccCircleUseHand_Patch.CheckNotCountAndQuick)));
                    }
                    else
                    {
                        yield return ci;
                    }
                }
            }
        }



    }
}
