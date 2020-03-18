﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace MMR_Tracker_V2
{
    public partial class RandomizeOptions : Form
    {
        public RandomizeOptions()
        {
            InitializeComponent();
        }

        //Form Objects

        private void RandomizeOptions_Load(object sender, EventArgs e) { WriteToListVeiw(); }

        private void BTNRandomized_Click(object sender, EventArgs e) { UpdateRandomOption(0); }

        private void BTNUnrando_Click(object sender, EventArgs e) { UpdateRandomOption(1); }

        private void BTNUnrandMan_Click(object sender, EventArgs e) { UpdateRandomOption(2); }

        private void BTNJunk_Click(object sender, EventArgs e) { UpdateRandomOption(3); }

        private void BTNStarting_Click(object sender, EventArgs e) { UpdateRandomOption(4); }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string settingString = "";
            foreach (var item in LogicObjects.Logic)
            {
                if (item.IsFake) { continue; }

                int Setting = item.RandomizedState;
                if (item.StartingItem) { Setting += 4; }

                settingString += Setting.ToString();
            }
            string[] Options = new string[2 + LogicObjects.RawLogicText.Count];
            Options[0] = settingString;
            Options[1] = VersionHandeling.Version.ToString();
            var count = 2;
            foreach(var i in LogicObjects.RawLogicText)
            {
                Options[count] = i;
                count++;
            }
            SaveFileDialog saveDialog = new SaveFileDialog { Filter = "MMR Tracker Settings (*.MMRTSET)|*.MMRTSET", FilterIndex = 1 };
            if (saveDialog.ShowDialog() == DialogResult.OK) { File.WriteAllLines(saveDialog.FileName, Options); }
            
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            string file = Utility.FileSelect("Select A Settings File", "MMR Tracker Settings (*.MMRTSET)|*.MMRTSET");
            if (file == "") { return; }
            string[] options = File.ReadAllLines(file);
            UpdateRandomOptionsFromFile(options);
            WriteToListVeiw();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e) { WriteToListVeiw(); }

        //Functions

        public void UpdateRandomOption(int option)
        {
            foreach (ListViewItem selection in listView1.SelectedItems)
            {
                if (option == 4)
                {
                    var logObj = LogicObjects.Logic[Int32.Parse(selection.Tag.ToString())];
                    logObj.StartingItem = !logObj.StartingItem;
                }
                else
                {
                    int index = Int32.Parse(selection.Tag.ToString());
                    LogicObjects.Logic[index].RandomizedState = option;
                    //Console.WriteLine(index);
                }
            }
            WriteToListVeiw();
        }

        public void WriteToListVeiw()
        {
            listView1.Items.Clear();
            var logic = LogicObjects.Logic;
            List<string> randomizedOptions = new List<string> { "Randomized", "Unrandomized", "Unrandomized (Manual)", "Forced Junk" };
            listView1.FullRowSelect = true;
            foreach (var entry in logic)
            {
                if (!entry.IsFake && Utility.FilterSearch(entry, txtSearch.Text, entry.DictionaryName))
                {
                    string[] row = { entry.DictionaryName, randomizedOptions[entry.RandomizedState], entry.StartingItem.ToString() };
                    ListViewItem listViewItem = new ListViewItem(row);
                    listViewItem.Tag = entry.ID;
                    listView1.Items.Add(listViewItem);
                }
            }
        }

        public static void UpdateRandomOptionsFromFile(string[] options)
        {

            int Version = Int32.Parse(options[1]);

            if (VersionHandeling.Version != Version)
            {
                MessageBox.Show("This settings file was not made using the current logic version.");
                return;
            }
            int counter = 0;
            foreach (var item in LogicObjects.Logic)
            {
                if (item.IsFake) { continue; }
                int setting = Int32.Parse(options[0][counter].ToString());
                item.StartingItem = setting > 3;
                item.RandomizedState = (setting > 3) ? setting - 4 : setting;
                counter++;
            }
        }
    }
}
