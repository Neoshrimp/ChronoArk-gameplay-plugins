using BepInEx;
using GameDataEditor;
using HarmonyLib;
using System.Collections;


namespace Lift_Curse_Card_placement
{
    [BepInPlugin(GUID, "Uncurse card at bottom of the hand", version)]
    [BepInProcess("ChronoArk.exe")]
    public class LifCurseCardPlacementPlugin : BaseUnityPlugin
    {
        public const string GUID = "org.neo.chronoark.qolmods.uncursecard";
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



        // didn't find an exact point of injection where battle has started and first hand has been drawn but simple TurnNum check should suffice 
        [HarmonyPatch(typeof(BattleTeam))]
        class Move_Uncurse_Card_Patch
        {
            [HarmonyPatch(nameof(BattleTeam.MyTurn))]
            [HarmonyPostfix]
            static void MyTurnPostfix(BattleTeam __instance)
            {
                if (BattleSystem.instance != null)
                {
                    if (BattleSystem.instance.TurnNum == 0)
                    {

                        Skill unCurseCard = BattleSystem.instance.AllyTeam.Skills.Find(x => x.MySkill.KeyID == GDEItemKeys.Skill_S_UnCurse);

                        if (BattleSystem.instance.AllyTeam.Skills.Remove(unCurseCard))
                        {

                            // in reality there should be a separate function for moving a card
                            BattleSystem.instance.AllyTeam.Add(unCurseCard, true);
                        }

                    }
                }
            }
        }



    }
}
