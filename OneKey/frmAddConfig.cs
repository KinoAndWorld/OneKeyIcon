using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public enum EditType{
    EditTypeAdd = 0,
    EditTypeUpdate
};

namespace OneKey
{ 
    public partial class frmAddConfig : Form
    {
        //public Form1 mainForm;
        public ConvertConfig convertConf;
        public SizeConfig sizeConf;
        public EditType editType;

        public int curItemIndex;

        public delegate void RemoveItemBlock(int index);
        public event RemoveItemBlock removeBlock;


        public frmAddConfig()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void frmAddConfig_Load(object sender, EventArgs e)
        {
            if (sizeConf != null)
            {
                editType = EditType.EditTypeUpdate;
                addButton.Text = "确认修改";
                Button deleteButton = new Button();
                deleteButton.Text = "删除配置";
                deleteButton.Width = addButton.Width;
                deleteButton.Height = addButton.Height;
                deleteButton.Left = addButton.Left;
                deleteButton.Top = addButton.Top - deleteButton.Height - 5;
                deleteButton.ForeColor = Color.White;
                deleteButton.BackColor = Color.Red;
                deleteButton.Click += new EventHandler(deleteButton_Click);

                this.Controls.Add(deleteButton);
                // other 
                numericUpDown1.Value = decimal.Parse(sizeConf.size.Width.ToString());
                numericUpDown2.Value = decimal.Parse(sizeConf.size.Height.ToString());
                textBox1.Text = sizeConf.name;
            }
            else
            {
                editType = EditType.EditTypeAdd;
                addButton.Text = "确认添加";
            }
        }

        

        private void addButton_Click(object sender, EventArgs e){
            if (editType == EditType.EditTypeAdd) {
                addSizeConfig();
            }else {
                updateSizeConfig();
            }
        }

        void deleteButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("大人要删除" + sizeConf.name + "这个尺寸吗", "提示",
                    MessageBoxButtons.OKCancel) ==
                    System.Windows.Forms.DialogResult.OK){
                if (removeBlock!=null && removeBlock.Target != null){
                    this.Close();
                    removeBlock(curItemIndex);
                }
            }
        }

        private void addSizeConfig() {
            SizeConfig sizeConf = new SizeConfig();
            sizeConf.size = new Size(decimal.ToInt32(numericUpDown1.Value),
                                     decimal.ToInt32(numericUpDown2.Value));

            sizeConf.name = textBox1.Text;
            if (convertConf != null)
            {
                convertConf.addSizeConfig(sizeConf);
            }
        }

        private void updateSizeConfig() {
            if (textBox1.Text == "" || textBox1.Text.Trim() == "")
            {
                MessageBox.Show("名称不能为空");
                return;
            }

            if (decimal.ToInt32(numericUpDown1.Value) > 0 &&
                decimal.ToInt32(numericUpDown1.Value) < 10000 &&
                decimal.ToInt32(numericUpDown2.Value) > 0 &&
                decimal.ToInt32(numericUpDown2.Value) < 10000)
            {
                //SizeConfig sizeConf = new SizeConfig();
                sizeConf.size = new Size(decimal.ToInt32(numericUpDown1.Value),
                                         decimal.ToInt32(numericUpDown2.Value));

                sizeConf.name = textBox1.Text;
                if (convertConf != null)
                {
                    convertConf.updateSizeConfig(sizeConf,curItemIndex);
                }
            }
        }
    }
}
