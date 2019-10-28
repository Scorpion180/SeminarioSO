using Google.Apis.Drive.v2;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace WindowsFormsApp1
{
    public partial class GUI : Form
    {
        string copied;
        string name;
        IWavePlayer waveOut;
        AudioFileReader fr;
        bool paused;
        string fileRoute;
        List<PictureBox> casillas;
        int current;
        int fig;
        int state;
        enum figuras { I, J, L, O, Z1, Z2, T };
        bool jugando;
        int[] pos;
        public GUI()
        {
            InitializeComponent();
            PopulateTreeView();
            copied = "$";
            label1.Text = showBattery();
            label2.Text = showTime();
            waveOut = new WaveOut();
            paused = false;
            //wplayer.URL = "C:\\test\\sound.mp3";
            populateTetrisPanel();
            jugando = false;
        }
        #region Explorador de archivos
        private void WindowsIcon_Click(object sender, EventArgs e)
        {
            if (explorador.Visible == true)
            {
                explorador.Visible = false;
            }
            else
            {
                explorador.Visible = true;
            }
        }

        private void CMDIcon_Click(object sender, EventArgs e)
        {
            CMD cmd = new CMD();
            cmd.Show();
        }

        private void PopulateTreeView()
        {
            TreeNode rootNode;
            DirectoryInfo info = new DirectoryInfo(@"C:\");
            if (info.Exists)
            {
                rootNode = new TreeNode(info.Name);
                rootNode.Tag = info;
                GetDirectories(info.GetDirectories(), rootNode);
                treeView1.Nodes.Add(rootNode);
            }
        }

        private void GetDirectories(DirectoryInfo[] subDirs,
            TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                bool isHidden = ((subDir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden);
                if (!isHidden)
                {
                    try
                    {
                        aNode = new TreeNode(subDir.Name, 0, 0);
                        aNode.Tag = subDir;
                        aNode.ImageKey = "folder";
                        subSubDirs = subDir.GetDirectories();
                        if (subSubDirs.Length != 0)
                        {
                            GetDirectories(subSubDirs, aNode);
                        }
                        nodeToAddTo.Nodes.Add(aNode);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }
        void treeView1_NodeMouseClick(object sender,
    TreeNodeMouseClickEventArgs e)
        {
            TreeNode newSelected = e.Node;
            listView1.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            item = new ListViewItem("...", 1);
            subItems = new ListViewItem.ListViewSubItem[]
                { new ListViewItem.ListViewSubItem(item, ""),
             new ListViewItem.ListViewSubItem(item,"")};



            item.SubItems.AddRange(subItems);
            listView1.Items.Add(item);
            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                item = new ListViewItem(dir.Name, 0);
                item.Name = "folder";
                subItems = new ListViewItem.ListViewSubItem[]
                    {new ListViewItem.ListViewSubItem(item, "Directory"),
             new ListViewItem.ListViewSubItem(item,
                dir.LastAccessTime.ToShortDateString())};
                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }
            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                item = new ListViewItem(file.Name, 1);
                subItems = new ListViewItem.ListViewSubItem[]
                    { new ListViewItem.ListViewSubItem(item, "File"),
             new ListViewItem.ListViewSubItem(item,
                file.LastAccessTime.ToShortDateString())};

                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listView1.FocusedItem.Bounds.Contains(e.Location) && listView1.FocusedItem.Name != "folder"
                    && !listView1.FocusedItem.Name.Contains("mp3") && !selectButton.Visible)
                {
                    contextMenuStrip1.Show(Cursor.Position);
                }

            }
        }
        private void nuevo_Click(object sender, EventArgs e)
        {
            string input = "";
            ShowInputDialog(ref input);
            if (input != "")
            {
                File.Create(treeView1.SelectedNode.FullPath + "\\" + input);
                setNewListView();
                input = "";
            }

        }
        private void eliminar_Click(object sender, EventArgs e)
        {
            File.Delete(treeView1.SelectedNode.FullPath + "\\" + listView1.SelectedItems[0].Text);
            setNewListView();
        }

        private void renombrar_Click(object sender, EventArgs e)
        {
            string input = "";
            ShowInputDialog(ref input);
            if (input != "")
            {
                File.Move(treeView1.SelectedNode.FullPath + "\\" + listView1.SelectedItems[0].Text, treeView1.SelectedNode.FullPath + "\\" + input);
                setNewListView();
                input = "";
            }
        }

        private void copiar_click(object sender, EventArgs e)
        {
            copied = treeView1.SelectedNode.FullPath + "\\" + listView1.SelectedItems[0].Text;
            name = listView1.SelectedItems[0].Text;
            pegarToolStripMenuItem.Visible = true;
        }

        private void pegar_Click(object sender, EventArgs e)
        {
            if (copied != "$")
            {
                File.Move(copied, treeView1.SelectedNode.FullPath + "\\" + name);
                name = "";
                copied = "";
                setNewListView();
                pegarToolStripMenuItem.Visible = false;
            }
        }


        private void setNewListView()
        {
            TreeNode aNode = treeView1.SelectedNode;

            TreeNode newSelected = aNode;
            listView1.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            item = new ListViewItem("...", 1);
            subItems = new ListViewItem.ListViewSubItem[]
                { new ListViewItem.ListViewSubItem(item, ""),
             new ListViewItem.ListViewSubItem(item,"")};

            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                item = new ListViewItem(dir.Name, 0);
                subItems = new ListViewItem.ListViewSubItem[]
                    {new ListViewItem.ListViewSubItem(item, "Directory"),
             new ListViewItem.ListViewSubItem(item,
                dir.LastAccessTime.ToShortDateString())};
                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }
            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                item = new ListViewItem(file.Name, 1);
                subItems = new ListViewItem.ListViewSubItem[]
                    { new ListViewItem.ListViewSubItem(item, "File"),
             new ListViewItem.ListViewSubItem(item,
                file.LastAccessTime.ToShortDateString())};

                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
        private static DialogResult ShowInputDialog(ref string input)
        {
            System.Drawing.Size size = new System.Drawing.Size(200, 70);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = "Inserte el nuevo";

            System.Windows.Forms.TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 5);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancelar";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
        }
        #endregion
        #region Bateria y hora

        private string showTime()
        {
            return DateTime.Now.ToLongTimeString();
        }
        private string showBattery()
        {
            try
            {
                string txt;
                PowerStatus status = SystemInformation.PowerStatus;
                txt = status.BatteryLifePercent.ToString();
                string[] temp = txt.Split('.');
                if (Int32.Parse(temp[1]) >= 75)
                    battery.BackgroundImage = imageList2.Images[0];
                else if (Int32.Parse(temp[1]) >= 50)
                    battery.BackgroundImage = imageList2.Images[1];
                else if (Int32.Parse(temp[1]) >= 25)
                    battery.BackgroundImage = imageList2.Images[2];
                else
                    battery.BackgroundImage = imageList2.Images[3];
                battery.BackgroundImageLayout = ImageLayout.Stretch;
                return temp[1] + "%";
            } catch (Exception ex) { return ""; }

        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = showBattery();
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            label2.Text = showTime();
        }

        private void AbrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reproductor.Visible = true;
        }
        #endregion
        #region Reproductor de musica
        private void Play_Click(object sender, EventArgs e)
        {
            if (paused)
            {
                waveOut.Play();
                string[] txt = actual.Text.Split(' ');
                actual.Text = "";
                for (int i = 0; i < txt.Length - 1; i++)
                {
                    actual.Text += txt[i] + " ";
                }
                paused = false;
            }
            else
            {
                fr = new AudioFileReader(treeView1.SelectedNode.FullPath + "\\" + listView1.SelectedItems[0].Text);
                waveOut.Init(fr);
                waveOut.Play();
                actual.Text = listView1.SelectedItems[0].Text;
            }
        }

        private void Pause_Click(object sender, EventArgs e)
        {
            waveOut.Pause();
            paused = true;
            actual.Text += " PAUSADO";
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            waveOut.Stop();
            actual.Text = "";
        }

        private void CloseReproductor_Click(object sender, EventArgs e)
        {
            reproductor.Visible = false;
        }
        #endregion
        #region Manejo de descarga
        delegate void SetControlValueCallback(Control oControl, string propName, object propValue);
        private void SetControlPropertyValue(Control oControl, string propName, object propValue)
        {
            if (oControl.InvokeRequired)
            {
                SetControlValueCallback d = new SetControlValueCallback(SetControlPropertyValue);
                oControl.Invoke(d, new object[] { oControl, propName, propValue });
            }
            else
            {
                Type t = oControl.GetType();
                PropertyInfo[] props = t.GetProperties();
                foreach (PropertyInfo p in props)
                {
                    if (p.Name.ToUpper() == propName.ToUpper())
                    {
                        p.SetValue(oControl, propValue, null);
                    }
                }
            }
        }

        public void download()
        {
            SetControlPropertyValue(progressBar, "value", 0);
            string[] splited = urlTxt.Text.Split('/');
            string downloadUrl = "https://drive.google.com/uc?export=download&id=" + splited[5];
            DriveService service = new DriveService();

            string path = fileRoute + "\\" + fileName.Text;
            SetControlPropertyValue(progressBar, "value", 25);
            var stream = service.HttpClient.GetStreamAsync(downloadUrl);
            SetControlPropertyValue(progressBar, "value", 50);
            var result = stream.Result;
            SetControlPropertyValue(progressBar, "value", 75);
            using (var fileStream = File.Create(path))
            {
                result.CopyTo(fileStream);
            }
            SetControlPropertyValue(progressBar, "value", 100);
            //pathLabel.Text = "";
            //urlTxt.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(download);
            thr.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            selectButton.Visible = true;
            if (explorador.Visible == true)
            {
                explorador.Visible = false;
            }
            else
            {
                explorador.Visible = true;
            }
        }

        private void selectButton_Click(object sender, EventArgs e)
        {
            pathLabel.Text = "";
            selectButton.Visible = false;
            fileRoute = treeView1.SelectedNode.FullPath;
            pathLabel.Text = fileRoute;
            explorador.Visible = false;
        }
        #endregion
        #region Tetris
        private void populateTetrisPanel()
        {
            casillas = new List<PictureBox>();
            for (int i = 0; i < 200; i++)
            {
                PictureBox pb = new PictureBox();
                pb.Size = new Size(23, 23);
                pb.BackColor = Color.Black;
                pb.Margin = new Padding(1, 1, 1, 1);
                tetrisPanel.Controls.Add(pb);
            }
        }
        private void dropFigure()
        {

            switch (fig)
            {
                case (int)figuras.I:
                    current = (int)figuras.I;
                    state = 0;
                    dropI();
                    break;
                case (int)figuras.J:
                    current = (int)figuras.J;
                    state = 0;
                    dropJ();
                    break;
                case (int)figuras.L:
                    current = (int)figuras.L;
                    state = 0;
                    dropL();
                    break;
                case (int)figuras.O:
                    current = (int)figuras.O;
                    state = 0;
                    dropO();
                    break;
                case (int)figuras.T:
                    current = (int)figuras.T;
                    state = 0;
                    dropT();
                    break;
                case (int)figuras.Z1:
                    current = (int)figuras.Z1;
                    state = 0;
                    dropZ1();
                    break;
                case (int)figuras.Z2:
                    current = (int)figuras.Z2;
                    state = 0;
                    dropZ2();
                    break;
            }
        }
        private void dropI()
        {
            pos[0] = 4;
            pos[1] = 14;
            pos[2] = 24;
            pos[3] = 34;
            tetrisPanel.Controls[pos[0]].BackColor = Color.Blue;
            tetrisPanel.Controls[pos[1]].BackColor = Color.Blue;
            tetrisPanel.Controls[pos[2]].BackColor = Color.Blue;
            tetrisPanel.Controls[pos[3]].BackColor = Color.Blue;
        }
        private void dropJ()
        {
            tetrisPanel.Controls[4].BackColor = Color.Blue;
            tetrisPanel.Controls[14].BackColor = Color.Blue;
            tetrisPanel.Controls[24].BackColor = Color.Blue;
            tetrisPanel.Controls[23].BackColor = Color.Blue;
        }
        private void dropL()
        {
            tetrisPanel.Controls[4].BackColor = Color.Blue;
            tetrisPanel.Controls[14].BackColor = Color.Blue;
            tetrisPanel.Controls[24].BackColor = Color.Blue;
            tetrisPanel.Controls[25].BackColor = Color.Blue;
        }
        private void dropO()
        {
            tetrisPanel.Controls[4].BackColor = Color.Blue;
            tetrisPanel.Controls[5].BackColor = Color.Blue;
            tetrisPanel.Controls[14].BackColor = Color.Blue;
            tetrisPanel.Controls[15].BackColor = Color.Blue;
        }
        private void dropT()
        {
            tetrisPanel.Controls[4].BackColor = Color.Blue;
            tetrisPanel.Controls[5].BackColor = Color.Blue;
            tetrisPanel.Controls[6].BackColor = Color.Blue;
            tetrisPanel.Controls[15].BackColor = Color.Blue;
        }
        private void dropZ1()
        {
            tetrisPanel.Controls[4].BackColor = Color.Blue;
            tetrisPanel.Controls[5].BackColor = Color.Blue;
            tetrisPanel.Controls[15].BackColor = Color.Blue;
            tetrisPanel.Controls[16].BackColor = Color.Blue;
        }
        private void dropZ2()
        {
            tetrisPanel.Controls[4].BackColor = Color.Blue;
            tetrisPanel.Controls[5].BackColor = Color.Blue;
            tetrisPanel.Controls[14].BackColor = Color.Blue;
            tetrisPanel.Controls[13].BackColor = Color.Blue;
        }

        private void playTetris_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            //fig = rnd.Next(0, 6);
            fig = 0;
            Thread crear = new Thread(dropFigure);
            jugando = true;
            pos = new int[4];
            crear.Start();
        }
        //--->
        private void arrowDerI(){
            tetrisPanel.Controls[pos[0]].BackColor = Color.Black;
            tetrisPanel.Controls[pos[1]].BackColor = Color.Black;
            tetrisPanel.Controls[pos[2]].BackColor = Color.Black;
            tetrisPanel.Controls[pos[3]].BackColor = Color.Black;
            pos[0] = 4;
            pos[1] = 5;
            pos[2] = 6;
            pos[3] = 7;
            tetrisPanel.Controls[pos[0]].BackColor = Color.Blue;
            tetrisPanel.Controls[pos[1]].BackColor = Color.Blue;
            tetrisPanel.Controls[pos[2]].BackColor = Color.Blue;
            tetrisPanel.Controls[pos[3]].BackColor = Color.Blue;
        }
        //Abajo
        private void arrowDownI()
        {
            tetrisPanel.Controls[pos[0]].BackColor = Color.Black;
            tetrisPanel.Controls[pos[1]].BackColor = Color.Black;
            tetrisPanel.Controls[pos[2]].BackColor = Color.Black;
            tetrisPanel.Controls[pos[3]].BackColor = Color.Black;
            pos[0] = 4;
            pos[1] = 14;
            pos[2] = 24;
            pos[3] = 34;
            tetrisPanel.Controls[pos[0]].BackColor = Color.Blue;
            tetrisPanel.Controls[pos[1]].BackColor = Color.Blue;
            tetrisPanel.Controls[pos[2]].BackColor = Color.Blue;
            tetrisPanel.Controls[pos[3]].BackColor = Color.Blue;
        }
        //<-----
        private void arrowIzqI()
        {
            tetrisPanel.Controls[pos[0]].BackColor = Color.Black;
            tetrisPanel.Controls[pos[1]].BackColor = Color.Black;
            tetrisPanel.Controls[pos[2]].BackColor = Color.Black;
            tetrisPanel.Controls[pos[3]].BackColor = Color.Black;
            pos[0] = 4;
            pos[1] = 5;
            pos[2] = 6;
            pos[3] = 7;
            tetrisPanel.Controls[pos[0]].BackColor = Color.Blue;
            tetrisPanel.Controls[pos[1]].BackColor = Color.Blue;
            tetrisPanel.Controls[pos[2]].BackColor = Color.Blue;
            tetrisPanel.Controls[pos[3]].BackColor = Color.Blue;
        }
        private void arrowUpI()
        {
            tetrisPanel.Controls[pos[0]].BackColor = Color.Black;
            tetrisPanel.Controls[pos[1]].BackColor = Color.Black;
            tetrisPanel.Controls[pos[2]].BackColor = Color.Black;
            tetrisPanel.Controls[pos[3]].BackColor = Color.Black;
            pos[0] = 4;
            pos[1] = 5;
            pos[2] = 6;
            pos[3] = 7;
            tetrisPanel.Controls[pos[0]].BackColor = Color.Blue;
            tetrisPanel.Controls[pos[1]].BackColor = Color.Blue;
            tetrisPanel.Controls[pos[2]].BackColor = Color.Blue;
            tetrisPanel.Controls[pos[3]].BackColor = Color.Blue;
        }
        private void tetrisPanel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
           
        }
        private void girarFigura()
        {
            switch (current)
            {
                case (int)figuras.I:
                    switch (state)
                    {
                        case 0:
                            arrowDerI();
                            state = 1;
                            break;
                        case 1:
                            arrowDownI();
                            state = 2;
                            break;
                        case 2:
                            arrowIzqI();
                            state = 3;
                            break;
                        case 3:
                            arrowUpI();
                            state = 0;
                            break;
                    }
                    break;
                case (int)figuras.J:
                    break;
                case (int)figuras.L:
                    break;
                case (int)figuras.O:
                    break;
                case (int)figuras.T:
                    break;
                case (int)figuras.Z1:
                    break;
                case (int)figuras.Z2:
                    break;
            }
        }
        #endregion

        private void GUI_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (jugando)
            {
                switch (e.KeyCode)
                {
                    case Keys.C:
                        girarFigura();
                        break;
                }
            }
        }

        private void selectButton_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.C || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                e.Handled = true;
            }
        }

        private void urlTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.C || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                e.Handled = true;
            }
        }

        private void fileName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.C || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                e.Handled = true;
            }
        }

        private void button2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.C || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                e.Handled = true;
            }
        }

        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.C || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                e.Handled = true;
            }
        }

        private void playTetris_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.C || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                e.Handled = true;
            }
        }
    }
}
