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
    class SystemSwiftnessChanges
    {

		static QuickManager quickManager = SwiftnessReworkPlugin.quickManager;

		static bool CheckQuick(Skill skill)
		{
			return quickManager.SkillGetQuick(skill);
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


		[HarmonyPatch(typeof(SkillButton), nameof(SkillButton.ChoiceSkill))]
		class SkillButton_Patch
		{

			static void AssignQuick(Skill skill)
			{
				if (quickManager.GetVal(BattleSystem.instance.SelectedSkill))
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
				int i = 0;
				var ciList = instructions.ToList();
				var c = ciList.Count();
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
					i++;
				}
			}
		}


		[HarmonyPatch(typeof(BattleAlly), nameof(BattleAlly.UseSkillAfter))]
		class Swiftness2IgnoreOverlaodPatch
		{

			static void IcreaseOverload(Skill skill, BattleAlly ally)
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
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Swiftness2IgnoreOverlaodPatch),
							nameof(Swiftness2IgnoreOverlaodPatch.IcreaseOverload)));
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
