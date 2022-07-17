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
    class FieldManagementPatches
    {

		static QuickManager quickManager = SwiftnessReworkPlugin.quickManager;



        [HarmonyPatch(typeof(Skill), nameof(Skill.initField))]
        class Skill_Patch
        {
			static void Postfix(Skill __instance)
			{

				quickManager.AddField(__instance, false);
			}
		}


		[HarmonyPatch]
		class SkillExtendedConstPatch
		{
			static IEnumerable<MethodBase> TargetMethods()
			{
				yield return AccessTools.GetDeclaredConstructors(typeof(Skill_Extended))[0];
			}

			static void Postfix(Skill_Extended __instance)
			{
				quickManager.AddField(__instance, false);
			}
		}


		[HarmonyPatch(typeof(Skill), nameof(Skill.initField))]
		class SkillinitFieldPatch
		{
			static void Postfix(Skill __instance)
			{
				if (quickManager.defaultQuickness.Contains(__instance.MySkill.KeyID))
				{
					quickManager.SetVal(__instance, true);
				}
			}
		}

		static void AddQuick(object newSkill, object oldSkill)
		{
			quickManager.SetVal(newSkill, quickManager.GetVal(oldSkill));
		}

		[HarmonyPatch]
        class SkillCloneSkill_Patch
        {

            static IEnumerable<MethodBase> TargetMethods()
            {
				yield return AccessTools.Method(typeof(Skill), nameof(Skill.CloneSkill));
				yield return AccessTools.Method(typeof(Skill_Extended), nameof(Skill_Extended.Clone));

			}


			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                CodeInstruction prevCi = null;
                foreach (var ci in instructions)
                {
                    if (ci.opcode == OpCodes.Ret)
                    {
						yield return new CodeInstruction(OpCodes.Dup);
						yield return new CodeInstruction(OpCodes.Ldarg_0);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FieldManagementPatches), nameof(FieldManagementPatches.AddQuick)));
						yield return ci;

					}
					else
                    {
                        yield return ci;
                    }
                    prevCi = ci;
                }
            }


        }


		// memberwiseClone doesn't call default constructor
		[HarmonyPatch(typeof(Skill_Extended), nameof(Skill_Extended.Clone))]
		class Skill_ExtendedMemberwiseClone_Patch
		{

			static void AddFieldOnClone(object newSkillExtend, object oldSkillExtend)
			{
				quickManager.AddField(newSkillExtend, quickManager.GetVal(oldSkillExtend));
			}
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				bool injected = false;
				foreach (var ci in instructions)
				{
					if (ci.opcode == OpCodes.Stloc_0 && !injected)
					{
						injected = true;
						yield return new CodeInstruction(OpCodes.Dup);
						yield return new CodeInstruction(OpCodes.Ldarg_0);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Skill_ExtendedMemberwiseClone_Patch), nameof(Skill_ExtendedMemberwiseClone_Patch.AddFieldOnClone)));
						yield return ci;

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
