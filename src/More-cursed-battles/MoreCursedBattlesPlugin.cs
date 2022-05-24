using BepInEx;
using BepInEx.Configuration;
using GameDataEditor;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TileTypes;
using UnityEngine;
using Random = UnityEngine.Random;


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
        private static ConfigEntry<bool> enabledInCW;

        private static ConfigEntry<int> cursedGoldReward;
        private static ConfigEntry<bool> betterCursedRewardsInSanctuary;
        private static ConfigEntry<bool> betterCursedRewardsInCW;
        private static ConfigEntry<bool> restoreUncommonRewards;






        void Awake()
        {
            cursedBattleNumberConf = Config.Bind("Generation config", "number_of_cursed_battles_per_stage", 2, "Additional cursed battles for each stage. Set to 4 to curse all non-boss battles on every stage.");
            enabledInSanctuary = Config.Bind("Generation config", "cursed_battles_in_Sanctuary", true, "Enables/Disables cursed fight generation in the final area. (acceptable values: true/false)");
            enabledInCW = Config.Bind("Generation config", "cursed_battles_in_Crimson_Wilderness", true, "Enables/Disables cursed fight generation in Crimson Wilderness. (acceptable values: true/false)");

            startingLiftingAmountConf = Config.Bind("Item config", "starting_lifting_scroll_amount", 2, "Amount of starting lifting scrolls. Lifting scrolls are identified. Mind that 1-2 cursed fights no longer drop lifting scrolls.");
            cursedGoldReward = Config.Bind("Item config", "cursed_gold_reward", 100, "Sets the amount of gold commonly rewarded by killing cursed enemies. Vanilla amount is 200.");
            betterCursedRewardsInSanctuary = Config.Bind("Item config", "enable_better_cursed_rewards_in_Sanctuary", true, "Cursed enemies in Sanctuary drop better rewards like potions or rare items. (acceptable values: true/false)");
            betterCursedRewardsInCW = Config.Bind("Item config", "enable_better_cursed_rewards_in_Crimson_Wilderness", true, "Cursed enemies in Crimson Wilderness drop better rewards. (acceptable values: true/false)");
            restoreUncommonRewards = Config.Bind("Item config", "restore_uncommon_rewards", true, "Make certain enemies drop blue equipment as they used to (acceptable values: true/false)");

            harmony.PatchAll();
        }
        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchAll(GUID);
        }

        //TODO
        //add 

        //loading removes original red particles
        //check cursed deathbringer. checked. it's kinda bs
        //cursed lightning hedgehogs produces exception


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

        static private List<MapTile> ogCursedTiles = new List<MapTile>();

        [HarmonyPatch(typeof(StageSystem))]
        class Generate_Cursed_Battles_Patch
        {
            [HarmonyPatch(nameof(StageSystem.InstantiateIsometric))]
            [HarmonyPostfix]
            static void InstantiateIsometric(HexMap ___Map)
            {
                if (!PlayData.TSavedata.IsLoaded)
                {
                    bool isSanctuary = ___Map.StageData.Key == GDEItemKeys.Stage_Stage4;
                    bool isCrimson = ___Map.StageData.Key == GDEItemKeys.Stage_Stage_Crimson;

                    Debug.Log(isSanctuary);
                    Debug.Log(isCrimson);

                    if ((!isSanctuary && !isCrimson) || (isSanctuary && enabledInSanctuary.Value) || (isCrimson && enabledInCW.Value))
                    {


                        List<MapTile> battleList =
                            ___Map.EventTileList.FindAll(x => (x.Info.Type is Monster) ||
                            (x.TileEventObject != null && x.TileEventObject.ObjectData != null && x.TileEventObject.Monster));

                        ogCursedTiles = battleList.FindAll(x => x.Info.Cursed == true);

                        List<MapTile> updatedBattleList = new List<MapTile>();
                        var chainList = new HashSet<MapTile>();


                        // exclude linked buildings from battles list
                        foreach (MapTile mt in battleList)
                        {
                            if (mt.Info.Cursed)
                                continue;
                            if (mt.TileEventObject == null)
                            {
                                updatedBattleList.Add(mt);
                            }
                            else if (mt.TileEventObject.MainChain == null)
                            {
                                updatedBattleList.Add(mt);
                            }
                            else if (!chainList.Contains(mt))
                            {
                                foreach (EventObject eo in mt.TileEventObject.MainChain.MainObjectEvent)
                                {
                                    if (eo.Tile != mt)
                                        chainList.Add(eo.Tile);
                                }
                                updatedBattleList.Add(mt);
                            }
                        }

                        int curseCount = 0;
                        KnuthShuffle(updatedBattleList);

                        foreach (MapTile mt in updatedBattleList)
                        {
                            if (curseCount >= cursedBattleNumberConf.Value)
                                break;
                            if (mt.Info.Cursed == false)
                            {
                                mt.Info.Cursed = true;
                                // no longer relevant in 1.82 as there are no more chained events
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

        // changes emitted particle colour for additionally generated cursed battles
        [HarmonyPatch(typeof(MiniHex), nameof(MiniHex.SightUpdate))]
        class MiniHexPatch
        {
            static void Postfix(MiniHex __instance)
            {
                MapTile mt = __instance.MyTile;
                if (mt != null && __instance.Cursed != null && 
                    (mt.Info.Type is Monster || (mt.TileEventObject != null && mt.TileEventObject.ObjectData != null && mt.TileEventObject.Monster)))
                {
                    if (!ogCursedTiles.Contains(mt))
                    {
                        ParticleSystem ps = __instance.Cursed.GetComponent<ParticleSystem>();
                        if (ps != null)
                        {
                            var main = ps.main;
                            main.startColor = new ParticleSystem.MinMaxGradient(new Color(216f / 255f, 43f / 255f, 200f / 255f), Color.black);
                        }
                    }
                }
            }
        }



        [HarmonyPatch(typeof(B_CursedMob))]
        class Curse_Reward_Patch
        {
            static int vanillaGoldReward;

            [HarmonyPatch(nameof(B_CursedMob.Init))]
            [HarmonyPostfix]
            static void InitPostfix(B_CursedMob __instance, List<ItemBase> ___Itemviews)
            {
                void addGold(int amount)
                {
                    if (amount > 0)
                        ___Itemviews.Add(ItemBase.GetItem(GDEItemKeys.Item_Misc_Gold, amount));
                }

                if (betterCursedRewardsInSanctuary.Value && PlayData.TSavedata.NowStageMapKey == GDEItemKeys.Stage_Stage4)
                {

                    // add orange item reward if facing cursed dickhead trio
                    if (__instance.BChar.Info.KeyData == GDEItemKeys.Enemy_S4_Guard_0 || __instance.BChar.Info.KeyData == GDEItemKeys.Enemy_S4_Guard_1
                        || __instance.BChar.Info.KeyData == GDEItemKeys.Enemy_S4_Guard_2)
                    {
                        ___Itemviews.RemoveAll(x => x.itemkey == GDEItemKeys.Item_Misc_Gold && x.StackCount == vanillaGoldReward);
                        ___Itemviews.Add(ItemBase.GetItem(PlayData.GetEquipRandom(4)));
                    }
                    // add purple item reward
                    if (__instance.BChar.Info.KeyData == GDEItemKeys.Enemy_S4_Summoner || __instance.BChar.Info.KeyData == GDEItemKeys.Enemy_S4_SleepDochi
                        || __instance.BChar.Info.KeyData == GDEItemKeys.Enemy_S4_Golem || __instance.BChar.Info.KeyData == GDEItemKeys.Enemy_S4_Golem2)
                    {
                        ___Itemviews.RemoveAll(x => x.itemkey == GDEItemKeys.Item_Misc_Gold && x.StackCount == vanillaGoldReward);
                        ___Itemviews.Add(ItemBase.GetItem(PlayData.GetEquipRandom(3)));
                    }
                    // potions as a reward for 'mandatory' fight
                    if (__instance.BChar.Info.KeyData == GDEItemKeys.Enemy_S4_4thDochi || __instance.BChar.Info.KeyData == GDEItemKeys.Enemy_S4_MagicDochi
                        || __instance.BChar.Info.KeyData == GDEItemKeys.Enemy_S4_AngryDochi)
                    {
                        ___Itemviews.RemoveAll(x => x.itemkey == GDEItemKeys.Item_Misc_Gold && x.StackCount == vanillaGoldReward);
                        ___Itemviews.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_GetPotion, false));
                    }
                }

                if (___Itemviews.RemoveAll(x => x.itemkey == GDEItemKeys.Item_Misc_Gold && x.StackCount == vanillaGoldReward) > 0)
                    addGold(cursedGoldReward.Value);

                if (PlayData.TSavedata.StageNum == 1)
                {
                    if (___Itemviews.RemoveAll(x => x.itemkey == GDEItemKeys.Item_Scroll_Scroll_Uncurse) > 0)
                        addGold(cursedGoldReward.Value);
                }
            }

            [HarmonyPatch(nameof(B_CursedMob.Init))]
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> InitTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                int i = 0;
                var list = instructions.ToList();
                var c = list.Count;
                foreach (var ci in instructions)
                {
                    if (ci.opcode == OpCodes.Ldc_I4_1
                        && list[Math.Min(i+1, c-1)].opcode == OpCodes.Call
                        && ((MethodInfo)list[Math.Min(i + 1, c - 1)].operand).Equals(AccessTools.Method(typeof(PlayData), nameof(PlayData.GetEquipRandom)))
                        && restoreUncommonRewards.Value)
                    {
                        yield return new CodeInstruction(OpCodes.Ldc_I4_2);
                    }
                    else if (ci.opcode == OpCodes.Ldc_I4 && list[Math.Max(i - 1, 0)].opcode == OpCodes.Ldsfld 
                        && ((FieldInfo)list[Math.Max(i - 1, 0)].operand).Equals(AccessTools.Field(typeof(GDEItemKeys), nameof(GDEItemKeys.Item_Misc_Gold))))
                    {
                        vanillaGoldReward = (int)ci.operand;
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
