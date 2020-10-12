﻿using System;
using System.Collections.Generic;
using System.Linq;
using MMR_Tracker_V2;
using System.Windows.Forms;

namespace MMR_Tracker.Class_Files
{
    class PlaythroughGenerator
    {
        public static void GeneratePlaythrough(LogicObjects.TrackerInstance Instance)
        {
            List<LogicObjects.PlaythroughItem> Playthrough = new List<LogicObjects.PlaythroughItem>();
            Dictionary<int, int> SpoilerToID = new Dictionary<int, int>();
            LogicObjects.TrackerInstance playLogic = Utility.CloneTrackerInstance(Instance);
            var GameClear = GetGameClearEntry(playLogic.Logic, Instance.EntranceRando);

            if (GameClear < 0) { MessageBox.Show("Could not find game clear requirements. Playthrough can not be generated."); return; }

            if (!Utility.CheckforSpoilerLog(playLogic.Logic))
            {
                var file = Utility.FileSelect("Select A Spoiler Log", "Spoiler Log (*.txt;*html)|*.txt;*html");
                if (file == "") { return; }
                LogicEditing.WriteSpoilerLogToLogic(playLogic, file);
            }

            if (!Utility.CheckforSpoilerLog(playLogic.Logic, true))
            { MessageBox.Show("Not all items have spoiler data. Playthrough may not generate correctly. Ensure you are using the same version of logic used to generate your selected spoiler log"); }

            List<int> importantItems = new List<int>();
            foreach (var i in playLogic.Logic)
            {
                i.Available = false;
                i.Checked = false;
                i.Aquired = false;
                if (i.IsFake) { i.SpoilerRandom = i.ID; i.RandomizedItem = i.ID; i.LocationName = i.DictionaryName; i.ItemName = i.DictionaryName; }
                if (i.Unrandomized() && i.ID == i.SpoilerRandom) { i.IsFake = true; }
                if (i.SpoilerRandom > -1) { i.RandomizedItem = i.SpoilerRandom; }
                if (SpoilerToID.ContainsKey(i.SpoilerRandom) || i.SpoilerRandom < 0) { continue; }
                SpoilerToID.Add(i.SpoilerRandom, i.ID);
                //Check for all items mentioned in the logic file
                if (i.Required != null)
                {
                    foreach (var k in i.Required)
                    {
                        if (!importantItems.Contains(k)) { importantItems.Add(k); }
                    }
                }
                if (i.Conditionals != null)
                {
                    foreach (var j in i.Conditionals)
                    {
                        foreach (var k in j) { if (!importantItems.Contains(k)) { importantItems.Add(k); } }
                    }
                }
                if (i.ID == GameClear) { importantItems.Add(i.ID); }
            }

            SwapAreaClearLogic(playLogic);
            MarkAreaClearAsEntry(playLogic);
            CalculatePlaythrough(playLogic.Logic, Playthrough, 0, importantItems);

            importantItems = new List<int>();
            var GameClearPlaythroughItem = Playthrough.Find(x => x.Check.ID == GameClear);
            if (GameClearPlaythroughItem == null) { MessageBox.Show("This seed is not beatable using this logic! Playthrough could not be generated!"); return; }

            importantItems.Add(GameClearPlaythroughItem.Check.ID);
            FindImportantItems(GameClearPlaythroughItem, importantItems, Playthrough, SpoilerToID);

            Playthrough = Playthrough.OrderBy(x => x.SphereNumber).ThenBy(x => x.Check.ItemSubType).ThenBy(x => x.Check.LocationArea).ThenBy(x => x.Check.LocationName).ToList();

            //Replace all fake items with the real items used to unlock those fake items
            foreach (var i in Playthrough) { i.ItemsUsed = Tools.ResolveFakeToRealItems(i, Playthrough, playLogic.Logic).Distinct().ToList(); }

            var ImportantPlaythrough = Playthrough.Where(i => (importantItems.Contains(i.Check.ID) && !i.Check.IsFake) || i.Check.ID == GameClear).ToList();

            //foreach (var li in ImportantPlaythrough) { Debugging.Log(li.Check.DictionaryName); }

            //Convert Progressive Items Back to real items
            ConvertProgressiveItems(ImportantPlaythrough, playLogic);

            List<string> PlaythroughString = new List<string>();
            int lastSphere = -1;
            string FinalTask = (Instance.IsMM()) ? "Defeat Majora" : "Beat the game";
            foreach (var i in ImportantPlaythrough)
            {
                if (i.SphereNumber != lastSphere)
                {
                    PlaythroughString.Add("Sphere: " + i.SphereNumber + " ====================================="); lastSphere = i.SphereNumber;
                }
                if (i.Check.ID == GameClear) { PlaythroughString.Add(FinalTask); }
                else
                {
                    PlaythroughString.Add("Check \"" + i.Check.LocationName + "\" to obtain \"" + playLogic.Logic[i.Check.RandomizedItem].ItemName + "\"");
                }
                string items = "    Using Items: ";
                foreach (var j in i.ItemsUsed) { items = items + playLogic.Logic[j].ItemName + ", "; }
                if (items != "    Using Items: ") { PlaythroughString.Add(items); }
            }

            InformationDisplay DisplayPlaythrough = new InformationDisplay();
            InformationDisplay.Playthrough = PlaythroughString;
            DisplayPlaythrough.DebugFunction = 3;
            DisplayPlaythrough.Show();
            InformationDisplay.Playthrough = new List<string>();
        }

        public static void ConvertProgressiveItems(List<LogicObjects.PlaythroughItem> Playthrough, LogicObjects.TrackerInstance Instance)
        {
            if (!LogicObjects.MainTrackerInstance.IsMM() || !LogicObjects.MainTrackerInstance.Options.ProgressiveItems) { return; }
            var SW1 = Instance.Logic.Find(x => x.DictionaryName == "Starting Sword");
            var SW2 = Instance.Logic.Find(x => x.DictionaryName == "Razor Sword");
            var SW3 = Instance.Logic.Find(x => x.DictionaryName == "Gilded Sword");
            var MM1 = Instance.Logic.Find(x => x.DictionaryName == "Great Fairy Magic Meter");
            var MM2 = Instance.Logic.Find(x => x.DictionaryName == "Great Fairy Extended Magic");
            var WL1 = Instance.Logic.Find(x => x.DictionaryName == "Town Wallet (200)");
            var WL2 = Instance.Logic.Find(x => x.DictionaryName == "Ocean Wallet (500)");
            var BB1 = Instance.Logic.Find(x => x.DictionaryName == "Bomb Bag (20)");
            var BB2 = Instance.Logic.Find(x => x.DictionaryName == "Town Bomb Bag (30)");
            var BB3 = Instance.Logic.Find(x => x.DictionaryName == "Mountain Bomb Bag (40)");
            var BW1 = Instance.Logic.Find(x => x.DictionaryName == "Hero's Bow");
            var BW2 = Instance.Logic.Find(x => x.DictionaryName == "Town Archery Quiver (40)");
            var BW3 = Instance.Logic.Find(x => x.DictionaryName == "Swamp Archery Quiver (50)");

            List<List<LogicObjects.LogicEntry>> ProgressiveItemSets = new List<List<LogicObjects.LogicEntry>>
            {
                new List<LogicObjects.LogicEntry> { SW1, SW2, SW3 },
                new List<LogicObjects.LogicEntry> { MM1, MM2 },
                new List<LogicObjects.LogicEntry> { WL1, WL2 },
                new List<LogicObjects.LogicEntry> { BB1, BB2, BB3 },
                new List<LogicObjects.LogicEntry> { BW1, BW2, BW3 },
            };

            foreach(var i in ProgressiveItemSets) { if (i == null || !i.Any() || i.Where(x => x == null).Any()) { return; } }

            foreach (var ProgressiveItemSet in ProgressiveItemSets)
            {
                int TimesItemObtained = -1;
                foreach(var ObtainedItem in Playthrough)
                {
                    var RandomItem = Instance.Logic[ObtainedItem.Check.RandomizedItem];
                    if (ProgressiveItemSet.Contains(RandomItem))
                    {
                        TimesItemObtained++;
                        ObtainedItem.Check.RandomizedItem = ProgressiveItemSet[TimesItemObtained].ID;
                        if (TimesItemObtained >= ProgressiveItemSet.Count()) { TimesItemObtained = ProgressiveItemSet.Count() - 1; }
                    }

                    int TimesItemUsed = ObtainedItem.ItemsUsed.Where(x => ProgressiveItemSet.Where(y => y.ID == x).Any()).Count() - 1;
                    if (TimesItemUsed < 0) { continue; }
                    if (TimesItemUsed >= ProgressiveItemSet.Count()) { TimesItemUsed = ProgressiveItemSet.Count() - 1; }
                    //Debugging.Log($"{TimesItemUsed} Progressive Items where used to check {ObtainedItem.Check.LocationName}");
                    List<int> newItemsUsed = new List<int>();
                    bool ItemAdded = false;
                    foreach (var UsedItem in ObtainedItem.ItemsUsed)
                    {
                        if (ProgressiveItemSet.Where(y => y.ID == UsedItem).Any())
                        {
                            if (!ItemAdded)
                            {
                                //Debugging.Log($"{Instance.Logic[UsedItem].ItemName} was replace by {ProgressiveItemSet[TimesItemObtained].ItemName}");
                                newItemsUsed.Add(ProgressiveItemSet[TimesItemObtained].ID);
                                ItemAdded = true;
                            }
                        }
                        else
                        {
                            newItemsUsed.Add(UsedItem);
                        }
                    }
                    ObtainedItem.ItemsUsed = newItemsUsed.Distinct().ToList();
                }
            }
        }


        public static void CalculatePlaythrough(List<LogicObjects.LogicEntry> logic, List<LogicObjects.PlaythroughItem> Playthrough, int sphere, List<int> ImportantItems)
        {
            bool RealItemObtained = false;
            bool recalculate = false;
            List<LogicObjects.LogicEntry> itemCheckList = new List<LogicObjects.LogicEntry>();


            foreach (var item in logic)
            {
                List<int> UsedItems = new List<int>();
                item.Available = (LogicEditing.RequirementsMet(item.Required, logic, UsedItems) && LogicEditing.CondtionalsMet(item.Conditionals, logic, UsedItems));

                if (LogicEditing.ParseCombinationEntry(logic, item, UsedItems)) { item.Available = true; }

                if (!item.IsFake && item.SpoilerRandom > -1 && item.Available && !logic[item.SpoilerRandom].Aquired)
                {
                    itemCheckList.Add(item);
                    recalculate = true;
                    if (ImportantItems.Contains(item.SpoilerRandom) && item.Available)
                    {
                        Playthrough.Add(new LogicObjects.PlaythroughItem { SphereNumber = sphere, Check = item, ItemsUsed = UsedItems });
                        RealItemObtained = true;
                    }
                }
            }
            foreach (var item in itemCheckList)
            {
                logic[item.SpoilerRandom].Aquired = item.Available;
            }

            int NewSphere = (RealItemObtained) ? sphere + 1 : sphere;

            if (UnlockAllFake(logic, ImportantItems, NewSphere, Playthrough)) { recalculate = true; }

            if (recalculate) { CalculatePlaythrough(logic, Playthrough, NewSphere, ImportantItems); }
        }

        public static bool UnlockAllFake(List<LogicObjects.LogicEntry> logic, List<int> ImportantItems, int sphere, List<LogicObjects.PlaythroughItem> Playthrough)
        {
            var recalculate = false;
            foreach (var item in logic)
            {
                List<int> UsedItems = new List<int>();
                item.Available = (LogicEditing.RequirementsMet(item.Required, logic, UsedItems) && LogicEditing.CondtionalsMet(item.Conditionals, logic, UsedItems));

                if (LogicEditing.ParseCombinationEntry(logic, item, UsedItems)) { item.Available = true; }

                if (item.Aquired != item.Available && item.IsFake)
                {
                    item.Aquired = item.Available;
                    recalculate = true;
                    if (ImportantItems.Contains(item.SpoilerRandom) && item.Available)
                    {
                        Playthrough.Add(new LogicObjects.PlaythroughItem { SphereNumber = sphere, Check = item, ItemsUsed = UsedItems });
                    }
                }
            }
            if (recalculate) { UnlockAllFake(logic, ImportantItems, sphere, Playthrough); }
            return recalculate;
        }

        public static void FindImportantItems(LogicObjects.PlaythroughItem EntryToCheck, List<int> importantItems, List<LogicObjects.PlaythroughItem> Playthrough, Dictionary<int, int> SpoilerToID)
        {
            foreach (var i in EntryToCheck.ItemsUsed ?? new List<int>())
            {
                var locToCheck = SpoilerToID[i];
                if (importantItems.Contains(locToCheck)) { continue; }
                importantItems.Add(locToCheck);
                var NextLocation =  Playthrough.Find(j => j.Check.ID == locToCheck) ?? new LogicObjects.PlaythroughItem();
                FindImportantItems(NextLocation, importantItems, Playthrough, SpoilerToID);
            }
        }

        public static int GetGameClearEntry(List<LogicObjects.LogicEntry> playLogic, bool EntranceRadno)
        {
            var GameClearEntry = playLogic.Find(x => x.DictionaryName == "MMRTGameClear") ?? new LogicObjects.LogicEntry { ID = -1 };
            int GameClear = GameClearEntry.ID;

            if (GameClear > -1) { return GameClear; }

            int StunMajora = -1;
            int DamageMajora = -1;
            int AccessMajora = -1;
            List<List<int>> Conditionals = new List<List<int>>();

            StunMajora = playLogic.Count();
            playLogic.Add(new LogicObjects.LogicEntry { ID = StunMajora, DictionaryName = "MMRTStunMajora", IsFake = true });

            DamageMajora = playLogic.Count();
            playLogic.Add(new LogicObjects.LogicEntry { ID = DamageMajora, DictionaryName = "MMRTDamageMajora", IsFake = true });

            AccessMajora = playLogic.Count();
            playLogic.Add(new LogicObjects.LogicEntry { ID = AccessMajora, DictionaryName = "MMRTAccessMajora", IsFake = true });

            GameClear = playLogic.Count();
            playLogic.Add(new LogicObjects.LogicEntry { ID = GameClear, DictionaryName = "MMRTGameClear", IsFake = true });

            try
            {
                playLogic[DamageMajora].Conditionals = new int[4][];
                playLogic[DamageMajora].Conditionals[0] = new int[] { playLogic.Where(x => x.DictionaryName == "Starting Sword").First().ID };
                playLogic[DamageMajora].Conditionals[1] = new int[] { playLogic.Where(x => x.DictionaryName == "Razor Sword").First().ID };
                playLogic[DamageMajora].Conditionals[2] = new int[] { playLogic.Where(x => x.DictionaryName == "Gilded Sword").First().ID };
                playLogic[DamageMajora].Conditionals[3] = new int[] { playLogic.Where(x => x.DictionaryName == "Great Fairy's Sword").First().ID };
                playLogic[StunMajora].Conditionals = new int[4][];
                playLogic[StunMajora].Conditionals[0] = new int[] { playLogic.Where(x => x.DictionaryName == "Town Archery Quiver (40)").First().ID };
                playLogic[StunMajora].Conditionals[1] = new int[] { playLogic.Where(x => x.DictionaryName == "Swamp Archery Quiver (50)").First().ID };
                playLogic[StunMajora].Conditionals[2] = new int[] { playLogic.Where(x => x.DictionaryName == "Hero's Bow").First().ID };
                playLogic[StunMajora].Conditionals[3] = new int[] { playLogic.Where(x => x.DictionaryName == "Zora Mask").First().ID };

                if (EntranceRadno)
                {
                    playLogic[AccessMajora].Required = new int[] { playLogic.Where(x => x.DictionaryName == "EntranceMajorasLairFromTheMoon").First().ID };
                }
                else
                {
                    playLogic[AccessMajora].Required = new int[] { playLogic.Where(x => x.DictionaryName == "Moon Access").First().ID };
                }

                playLogic[GameClear].Required = new int[] { AccessMajora };

                playLogic[GameClear].Conditionals = new int[2][];
                playLogic[GameClear].Conditionals[0] = new int[] { StunMajora, DamageMajora };

                var FD = playLogic.Where(x => x.DictionaryName == "Fierce Deity's Mask").First().ID;
                var M1 = playLogic.Where(x => x.DictionaryName == "Magic Meter").First().ID;

                playLogic[GameClear].Conditionals[1] = new int[] { FD, M1 };
            }
            catch
            {
                Debugging.Log("Could not find items for game clear conditional");
                return -1;
            }
            return GameClear;
        }

        public static void MarkAreaClearAsEntry(LogicObjects.TrackerInstance instance)
        {

            var EntAreaDict = instance.EntranceAreaDic;
            LogicObjects.LogicEntry Default = new LogicObjects.LogicEntry();
            var WoodFallClear = instance.Logic.Find(x => x.DictionaryName == "Woodfall clear") ?? Default;
            var SnowheadClear = instance.Logic.Find(x => x.DictionaryName == "Snowhead clear") ?? Default;
            var GreatBayClear = instance.Logic.Find(x => x.DictionaryName == "Great Bay clear") ?? Default;
            var IkanaClear = instance.Logic.Find(x => x.DictionaryName == "Ikana clear") ?? Default;

            WoodFallClear.IsFake = false;
            SnowheadClear.IsFake = false;
            GreatBayClear.IsFake = false;
            IkanaClear.IsFake = false;
            //Set the area clear name to their defualt
            WoodFallClear.LocationName = "Defeat Odolwa";
            WoodFallClear.ItemName = "Odolwas Remians";
            SnowheadClear.LocationName = "Defeat Goht";
            SnowheadClear.ItemName = "Gohts Remians";
            GreatBayClear.LocationName = "Defeat Gyrog";
            GreatBayClear.ItemName = "Gyrogs Remians";
            IkanaClear.LocationName = "Defeat Twinmold";
            IkanaClear.ItemName = "Twinmolds Remians";
            //Find the name of the randomized area clear
            var newWoodfallLocation = (WoodFallClear.RandomizedAreaClear(instance) ?? WoodFallClear).LocationName;
            var newWoodfallItem = (WoodFallClear.RandomizedAreaClear(instance) ?? WoodFallClear).ItemName;
            var newSnowheadLocation = (SnowheadClear.RandomizedAreaClear(instance) ?? SnowheadClear).LocationName;
            var newSnowheadItem = (SnowheadClear.RandomizedAreaClear(instance) ?? SnowheadClear).ItemName;
            var newGreatBayLocation = (GreatBayClear.RandomizedAreaClear(instance) ?? GreatBayClear).LocationName;
            var newGreatBayItem = (GreatBayClear.RandomizedAreaClear(instance) ?? GreatBayClear).ItemName;
            var newIkanaLocation = (IkanaClear.RandomizedAreaClear(instance) ?? IkanaClear).LocationName;
            var newIkanaItem = (IkanaClear.RandomizedAreaClear(instance) ?? IkanaClear).ItemName;
            //Set the randomized area clear name to the original area clear
            WoodFallClear.LocationName = newWoodfallLocation;
            WoodFallClear.ItemName = newWoodfallItem;
            SnowheadClear.LocationName = newSnowheadLocation;
            SnowheadClear.ItemName = newSnowheadItem;
            GreatBayClear.LocationName = newGreatBayLocation;
            GreatBayClear.ItemName = newGreatBayItem;
            IkanaClear.LocationName = newIkanaLocation;
            IkanaClear.ItemName = newIkanaItem;
        }

        public static void SwapAreaClearLogic(LogicObjects.TrackerInstance Instance)
        {
            var areaClearData = Instance.EntranceAreaDic;
            var ReferenceLogic = Utility.CloneLogicList(Instance.Logic);
            foreach (var i in Instance.Logic)
            {
                if (areaClearData.ContainsKey(i.ID))
                {
                    var Dungeon = Instance.Logic[areaClearData[i.ID]];
                    if (Dungeon.RandomizedItem < 0) { return; }
                    var DungoneRandItem = Dungeon.RandomizedItem;
                    var RandomClear = areaClearData.FirstOrDefault(x => x.Value == DungoneRandItem).Key;
                    Instance.Logic[i.ID].Required = ReferenceLogic[RandomClear].Required;
                    Instance.Logic[i.ID].Conditionals = ReferenceLogic[RandomClear].Conditionals;
                }
            }
        }
    }
}
