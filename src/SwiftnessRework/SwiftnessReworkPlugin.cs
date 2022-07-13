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

namespace SwiftnessRework
{
    [BepInPlugin(GUID, "Swiftness Rework", version)]
    [BepInProcess("ChronoArk.exe")]
    public class SwiftnessReworkPlugin : BaseUnityPlugin
    {

        public const string GUID = "neo.ca.gameplay.swiftnessRework";
        public const string version = "1.0.0";


        private static readonly Harmony harmony = new Harmony(GUID);

        private static BepInEx.Logging.ManualLogSource logger;

		static QuickManager quickManager;

		void Awake()
        {

            logger = Logger;
			var defaultQuickness = new HashSet<string>();
			defaultQuickness.Add(GDEItemKeys.Skill_S_Public_9);
			defaultQuickness.Add(GDEItemKeys.Skill_S_Azar_P_0);
			defaultQuickness.Add(GDEItemKeys.Skill_S_Azar_9);
			defaultQuickness.Add(GDEItemKeys.Skill_S_Azar_0);
			defaultQuickness.Add(GDEItemKeys.Skill_S_Azar_2);
			defaultQuickness.Add(GDEItemKeys.Skill_S_Azar_11);

			quickManager = new QuickManager(defaultQuickness);
            harmony.PatchAll();

			StartCoroutine(CleanFields());
        }
        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchAll(GUID);
        }

		public static bool cullFields = true;

		IEnumerator CleanFields()
		{
			while (cullFields)
			{
				yield return new WaitForSeconds(30);
				quickManager.CullDestroyed();
			}

		}


		[HarmonyPatch]
		class SkillConstPatch
		{
			static IEnumerable<MethodBase> TargetMethods()
			{
				//Debug.Log(AccessTools.GetDeclaredConstructors(typeof(Skill))[0]);
				yield return AccessTools.GetDeclaredConstructors(typeof(Skill))[0];
			}

			static void Postfix(Skill __instance)
			{

				//AddField(__instance, true);
				// doesnt have key yet
				quickManager.AddField(__instance, false);
			}
		}

		[HarmonyPatch(typeof(Skill), nameof(Skill.initField))]
		class SkillinitFieldPatch
		{
			static void Postfix(Skill __instance)
			{
				if (quickManager.defaultQuickness.Contains(__instance.MySkill.Key) || quickManager.defaultQuickness.Contains(__instance.MySkill.KeyID))
				{
					quickManager.AddField(__instance, true);
				}
			}
		}

		[HarmonyPatch]
		class SkillExtendedConstPatch
		{
			static IEnumerable<MethodBase> TargetMethods()
			{
				//Debug.Log(AccessTools.GetDeclaredConstructors(typeof(Skill))[0]);
				yield return AccessTools.GetDeclaredConstructors(typeof(Skill_Extended))[0];
			}

			static void Postfix(Skill __instance)
			{
				quickManager.AddField(__instance, false);
			}
		}

		/*        [HarmonyPatch(typeof(Skill), nameof(Skill.CloneSkill)]
				class Skill_ClonePatch
				{

				}*/



		[HarmonyPatch(typeof(BattleActWindow), nameof(BattleActWindow.CountSkillPointEnter))]
        class BattleActWindow_CountSkillPointEntePatch
        {
            static bool Prefix(Skill CastPreview, BattleActWindow __instance)
            {
				if (__instance.CastingGroup.transform.childCount != 0)
				{
					__instance.CountLine.transform.SetParent(__instance.CastingGroup.transform);
					__instance.CountLine.SetActive(true);
					__instance.CountLine.GetComponent<Animator>().Play("SkillButton_LineDraw");
					__instance.CountLine.GetComponent<Animator>().Play("SkillButton_LineIdle");
					List<CastingSkill> list = new List<CastingSkill>();
					list.AddRange(BattleSystem.instance.CastSkills);
					CastingSkill castingSkill = new CastingSkill();
					castingSkill.skill = CastPreview;
					castingSkill._CastSpeed = castingSkill.skill.Counting;
                    if (!quickManager.SkillGetQuick(castingSkill.skill))
                    {
                        castingSkill._CastSpeed++;
                    }
                    list.Add(castingSkill);
					list.AddRange(BattleSystem.instance.EnemyCastSkills);
					list.AddRange(BattleSystem.instance.SaveSkill);
					list = (from x in list
							orderby x.CastSpeed
							select x).ToList<CastingSkill>();
					for (int i = 0; i < list.Count; i++)
					{
						if (CastPreview == list[i].skill)
						{
							__instance.CountLine.transform.SetAsLastSibling();
						}
						else
						{
							foreach (SkillButton skillButton in __instance.CastingGroup.GetComponentsInChildren<SkillButton>())
							{
								if (list[i].CastButton != null && skillButton == list[i].CastButton)
								{
									skillButton.transform.parent.SetAsLastSibling();
									break;
								}
							}
						}
					}
				}
				return false;
			}
			
        }

		[HarmonyPatch(typeof(BattleAlly), nameof(BattleAlly.UseSkillAfter))]
		class BattleAlly_UseSkillAfterPatch
		{
			static bool Prefix(Skill skill, BattleAlly __instance)
			{
				int ap = skill.AP;
				skill.UsedApNum = ap;
				if (!skill.FreeUse)
				{
					if (!skill.IsNowCasting)
					{
						__instance.MyTeam.AP -= skill.UsedApNum;
					}
					if (SaveManager.NowData.GameOptions.Difficulty == 1 && __instance.IsLucy && !skill.IsNowCasting)
					{
						__instance.Overload++;
					}
					if (!quickManager.SkillGetQuick(skill) && !skill.IsNowCasting) //!quick
					{
						__instance.ActionCount--;
						__instance.MyTeam.TurnActionNum++;
					}
					if (!skill.NotCount)
					{
						if (SaveManager.NowData.GameOptions.Difficulty != 1)
						{
							__instance.Overload++;
						}
					}
					if (skill.BasicSkill)
					{
						__instance.MyBasicSkill.ThisSkillUse = true;
						foreach (Skill_Extended skill_Extended in __instance.MyBasicSkill.buttonData.AllExtendeds)
						{
							if (skill_Extended.SkillParticleLive != null)
							{
								UnityEngine.Object.Destroy(skill_Extended.SkillParticleLive);
							}
						}
						if (SaveManager.Difficalty == 2)
						{
							__instance.MyBasicSkill.CoolDownNum = 2;
							if (__instance.MyBasicSkill.buttonData.BasicOption)
							{
								__instance.MyBasicSkill.CoolDownNum--;
							}
						}
						if (__instance.MyBasicSkill.buttonData.BasicOption)
						{
							__instance.MyBasicSkill.InActive = true;
						}
						else
						{
							for (int i = 0; i < __instance.MyTeam.Chars.Count; i++)
							{
								BattleAlly battleAlly = __instance.MyTeam.Chars[i] as BattleAlly;
								if (!battleAlly.IsDead && battleAlly.MyBasicSkill != null && !battleAlly.MyBasicSkill.buttonData.BasicOption)
								{
									battleAlly.MyBasicSkill.InActive = true;
								}
							}
						}
					}
					foreach (IP_SkillUse_Team ip_SkillUse_Team in __instance.IReturn<IP_SkillUse_Team>(null))
					{
						if (ip_SkillUse_Team != null)
						{
							ip_SkillUse_Team.SkillUseTeam(skill);
						}
					}
				}
				else if (skill.OriginalSelectSkill != null)
				{
					if (skill.BasicOption)
					{
						skill.OriginalSelectSkill.BasicOption = true;
					}
					__instance.UseSkillAfter(skill.OriginalSelectSkill);
				}
				if (!__instance.Dummy && !__instance.IsLucyNoC)
				{
					__instance.WindowAni.SetTrigger("Deselected");
				}
				return false;
			}

		}


		[HarmonyPatch(typeof(BattleSystem), nameof(BattleSystem.CastEnqueue))]
		class BattleSystem_CastEnqueuePatch
		{
			static bool Prefix(BattleSystem __instance, BattleChar Target = null, Skill Targetskill = null)
			{
				MasterAudio.PlaySound("WaitButton", 1f, null, 0f, null, null, false, false);
				CastingSkill castingSkill = new CastingSkill();
				castingSkill.skill = __instance.SelectedSkill;
				castingSkill.Target = Target;
				castingSkill.Usestate = __instance.ActionAlly;
				castingSkill.SkillTarget = Targetskill;
				__instance.ActionAlly.UseSkillAfter(__instance.SelectedSkill);
				__instance.CastSkills.Add(castingSkill);
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.ActWindow.Window.SkillButton, __instance.ActWindow.CastingGroup.transform);
				SkillButton componentInChildren = gameObject.GetComponentInChildren<SkillButton>();
				componentInChildren.InputData(__instance.SelectedSkill, castingSkill, false);
				castingSkill.CastButton = componentInChildren;
                if (!quickManager.SkillGetQuick(__instance.SelectedSkill))
                {
                    castingSkill._CastSpeed++;
                }
                List<CastingSkill> list = new List<CastingSkill>();
				list.AddRange(__instance.CastSkills);
				list.AddRange(__instance.EnemyCastSkills);
				list.AddRange(__instance.SaveSkill);
				list = (from x in list
						orderby x.CastSpeed
						select x).ToList<CastingSkill>();
				for (int i = 0; i < list.Count; i++)
				{
					foreach (SkillButton skillButton in __instance.ActWindow.CastingGroup.GetComponentsInChildren<SkillButton>())
					{
						if (list[i].CastButton != null && skillButton == list[i].CastButton)
						{
							skillButton.transform.parent.SetAsLastSibling();
							break;
						}
					}
				}
				__instance.ActWindow.CountLine.SetActive(false);
                if (!!quickManager.SkillGetQuick(__instance.SelectedSkill))
                {
                    castingSkill._CastSpeed--;
                }
                return false;
			}
		}

		//[HarmonyPatch(typeof(SkillButton), nameof(SkillButton.ChoiceSkill))] quick

	}
}
