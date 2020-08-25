using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace img_slider
{
    public partial class Form1 : Form
    {
        #region variables
        List<ImageInfo> myList = new List<ImageInfo>();
        PictureBox pb1 = new PictureBox();
        public int firstIndex;
        string concat; 
        private int _xPos;
        private int _yPos;
        private bool _dragging;
        int num = 0;
        #endregion
        class ImageInfo // this is the data that needs to be captured
        {
            public double imgSizeX; 
            public double imgSizeY;
            public string imgName;
            public string imgPath;
            public bool isSelected;
            public int imgIndex;
        }
        public Form1(string[] args)
        {
            InitializeComponent();
            if (args.Length > 0)
            {
                int count = 0;
                string openedOne = Reverse(Forward(args[0]));
                foreach (var item in ProcessDirectory(Path.GetDirectoryName(args[0])))
                {
                    File(item, count, openedOne);
                    count++;
                }
                DrawImage(myList, firstIndex);
            }
        }
        private List<string> ProcessDirectory(string dir) // processing directory data(every single image inside the specified directory
        {
            string[] extensions = { "jpg", "gif", "png" };
            List<string> everyIMG = Directory.GetFiles(dir, "*.*")
                .Where(f => extensions.Contains(f.Split('.').Last().ToLower())).ToList();
            return everyIMG;
        }
        public void OpenFile()
        {
            using (OpenFileDialog openfile = new OpenFileDialog(){ 
            Multiselect = false,Filter = "Image Files(*.PNG; *.JPG; *.GIF)| *.PNG; *.JPG; *.GIF"})
            {
                if (openfile.ShowDialog() == DialogResult.OK)
                {
                    pb1.Dispose();
                    myList.Clear();
                    int count = 0;
                    foreach (var item in ProcessDirectory(Path.GetDirectoryName(openfile.FileName)))
                    {
                        File(item,count,openfile.SafeFileName);
                        count++;
                    }
                    DrawImage(myList, firstIndex);
                }
            }
        }
        private string Forward(string item)
        {
            string concat = "";
            for (int i = item.Length - 1; i > 0; i--)
            {
                if (item[i] == '\\')
                    break;
                else
                    concat += item[i];
            }
            return concat;
        }
        private void File(string item,int count, string firstName)
        {
            ImageInfo imgCreate = new ImageInfo();
            concat = Reverse(Forward(item));
            using (FileStream fs = new FileStream(item, FileMode.Open, FileAccess.Read))
            {
                using (Image original = Image.FromStream(fs))
                {
                    var scale = Math.Min((this.Width * 0.85) / original.Width, (this.Height * 0.85) / original.Height);
                    if (scale > 1)
                        scale = 1;
                    imgCreate.imgSizeX = original.Width * scale;
                    imgCreate.imgSizeY = original.Height * scale;
                }
            }
            imgCreate.imgName = concat;
            imgCreate.imgPath = item;
            imgCreate.imgIndex = count;
            if (concat == firstName)
            {
                firstIndex = count;
                imgCreate.isSelected = true;
            }
            else
                imgCreate.isSelected = false;
            myList.Add(imgCreate);
        }
        private void SearchIndex(List<ImageInfo> myList, bool beforeafter)
        {
            for (int i = 0; i < myList.Count; i++)
            {
                if (myList[i].isSelected == true)
                {
                    if (i == myList.Count-1 && beforeafter == true)
                    {
                        myList[i].isSelected = false;
                        myList[0].isSelected = true;
                        DrawImage(myList, 0);
                        break;
                    }
                    else if(i == 0 && beforeafter == false)
                    {
                        myList[i].isSelected = false;
                        myList[myList.Count-1].isSelected = true;
                        DrawImage(myList, myList.Count-1);
                        break;
                    }
                    if (beforeafter == true)
                    {
                        myList[i].isSelected = false;
                        myList[i+1].isSelected = true;
                        DrawImage(myList, i+1);
                        break;
                    }
                    else
                    {
                        myList[i].isSelected = false;
                        myList[i - 1].isSelected = true;
                        DrawImage(myList, i-1);
                        break;
                    }
                }
            }
        }
        
        private void DrawImage(List<ImageInfo> myList,int num)
        {
            this.num = num;
            FileStream fs = null;
            try
            {
                fs = new FileStream(myList[num].imgPath, FileMode.Open, FileAccess.Read);
                pb1 = new PictureBox();
                pb1.MouseUp += (sender, args) =>
                {
                    var c = sender as PictureBox;
                    if (null == c) return;
                    _dragging = false;
                };
                pb1.MouseDown += (sender, args) =>
                {
                    if (args.Button != MouseButtons.Left) return;
                    _dragging = true;
                    _xPos = args.X;
                    _yPos = args.Y;
                };
                pb1.MouseMove += (sender, args) =>
                {
                    var c = sender as PictureBox;
                    if (!_dragging || null == c) return;
                    c.Top = args.Y + c.Top - _yPos;
                    c.Left = args.X + c.Left - _xPos;
                };
                pb1.Image = Image.FromStream(fs);
                pb1.SizeMode = PictureBoxSizeMode.Zoom;
                pb1.Location = new Point((this.Width / 2) - (int)(myList[num].imgSizeX / 2), (this.Height / 2) - (int)(myList[num].imgSizeY / 2));
                pb1.Size = new Size((int)myList[num].imgSizeX, (int)myList[num].imgSizeY);
                this.Controls.Add(pb1);
                pb1.Show();
            }
            finally
            {
                fs.Close();
            }
        }
        protected override void OnMouseWheel(MouseEventArgs ea)
        {
            double ZoomFactor = 1.25;
            if (pb1.Image != null)
            {
                if (ea.Delta > 0)
                {
                    if ((pb1.Width < (1.5 * this.Width)) && (pb1.Height < (1.5 * this.Height)))
                    {
                        pb1.Width = (int)(pb1.Width * ZoomFactor);
                        pb1.Height = (int)(pb1.Height * ZoomFactor);
                        pb1.Top = (int)(ea.Y - ZoomFactor * (ea.Y - pb1.Top));
                        pb1.Left = (int)(ea.X - ZoomFactor * (ea.X - pb1.Left));
                    }
                }
                else
                {
                    if ((pb1.Width > (myList[num].imgSizeX)) && (pb1.Height > (myList[num].imgSizeY)))
                    {
                        pb1.Width = (int)(pb1.Width / ZoomFactor);
                        pb1.Height = (int)(pb1.Height / ZoomFactor);
                        pb1.Top = (int)(ea.Y - 0.80 * (ea.Y - pb1.Top));
                        pb1.Left = (int)(ea.X - 0.80 * (ea.X - pb1.Left));
                    }
                }
            }
        }
        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (pb1 != null)
                pb1.Dispose();
            SearchIndex(myList, true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pb1 != null)
                pb1.Dispose();
           SearchIndex(myList, false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}