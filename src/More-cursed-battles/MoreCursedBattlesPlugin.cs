using BepInEx;
using BepInEx.Configuration;
using GameDataEditor;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TileTypes;
using UnityEngine;


namespace More_cursed_battles
{
    [BepInPlugin(GUID, "More cursed battles", version)]
    [BepInProcess("ChronoArk.exe")]
    public class MoreCursedBattlesPlugin : BaseUnityPlugin
    {
        public const string GUID = "org.neo.chronoark.runmutators.morecursedbattles";
        public const string version = "1.0.0";

        private static readonly Harmony harmony = new Harmony(GUID);

        private static ConfigEntry<int> startingLiftingAmountConf;
        private static ConfigEntry<int> cursedBattleNumberConf;
        private static ConfigEntry<bool> enabledInSanctuary;
        private static ConfigEntry<int> cursedGoldReward;




        void Awake()
        {
            cursedBattleNumberConf = Config.Bind("Generation config", "number_of_cursed_battles_per_stage", 2, "Maximum number of cursed battles per stage counting default one. Set to 5 to curse all non-boss battles on every stage");
            enabledInSanctuary = Config.Bind("Generation config", "cursed_battles_in_Sanctuary", false, "ATTENTION! currently isn't implemented properly. Keep it off\nEnables/Disables cursed fight generation in the final area.");

            startingLiftingAmountConf = Config.Bind("Item config", "starting_lifting_scroll_amount", 2, "Amount of starting lifting scrolls. Lifting scrolls are identified. Mind that 1-2 cursed fights no longer drop lifting scrolls");
            cursedGoldReward = Config.Bind("Item config", "cursed_gold_reward", 150, "Sets the amount of gold commonly rewarded by the cursed enemies. Vanilla amount is 250");

            harmony.PatchAll();
        }
        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchAll(GUID);
        }

        //TODO
        //sanctuary dual golem mobs bug out with extra action
        //patch shared health buff to work with different max hp amounts
        //add option for better cursed rewards in sanctuary


        //maybe reduce particle intensity on cursed enemies and/or map objects




        // add starting lifting scrolls
        [HarmonyPatch(typeof(FieldSystem))]
        class FieldSystem_Patch
        {
            [HarmonyPatch(nameof(FieldSystem.StageStart))]
            [HarmonyPrefix]
            static void StageStartPrefix()
            {
                // copied from FieldSystem.StageStart
                if (PlayData.TSavedata.StageNum == 0 && !PlayData.TSavedata.GameStarted)
                {
                    // identifies lifting scroll
                    if (PlayData.TSavedata.IdentifyItems.Find((string x) => x == GDEItemKeys.Item_Scroll_Scroll_Uncurse) == null)
                        PlayData.TSavedata.IdentifyItems.Add(GDEItemKeys.Item_Scroll_Scroll_Uncurse);

                    if (startingLiftingAmountConf.Value > 0)
                        PartyInventory.InvenM.AddNewItem(ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Uncurse, startingLiftingAmountConf.Value));
                }
            }

        }

        // Fisher–Yates shuffle
        private static void KnuthShuffle<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = Random.Range(i, list.Count); // Don't select from the entire array on subsequent loops
                T temp = list[i]; list[i] = list[j]; list[j] = temp;
            }
        }

        [HarmonyPatch(typeof(StageSystem))]
        class Generate_Cursed_Battles_Patch
        {
            [HarmonyPatch(nameof(StageSystem.InstantiateIsometric))]
            [HarmonyPostfix]
            static void InstantiateIsometric(HexMap ___Map)
            {
                if (!PlayData.TSavedata.IsLoaded)
                {
                    bool isSanctuary = PlayData.TSavedata.StageNum == 5;

                    if (!isSanctuary)
                    {
                        // checks are required for both tile battles and building battles
                        List<MapTile> battleList =
                            ___Map.EventTileList.FindAll(x => (x.Info.Type is Monster) ||
                            (x.TileEventObject != null && x.TileEventObject.ObjectData != null && x.TileEventObject.Monster));
                        int curseCount = battleList.FindAll(x => x.Info.Cursed == true).Count;

                        KnuthShuffle(battleList);
                        foreach (MapTile mt in battleList)
                        {
                            if (curseCount >= cursedBattleNumberConf.Value)
                                break;
                            if (mt.Info.Cursed == false)
                            {
                                mt.Info.Cursed = true;
                                curseCount++;
                            }
                        }
                    }
                    else if (isSanctuary && enabledInSanctuary.Value)
                    {


                        List<MapTile> battleList =
                            ___Map.EventTileList.FindAll(x => (x.Info.Type is Monster) ||
                            (x.TileEventObject != null && x.TileEventObject.ObjectData != null && x.TileEventObject.Monster));

                        List<MapTile> updatedBattleList = new List<MapTile>();
                        List<MapTile> chainList = new List<MapTile>();

                        // exclude linked buildings from battles list
                        foreach (MapTile mt in battleList)
                        {
                            if (mt.TileEventObject == null)
                            {
                                updatedBattleList.Add(mt);
                            }
                            else
                            {
                                if (mt.TileEventObject.MainChain != null && !chainList.Contains(mt))
                                {
                                    foreach (EventObject eo in mt.TileEventObject.MainChain.MainObjectEvent)
                                    {
                                        if (eo.Tile != mt)
                                            chainList.Add(eo.Tile);
                                    }
                                    updatedBattleList.Add(mt);
                                }
                            }
                        }

                        int curseCount = updatedBattleList.FindAll(mt => mt.Info.Cursed).Count;
                        KnuthShuffle(updatedBattleList);

                        foreach (MapTile mt in updatedBattleList)
                        {
                            if (curseCount >= cursedBattleNumberConf.Value)
                                break;
                            if (mt.Info.Cursed == false)
                            {
                                mt.Info.Cursed = true;
                                // curse linked buildings if they exists
                                if (mt.TileEventObject != null && mt.TileEventObject.MainChain != null)
                                {
                                    foreach (EventObject eo in mt.TileEventObject.MainChain.MainObjectEvent)
                                    {
                                        eo.Tile.Info.Cursed = true;
                                    }
                                }
                                curseCount++;
                            }
                        }

                    }
                }
            }
        }

        [HarmonyPatch(typeof(B_CursedMob))]
        class Curse_Reward_Patch
        {
            [HarmonyPatch(nameof(B_CursedMob.Init))]
            [HarmonyPostfix]
            static void InitPostfix(B_CursedMob __instance, List<ItemBase> ___Itemviews)
            {
                void addGold(int amount)
                {
                    if (amount > 0)
                        ___Itemviews.Add(ItemBase.GetItem(GDEItemKeys.Item_Misc_Gold, amount));
                }

                if (___Itemviews.RemoveAll(x => x.itemkey == GDEItemKeys.Item_Misc_Gold && x.StackCount == 250) > 0)
                    addGold(cursedGoldReward.Value);

                if (PlayData.TSavedata.StageNum == 1)
                {
                    if (___Itemviews.RemoveAll(x => x.itemkey == GDEItemKeys.Item_Scroll_Scroll_Uncurse) > 0)
                        addGold(cursedGoldReward.Value);
                }
            }
        }


    }
}
