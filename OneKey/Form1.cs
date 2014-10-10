using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using System.Drawing.Imaging;

namespace OneKey
{
    public partial class Form1 : Form , iConfigChange
    {
        private ConvertConfig convConfig;
        private int curOperaIndex = 0;
        public Form1()
        {
            InitializeComponent();
            
            this.StartPosition = FormStartPosition.CenterScreen;

            convConfig = new ConvertConfig();
            convConfig.delegateForm = this;

            this.loadLastConfig();

            ConvertButton.Enabled = false;
            this.listBox1.MouseDoubleClick += new MouseEventHandler(listBox1_DoubleClick);
        }
        
        /// <summary>
        /// 读取XML的配置信息
        /// </summary>
        private void loadLastConfig() {
            List<string> res = convConfig.loadLocalConfig();
            listBox1.Items.Clear();
            listBox1.Items.AddRange(res.ToArray());
            //comboBox1.SelectedIndex = 0;
            //convConfig.outputFormat = convConfig.getFormatByString(comboBox1.Items[comboBox1.SelectedIndex].ToString());
            comboBox1.SelectedItem = convConfig.getFormatStringByFormat(convConfig.outputFormat);
            textBox1.Text = string.Format("输出路径：{0}", convConfig.savePath);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = string.Format("输出路径：{0}", folderBrowserDialog1.SelectedPath);
                convConfig.savePath = folderBrowserDialog1.SelectedPath;
            }
        }
        /// <summary>
        /// 添加图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    Image img = Image.FromFile(openFileDialog1.FileName);
                    label2.Text = string.Format("尺寸：{0}*{1}", img.Width, img.Height);
                    String format = convConfig.getFormatStringByImage(img);

                    label3.Text = String.Format("格式：{0}", format);

                    System.IO.FileInfo file = new System.IO.FileInfo(openFileDialog1.FileName);
                    label4.Text = String.Format("大小：{0}kb", (file.Length / 1024).ToString());
                    //get small image
                    convConfig.image = img;
                    Size size = new Size(pictureBox1.Width, pictureBox1.Height);
                    Image newImg = convConfig.getThumbnailBySize(size);
                    pictureBox1.Image = newImg;
                    ConvertButton.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("选择的文件有问题啊");
                }
            }
        }

        


        private void ConvertButton_Click(object sender, EventArgs e){
            /*convConfig.sizes.Clear();
            foreach(String str in listBox1.Items){
                string[] sizeStr = str.Split('-');
                if (sizeStr.Length < 2) continue;
                string[] sizeStrs = sizeStr[0].Split('*');
                Size size = new Size(int.Parse(sizeStrs[0]), int.Parse(sizeStrs[1]));
                string name = sizeStr[1];
                SizeConfig sc = new SizeConfig();
                sc.size = size;
                sc.name = name;
                convConfig.sizes.Add(sc);         
            }*/
            convConfig.saveImageToPath();
            MessageBox.Show("导出成功,请慢用");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            convConfig.outputFormat = convConfig.getFormatByString(comboBox1.Items[comboBox1.SelectedIndex].ToString());
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*try
            {
                string selectItem = listBox1.SelectedItem.ToString();
                string[] sizeStr = selectItem.Split('-');
                if (sizeStr.Length < 2) return;
                string[] sizeStrs = sizeStr[0].Split('*');
                numericUpDown1.Value = decimal.Parse(sizeStrs[0]);
                numericUpDown2.Value = decimal.Parse(sizeStrs[1]);
            }
            finally { }*/
        }

        private void button3_Click(object sender, EventArgs e){
            frmAddConfig frmAdd = new frmAddConfig();
            frmAdd.convertConf = convConfig;
            frmAdd.ShowDialog();
        }

        private void listBox1_DoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                int index = this.listBox1.IndexFromPoint(e.Location);
                if (index != System.Windows.Forms.ListBox.NoMatches)
                {
                    curOperaIndex = index;
                    string item = listBox1.Items[index].ToString();
                    SizeConfig sc = convConfig.sizes[index];
                    frmAddConfig frmAdd = new frmAddConfig();
                    frmAdd.removeBlock += new frmAddConfig.RemoveItemBlock(frmAdd_removeBlock);
                    frmAdd.convertConf = convConfig;
                    frmAdd.sizeConf = new SizeConfig(sc.name,sc.size);
                    frmAdd.curItemIndex = index;
                    frmAdd.ShowDialog();
                    //do your stuff here
                }
            }
            catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }
            
        }


        void frmAdd_removeBlock(int index)
        {
            listBox1.Items.RemoveAt(index);
            listBox1.SelectedIndex = 0;
        }

        /// <summary>
        /// interface 
        /// </summary>
        /// <param name="sc"></param>
        public void sizeConfigHasAdd(SizeConfig sc)
        {
            string item = string.Format("{0}*{1}-{2}", sc.size.Width, sc.size.Height,sc.name);
            listBox1.Items.Add(item);
        }

        public void sizeConfigHasUpdate(SizeConfig sc, int changeIndex)
        {
            try
            {
                string item = string.Format("{0}*{1}-{2}", sc.size.Width, sc.size.Height, sc.name);
                listBox1.Items.RemoveAt(changeIndex);
                listBox1.Items.Add(item);
            }
            finally { }
        }

        public void sizeConfigHasDeleted()
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            convConfig.saveConfigToLocal();
        }

    }
}
