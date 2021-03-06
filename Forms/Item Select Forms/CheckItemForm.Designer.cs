﻿namespace MMR_Tracker.Forms.Sub_Forms
{
    partial class CheckItemForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CheckItemForm));
            this.LBItemSelect = new System.Windows.Forms.ListBox();
            this.btnJunk = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.TXTSearch = new System.Windows.Forms.TextBox();
            this.chkSort = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // LBItemSelect
            // 
            this.LBItemSelect.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LBItemSelect.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LBItemSelect.FormattingEnabled = true;
            this.LBItemSelect.IntegralHeight = false;
            this.LBItemSelect.Location = new System.Drawing.Point(13, 52);
            this.LBItemSelect.Name = "LBItemSelect";
            this.LBItemSelect.Size = new System.Drawing.Size(428, 576);
            this.LBItemSelect.TabIndex = 0;
            this.LBItemSelect.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.LBItemSelect_DrawItem);
            this.LBItemSelect.DoubleClick += new System.EventHandler(this.HandleSelectedItem);
            // 
            // btnJunk
            // 
            this.btnJunk.Location = new System.Drawing.Point(274, 5);
            this.btnJunk.Name = "btnJunk";
            this.btnJunk.Size = new System.Drawing.Size(68, 20);
            this.btnJunk.TabIndex = 1;
            this.btnJunk.Text = "Junk";
            this.btnJunk.UseVisualStyleBackColor = true;
            this.btnJunk.Click += new System.EventHandler(this.HandleSelectedItem);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(348, 26);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(93, 20);
            this.numericUpDown1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(345, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Belongs To Player:";
            // 
            // TXTSearch
            // 
            this.TXTSearch.Location = new System.Drawing.Point(13, 26);
            this.TXTSearch.Name = "TXTSearch";
            this.TXTSearch.Size = new System.Drawing.Size(329, 20);
            this.TXTSearch.TabIndex = 4;
            this.TXTSearch.TextChanged += new System.EventHandler(this.TXTSearch_TextChanged);
            this.TXTSearch.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TXTSearch_MouseUp);
            // 
            // chkSort
            // 
            this.chkSort.AutoSize = true;
            this.chkSort.BackColor = System.Drawing.Color.Transparent;
            this.chkSort.Checked = true;
            this.chkSort.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSort.ForeColor = System.Drawing.SystemColors.Control;
            this.chkSort.Location = new System.Drawing.Point(200, 7);
            this.chkSort.Name = "chkSort";
            this.chkSort.Size = new System.Drawing.Size(68, 17);
            this.chkSort.TabIndex = 5;
            this.chkSort.Text = "Sort A>Z";
            this.chkSort.UseVisualStyleBackColor = false;
            this.chkSort.CheckedChanged += new System.EventHandler(this.chkSort_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.SystemColors.Control;
            this.label2.Location = new System.Drawing.Point(12, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Filter By:";
            // 
            // CheckItemForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(361, 641);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.chkSort);
            this.Controls.Add(this.TXTSearch);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.btnJunk);
            this.Controls.Add(this.LBItemSelect);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CheckItemForm";
            this.Text = "CheckItemForm";
            this.Load += new System.EventHandler(this.CheckItemForm_Load);
            this.Shown += new System.EventHandler(this.CheckItemForm_Shown);
            this.Resize += new System.EventHandler(this.CheckItemForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox LBItemSelect;
        private System.Windows.Forms.Button btnJunk;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TXTSearch;
        private System.Windows.Forms.CheckBox chkSort;
        private System.Windows.Forms.Label label2;
    }
}