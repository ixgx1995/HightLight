using DataTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RichTextTest
{
    public partial class Form1 : Form
    {
        RichTextBoxHelp rtbh = null;

        /// <summary>
        /// 提示框数据
        /// </summary>
        List<string> TipsList = new List<string>();

        static bool ThOpen = true;//线程控制标记（隐藏属性有延迟）
        System.Drawing.Icon icon;//箭头图标
        bool ThRunning = false;//是否开启 行号和箭头
        bool ThHighLight = false;//是否开启 高亮
        bool ThTips = false;//是否开启 提示

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TipsList = new List<string>() { "测试数据", "哈喽", "涨工资" };
            icon = RichTextBoxHelp.ConvertToIcon(imageList1.Images[0]);

            rtbh = new RichTextBoxHelp(richTextBox1, panel1) { SetRichText = SetRichText };
            TipsList.AddRange(RichTextBoxHelp.MatchingList.Select(p => p.Name.ToUpper()));
            new Thread(new ThreadStart(HandleFuction)) { IsBackground = true}.Start();

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ThOpen = false;
        }

        private void HandleFuction()
        {
            while (ThOpen)
            {
                //行号箭头
                if (ThRunning)
                {
                    this.Invoke(new Action(() =>
                    {
                        int row = richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart);
                        rtbh.showLineNo(row, icon);
                        ThRunning = false;
                    }));
                }
                //高亮
                if (ThHighLight)
                {
                    this.Invoke(new Action(() =>
                    {
                        rtbh.SetHighLight();
                        ThHighLight = false;
                    }));
                }
                //提示
                if (ThTips)
                {
                    this.Invoke(new Action(() => {
                        rtbh.SetTips(TipsList);
                        ThTips = false;
                    }));
                }
                Thread.Sleep(1);
            }
        }

        private void richTextBox1_MouseDown(object sender, MouseEventArgs e)
        {
            ThRunning = true;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            ThHighLight = true;
            ThRunning = true;
        }

        private void richTextBox1_VScroll(object sender, EventArgs e)
        {
            ThRunning = true;
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            //数据库执行 — 快捷键
            if ((e.Control && e.KeyCode == Keys.F) || e.KeyCode == Keys.F5)
            {

            }

            string data;
            bool adopt = IsLetterNumber(e, out data);
            if ((adopt || e.KeyCode == Keys.Back || e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
                && !(e.Alt || e.Control || e.Shift)
                )//检查是否是数字或字母
            {
                //开启提示
                ThTips = true;
            }

            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                ThRunning = true;
            }

            if (e.KeyCode == Keys.Down)
            {

            }
            else if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                CloseTips();
            }
        }

        #region 其他必须函数

        /// <summary>
        /// 提示框形式追加文本
        /// </summary>
        /// <param name="text"></param>
        public void SetRichText(string text)
        {
            Action<string> actionDelegate = (x) =>
            {
                int foucindex = this.richTextBox1.SelectionStart;
                int index = this.richTextBox1.GetFirstCharIndexOfCurrentLine();
                int row = this.richTextBox1.GetLineFromCharIndex(foucindex);
                int lenght = this.richTextBox1.SelectionLength;
                if (this.richTextBox1.Lines.Length <= 0) return;
                string line = this.richTextBox1.Lines[row];
                int textLenght = 0;
                for (int i = foucindex - index - 1; i >= 0; i--)
                {
                    if (RichTextBoxHelp.ExcludeList.Contains(line[i]))
                        break;
                    textLenght++;
                }
                this.richTextBox1.SelectionStart = foucindex - textLenght;
                this.richTextBox1.SelectionLength = textLenght;
                this.richTextBox1.SelectedText = x.ToString();

                CloseTips();

            };
            this.richTextBox1.Invoke(actionDelegate, text);
        }

        /// <summary>
        /// 关闭提示框
        /// </summary>
        public void CloseTips()
        {
            Control[] c = this.richTextBox1.Controls.Find("mylb", false);
            if (c.Length > 0)
                ((ListBox)c[0]).Dispose();
        }

        /// <summary>
        /// 特殊按键判断
        /// </summary>
        /// <param name="e"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsLetterNumber(KeyEventArgs e, out string data)
        {
            data = "";
            //键盘数字
            if (e.KeyValue > 47 && e.KeyValue < 58)
            {
                data = (e.KeyValue - 48).ToString();
                return true;
            }
            //小键盘数字
            if (e.KeyValue > 95 && e.KeyValue < 106)
            {
                data = (e.KeyValue - 96).ToString();
                return true;
            }
            //字母
            if (e.KeyValue > 64 && e.KeyValue < 91)
            {
                data = e.KeyData.ToString();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 按esc退出
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)

        {
            int WM_KEYDOWN = 256;
            int WM_SYSKEYDOWN = 260;
            if (msg.Msg == WM_KEYDOWN | msg.Msg == WM_SYSKEYDOWN)
            {
                switch (keyData)
                {
                    //Esc关闭窗体
                    case Keys.Escape:
                        this.Close();
                        return false;
                    case Keys.Down:
                        Control[] c = richTextBox1.Controls.Find("mylb", false);
                        if (c.Length > 0 && !c[0].Focused)
                        {
                            ListBox lb = (ListBox)c[0];
                            lb.Focus();
                            lb.SelectedIndex = 0;
                            return true;
                        }
                        break;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        
    }
}
