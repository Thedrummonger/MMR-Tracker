﻿using MMR_Tracker_V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker.Class_Files
{
    public static class LogicObjectExtentions
    {
        //Logic Entry Extentions
        public static bool IsEntrance(this LogicObjects.LogicEntry entry)
        {
            return entry.ItemSubType == "Entrance";
        }
        public static LogicObjects.LogicEntry RandomizedEntry(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, bool ReturnJunkAsItem = false)
        {
            if (ReturnJunkAsItem && entry.HasRandomItem(false) && !entry.HasRandomItem(true))
            { return new LogicObjects.LogicEntry { ID = -1, DictionaryName = "Junk", DisplayName = "Junk", LocationName = "Junk", ItemName = "Junk" }; }
            if (!entry.HasRandomItem(true) || entry.RandomizedItem >= Instance.Logic.Count) { return null; }
            return Instance.Logic[entry.RandomizedItem];
        }
        public static LogicObjects.LogicEntry PairedEntry(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, bool RandomizedItem = false)
        {
            var Pairs = Instance.EntrancePairs;
            int ID = (RandomizedItem) ? entry.RandomizedItem : entry.ID;
            if (Pairs.ContainsKey(ID) && Pairs[ID] < Instance.Logic.Count) { return Instance.Logic[Pairs[ID]]; }
            return null;
        }
        public static LogicObjects.LogicEntry ClearRandomizedDungeonInThisArea(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance)
        {
            //Finds the area clear related to the dungeon that is randomized to the current area.
            //If woodfall entrance leads to snowhead and you pass this function woodfall clear it will return snowhead clear.
            if (!Instance.EntranceAreaDic.ContainsKey(entry.ID)) { return null; }
            var templeEntrance = Instance.EntranceAreaDic[entry.ID];//What is the dungeon entrance in this area
            var RandTempleEntrance = Instance.Logic[templeEntrance].RandomizedItem;//What dungeon does this areas dungeon entrance lead to
            var RandAreaClear = RandTempleEntrance < 0 ? -1 : Instance.EntranceAreaDic.FirstOrDefault(x => x.Value == RandTempleEntrance).Key;//What is the Area clear Value For That Dungeon
            var RandClearLogic = RandAreaClear == -1 ? null : Instance.Logic[RandAreaClear]; //Get the full logic data for the area clear that we want to check the availability of.
            return RandClearLogic;
        }
        public static bool HasRandomItem(this LogicObjects.LogicEntry entry, bool RealItem)
        {
            return (RealItem) ? entry.RandomizedItem > -1 : entry.RandomizedItem > -2;
        }
        public static bool Unrandomized(this LogicObjects.LogicEntry entry, int UnRand0Manual1Either2 = 0)
        {
            if (UnRand0Manual1Either2 == 0) { return entry.RandomizedState() == 1; }
            if (UnRand0Manual1Either2 == 1) { return entry.RandomizedState() == 2; }
            if (UnRand0Manual1Either2 == 2) { return entry.RandomizedState() == 1 || entry.RandomizedState() == 2; }
            return false;
        }
        public static bool Randomized(this LogicObjects.LogicEntry entry)
        {
            return entry.RandomizedState() == 0;
        }
        public static int RandomizedState(this LogicObjects.LogicEntry entry)
        {
            int option = entry.Options;
            return (entry.Options > 3) ? option - 4 : option;
        }
        public static bool StartingItem(this LogicObjects.LogicEntry entry)
        {
            return (entry.Options > 3);
        }
        public static bool IsWarpSong(this LogicObjects.LogicEntry entry)
        {
            return (entry.LocationArea == "Owl Warp");
        }
        public static bool AppearsInListbox(this LogicObjects.LogicEntry entry)
        {
            return (entry.Randomized() || entry.Unrandomized(1)) && !entry.IsFake && !string.IsNullOrWhiteSpace(entry.LocationName);
        }
        public static bool LogicItemAquired(this LogicObjects.LogicEntry entry)
        {
            return (entry.Aquired || entry.StartingItem() || (entry.Unrandomized() && entry.Available));
        }
        public static LogicObjects.LogicEntry GetItemsNewLocation(this LogicObjects.LogicEntry entry, List<LogicObjects.LogicEntry> Logic)
        {
            return Logic.Find(x => x.RandomizedItem == entry.ID);
        }
        public static LogicObjects.LogicEntry GetItemsSpoilerLocation(this LogicObjects.LogicEntry entry, List<LogicObjects.LogicEntry> Logic)
        {
            return Logic.Find(x => x.SpoilerRandom == entry.ID);
        }
        public static bool ItemHasBeenPlaced(this LogicObjects.LogicEntry entry, List<LogicObjects.LogicEntry> Logic)
        {
            return Logic.Where(x => x.RandomizedItem == entry.ID || x.SpoilerRandom == entry.ID).Any();
        }
        public static bool UserCreatedFakeItem(this LogicObjects.LogicEntry entry, List<LogicObjects.LogicEntry> Logic)
        {
            int lastRealItem = -1;
            foreach (var i in Logic)
            {
                if (!i.IsFake) { lastRealItem = i.ID; }
            }
            return (entry.ID > lastRealItem);
        }
        public static bool ItemBelongsToMe(this LogicObjects.LogicEntry entry)
        {
            if (!LogicObjects.MainTrackerInstance.Options.IsMultiWorld) { return true; }
            if (entry.IsEntrance()) { return true; }
            return entry.PlayerData.ItemBelongedToPlayer == -1 || entry.PlayerData.ItemBelongedToPlayer == LogicObjects.MainTrackerInstance.Options.MyPlayerID;
        }
        public static bool IsProgressiveItem(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance)
        {
            return entry.ProgressiveItemSet(Instance) != null;
        }
        public static string ProgressiveItemName(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance)
        {
            if (entry.IsProgressiveItem(Instance) && Instance.Options.ProgressiveItems && Instance.IsMM())
            {
                return (entry.SpoilerItem.Count() > 1) ? entry.SpoilerItem[1] : entry.ItemName ?? entry.DictionaryName;
            }
            return entry.ItemName ?? entry.DictionaryName;
        }
        public static int ProgressiveItemsNeeded(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, bool IndexValue = false)
        {
            var set = entry.ProgressiveItemSet(Instance);
            if (set == null) { return 0; }
            int offset = (IndexValue) ? 0 : 1;
            return set.IndexOf(entry) + offset;
        }
        public static int ProgressiveItemsAquired(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, bool Unique = true)
        {
            var set = entry.ProgressiveItemSet(Instance).Where(x => x.LogicItemAquired()).ToList();
            var setIDs = set.Select(x => x.ID);
            if (Unique) { return set.Where(x => x.LogicItemAquired()).Count(); }
            return Instance.Logic.Where(x => setIDs.Contains(x.RandomizedItem) && x.Checked).Count();
        }
        public static List<LogicObjects.LogicEntry> ProgressiveItemSet(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance)
        {
            if (!Instance.Options.ProgressiveItems || !Instance.IsMM()) { return null; }
            List<List<LogicObjects.LogicEntry>> ProgressiveItemSets = Utility.GetProgressiveItemSets(Instance);
            var set = ProgressiveItemSets.Find(x => x.Contains(entry));
            return set;
        }
        public static bool ItemUseable(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, List<int> usedItems = null)
        {
            if (usedItems == null) { usedItems = new List<int>(); }
            var Set = entry.ProgressiveItemSet(Instance);
            if (Set == null) { return NonProgressiveItemUseable(); }

            var AquiredSet = Set.Where(x => x.LogicItemAquired()).ToList();
            var ItemsNeeded = entry.ProgressiveItemsNeeded(Instance);
            if (AquiredSet.Count() >= ItemsNeeded)
            {
                for (var i = 0; i < ItemsNeeded; i++) { usedItems.Add(AquiredSet[i].ID); }
                return true;
            }
            return false;

            bool NonProgressiveItemUseable()
            {
                if (entry.LogicItemAquired())
                {
                    usedItems.Add(entry.ID);
                    return true;
                }
                else { return false; }
            }
        }
        public static bool CheckAvailability(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, List<int> usedItems = null)
        {
            var logic = Instance.Logic;
            usedItems = usedItems ?? new List<int>();
            if (string.IsNullOrWhiteSpace(entry.LocationName) && !entry.IsFake) { return false; }

            //Check for a "Combinations" Entry
            if (entry.Required != null && entry.Conditionals != null && entry.Required.Where(x => logic[x].DictionaryName.StartsWith("MMRTCombinations")).Any())
            {
                List<int> CondItemsUsed = new List<int>();
                int ComboEntry = entry.Required.ToList().Find(x => logic[x].DictionaryName.StartsWith("MMRTCombinations"));
                var Required = entry.Required.Where(x => !logic[x].DictionaryName.StartsWith("MMRTCombinations")).ToArray();
                int ConditionalsAquired = 0;
                if (int.TryParse(logic[ComboEntry].DictionaryName.Replace("MMRTCombinations", ""), out int ConditionalsNeeded))
                {
                    if (!Required.Any() || LogicEditing.RequirementsMet(Required, Instance, CondItemsUsed))
                    {
                        foreach (var i in entry.Conditionals)
                        {
                            List<int> ReqItemsUsed = new List<int>();
                            if (LogicEditing.RequirementsMet(i, Instance, ReqItemsUsed))
                            {
                                foreach (var q in ReqItemsUsed) { CondItemsUsed.Add(q); }
                                ConditionalsAquired++;
                            }
                            if (ConditionalsAquired >= ConditionalsNeeded)
                            {
                                foreach (var q in CondItemsUsed) { usedItems.Add(q); }
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            //Check for a "Check contains Item" Entry
            else if (entry.Required != null && entry.Conditionals != null && entry.Required.Where(x => logic[x].DictionaryName == "MMRTCheckContains").Any())
            {
                var Checks = entry.Required.Where(x => logic[x].DictionaryName != "MMRTCheckContains").ToArray();
                if (!Checks.Any()) { return false; }
                var entries = Math.Min(Checks.Count(), entry.Conditionals.Count());
                for (var i = 0; i < entries; i++)
                {
                    var Check = logic[entry.Required[i]].RandomizedItem;
                    var Items = entry.Conditionals[i];
                    foreach(var j in Items)
                    {
                        if (Check == j) { usedItems.Add(j); return true; }
                    }
                }
                return false;
            }
            //Check for a MMR Dungeon clear Entry
            else if (Instance.IsMM() && entry.IsFake && Instance.EntranceAreaDic.Count > 0 && Instance.EntranceAreaDic.ContainsKey(entry.ID))
            {
                var RandClearLogic = entry.ClearRandomizedDungeonInThisArea(Instance);
                if (RandClearLogic == null) { return false; }
                else
                {
                    return LogicEditing.RequirementsMet(RandClearLogic.Required, Instance, usedItems) &&
                            LogicEditing.CondtionalsMet(RandClearLogic.Conditionals, Instance, usedItems);
                }
            }
            //Check availability the standard way
            else
            {
                return LogicEditing.RequirementsMet(entry.Required, Instance, usedItems) &&
                            LogicEditing.CondtionalsMet(entry.Conditionals, Instance, usedItems);
            }
        }
        public static bool FakeItemStatusChange(this LogicObjects.LogicEntry entry)
        {
            if (entry.Aquired != entry.Available && entry.IsFake)
            {
                entry.Aquired = entry.Available;
                return true;
            }
            return false;
        }
        public static bool CanBeStartingItem(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance)
        {
            if (!Instance.IsMM()) { return true; }
            List<string> StartingItems = new List<string>
            {
                "Deku Mask",
                "Hero's Bow",
                "Fire Arrow",
                "Ice Arrow",
                "Light Arrow",
                "Bomb Bag",
                "Pictobox",
                "Lens of Truth",
                "Hookshot",
                "Magic Power",
                "Spin Attack",
                "Extended Magic Power",
                "Double Defense",
                "Great Fairy's Sword",
                "Bombers' Notebook",
                "Razor Sword",
                "Gilded Sword",
                "Mirror Shield",
                "Large Quiver",
                "Largest Quiver",
                "Big Bomb Bag",
                "Biggest Bomb Bag",
                "Adult Wallet",
                "Giant Wallet",
                "Postman's Hat",
                "All Night Mask",
                "Blast Mask",
                "Stone Mask",
                "Great Fairy's Mask",
                "Keaton Mask",
                "Bremen Mask",
                "Bunny Hood",
                "Don Gero's Mask",
                "Mask of Scents",
                "Romani's Mask",
                "Circus Leader's Mask",
                "Kafei's Mask",
                "Couple's Mask",
                "Mask of Truth",
                "Kamaro's Mask",
                "Gibdo Mask",
                "Garo's Mask",
                "Captain's Hat",
                "Giant's Mask",
                "Goron Mask",
                "Zora Mask",
                "Song of Healing",
                "Song of Soaring",
                "Epona's Song",
                "Song of Storms",
                "Sonata of Awakening",
                "Goron Lullaby",
                "New Wave Bossa Nova",
                "Elegy of Emptiness",
                "Oath to Order",
                "Woodfall Map",
                "Woodfall Compass",
                "Snowhead Map",
                "Snowhead Compass",
                "Great Bay Map",
                "Great Bay Compass",
                "Stone Tower Map",
                "Stone Tower Compass",
                "Map of Clock Town",
                "Map of Woodfall",
                "Map of Snowhead",
                "Map of Romani Ranch",
                "Map of Great Bay",
                "Map of Stone Tower",
                "Fierce Deity's Mask",
                "Kokiri Sword",
                "Hero's Shield"
            };
            return StartingItems.Contains(entry.ItemName);
        }
        //Logic Instance Extentions
        public static void RefreshFakeItems(this LogicObjects.TrackerInstance Instance)
        {
            foreach(var i in Instance.Logic.Where(j => j.IsFake)) { i.Aquired = false; i.Available = false; }
        }
        public static bool IsEntranceRando(this LogicObjects.TrackerInstance Instance)
        {
            return (Instance.Logic.Where(x => x.IsEntrance()).Count() > 0);
        }
        public static bool IsMM(this LogicObjects.TrackerInstance Instance)
        {
            return Instance.GameCode == "MMR";
        }
        public static bool ItemInRange(this LogicObjects.TrackerInstance Instance, int Item)
        {
            return Item > -1 && Item < Instance.Logic.Count;
        }
    }
}
