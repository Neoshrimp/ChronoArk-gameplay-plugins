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



		// 2do finish
		/*        [HarmonyPatch]
				class QuickKeywordPatch
				{

					static bool CheckQuick1(Skill skill)
					{
						return quickManager.SkillGetQuick(skill) || skill.Disposable;
					}

					static IEnumerable<MethodBase> TargetMethods()
					{
						yield return AccessTools.Method(typeof(SkillToolTip), nameof(SkillToolTip.Input));
					}

					static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
					{
						int i = 0;
						var ciList = instructions.ToList();
						var c = ciList.Count();
						foreach (var ci in instructions)
						{
							if (ci.Is(OpCodes.Callvirt, AccessTools.Method(typeof(Skill), "get_Disposable")) && ciList[Math.Min(i+1, c-1)].opcode == OpCodes.Brtrue)
							{
								yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(QuickKeywordPatch), nameof(QuickKeywordPatch.CheckQuick1)));
							}
							else
							else
							{
								yield return ci;
							}
							i++;
						}
					}

				}*/


		[HarmonyPatch(typeof(ScriptLocalization))]
		class Swift2IODesc_Patch
		{

			[HarmonyPatch("get_Quick"), HarmonyPostfix]
			static void NamePostfix(ref string __result)
			{
				__result = "Ignore Taunt";
			}

			[HarmonyPatch("get_Quick_Desc"), HarmonyPostfix]
			static void DescPostfix(ref string __result)
			{
				__result = "Does not overload caster.";
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
