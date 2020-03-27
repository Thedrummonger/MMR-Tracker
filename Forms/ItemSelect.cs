﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{
    public partial class ItemSelect : Form
    {
        public ItemSelect()
        {
            InitializeComponent();
        }

        public static int Function = 0;

        private void ItemSelect_Load(object sender, EventArgs e)
        {
            this.ActiveControl = TXTSearch;
            if (Function == 3)
            {
                BTNJunk.Visible = false;
                this.Height = this.Height - BTNJunk.Height;
                LBItemSelect.SelectionMode = SelectionMode.One;
            }
            else if (Function > 0)
            {
                BTNJunk.Text = "Select";
                LBItemSelect.SelectionMode = SelectionMode.MultiExtended;
            }
            else
            {
                BTNJunk.Text = "Junk";
                LBItemSelect.SelectionMode = SelectionMode.One;
            }

            if (Function == 0) { ItemSelectList(); this.Text = "Item at " + LogicObjects.CurrentSelectedItem.LocationName; }
            if (Function == 1) { SeedCheckLocations(); this.Text = "Select a location"; }
            if (Function == 2) { SeedCheckItems(); this.Text = "Select an item"; }
            if (Function == 3) { AvailableLocations(); this.Text = "Select a location"; }
        }

        private void ItemSelectList()
        {
            LBItemSelect.Items.Clear();
            List<string> Duplicates = new List<string>();
            for (var i = 0; i < LogicObjects.Logic.Count; i++)
            {
                if (!LogicObjects.Logic[i].Aquired
                    && (!LogicObjects.Logic[i].IsFake)
                    && !Duplicates.Contains(LogicObjects.Logic[i].ItemName)
                    && LogicObjects.Logic[i].ItemName != null
                    && Utility.FilterSearch(LogicObjects.Logic[i], TXTSearch.Text, LogicObjects.Logic[i].DisplayName)
                    && (LogicObjects.CurrentSelectedItem.ItemSubType == LogicObjects.Logic[i].ItemSubType || LogicObjects.CurrentSelectedItem.ItemSubType == "ALL"))
                {
                    LogicObjects.Logic[i].DisplayName = LogicObjects.Logic[i].ItemName;
                    LBItemSelect.Items.Add(LogicObjects.Logic[i]);
                    Duplicates.Add(LogicObjects.Logic[i].ItemName);
                }
            }
        }

        private void SeedCheckLocations()
        {
            LBItemSelect.Items.Clear();
            for (var i = 0; i < LogicObjects.Logic.Count; i++)
            {
                LogicObjects.Logic[i].DisplayName = LogicObjects.Logic[i].LocationName ?? LogicObjects.Logic[i].DictionaryName;
                if (Utility.FilterSearch(LogicObjects.Logic[i], TXTSearch.Text, LogicObjects.Logic[i].DisplayName) && !LogicObjects.Logic[i].IsFake)
                {
                    LBItemSelect.Items.Add(LogicObjects.Logic[i]);
                }
            }
        }

        private void SeedCheckItems()
        {
            LBItemSelect.Items.Clear();
            for (var i = 0; i < LogicObjects.Logic.Count; i++)
            {
                LogicObjects.Logic[i].DisplayName = LogicObjects.Logic[i].ItemName ?? LogicObjects.Logic[i].DictionaryName;
                if (Utility.FilterSearch(LogicObjects.Logic[i], TXTSearch.Text, LogicObjects.Logic[i].DisplayName))
                {
                    LBItemSelect.Items.Add(LogicObjects.Logic[i]);
                }
            }
        }

        private void AvailableLocations()
        {
            LBItemSelect.Items.Clear();
            for (var i = 0; i < LogicObjects.Logic.Count; i++)
            {
                LogicObjects.Logic[i].DisplayName = LogicObjects.Logic[i].LocationName ?? LogicObjects.Logic[i].DictionaryName;
                if (Utility.FilterSearch(LogicObjects.Logic[i], TXTSearch.Text, LogicObjects.Logic[i].DisplayName) && LogicObjects.Logic[i].Available && !LogicObjects.Logic[i].IsFake)
                {
                    LBItemSelect.Items.Add(LogicObjects.Logic[i]);
                }
            }
        }

        private void LBItemSelect_DoubleClick(object sender, EventArgs e)
        {
            if (Function > 0 && Function != 3) 
            {
                foreach (var i in LBItemSelect.SelectedItems)
                {
                    var item = i as LogicObjects.LogicEntry;
                    LogicObjects.selectedItems.Add(item);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                LogicObjects.CurrentSelectedItem = LBItemSelect.SelectedItem as LogicObjects.LogicEntry;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void TXTSearch_TextChanged(object sender, EventArgs e)
        {
            if (Function == 0) { ItemSelectList(); }
            if (Function == 1) { SeedCheckLocations(); }
            if (Function == 2) { SeedCheckItems(); }
            if (Function == 3) { AvailableLocations(); }
        }

        private void BTNJunk_Click(object sender, EventArgs e)
        {
            if (Function > 0)
            {
                foreach (var i in LBItemSelect.SelectedItems)
                {
                    var item = i as LogicObjects.LogicEntry;
                    LogicObjects.selectedItems.Add(item);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                LogicObjects.CurrentSelectedItem = new LogicObjects.LogicEntry { ID = -1 };
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}