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
    // for sources (mostly skill extends) which apply swiftness
    class AdHocSwiftnessChanges
    {

        static QuickManager quickManager = SwiftnessReworkPlugin.quickManager;

        [HarmonyPatch(typeof(GDESkillData), nameof(GDESkillData.LoadFromDict))]
        class GDESkillData_Patch
        {
            static HashSet<string> addQuickSet = new HashSet<string>() 
            {
                GDEItemKeys.Skill_S_Azar_7,
                GDEItemKeys.Skill_S_Lucy_0,
                GDEItemKeys.Skill_S_Lucy_6,
                GDEItemKeys.Skill_S_Lucy_14,
                GDEItemKeys.Skill_S_Lucy_15,
                GDEItemKeys.Skill_S_MissChain_2,
                GDEItemKeys.Skill_S_Prime_12
            };

            static void Postfix(GDESkillData __instance, Dictionary<string, object> dict)
            {
                if (addQuickSet.Contains(__instance.Key))
                {
                    dict.TryGetString("Description", out string ogDesc, __instance.Key);
                    __instance.Description = ogDesc.Replace("Swiftness", "<b>Agile</b> and <b>Quick</b>");
                    if (!quickManager.defaultQuickness.Contains(__instance.Key))
                    {
                        dict.TryGetCustomList("PlusKeyWords", out List<GDESkillKeywordData> ogKeyWords);
                        ogKeyWords.Add(new GDESkillKeywordData(SwiftnessReworkPlugin.QuickKeyWordKey));
                        __instance.PlusKeyWords = ogKeyWords;
                    }

                }
                // eve help
                else if (__instance.Key == GDEItemKeys.Skill_S_Sizz_0)
                {
                    __instance.Description = @"Transfer all Pincer Attack buffs to the target.
 Create a 1 cost 'Eve, Help!' skill in your hand.
 It gains <b>Agile</b> and <b>Quick</b>.
 The created skill can only be cast this turn.";
                    if (!quickManager.defaultQuickness.Contains(__instance.Key))
                    {
                        dict.TryGetCustomList("PlusKeyWords", out List<GDESkillKeywordData> ogKeyWords);
                        ogKeyWords.Add(new GDESkillKeywordData(SwiftnessReworkPlugin.QuickKeyWordKey));
                        __instance.PlusKeyWords = ogKeyWords;
                    }
                }
            }
        }



        [HarmonyPatch(typeof(GDESkillExtendedData), nameof(GDESkillExtendedData.LoadFromDict))]
        class GDESkillExtendedData_Patch
        {
            static void Postfix(GDESkillExtendedData __instance, Dictionary<string, object> dict)
            {
                // illya's passive enforcement
                if (__instance.Key == GDEItemKeys.SkillExtended_SkillEn_IlyaPassive)
                {
                    dict.TryGetString("EnforceString", out string ogEnforceString, __instance.Key);
                    __instance.EnforceString = ogEnforceString.Replace("Swiftness", "<b>Agile</b>");
                }
            }
        }


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

        // probably not strictly mandatory and only should be enabled if accel circle grants quick
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



        [HarmonyPatch]
        class AddQuickWhereNotCountIsAdded_Patch
        {

            static IEnumerable<MethodBase> TargetMethods()
            {
                
                // mandatory (pretty much)
                yield return AccessTools.Method(typeof(B_Butler_T_1), nameof(B_Butler_T_1.Turn1)); // butler's fury
                yield return AccessTools.Method(typeof(P_Hein), nameof(P_Hein.KillEffect));
                yield return AccessTools.Method(typeof(SkillEn_Hein_1), nameof(SkillEn_Hein_1.SkillUseSingle));
                // optional for skill effect
                yield return AccessTools.Method(typeof(Extended_Lucy_6), nameof(Extended_Lucy_6.SkillTargetSingle));
                yield return AccessTools.Method(typeof(Extended_S_Sizz_0), nameof(Extended_S_Sizz_0.SkillUseSingle));
                yield return AccessTools.Method(typeof(S_Prime_12), nameof(S_Prime_12.SkillUseSingle));


            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                CodeInstruction prevCi = null;
                foreach (var ci in instructions)
                {
                    if ( ci.Is(OpCodes.Callvirt, AccessTools.Method(typeof(Skill), "set_NotCount"))
                        //|| ci.Is(OpCodes.Stfld, AccessTools.Field(typeof(Skill_Extended), nameof(Skill_Extended.NotCount)))
                        )
                    {
                        yield return ci;
                        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(AdHocSwiftnessChanges), nameof(quickManager)));
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
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

    }
}
