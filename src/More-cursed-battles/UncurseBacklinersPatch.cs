using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using GameDataEditor;
using HarmonyLib;
using UnityEngine;

namespace More_cursed_battles
{
    [HarmonyPatch(typeof(SkillExtended_UnCurse), nameof(SkillExtended_UnCurse.SkillUseSingle))]
    class UncurseBacklinersPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var ci in instructions)
            {
                if (ci.opcode == OpCodes.Callvirt && ((MethodInfo)ci.operand).Equals(AccessTools.PropertyGetter(typeof(BattleSystem), nameof(BattleSystem.EnemyList))))
                {
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BattleSystem), nameof(BattleSystem.EnemyTeam)));
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(EnemyTeam), nameof(EnemyTeam.AliveChars_Vanish)));
                }
                else
                {
                    yield return ci;
                }
            }
        }


    }
}
