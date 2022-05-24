using BepInEx;
using GameDataEditor;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using TileTypes;
using UnityEngine;

namespace Cursed_Bosses
{
    [BepInPlugin(GUID, "Cursed Bosses", version)]
    [BepInProcess("ChronoArk.exe")]
    public class CursedBossesPlugin : BaseUnityPlugin
    {

        public const string GUID = "org.neo.chronoark.runmutators.cursedbosses";
        public const string version = "1.0.0";


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

        //todo
        //create custom boss curse class which modifies extra attack speed and perhaps damage/debuff chance
        //reduce hp, resist bonus
        //increase fog timer
        //integrate it in stagesyste code. properly display on minimap
        //add options to change modifiers 
        //create transmute curse scroll cards. number of lifting scrolls = number of uses?
        //dont regular lifting scrolls
        //ban executioner on tank(at least first turn)
        //move dorchi to 2-1
        //make tfk transition to phase 2 at half hp
        //test twins, pharos leader special attacks


        // ISSUE particles doesn't disappear after battle
        // BIG ISSUE bricks game on area transition. Perhaps patch on destroy
        // only visual cursed tile effect
        /*        [HarmonyPatch(typeof(StageSystem), nameof(StageSystem.InstantiateIsometric))]
                class CurseParticlesBossTile
                {
                    [HarmonyPostfix]
                    static void InstantiateIsometric(HexMap ___Map)
                    {
                        ___Map.EventTileList.Find((MapTile map) => map.Info.Type is Boss).Info.Cursed = true;
                    }
                }*/


        [HarmonyPatch(typeof(BattleSystem))]
        class BattleSystem_Patch
        {
            [HarmonyPatch(nameof(BattleSystem.CreatEnemy))]
            [HarmonyPrefix]
            static void CreatEnemyPrefix(string EnemyString, ref bool Curse)
            {
                if (BattleSystem.instance != null)
                {
                    if (BattleSystem.instance.TurnNum == 0)
                    {
                        GDEEnemyData gdeenemyData = new GDEEnemyData(EnemyString);
                        if (gdeenemyData.Boss == true)
                        {
                            //Curse = true;
                            Curse = false;
                        }
                    }
                }
            }
        }

/*        [HarmonyPatch(typeof(StageSystem))]
        class Generate_Cursed_Battles_Patch
        {
            [HarmonyPatch(nameof(StageSystem.InstantiateIsometric))]
            [HarmonyPostfix]
            static void Postfix(HexMap ___Map)
            {
                MapTile bossmt = ___Map.EventTileList.Find(x => (x.Info.Type is Boss));
                if (bossmt != null)
                {
                    *//*                    foreach (FieldInfo f in typeof(Boss).GetFields())
                                        {
                                            Debug.Log(f.Name + ": " + f.GetValue(bossmt.Info.Type));
                                        }
                                        foreach (var f in typeof(MapTile).GetFields())
                                        {
                                            Debug.Log(f.Name + ": " + f.GetValue(bossmt));
                                        }*//*
                    if (bossmt.TileEventObject != null)
                    {
                        foreach (var f in typeof(EventObject).GetFields())
                        {
                            Debug.Log(f.Name + ": " + f.GetValue(bossmt.TileEventObject));
                        }
                        Debug.Log("----------------------------------------");
                        if (bossmt.TileEventObject.ObjectData != null)
                            foreach (var f in typeof(GDEFieldObjectData).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                            {
                                Debug.Log(f.Name + ": " + f.GetValue(bossmt.TileEventObject.ObjectData));
                            }
                        Debug.Log("----------------------------------------");

                        //                        bossmt.TileEventObject.MonsterEvent.Queue = new GDEEnemyQueueData(GDEItemKeys.EnemyQueue_Queue_S3_Reaper);

                    }

                }
            }
        }
*/

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
                        FieldSystem.instance.BattleStart(new GDEEnemyQueueData(GDEItemKeys.EnemyQueue_Queue_S3_PharosLeader), __instance.StageData.BattleMap.Key, false, string.Empty, string.Empty);
                        break;

                }
            }
        }

        
/*        [HarmonyPatch(typeof(Extended_Trisha_1), "Init")]
        class bstbP
        {
            static void Postfix(Extended_Trisha_1 __instance)
            {
                __instance.Fatal = true;
                foreach (FieldInfo fi in AccessTools.GetDeclaredFields(typeof(Extended_Trisha_1)))
                {
                    Debug.Log(fi.Name + ": " + fi.GetValue(__instance));
                }
                Debug.Log("-----Skill-----");
                if(__instance.MySkill != null)
                    foreach (FieldInfo fi in AccessTools.GetDeclaredFields(typeof(Skill)))
                    {
                        Debug.Log(fi.Name + ": " + fi.GetValue(__instance.MySkill));
                    }

            }
        }*/



    }
}
