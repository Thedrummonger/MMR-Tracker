﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{
    public partial class DebugScreen : Form
    {

        public int DebugFunction = 0;
        public DebugScreen()
        {
            InitializeComponent();
        }

        private void DebugScreen_Load(object sender, EventArgs e)
        {
            switch (DebugFunction)
            {
                case 0:
                    this.Close();
                    break;
                case 1:
                    PrintLogicToListBox();
                    break;
                case 2:
                    PrintInfo();
                    break;
            }
        }
        public void PrintLogicToListBox()
        {
            List<LogicObjects.LogicEntry> Logic = LogicObjects.Logic;
            for (int i = 0; i < Logic.Count; i++)
            {
                listBox1.Items.Add("---------------------------------------");
                listBox1.Items.Add("ID: " + Logic[i].ID);
                listBox1.Items.Add("Name: " + Logic[i].DictionaryName);
                listBox1.Items.Add("Location: " + Logic[i].LocationName);
                listBox1.Items.Add("Item: " + Logic[i].ItemName);
                listBox1.Items.Add("Location area: " + Logic[i].LocationArea);
                listBox1.Items.Add("Available: " + Logic[i].Available);
                listBox1.Items.Add("Aquired: " + Logic[i].Aquired);
                listBox1.Items.Add("Checked: " + Logic[i].Checked);
                listBox1.Items.Add("Fake Item: " + Logic[i].IsFake);

                if (Logic[i].RandomizedItem > -1) { listBox1.Items.Add("Random Item: " + Logic[Logic[i].RandomizedItem].DictionaryName); }
                else if (Logic[i].RandomizedItem == -1) { listBox1.Items.Add("Random Item: Junk"); }
                else if (Logic[i].RandomizedItem == -2) { listBox1.Items.Add("Random Item: Undiscovered"); }

                listBox1.Items.Add("Spoiler Log Location name: " + Logic[i].SpoilerLocation);
                listBox1.Items.Add("Spoiler Log Item name: " + Logic[i].SpoilerItem);

                if (Logic[i].SpoilerRandom > -1) { listBox1.Items.Add("Spoiler Log Randomized Item: " + Logic[Logic[i].SpoilerRandom].DictionaryName); }
                else { listBox1.Items.Add("Spoiler Log Randomized Item: None"); }

                if (Logic[i].RandomizedState == 0) { listBox1.Items.Add("Randomized State: Randomized"); }
                if (Logic[i].RandomizedState == 1) { listBox1.Items.Add("Randomized State: Unrandomized"); }
                if (Logic[i].RandomizedState == 2) { listBox1.Items.Add("Randomized State: Forced Fake"); }
                if (Logic[i].RandomizedState == 3) { listBox1.Items.Add("Randomized State: Forced Junk"); }

                listBox1.Items.Add("Starting Item: " + Logic[i].StartingItem);

                var test2 = Logic[i].Required;
                if (test2 == null) { listBox1.Items.Add("NO REQUIRE"); }
                else
                {
                    listBox1.Items.Add("Required--------------");
                    for (int j = 0; j < test2.Length; j++)
                    {
                        listBox1.Items.Add("--" + Logic[test2[j]].DictionaryName);
                    }
                }
                var test3 = Logic[i].Conditionals;
                if (test3 == null) { listBox1.Items.Add("NO CONDITIONAL"); }
                else
                {
                    for (int j = 0; j < test3.Length; j++)
                    {
                        listBox1.Items.Add("Conditional " + j + "--------------");
                        for (int k = 0; k < test3[j].Length; k++)
                        {
                            listBox1.Items.Add("--" + Logic[test3[j][k]].DictionaryName);
                        }
                    }
                }
            }
        }
        public void PrintInfo()
        {
            this.Text = "Info";
            listBox1.Items.Add("Majoras Mask Randomizer Tracker");
            listBox1.Items.Add("Tracker Github:  (Double Click)");
            listBox1.Items.Add("Version: 2.0");
            listBox1.Items.Add("By: Thedrummonger");
            listBox1.Items.Add("Twitter: @thedrummonger https://twitter.com/thedrummonger (Double Click)");
            listBox1.Items.Add("================For use with the Majoras Mask Randomizer================");
            listBox1.Items.Add("Majoras Mask Randomizer By: ZoeyZolotova");
            listBox1.Items.Add("Randomizer Github: https://github.com/ZoeyZolotova/mm-rando (Double Click)");
            listBox1.Items.Add("Randomizer Discord: https://discord.gg/TJZ4uCP (Double Click)");
            listBox1.Items.Add("==================================================================");
            listBox1.Items.Add("General Use:");
            listBox1.Items.Add("The tracker will show you all available locations based on your logic and");
            listBox1.Items.Add("        obtained items.");
            listBox1.Items.Add("Double clicking a location will bring up the item select list.");
            listBox1.Items.Add("From here you will select the item you found at this location.");
            listBox1.Items.Add("This will mark that item as obtained and recalculate what locations are available.");
            listBox1.Items.Add("The location will be moved to \"Checked locations\" and display what");
            listBox1.Items.Add("        item it contained.");
            listBox1.Items.Add("Double Clicking an item in \"Checked locations\" will uncheck that location and mark");
            listBox1.Items.Add("        the corrisponding item as unobtained");
            listBox1.Items.Add("==================================================================");
            listBox1.Items.Add("List Filtering:");
            listBox1.Items.Add("Typing in the text box above a list will filter the items in the list.");
            listBox1.Items.Add("You can filter by area by typing # at the beggining of your filter.");
            listBox1.Items.Add("You can filter multiple things at once by seperating them with a |. (Pipe)");
            listBox1.Items.Add("(| (Pipe) is located below the backspace key on most keyboards)");
            listBox1.Items.Add("So for example typing \"Clock|Wood\" will show all checks that contain the word");
            listBox1.Items.Add("        \"Clock\" as well as all checks that contain \"Wood\".");
            listBox1.Items.Add("You can also filter by multiple words by seperating them with a ,. (Comma)");
            listBox1.Items.Add("So for example typing \"Clock,Wood\" will show only checks that contain both.");
            listBox1.Items.Add("        the word \"clock\" and the word \"wood\".");
            listBox1.Items.Add("Advanced filtering using , and | only works with available Location/entrances");
            listBox1.Items.Add("==================================================================");
            listBox1.Items.Add("Setting Item vs Marking Item:");
            listBox1.Items.Add("The set item and set entrance button will mark an item/entrance as being at a");
            listBox1.Items.Add("        location without actually marking that item/entrance as being obtained.");
            listBox1.Items.Add("This is usefull when you know what item is in a location but haven't actually");
            listBox1.Items.Add("        obtianed it such as if you see it in a shop or read about it in a hint.");
            listBox1.Items.Add("==================================================================");
            listBox1.Items.Add("Entrance radnomizer Features:");
            listBox1.Items.Add("This is only useful in versions with full entrance randomizer");
            listBox1.Items.Add("It will toggle the available entrances and path finder lists");
            listBox1.Items.Add("If this Feature is off, entrances will show up in the available items list.");
            listBox1.Items.Add("The path finder will show you the path from one entrance to another.");
            listBox1.Items.Add("You will enter the last exit you came out of (or exit closest to you) along");
            listBox1.Items.Add("        with the entrance you want to end up in front of.");
            listBox1.Items.Add("It will not consider you actually going through the entrance you end up infront of.");
            listBox1.Items.Add("==================================================================");
            listBox1.Items.Add("Importing Spoiler log:");
            listBox1.Items.Add("This will allow you to import the spoiler log generated with your rom.");
            listBox1.Items.Add("After you have imoprted the spoiler log checking/marking a location will ");
            listBox1.Items.Add("        automatically Fill in the appropriate item based on your spoiler log.");
            listBox1.Items.Add("==================================================================");
            listBox1.Items.Add("Randomization options:");
            listBox1.Items.Add("There are 4 different states a location can be");
            listBox1.Items.Add("1. Randomized");
            listBox1.Items.Add("The location is randomized as normal. It will appear in the tracker and ask for the");
            listBox1.Items.Add("        user to input what item is located there.");
            listBox1.Items.Add("2. Unrandomized");
            listBox1.Items.Add("The location is not randomized. It will not appear in the tracker and automatically");
            listBox1.Items.Add("        be marked as obtained whenever it becomes available.");
            listBox1.Items.Add("3. Unrandomized (Manual)");
            listBox1.Items.Add("The location is not randomized but must be marked manually. It will still appear in");
            listBox1.Items.Add("        the tracker, but must be double clicked to be marked as obtained.");
            listBox1.Items.Add("4. Forced Junk");
            listBox1.Items.Add("The lcoation is randomized but will never contain an item usefull to logic The");
            listBox1.Items.Add("        location will simply not appear in the tracker.");
            listBox1.Items.Add("An item can also be marked as a starting item.");
            listBox1.Items.Add("This will make the tracker always consider the item obtained when calculating logic.");
            listBox1.Items.Add("==================================================================");
            listBox1.Items.Add("Strict Logic Handeling:");
            listBox1.Items.Add("This option might make your logic calculations a bit slower, but will prevent rare");
            listBox1.Items.Add("        bugs that occur involvolving circular dependencies in logic.");
            listBox1.Items.Add("You should never need to enable this, but it's worth a try if logic is being buggy.");
        }
        private void ListBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == 1) { System.Diagnostics.Process.Start("https://github.com/Thedrummonger"); }
            if (listBox1.SelectedIndex == 4) { System.Diagnostics.Process.Start("https://twitter.com/thedrummonger"); }
            if (listBox1.SelectedIndex == 7) { System.Diagnostics.Process.Start("https://github.com/ZoeyZolotova/mm-rando"); }
            if (listBox1.SelectedIndex == 8) { System.Diagnostics.Process.Start("https://discord.gg/TJZ4uCP"); }
        }
    }
}
