using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameDataEditor;
using HarmonyLib;
using UnityEngine;

namespace More_cursed_battles
{
    // TODO
    // make more compatible with other potential max hp altering mods. Create list of custom hidden buffs to modify max hp

    [HarmonyPatch(typeof(BattleTeam), nameof(BattleTeam.MyTurn))]
    class AvarageLifeLinkMaxHpPatch
    {
        static void Postfix()
        {
            if (BattleSystem.instance != null)
            {
                if (BattleSystem.instance.TurnNum == 0)
                {
                    List<float> linkMaxHps = new List<float>();
                    foreach (BattleEnemy battleEnemy in BattleSystem.instance.EnemyList)
                    {
                        if (battleEnemy.BuffFind(GDEItemKeys.Buff_P_Guard_LifeShare, false))
                        {
                            linkMaxHps.Add((float)battleEnemy.Info.get_stat.maxhp);
                        }
                    }

                    if (linkMaxHps.Count > 0)
                    {
                        int avgMaxHp = (int)linkMaxHps.Average();
                        //Debug.Log(avgMaxHp);
                        foreach (BattleEnemy battleEnemy in BattleSystem.instance.EnemyList)
                        {
                            if (battleEnemy.BuffFind(GDEItemKeys.Buff_P_Guard_LifeShare, false))
                            {
                                battleEnemy.BuffReturn(GDEItemKeys.Buff_P_Guard_LifeShare).PlusStat.maxhp += avgMaxHp - battleEnemy.Info.get_stat.maxhp;
                            }
                        }
                    }
                }
            }
        }
    }


    // reset max hp modification on in case of uncurse
    [HarmonyPatch(typeof(SkillExtended_UnCurse), nameof(SkillExtended_UnCurse.SkillUseSingle))]
    class UncurseCardPatch
    {
        static void Prefix()
        {
            foreach (BattleEnemy battleEnemy in BattleSystem.instance.EnemyList)
            {
                if (battleEnemy.BuffFind(GDEItemKeys.Buff_P_Guard_LifeShare, false))
                {
                    battleEnemy.BuffReturn(GDEItemKeys.Buff_P_Guard_LifeShare).PlusStat.maxhp = 0;
                }
            }
        }
    }

}
