using BepInEx;
using GameDataEditor;
using HarmonyLib;
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
                if (BattleSystem.instance.TurnNum == 0)
                {
                    GDEEnemyData gdeenemyData = new GDEEnemyData(EnemyString);
                    if (gdeenemyData.Boss == true)
                    {
                        Curse = true;
                    }
                }
            }
        }


    }
}
