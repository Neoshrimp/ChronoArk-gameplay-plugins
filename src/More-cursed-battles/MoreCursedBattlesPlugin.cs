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
using BepInEx.Logging;
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

        public static BepInEx.Logging.ManualLogSource logger;

        private static ConfigEntry<int> startingLiftingAmountConf;
        private static ConfigEntry<int> cursedBattleNumberConf;
        private static ConfigEntry<bool> enabledInSanctuary;
        private static ConfigEntry<bool> enabledInCW;
        private static ConfigEntry<bool> limitedUncurse;


        private static ConfigEntry<int> cursedGoldReward;
        private static ConfigEntry<bool> betterCursedRewardsInSanctuary;
        private static ConfigEntry<bool> betterCursedRewardsInCW;
        private static ConfigEntry<bool> restoreUncommonRewards;
        private static ConfigEntry<bool> cwMiniBossCurseChance;






        void Awake()
        {
            cursedBattleNumberConf = Config.Bind("Generation config", "number_of_cursed_battles_per_stage", 2, "Additional cursed battles for each stage. Set to 4 to curse all non-boss battles on every stage.");
            enabledInSanctuary = Config.Bind("Generation config", "cursed_battles_in_Sanctuary", true, "Enables/Disables cursed fight generation in the final area. (acceptable values: true/false)");
            enabledInCW = Config.Bind("Generation config", "cursed_battles_in_Crimson_Wilderness", true, "Enables/Disables cursed fight generation in Crimson Wilderness. (acceptable values: true/false)");
            cwMiniBossCurseChance = Config.Bind("Generation config", "exclusive_curse_miniBosses_in_Crimson_Wilderness", true, "In Crimson Wilderness cursed fights the cursed mob will always be a miniboss. Else cursed mob is random (acceptable values: true/false)");
            limitedUncurse = Config.Bind("Generation config", "limited_uncurse_mode_enabled", false, "Items obtained at the beginning of the run are special. Only they can be used to remove curse mid-battle. Regular lifting scrolls can only be used to uncurse equipment and find secret tiles! Use starting_lifting_scroll_amount to set the amount of special items obtained. (acceptable values: true/false)");

            startingLiftingAmountConf = Config.Bind("Item config", "starting_lifting_scroll_amount", 2, "Amount of starting lifting scrolls. Lifting scrolls are identified. Mind that 1-2 cursed fights no longer drop lifting scrolls.");
            cursedGoldReward = Config.Bind("Item config", "cursed_gold_reward", 100, "Sets the amount of gold commonly rewarded by killing cursed enemies. Vanilla amount is 200.");
            betterCursedRewardsInSanctuary = Config.Bind("Item config", "enable_better_cursed_rewards_in_Sanctuary", true, "Cursed enemies in Sanctuary drop better rewards like potions or rare items. (acceptable values: true/false)");
            betterCursedRewardsInCW = Config.Bind("Item config", "enable_better_cursed_rewards_in_Crimson_Wilderness", true, "Cursed enemies in Crimson Wilderness drop better rewards. (acceptable values: true/false)");
            restoreUncommonRewards = Config.Bind("Item config", "restore_uncommon_rewards", true, "Make certain enemies drop blue equipment as they used to (acceptable values: true/false)");

            logger = Logger;

            harmony.PatchAll();
        }
        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchAll(GUID);
        }

        //TODO
        //add custom item icon

        //loading removes original red particles
        //cursed lightning hedgehogs produces exception


        //maybe reduce particle intensity on cursed enemies and/or map objects





        // add starting lifting scrolls
        [HarmonyPatch(typeof(FieldSystem), nameof(FieldSystem.StageStart))]
        class FieldSystem_Patch
        {
            [HarmonyPostfix]
            static void AddStartingItems()
            {
                // copied from FieldSystem.StageStart
                // !PlayData.TSavedata.GameStarted
                if (PlayData.TSavedata.StageNum == 0 && !PlayData.TSavedata.IsLoaded)
                {
                    if (!limitedUncurse.Value)
                    {
                        // identifies lifting scroll
                        if (PlayData.TSavedata.IdentifyItems.Find((string x) => x == GDEItemKeys.Item_Scroll_Scroll_Uncurse) == null)
                            PlayData.TSavedata.IdentifyItems.Add(GDEItemKeys.Item_Scroll_Scroll_Uncurse);
                        if (startingLiftingAmountConf.Value > 0)
                            PartyInventory.InvenM.AddNewItem(ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Uncurse, startingLiftingAmountConf.Value));
                    }
                    else
                    {
                        if (startingLiftingAmountConf.Value > 0)
                            PartyInventory.InvenM.AddNewItem(ItemBase.GetItem(MiscSpecialUncurseKey, startingLiftingAmountConf.Value));

                    }
                }
            }


            //[HarmonyPatch()]
            static IEnumerable<CodeInstruction> AddItemsTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var ci in instructions)
                {
                    if (ci.Is(OpCodes.Callvirt, AccessTools.Method(typeof(UIManager), "FadeSquare_In")))
                    {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FieldSystem_Patch), nameof(FieldSystem_Patch.AddStartingItems)));
                        yield return ci;
                    }
                    else
                    {
                        yield return ci;
                    }
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
        class Cursed_Reward_Patch
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

                if (betterCursedRewardsInCW.Value && PlayData.TSavedata.NowStageMapKey == GDEItemKeys.Stage_Stage_Crimson)
                {

                    var cwConsumables = new List<string> { GDEItemKeys.Item_Consume_RedHammer, GDEItemKeys.Item_Consume_Dinner, 
                        GDEItemKeys.Item_Consume_RedWing, GDEItemKeys.Item_Consume_RedHerb };

                    if (__instance.BChar.Info.KeyData == GDEItemKeys.Enemy_SR_Samurai || __instance.BChar.Info.KeyData == GDEItemKeys.Enemy_SR_Gunner
                        || __instance.BChar.Info.KeyData == GDEItemKeys.Enemy_SR_Tumbledochi)
                    {
                        ___Itemviews.RemoveAll(x => x.itemkey == GDEItemKeys.Item_Misc_Gold && x.StackCount == vanillaGoldReward);
                        if (Misc.RandomPer(100, 80))
                        {
                            ___Itemviews.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_GetPotion, false));
                        }
                        else
                        {
                            ___Itemviews.Add(ItemBase.GetItem(cwConsumables.Random()));
                        }
                    }
                    if (__instance.BChar.Info.KeyData == GDEItemKeys.Enemy_SR_GuitarList)
                    {
                        ___Itemviews.RemoveAll(x => x.itemkey == GDEItemKeys.Item_Misc_Gold && x.StackCount == vanillaGoldReward);
                        ___Itemviews.Add(ItemBase.GetItem(PlayData.GetEquipRandom(3)));
                    }
                    if (__instance.BChar.Info.KeyData == GDEItemKeys.Enemy_SR_Shotgun || __instance.BChar.Info.KeyData == GDEItemKeys.Enemy_SR_Sniper)
                    {
                        ___Itemviews.RemoveAll(x => x.itemkey == GDEItemKeys.Item_Misc_Gold && x.StackCount == vanillaGoldReward);
                        ___Itemviews.Add(ItemBase.GetItem(PlayData.GetEquipRandom(3)));
                        ___Itemviews.Add(ItemBase.GetItem(cwConsumables.Random()));

                    }
                    if (__instance.BChar.Info.KeyData == GDEItemKeys.Enemy_SR_Outlaw || __instance.BChar.Info.KeyData == GDEItemKeys.Enemy_SR_Blade)
                    {
                        ___Itemviews.RemoveAll(x => x.itemkey == GDEItemKeys.Item_Misc_Gold && x.StackCount == vanillaGoldReward);
                        ___Itemviews.Add(ItemBase.GetItem(PlayData.GetEquipRandom(4)));
                        ___Itemviews.Add(ItemBase.GetItem(cwConsumables.Random()));
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
                        && list[Math.Min(i + 1, c - 1)].opcode == OpCodes.Call
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

        [HarmonyPatch(typeof(BattleSystem), nameof(BattleSystem.CurseEnemySelect))]

        class CurseCWminiBosses
        {
            // cursed enemy select per wave
            static void Postfix(ref string __result, List<GDEEnemyData> Enemydatas, BattleSystem __instance)
            {
                var cwMiniBosses = new HashSet<string>() { GDEItemKeys.Enemy_SR_GuitarList, GDEItemKeys.Enemy_SR_Shotgun, GDEItemKeys.Enemy_SR_Blade, GDEItemKeys.Enemy_SR_Outlaw, GDEItemKeys.Enemy_SR_Sniper};
                var mbList = Enemydatas.FindAll(data => cwMiniBosses.Contains(data.Key));
                if (mbList.Count > 0 && cwMiniBossCurseChance.Value)
                {
                    __result = mbList.Random().Key;
                }
            }
        }

        [HarmonyPatch(typeof(BattleSystem), nameof(BattleSystem.CreatEnemy))]
        class RemoveBrokenCWcombos
        {
            static void CheckCurse(List<string> curseList, string curseKey, string enemyKey)
            {
                if (curseKey == GDEItemKeys.Buff_B_CursedMob_2 && enemyKey == GDEItemKeys.Enemy_SR_Outlaw)
                    return;
                curseList.Add(curseKey);
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var i = 0;
                var list = instructions.ToList();
                var c = list.Count;
                foreach (var ci in instructions)
                {
                    if (ci.opcode == OpCodes.Callvirt && ((MethodInfo)ci.operand).Equals(AccessTools.Method(typeof(List<string>), "Add"))
                        && list[Math.Max(i-2, 0)].opcode == OpCodes.Ldloc_3)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RemoveBrokenCWcombos), nameof(RemoveBrokenCWcombos.CheckCurse)));
                    }
                    else
                    {
                        yield return ci;
                    }
                    i++;
                }
            }
        }

        public static readonly string ScrollSpecialUncurseKey = "ScrollSpecialUncurse";
        public static readonly string MiscSpecialUncurseKey = "MiscSpecialUncurse";


        [HarmonyPatch(typeof(GDEDataManager), nameof(GDEDataManager.BuildDataKeysBySchemaList))]
        class RegisterCustomKeysPatch
        {

            static void UpdateGDEDataStructures(string schema, string key, AccessTools.FieldRef<GDEDataManager, Dictionary<string, HashSet<string>>> dataKeysBySchemaRef)
            {

                dataKeysBySchemaRef()[schema].Add(key);

                try
                {
                    var schemaType = Type.GetType("GameDataEditor." + "GDE" + schema + "Data, " + typeof(FieldSystem).Assembly);
                    var schemaCtor = AccessTools.Constructor(schemaType, new Type[] { typeof(string) });
                    var data = schemaCtor.Invoke(new string[] { key });
                    var saveToDictMethod = AccessTools.Method(schemaType, "SaveToDict", new Type[] { });
                    var dict = (Dictionary<string, object>)saveToDictMethod.Invoke(data, new object[] { });

                    GDEDataManager.masterData.TryAddOrUpdateValue(key, dict);
                }
                catch (Exception e)
                {
                    logger.LogError($"Error to add (schema: {schema}, key: {key}) to masterdata");
                    logger.LogError(e);
                }
            }
            static void Postfix()
            {
                var dataKeysBySchemaRef = AccessTools.FieldRefAccess<GDEDataManager, Dictionary<string, HashSet<string>>>(AccessTools.Field(typeof(GDEDataManager), "dataKeysBySchema"));

                if (dataKeysBySchemaRef() == null)
                {
                    logger.LogError("Ref is null");
                    return;
                }
                //UpdateGDEDataStructures(GDESchemaKeys.Item_Scroll, ScrollSpecialUncurseKey, dataKeysBySchemaRef);
                UpdateGDEDataStructures(GDESchemaKeys.Item_Misc, MiscSpecialUncurseKey, dataKeysBySchemaRef);


            }
        }

        [HarmonyPatch(typeof(GDEItem_MiscData), nameof(GDEItem_MiscData.LoadFromSavedData))]
        class ConsumableDataPatch
        {
            static void Postfix(GDEItem_MiscData __instance)
            {
                if (__instance.Key == MiscSpecialUncurseKey)
                {
                    __instance.name = "Greater Lifting";
                    __instance.stack = true;
                    __instance.outinventory = false;
                    __instance.image = new Texture2D(1, 1);
                    __instance.Description = "Create Lift Curse card at the start of a cursed battle";
                    __instance.MaxStack = 99;
                    __instance.Price = 500;
                    __instance.Itemclass = new GDEItemClassData(GDEItemKeys.ItemClass_Legendary);

                }
            }
        }

        [HarmonyPatch]
        class EnableLimitedUncurse0
        {
            static string CheckLimitedUncurseConfig()
            {
                if (limitedUncurse.Value)
                    return MiscSpecialUncurseKey;
                return GDEItemKeys.Item_Scroll_Scroll_Uncurse;
            }

            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(B_CursedMob), nameof(B_CursedMob.BattleStart));
                yield return AccessTools.Method(typeof(SkillExtended_UnCurse), nameof(SkillExtended_UnCurse.SkillUseSingle));
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var ci in instructions)
                {
                    if (ci.Is(OpCodes.Ldsfld, AccessTools.Field(typeof(GDEItemKeys), nameof(GDEItemKeys.Item_Scroll_Scroll_Uncurse))))
                    {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EnableLimitedUncurse0), nameof(EnableLimitedUncurse0.CheckLimitedUncurseConfig)));
                    }
                    else
                    {
                        yield return ci;
                    }
                }
            }
            
        }

        [HarmonyPatch]
        class EnableLimitedUncurse1
        {

            static string CheckLimitedUncurseConfig(string arg0)
            {
                if (limitedUncurse.Value)
                    return arg0;
                return GDEItemKeys.Item_Scroll_Scroll_Uncurse;
            }
            static MethodInfo TargetMethod()
            {
                return AccessTools.Method(typeof(B_CursedMob), "<BattleStart>m__0");
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var ci in instructions)
                {
                    if (ci.Is(OpCodes.Ldsfld, AccessTools.Field(typeof(GDEItemKeys), nameof(GDEItemKeys.Item_Scroll_Scroll_Uncurse))))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EnableLimitedUncurse1), nameof(EnableLimitedUncurse1.CheckLimitedUncurseConfig)));
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
