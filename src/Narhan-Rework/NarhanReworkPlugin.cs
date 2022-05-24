using BepInEx;
using GameDataEditor;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkTonic.MasterAudio;

namespace Narhan_Rework
{
    [BepInPlugin(GUID, "Alternative Narhan kit", version)]
    [BepInProcess("ChronoArk.exe")]
    public class NarhanReworkPlugin : BaseUnityPlugin
    {

        public const string GUID = "org.neo.chronoark.characters.narhanrework";
        public const string version = "0.0.1";


        private static readonly Harmony harmony = new Harmony(GUID);

        private static BepInEx.Logging.ManualLogSource logger;

        void Awake()
        {
            logger = Logger;
            harmony.PatchAll();
        }
        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchAll(GUID);
        }

        [HarmonyPatch(typeof(StageSystem), nameof(StageSystem.CheatChack))]
        class debugBattlePatch
        {
            static void Postfix(StageSystem __instance)
            {

                string cheatChat = PlayData.CheatChat;
                switch (cheatChat)
                {
                    case "bs":
                        __instance.CheatEnabled();
                        FieldSystem.instance.BattleStart(new GDEEnemyQueueData(GDEItemKeys.EnemyQueue_Queue_S3_PharosLeader), __instance.StageData.BattleMap.Key, false, false, string.Empty, string.Empty);
                        break;

                }
            }
        }

        [HarmonyPatch(typeof(S_Control_1))]
        class ParanoiaPatch
        {

            static float baseStun = 90f;
            static float stunPerId = 13f;

            static int GetStunChance(BattleChar target, BattleChar caster)
            {
                Buff identified = target.BuffReturn(GDEItemKeys.Buff_B_Control_P);
                float extraStun = 0;
                if (identified != null)
                {
                    extraStun = identified.StackNum * stunPerId;
                }

                return (int)(baseStun + extraStun + caster.GetStat.HIT_CC);
            }

            static List<BattleChar> GetTargets(BattleChar hit)
            {
                List<BattleChar> finalTargets = new List<BattleChar>();

                int targetPos = 0;
                if (!hit.Info.Ally)
                {
                    Debug.Log("deeznuts");
                    List<BattleEnemy> targetList =(hit as BattleEnemy).EnemyPosNum(out targetPos);
                    Debug.Log("deeznuts");

                    if (targetPos != 0)
                    {
                        finalTargets.Add(targetList[targetPos - 1]);
                    }
                    else
                    {
                        finalTargets.Add(targetList[targetPos]);
                    }
                    if (targetList.Count > targetPos + 1)
                    {
                        finalTargets.Add(targetList[targetPos + 1]);
                    }
                    else
                    {
                        finalTargets.Add(targetList[targetPos]);
                    }
                }
                else
                {
                    List<BattleAlly> targetList = BattleSystem.instance.AllyList;
                    for (int i = 0; i < BattleSystem.instance.AllyList.Count; i++)
                    {
                        if (BattleSystem.instance.AllyTeam.AliveChars[i] == hit)
                        {
                            targetPos = i;
                            break;
                        }

                    }

                    if (targetPos != 0)
                    {
                        finalTargets.Add(targetList[targetPos - 1]);
                    }
                    else
                    {
                        finalTargets.Add(targetList[targetPos]);
                    }
                    if (targetList.Count > targetPos + 1)
                    {
                        finalTargets.Add(targetList[targetPos + 1]);
                    }
                    else
                    {
                        finalTargets.Add(targetList[targetPos]);
                    }
                }

                return finalTargets;
            }

            [HarmonyPatch(nameof(S_Control_1.DescExtended))]
            [HarmonyPrefix]
            //Adding total stun chance would be great
            static bool DescPrefix(ref string desc, ref string __result, S_Control_0 __instance)
            {
                //add base desc
                __result = "Stun rightmost or leftmost enemy from selected target. If there's only one enemy attempt to stun it twice. " +
                    "Stun chance is equal to &a + &b per stack of Indentified! debuff the targeted enemy has.";
                __result = __result.Replace("&a", ((int)(baseStun + __instance.BChar.GetStat.HIT_CC)).ToString());
                __result = __result.Replace("&b", ((int)stunPerId).ToString());
                //desc.Replace("&a", ((int)(baseStun + __instance.BChar.GetStat.HIT_CC)).ToString()) + " deeznuts";
                return false;
            }

            [HarmonyPatch(nameof(S_Control_1.AttackEffectSingle))]
            [HarmonyPrefix]

            static bool SkillUseSinglePrefix(BattleChar hit, SkillParticle SP, int DMG, int Heal, S_Control_1 __instance)
            {
                //add base attack

                List<BattleChar> finalTargets = GetTargets(hit);
                Debug.Log("deeznuts");

                if (finalTargets != null)
                {
                    BattleChar prevTarget = null;
                    foreach (BattleChar t in finalTargets)
                    {
                        //TODO add additional sfx for second hit

                        if (t.Equals(prevTarget))
                        {
                            Debug.Log("deeznuts");
                            //BattleSystem.DelayInput(Wait());
                            MasterAudio.PlaySound("Control_1", 1f, null, 5f, null, 5f, false, false);
                        }
                        t.BuffAdd(GDEItemKeys.Buff_B_Common_Rest, __instance.BChar, false, GetStunChance(t, __instance.BChar), false, -1, false);
                        prevTarget = t;
                    }
                }

                return false;
            }
            static IEnumerator Wait()
            {
                yield return new WaitForSeconds(1f);
                yield break;
            }

        }



        // i don't like this. This just makes NS plain value everytime
        [HarmonyPatch(typeof(S_Control_0))]
        class NightmareSindromePatch
        {

            static float baseDmg = 0.45f;

            [HarmonyPatch(nameof(S_Control_0.DescExtended))]
            [HarmonyPrefix]
            static bool DescPrefix(ref string desc, ref string __result, S_Control_0 __instance)
            {
                //base desc
                __result = desc + " deeznuts";
                return false;
            }

            [HarmonyPatch(nameof(S_Control_0.SkillUseSingle))]
            [HarmonyPrefix]

            static bool SkillUseSinglePrefix(ref Skill SkillD, ref List<BattleChar> Targets, S_Control_0 __instance)
            {
                //base.SkillUseSingle(SkillD, Targets);
                __instance.SkillBasePlus.Target_BaseDMG = 0;

                List<BattleChar> finalTargets = new List<BattleChar>();

                if (!Targets[0].Info.Ally)
                {
                    int targetPos = 0;
                    List<BattleEnemy> targetList = (Targets[0] as BattleEnemy).EnemyPosNum(out targetPos);
                    if (targetPos != 0)
                    {
                        finalTargets.Add(targetList[targetPos - 1]);
                    }
                    else
                    {
                        finalTargets.Add(targetList[targetPos]);
                    }
                    if (targetList.Count > targetPos + 1)
                    {
                        finalTargets.Add(targetList[targetPos + 1]);
                    }
                    else
                    {
                        finalTargets.Add(targetList[targetPos]);
                    }
                }
                else
                {
                    int targetPos = 0;
                    List<BattleAlly> targetList = BattleSystem.instance.AllyList;

                    for (int i = 0; i < BattleSystem.instance.AllyTeam.AliveChars.Count; i++)
                    {
                        if (BattleSystem.instance.AllyTeam.AliveChars[i] == Targets[0])
                        {
                            targetPos = i;
                        }

                    }
                    if (targetPos != 0)
                    {
                        finalTargets.Add(targetList[targetPos - 1]);
                    }
                    else
                    {
                        finalTargets.Add(targetList[targetPos]);
                    }
                    if (targetList.Count > targetPos + 1)
                    {
                        finalTargets.Add(targetList[targetPos + 1]);
                    }
                    else
                    {
                        finalTargets.Add(targetList[targetPos]);
                    }
                }

                if (finalTargets.Count != 0)
                {
                    Skill tempSkill = Skill.TempSkill(GDEItemKeys.Skill_S_Control_0_0, __instance.BChar, __instance.BChar.MyTeam);
                    tempSkill.AllExtendeds[0].IsDamage = true;
                    tempSkill.FreeUse = true;
                    tempSkill.AllExtendeds[0].SkillBasePlus.Target_BaseDMG = (int)((float)__instance.BChar.GetStat.maxhp * baseDmg);
                    __instance.BChar.ParticleOut(tempSkill, finalTargets);
                }

                return false;
            }
        }

    }
}
