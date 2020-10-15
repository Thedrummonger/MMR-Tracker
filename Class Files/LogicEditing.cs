﻿using MMR_Tracker.Class_Files;
using MMR_Tracker.Forms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{
    class LogicEditing
    {

        public static bool PopulateTrackerInstance(LogicObjects.TrackerInstance instance)
        {
            /* Sets the Values of the follwing using the data in instance.RawLogicFile
             * Version
             * Game
             * Entrance Area Dictionary
             * Logic
             * LogicDictionary
             * Name to ID Dictionary
             * Entrance pair Dictionary
             */
            instance.Logic.Clear();
            instance.DicNameToID.Clear();
            instance.EntrancePairs.Clear();
            LogicObjects.VersionInfo versionData = VersionHandeling.GetVersionDataFromLogicFile(instance.RawLogicFile);
            instance.LogicVersion = versionData.Version;
            instance.GameCode = versionData.Gamecode;
            string DictionaryPath = VersionHandeling.GetDictionaryPath(instance);
            int SubCounter = 0;
            int idCounter = 0;
            LogicObjects.LogicEntry LogicEntry1 = new LogicObjects.LogicEntry();
            if (!string.IsNullOrWhiteSpace(DictionaryPath))
            {
                try
                {
                    instance.LogicDictionary = JsonConvert.DeserializeObject<List<LogicObjects.LogicDictionaryEntry>>(Utility.ConvertCsvFileToJsonObject(DictionaryPath));
                }
                catch { MessageBox.Show($"The Dictionary File \"{DictionaryPath}\" has been corrupted. The tracker will not function correctly."); }
            }
            else { MessageBox.Show($"A valid dictionary file could not be found for this logic. The tracker will not function correctly."); }

            var NextLine = 1;
            foreach (string line in instance.RawLogicFile)
            {
                if (NextLine == 1) { NextLine++; continue; }
                if (line.StartsWith("-")) { SubCounter = 0; }
                switch (SubCounter)
                {
                    case 0:
                        LogicEntry1.ID = idCounter;
                        LogicEntry1.DictionaryName = line.Substring(2);
                        LogicEntry1.Checked = false;
                        LogicEntry1.RandomizedItem = -2;
                        LogicEntry1.IsFake = true;
                        LogicEntry1.SpoilerRandom = -2;

                        var DicEntry = instance.LogicDictionary.Find(x => x.DictionaryName == LogicEntry1.DictionaryName);
                        if (DicEntry == null) { break; }

                        LogicEntry1.IsFake = false;
                        LogicEntry1.IsTrick = false;
                        LogicEntry1.TrickEnabled = true;
                        LogicEntry1.TrickToolTip = "";
                        LogicEntry1.ItemName = (string.IsNullOrWhiteSpace(DicEntry.ItemName)) ? null : DicEntry.ItemName;
                        LogicEntry1.LocationName = (string.IsNullOrWhiteSpace(DicEntry.LocationName)) ? null : DicEntry.LocationName;
                        LogicEntry1.LocationArea = (string.IsNullOrWhiteSpace(DicEntry.LocationArea)) ? "Misc" : DicEntry.LocationArea;
                        LogicEntry1.ItemSubType = (string.IsNullOrWhiteSpace(DicEntry.ItemSubType)) ? "Item" : DicEntry.ItemSubType;
                        LogicEntry1.SpoilerLocation = (string.IsNullOrWhiteSpace(DicEntry.SpoilerLocation)) 
                            ? new List<string> { LogicEntry1.LocationName } : DicEntry.SpoilerLocation.Split('|').ToList();
                        LogicEntry1.SpoilerItem = (string.IsNullOrWhiteSpace(DicEntry.SpoilerItem)) 
                            ? new List<string> { LogicEntry1.ItemName } : DicEntry.SpoilerItem.Split('|').ToList();
                        break;
                    case 1:
                        if (string.IsNullOrWhiteSpace(line)) { LogicEntry1.Required = null; break; }
                        LogicEntry1.Required = line.Split(',').Select(y => int.Parse(y)).ToArray();
                        break;
                    case 2:
                        if (string.IsNullOrWhiteSpace(line)) { LogicEntry1.Conditionals = null; break; }
                        LogicEntry1.Conditionals = line.Split(';').Select(x => x.Split(',').Select(y => int.Parse(y)).ToArray()).ToArray();
                        break;
                    case 3:
                        LogicEntry1.NeededBy = Convert.ToInt32(line);
                        break;
                    case 4:
                        LogicEntry1.AvailableOn = Convert.ToInt32(line);
                        break;
                    case 5:
                        LogicEntry1.IsTrick = (line.StartsWith(";"));
                        LogicEntry1.TrickEnabled = true;
                        LogicEntry1.TrickToolTip = (line.Length > 1) ? line.Substring(1) : "No Tooltip Available";
                        //if (LogicEntry1.IsTrick) { Debugging.Log($"Trick {LogicEntry1.DictionaryName} Found. ToolTip =  { LogicEntry1.TrickToolTip }"); }
                        break;
                }
                if ((NextLine) >= instance.RawLogicFile.Count() || instance.RawLogicFile[NextLine].StartsWith("-"))
                {
                    //Push Data to the instance
                    instance.Logic.Add(LogicEntry1);
                    LogicEntry1 = new LogicObjects.LogicEntry();
                    idCounter++;
                }
                NextLine++;
                SubCounter++;
            }

            instance.EntranceRando = instance.IsEntranceRando();
            instance.EntranceAreaDic = VersionHandeling.AreaClearDictionary(instance);
            CreateDicNameToID(instance);
            if (instance.EntranceRando) { CreatedEntrancepairDcitionary(instance); }

            return true;
        }

        public static bool RequirementsMet(int[] list, List<LogicObjects.LogicEntry> logic, List<int> usedItems = null)
        {
            usedItems = usedItems ?? new List<int>();
            if (list == null) { return true; }
            foreach(var i in list)
            {
                if (!logic[i].ItemUseable(usedItems)) { return false; }
            }
            return true;
        }

        public static bool CondtionalsMet(int[][] list, List<LogicObjects.LogicEntry> logic, List<int> usedItems = null)
        {
            usedItems = usedItems ?? new List<int>();
            if (list == null) { return true; }
            //Remove any lines from the conditional that contain disabled tricks
            var ValidListEntries = list.Where(x => !x.Where(y => logic[y].IsTrick && !logic[y].TrickEnabled).Any());
            if (!ValidListEntries.Any()) { return true; }
            foreach (var i in ValidListEntries)
            {
                List<int> UsedItemsSet = new List<int>();
                if (RequirementsMet(i, logic, UsedItemsSet))
                {
                    foreach (var set in UsedItemsSet) { usedItems.Add(set); }
                    return true;
                }
            }
            return false;
        }

        public static void CalculateItems(LogicObjects.TrackerInstance Instance, bool ForceStrictLogicHendeling = false, bool InitialRun = true)
        {
            if (InitialRun && (Instance.Options.StrictLogicHandeling || ForceStrictLogicHendeling)) { Instance.RefreshFakeItems(); }
            bool recalculate = false;
            foreach (var item in Instance.Logic)
            {
                item.Available = item.CheckAvailability(Instance);
                if (item.FakeItemStatusChange()) { recalculate = true; }
            }
            if (recalculate) { CalculateItems(Instance, false, false); }
        }

        public static void WriteSpoilerLogToLogic(LogicObjects.TrackerInstance Instance, string path)
        {
            List<LogicObjects.SpoilerData> SpoilerData = new List<LogicObjects.SpoilerData>();
            if (path.Contains(".txt") || path.Contains(".json")) 
            {
                bool TXTOverride = false;
                if (Instance.IsMM())
                {
                    var txs = MessageBox.Show("If possible, the HTML spoiler log should always be imported in place of the text spoiler log.\n\n The text spoiler log will work most of the time but may be inconsistent.\n\nWould you like to select an HTML spoiler log instead?", "Text log used", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (txs == DialogResult.Yes) 
                    {
                        var HTMLPath = Utility.FileSelect("Select an HTML Spoiler Log", "Spoiler Log (*html)|*html");
                        if (HTMLPath != "")
                        {
                            TXTOverride = true;
                            SpoilerData = Tools.ReadHTMLSpoilerLog(HTMLPath, Instance);
                        }
                    }
                }
                if (!TXTOverride) { SpoilerData = Tools.ReadTextSpoilerlog(Instance, File.ReadAllLines(path)); }
            }
            else if (path.Contains(".html")) { SpoilerData = Tools.ReadHTMLSpoilerLog(path, Instance); }
            else { MessageBox.Show("This Spoiler log is not valid. Please use either an HTML or TXT file."); return;  }

            foreach (LogicObjects.SpoilerData data in SpoilerData)
            {
                if (data.LocationID > -1 && data.ItemID > -2 && data.LocationID < Instance.Logic.Count && data.ItemID < Instance.Logic.Count)
                {
                    Instance.Logic[data.LocationID].SpoilerRandom = data.ItemID;
                    if (data.BelongsTo > -1) { Instance.Logic[data.LocationID].PlayerData.ItemBelongedToPlayer = data.BelongsTo; }
                }
            }

            var entranceIDs = Instance.EntranceAreaDic;
            foreach (var i in Instance.Logic.Where(x => x.ItemSubType == "Dungeon Entrance" && entranceIDs.ContainsValue(x.ID) && x.SpoilerRandom < 0))
            {
                i.SpoilerRandom = i.ID;
            }
        }

        public static void CheckEntrancePair(LogicObjects.LogicEntry Location, LogicObjects.TrackerInstance Instance, bool Checking)
        {
            if (Location.ID < 0 || !Instance.Options.CoupleEntrances || !Location.HasRandomItem(true) || !Location.IsEntrance()) { return; }
            var reverseLocation = Location.PairedEntry(Instance, true);
            var reverseItem = Location.PairedEntry(Instance);
            if (reverseItem == null || reverseLocation == null) return;
            //is the reverse entrance already checked and randomized to something
            if ((reverseLocation.Checked || (reverseLocation.HasRandomItem(true) && reverseLocation.RandomizedEntry(Instance) != reverseItem) || reverseItem.Aquired) && Checking) { return; }
            //Does the spoiler log conflict with what the reverse check is trying to do
            if (reverseLocation.SpoilerRandom != reverseItem.ID && reverseLocation.SpoilerRandom > -1 && Checking) { return; }
            reverseLocation.Checked = Checking;
            reverseLocation.RandomizedItem = (Checking) ? reverseItem.ID : -2;
            reverseItem.Aquired = Checking;
        }

        public static void RecreateLogic(LogicObjects.TrackerInstance Instance, string[] LogicData = null)
        {
            var LogicFile = LogicData;
            var saveFile = false;
            LogicObjects.TrackerInstance template = null;
            if (LogicFile == null)
            {
                string file = "";
                file = Utility.FileSelect("Select A Logic File", "Logic File (*.txt;*.MMRTSAV)|*.txt;*.MMRTSAV");
                if (file == "") { return; }

                saveFile = file.EndsWith(".MMRTSAV");
                string[] SaveFileRawLogicFile = null;
                if (saveFile)
                {
                    try { template = JsonConvert.DeserializeObject<LogicObjects.TrackerInstance>(File.ReadAllText(file)); }
                    catch
                    {
                        MessageBox.Show("Save File Not Valid.");
                        return;
                    }
                    SaveFileRawLogicFile = template.RawLogicFile;
                }

                LogicFile = (saveFile) ? SaveFileRawLogicFile : File.ReadAllLines(file);
            }
            Tools.SaveState(Instance);

            var OldLogic = Utility.CloneLogicList(Instance.Logic);
            Instance.RawLogicFile = LogicFile;
            LogicEditing.PopulateTrackerInstance(Instance);

            var logic = Instance.Logic;
            foreach (var entry in OldLogic)
            {
                var logicEntry = logic.Find(x => x.DictionaryName == entry.DictionaryName);
                if (logicEntry == null) { continue; }
                logicEntry.Aquired = entry.Aquired;
                logicEntry.Checked = entry.Checked;
                logicEntry.RandomizedItem = entry.RandomizedItem;
                logicEntry.SpoilerRandom = entry.SpoilerRandom;
                logicEntry.Starred = entry.Starred;
                logicEntry.Options = entry.Options;
                logicEntry.TrickEnabled = entry.TrickEnabled;
                logicEntry.PlayerData = entry.PlayerData;
            }
            if (saveFile)
            {
                var Options = MessageBox.Show("Would you like to import the general tracker options from this save file?", "Options", MessageBoxButtons.YesNo);
                if (Options == DialogResult.Yes) { LogicObjects.MainTrackerInstance.Options = template.Options; }
                var RandOptions = MessageBox.Show("Would you like to import the Item Randomization options from this save file?", "Randomization Options", MessageBoxButtons.YesNo);
                if (RandOptions == DialogResult.Yes)
                {
                    foreach (var i in LogicObjects.MainTrackerInstance.Logic)
                    {
                        var TemplateData = template.Logic.Find(x => x.DictionaryName == i.DictionaryName);
                        if (TemplateData != null)
                        {
                            i.Options = TemplateData.Options;
                            i.TrickEnabled = TemplateData.TrickEnabled;
                        }
                    }
                }
            }
            Instance.Options.EntranceRadnoEnabled = Utility.CheckForRandomEntrances(Instance);
            Instance.Options.OverRideAutoEntranceRandoEnable = (Instance.Options.EntranceRadnoEnabled != Instance.EntranceRando);
            CalculateItems(Instance, true);
        }

        public static void CreatedEntrancepairDcitionary(LogicObjects.TrackerInstance instance)
        {
            foreach(var i in instance.Logic.Where(x => x.IsEntrance()))
            {
                var Pair = instance.LogicDictionary.Find(x => x.EntrancePair == i.DictionaryName);
                if (Pair == null || !instance.DicNameToID.ContainsKey(Pair.DictionaryName)) { continue; }
                instance.EntrancePairs.Add(i.ID, instance.DicNameToID[Pair.DictionaryName]);
            }   
        }

        public static void CreateDicNameToID(LogicObjects.TrackerInstance instance)
        {
            foreach (var LogicEntry1 in instance.Logic)
            {
                if (!instance.DicNameToID.ContainsKey(LogicEntry1.DictionaryName) && !LogicEntry1.IsFake)
                { instance.DicNameToID.Add(LogicEntry1.DictionaryName, LogicEntry1.ID); }
            }
        }

        public static string[] WriteLogicToArray(LogicObjects.TrackerInstance Instance, bool IncludeTrickData = true)
        {
            List<string> lines = new List<string>
            {
                (Instance.IsMM()) ? "-version " + Instance.LogicVersion : "-version" + Instance.GameCode + " " + Instance.LogicVersion
            };
            foreach (var line in Instance.Logic)
            {
                lines.Add("- " + line.DictionaryName);
                string Req = "";
                string Comma = "";
                foreach (var i in line.Required ?? new int[0])
                {
                    Req = Req + Comma + i.ToString();
                    Comma = ",";
                }
                lines.Add(Req);
                string cond = "";
                string colon = "";
                foreach (var j in line.Conditionals ?? new int[0][])
                {
                    Req = "";
                    Comma = "";
                    foreach (var i in j ?? new int[0])
                    {
                        Req = Req + Comma + i.ToString();
                        Comma = ",";
                    }
                    cond = cond + colon + Req;
                    colon = ";";
                }
                lines.Add(cond);
                lines.Add(line.NeededBy.ToString());
                lines.Add(line.AvailableOn.ToString());
                if (IncludeTrickData) 
                {
                    string trickLine = "";
                    if (line.IsTrick) 
                    { 
                        trickLine = ";";
                        if (line.TrickToolTip != "No Tooltip Available") { trickLine += line.TrickToolTip; }
                    }
                    lines.Add(trickLine);
                }
            }
            return lines.ToArray();
        }
    }
}
