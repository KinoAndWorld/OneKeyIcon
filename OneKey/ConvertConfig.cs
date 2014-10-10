using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Xml;

namespace OneKey
{
    
    public class SizeConfig {
        //public ConvertConfig convcConfig;
        public Size size;
        public string name;
        public SizeConfig() { }

        public SizeConfig(string name, Size size) {
            this.size = size;
            this.name = name;
        }
        public SizeConfig(string str)
        {
            string[] sizeStr = str.Split('-');
            //if (sizeStr.Length < 2) 
            string[] sizeStrs = sizeStr[0].Split('*');
            Size size = new Size(int.Parse(sizeStrs[0]), int.Parse(sizeStrs[1]));
            string name = sizeStr[1];

            this.size = size;
            this.name = name;
        }
    }

    public class ConvertConfig
    {
        //public Size size;
        public List<SizeConfig> sizes = new List<SizeConfig>();
        public Image image;
        public String savePath = "";
        public ImageFormat outputFormat;

        public iConfigChange delegateForm;

        public void saveImageToPath()
        {
            if(this.savePath == "") return;
            //检查是否存在这个路径
            /*if (!File.Exists(savePath)){
                MessageBox.Show("路径不存在");
                return;
            }*/
            try
            {
                //get user select format
                //
                foreach (SizeConfig sizeConfig in sizes)
                {
                    Size size = sizeConfig.size;
                    Image smallImage = getThumbnailBySize(size);
                    String newName = savePath + @"\" + size.Width.ToString() + "x" + size.Height.ToString() + "." + getFormatStringByFormat(outputFormat);
                    smallImage.Save(newName, outputFormat);
                }

                //保存当前配置
                //saveConfigToLocal();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public Image getThumbnailBySize(Size size)
        {
            if (image == null) return null;
            return image.GetThumbnailImage(size.Width, size.Height, null, System.IntPtr.Zero);
        }

        public void saveConfigToLocal()
        {
            //bool isExist = File.Exists("Config.xml");
            //if(isExist == false){
            string xmlPath = System.AppDomain.CurrentDomain.BaseDirectory + "config.xml";
            createXmlDoc(xmlPath);
            //}
        }

        /// <summary>
        /// xml handle
        /// </summary>
        private void createXmlDoc(string xmlPath) {

            XElement sizesEle = new XElement("sizes");
            foreach (SizeConfig sizeConfig in sizes)
            {
                Size size = sizeConfig.size;
                string name = sizeConfig.name;
                string field = String.Format("{0}*{1}-{2}", size.Width.ToString(), size.Height.ToString(), name);
                XElement ele = new XElement("size", field);
                sizesEle.Add(ele);
            }

            string outputFormatStr = getFormatStringByFormat(outputFormat);
            var xDoc = new XDocument(new XElement("config",
                new XElement("savePath", savePath),
                new XElement("outputFormat", outputFormatStr),
                new XElement(sizesEle)));

            //需要指定编码格式，否则在读取时会抛：根级别上的数据无效。 第 1 行 位置 1异常
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding(false);
            settings.Indent = true; 
            XmlWriter xw = XmlWriter.Create(xmlPath,settings);
            xDoc.Save(xw);

            xw.Flush();
            xw.Close();
        }

        /// <summary>
        /// load xml config
        /// </summary>
        public List<string> loadLocalConfig() {
            List<string> sizeStrs = new List<string>();
            try
            {
                string xmlPath = System.AppDomain.CurrentDomain.BaseDirectory + "config.xml";
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlPath);

                XmlNode configNode = xmlDocument.SelectSingleNode("config");
                XmlNodeList sizesList = configNode.SelectSingleNode("sizes").ChildNodes;
                foreach (XmlNode list in sizesList)
                {
                    string sizeStr = list.InnerText;
                    SizeConfig sc = new SizeConfig(sizeStr);
                    sizes.Add(sc);
                    sizeStrs.Add(sizeStr);
                }
                //other 
                savePath = configNode.SelectSingleNode("savePath").InnerText;
                string saveFormatStr = configNode.SelectSingleNode("outputFormat").InnerText;
                outputFormat = getFormatByString(saveFormatStr);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }

            return sizeStrs;
        }

        /// <summary>
        /// Add sub Size config
        /// </summary>
        /// <param name="sizeConf"></param>
        public void addSizeConfig(SizeConfig sizeConf) {
            if (sizeConf.name == "" || sizeConf.name.Trim() == "")
            {
                MessageBox.Show("名称不能为空");
                return;
            }

            if (sizeConf.name.Contains('-')) {
                MessageBox.Show("名字不能包含[-]这个符号~我偷一下懒0.0");
                return;
            }

            if (sizeConf.size.Width > 0 &&
                sizeConf.size.Width < 10000 &&
                sizeConf.size.Height > 0 &&
                sizeConf.size.Height < 10000) {
                bool isExist = false;
                string errorString = "";
                foreach (SizeConfig sizeConfig in sizes)
                {
                    Size size = sizeConfig.size;
                    if (size.Width == sizeConf.size.Width && 
                        size.Height == sizeConf.size.Height) {
                            isExist = true;
                            errorString = "这个尺寸已经存在输出配置里面啦";
                            break;
                    }
                    if(sizeConf.name == sizeConfig.name){
                        isExist = true;
                        errorString = "有重名哦";
                        break;
                    }
                }

                if (isExist) MessageBox.Show(errorString);
                else
                {
                    sizes.Add(sizeConf);
                    if (delegateForm is iConfigChange) {
                        MessageBox.Show("添加成功~");
                        delegateForm.sizeConfigHasAdd(sizeConf);
                    }
                }
            }
        }

        /// <summary>
        /// 更新尺寸配置
        /// </summary>
        /// <param name="sizeConf"></param>
        public void updateSizeConfig(SizeConfig sizeConf, int index)
        {
            if (sizeConf.name == "" || sizeConf.name.Trim() == "")
            {
                MessageBox.Show("名称不能为空");
                return;
            }

            if (sizeConf.size.Width > 0 &&
                sizeConf.size.Width < 10000 &&
                sizeConf.size.Height > 0 &&
                sizeConf.size.Height < 10000)
            {
                bool isExist = false;
                string errorString = "";
                int bakIndex = index;
                if (sizes == null || sizes.Count <= index)
                {
                    MessageBox.Show("数据有问题惹 ");
                    return;
                }
                sizes.RemoveAt(index);
                
                foreach (SizeConfig sizeConfig in sizes)
                {
                    Size size = sizeConfig.size;
                    if (size.Width == sizeConf.size.Width &&
                        size.Height == sizeConf.size.Height)
                    {
                        isExist = true;
                        errorString = "这个尺寸已经存在输出配置里面啦";
                        break;
                    }
                    if (sizeConf.name == sizeConfig.name)
                    {
                        isExist = true;
                        errorString = "有重名哦";
                        break;
                    }
                }

                if (isExist) MessageBox.Show(errorString);
                else
                {
                    sizes.Add(sizeConf);
                    if (delegateForm is iConfigChange)
                    {
                        MessageBox.Show("更新成功~");
                        delegateForm.sizeConfigHasUpdate(sizeConf, bakIndex);
                    }
                }
            }
        }

        /// <summary>
        /// 获取图片文件类型
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public String getFormatStringByImage(Image img)
        {
            String format = "";
            if (img.RawFormat.Equals(ImageFormat.Png))
            {
                format = "Png";
            }
            else if (img.RawFormat.Equals(ImageFormat.Jpeg))
            {
                format = "Jpeg";
            }
            else if (img.RawFormat.Equals(ImageFormat.Icon))
            {
                format = "Icon";
            }
            else if (img.RawFormat.Equals(ImageFormat.Bmp))
            {
                format = "Bmp";
            }
            else if (img.RawFormat.Equals(ImageFormat.Gif))
            {
                format = "Gif";
            }
            else
            {
                format = "Other";
            }
            return format;
        }

        public String getFormatStringByFormat(ImageFormat img)
        {
            
            String format = "Png";
            if (img == null) return format;

            if (img.Equals(ImageFormat.Png))
            {
                format = "Png";
            }
            else if (img.Equals(ImageFormat.Jpeg))
            {
                format = "Jpeg";
            }
            else if (img.Equals(ImageFormat.Icon))
            {
                format = "Icon";
            }
            else if (img.Equals(ImageFormat.Bmp))
            {
                format = "Bmp";
            }
            else if (img.Equals(ImageFormat.Gif))
            {
                format = "Gif";
            }
            return format;
        }

        public ImageFormat getFormatByString(string formatString)
        {
            ImageFormat format = ImageFormat.Bmp;
            if (formatString == "Png")
            {
                format = ImageFormat.Png;
            }
            else if (formatString == "Jpeg")
            {
                format = ImageFormat.Jpeg;
            }
            else if (formatString == "Icon")
            {
                format = ImageFormat.Icon;
            }
            else if (formatString == "Gif")
            {
                format = ImageFormat.Gif;
            }
            return format;
        }

    }
}
