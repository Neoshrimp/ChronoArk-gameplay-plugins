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
using I2.Loc;

namespace SwiftnessRework
{
    class SystemSwiftnessChanges
    {

		static QuickManager quickManager = SwiftnessReworkPlugin.quickManager;

		static bool CheckQuick(Skill skill)
		{
			return quickManager.SkillGetQuick(skill);
		}



        [HarmonyPatch]
        class QuickKeywordPatch
        {

            static bool CheckQuick(Skill skill)
            {
                return quickManager.SkillGetQuick(skill) || skill.Disposable;
            }

			static void AddQuickKeyword(Skill skill, List<string> kwList, SkillToolTip thisInst)
			{
				if (quickManager.SkillGetQuick(skill))
				{
					kwList.Add(SkillToolTip.ColorChange("FF7C34", SwiftnessReworkPlugin.QuickKeyWordName));
					// tooltip displayed out of order but whatever
					thisInst.PlusTooltipsView(SwiftnessReworkPlugin.QuickKeyWordName, SwiftnessReworkPlugin.QuickKeyWordDesc);
				}
			}

			static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(SkillToolTip), nameof(SkillToolTip.Input));
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                int i = 0;
                var ciList = instructions.ToList();
                var c = ciList.Count();
				bool firstAfterCheckInject = false;
				LocalBuilder kwListLb = null;
                foreach (var ci in instructions)
                {

					if (firstAfterCheckInject && ci.IsStloc() 
						&& ciList[Math.Max(i - 1, 0)].opcode == OpCodes.Newobj 
						&& ciList[Math.Max(i - 2, 0)].IsStloc() 
						&& ciList[Math.Max(i - 3, 0)].opcode == OpCodes.Newobj)
                    {
						kwListLb = (LocalBuilder)ci.operand;
						firstAfterCheckInject = false;

					}

					if (ci.Is(OpCodes.Callvirt, AccessTools.Method(typeof(Skill), "get_Disposable")) && ciList[Math.Min(i + 1, c - 1)].opcode == OpCodes.Brtrue)
					{
						firstAfterCheckInject = true;
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(QuickKeywordPatch), nameof(QuickKeywordPatch.CheckQuick)));
					}
					else if (
						ci.Is(OpCodes.Callvirt, AccessTools.Method(typeof(Skill), "get_Track")) && ciList[Math.Min(i + 1, c - 1)].opcode == OpCodes.Brfalse)
					{
						yield return new CodeInstruction(OpCodes.Dup);
						yield return new CodeInstruction(OpCodes.Ldloc, kwListLb);
						yield return new CodeInstruction(OpCodes.Ldarg_0);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(QuickKeywordPatch), nameof(QuickKeywordPatch.AddQuickKeyword)));
						yield return ci;
					}
					else
					{
						yield return ci;
					}
                    i++;
                }
            }

        }


        [HarmonyPatch(typeof(ScriptLocalization.Battle_Keyword))]
		class Swift2AgileDesc_Patch
		{

			[HarmonyPatch("get_Quick"), HarmonyPostfix]
			static void NamePostfix(ref string __result)
			{
				__result = "Effortless";
			}

			[HarmonyPatch("get_Quick_Desc"), HarmonyPostfix]
			static void DescPostfix(ref string __result)
			{
				__result = "Ignores overload.";
			}
		}

		[HarmonyPatch]
		class SwiftIconPatch
		{

			static bool CheckSwift(Skill skill)
			{
				return quickManager.SkillGetQuick(skill) && skill.NotCount;
			}

			static IEnumerable<MethodBase> TargetMethods()
			{
				yield return AccessTools.Method(typeof(BasicSkill), nameof(BasicSkill.SkillInput), new Type[] { typeof(GDESkillData), typeof(BattleChar), typeof(BattleTeam) });
				yield return AccessTools.Method(typeof(BasicSkill), nameof(BasicSkill.SkillInput), new Type[] { typeof(Skill) });
				yield return AccessTools.Method(typeof(BasicSkill), "Update");
				yield return AccessTools.Method(typeof(SkillButton), "Update");
				yield return AccessTools.Method(typeof(SkillToolTip), nameof(SkillToolTip.Input));
			}

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				bool injected = false;
				foreach (var ci in instructions)
				{
					if (ci.Is(OpCodes.Callvirt, AccessTools.Method(typeof(Skill), "get_NotCount")) && !injected)
					{
						injected = true;
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SwiftIconPatch), nameof(SwiftIconPatch.CheckSwift)));
					}
					else
					{
						yield return ci;
					}
				}
			}

		}



        [HarmonyPatch(typeof(SKillCollection), "SkillAdd")]
        class SKillCollection_Patch
        {
			static bool EncyclopediaCheckSwift(GDESkillData skillData)
			{
				return skillData.NotCount && quickManager.defaultQuickness.Contains(skillData.KeyID);
			}

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				foreach (var ci in instructions)
				{
					if (ci.Is(OpCodes.Callvirt, AccessTools.Method(typeof(GDESkillData), "get_NotCount")))
					{
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SKillCollection_Patch), nameof(SKillCollection_Patch.EncyclopediaCheckSwift)));
					}
					else
					{
						yield return ci;
					}
				}
			}
		}



        [HarmonyPatch(typeof(SkillButton), nameof(SkillButton.ChoiceSkill))]
		class SkillButton_Patch
		{

			static void AssignQuick(Skill skill)
			{
				if (quickManager.SkillGetQuick(BattleSystem.instance.SelectedSkill))
				{
					quickManager.SetVal(skill, true);
				}
			}

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
			{
				CodeInstruction prevCi = null;
				foreach (var ci in instructions)
				{
					if (ci.opcode == OpCodes.Ldloc_0 && prevCi != null &&
						prevCi.Is(OpCodes.Callvirt, AccessTools.Method(typeof(Skill), "set_NotCount")))
					{
						yield return ci;
						yield return new CodeInstruction(OpCodes.Dup);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SkillButton_Patch), nameof(SkillButton_Patch.AssignQuick)));
					}
					else
					{
						yield return ci;
					}
					prevCi = ci;
				}
			}

		}


		[HarmonyPatch]
		class CheckQuickPatch
		{
			static IEnumerable<MethodBase> TargetMethods()
			{
				yield return AccessTools.Method(typeof(BattleActWindow), nameof(BattleActWindow.CountSkillPointEnter));
				yield return AccessTools.Method(typeof(BattleSystem), nameof(BattleSystem.CastEnqueue));

			}

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				foreach (var ci in instructions)
				{
					if (ci.Is(OpCodes.Callvirt, AccessTools.Method(typeof(Skill), "get_NotCount")))
					{
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SystemSwiftnessChanges), nameof(SystemSwiftnessChanges.CheckQuick)));
					}
					else
					{
						yield return ci;
					}
				}
			}
		}


		[HarmonyPatch(typeof(BattleAlly), nameof(BattleAlly.UseSkillAfter))]
		class Swiftness2IgnoreOverloadPatch
		{

			static void IncreaseOverload(Skill skill, BattleAlly ally)
			{
				if (!skill.NotCount && !skill.IsNowCasting && SaveManager.NowData.GameOptions.Difficulty != 1)
				{
					ally.Overload++;
				}
			}

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				int i = 0;
				var ciList = instructions.ToList();
				var c = ciList.Count();

				bool notCountInject = false;
				foreach (var ci in instructions)
				{
					if (ci.Is(OpCodes.Callvirt, AccessTools.Method(typeof(Skill), "get_NotCount")))
					{
						notCountInject = true;

						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SystemSwiftnessChanges), nameof(SystemSwiftnessChanges.CheckQuick)));
					}
					else if (ci.Is(OpCodes.Callvirt, AccessTools.Method(typeof(BattleChar), "set_Overload")) && notCountInject)
					{

						yield return new CodeInstruction(OpCodes.Pop);
						yield return new CodeInstruction(OpCodes.Pop);
					}
					else if (ci.Is(OpCodes.Ldfld, AccessTools.Field(typeof(Skill), nameof(Skill.BasicSkill))))
					{
						yield return new CodeInstruction(OpCodes.Dup);
						yield return new CodeInstruction(OpCodes.Ldarg_0);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Swiftness2IgnoreOverloadPatch),
							nameof(Swiftness2IgnoreOverloadPatch.IncreaseOverload)));
						yield return ci;
					}
					else
					{
						yield return ci;
					}
					i++;
				}
			}
		}

	}
}
