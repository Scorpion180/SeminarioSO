using System;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class CMD : Form
    {
        public CMD()
        {
            InitializeComponent();
        }

        private void enter(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                string aux = "";
                foreach(var linea in console.Lines)
                {
                    aux = linea;
                }
                string[] txt = aux.Split(' ');
                try
                {
                    if (txt.Length == 2 && txt[0] == "rm")
                    {
                        File.Delete(txt[1]);
                    }
                    if (txt.Length == 3 && txt[0] == "cat" && txt[1] == ">")
                    {
                        File.Create(txt[2]).Close();
                    }

                    if (txt.Length == 3 && txt[0] == "mv")
                    {
                        string[] temp = txt[1].Split('\\');
                        File.Move(txt[1], temp[0] + "\\" + temp[1] + "\\" + txt[2]);
                    }

                    if (txt.Length == 3 && txt[0] == "cp")
                    {
                        string[] temp = txt[1].Split('\\');
                        File.Move(txt[1], txt[2]+"\\"+temp[2]);
                    }
                }
                catch (Exception ex)
                {

                }
                
            }

        }
    }
}
