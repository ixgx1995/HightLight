using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTools
{
    public class HighLight
    {
        /// <summary>
        /// 高亮名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 颜色
        /// </summary>
        public Color NameColor { get; set; }
        /// <summary>
        /// 粗体
        /// </summary>
        public bool IsBold { get; set; }
        /// <summary>
        /// 是否关键字
        /// </summary>
        public bool IsKey { get; set; }
        /// <summary>
        /// 是否分割符
        /// </summary>
        public bool IsDivision { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name">高亮名字</param>
        /// <param name="NameColor">颜色</param>
        /// <param name="IsBold">粗体</param>
        /// <param name="IsKey">是否关键字</param>
        /// <param name="IsDivision">是否分割符</param>
        public HighLight(string Name, Color NameColor, bool IsBold, bool IsKey, bool IsDivision)
        {
            this.Name = Name;
            this.NameColor = NameColor;
            this.IsBold = IsBold;
            this.IsDivision = IsDivision;
            this.IsKey = IsKey;
        }
    }

    public class RichTextBoxHelp
    {
        /// <summary>
        /// 随便加高亮
        /// </summary>
        public static List<HighLight> MatchingList = new List<HighLight>()
        {
            new HighLight("select",Color.Blue,true,true,false),
            new HighLight("insert",Color.Blue,true,true,false),
            new HighLight("update",Color.Blue,true,true,false),
            new HighLight("delete",Color.Blue,true,true,false),
            new HighLight("into",Color.Blue,true,true,false),
            new HighLight("in",Color.Blue,true,true,false),
            new HighLight("set",Color.Blue,true,true,false),
            new HighLight("from",Color.Blue,true,true,false),
            new HighLight("where",Color.Blue,true,true,false),
            new HighLight("values",Color.Blue,true,true,false),
            new HighLight("begin",Color.Blue,true,true,false),
            new HighLight("end",Color.Blue,true,true,false),
            new HighLight("between",Color.Blue,true,true,false),

            new HighLight("is",Color.Blue,true,true,false),
            new HighLight("if",Color.Blue,true,true,false),
            new HighLight("else",Color.Blue,true,true,false),
            new HighLight("and",Color.Blue,true,true,false),
            new HighLight("or",Color.Blue,true,true,false),

            new HighLight("left",Color.Gray,false,true,false),
            new HighLight("join",Color.Gray,false,true,false),
            new HighLight("right",Color.Gray,false,true,false),
            new HighLight("inner",Color.Gray,false,true,false),

            //分隔符也添加进去
            new HighLight("\\*",Color.DarkRed,true,false,true),
            new HighLight("\\(",Color.SkyBlue,false,false,true),
            new HighLight("\\)",Color.SkyBlue,false,false,true),
            new HighLight("\\.",Color.Red,false,false,true),
            new HighLight("\\{",Color.Yellow,false,false,true),
            new HighLight("\\}",Color.Yellow,false,false,true),
            new HighLight("\\[",Color.Yellow,false,false,true),
            new HighLight("\\]",Color.Yellow,false,false,true),
            new HighLight("\\&",Color.Violet,false,false,true),
            new HighLight("\\|",Color.Violet,false,false,true),
            new HighLight("\\;",Color.Violet,false,false,true),
        };

        /// <summary>
        /// 随便加特殊分割符
        /// </summary>
        public static char[] ExcludeList = { ' ', '.', '*', '(', ')', '}', '{', '"', '[', ']', '&', '|', ';', '\n', '\0', '\t', '\r', '\b' };

        RichTextBox _richTextBox;
        Control _control;

        public delegate void SetRichTextDelegate(string text);
        public SetRichTextDelegate SetRichText;

        public RichTextBoxHelp(RichTextBox richTextBox, Control control)
        {
            _richTextBox = richTextBox;
            _control = control;
        }

        #region 行号显示

        /// <summary>
        /// 显示行号
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <param name="icon"></param>
        public void showLineNo(int lineIndex = -1,Icon icon = null)
        {
            if (_control is null)
            {
                MessageBox.Show("无法显示行号！");
                return;
            }

            int foucindex = _richTextBox.SelectionStart;
            int index = _richTextBox.GetFirstCharIndexOfCurrentLine();
            int row = _richTextBox.GetLineFromCharIndex(foucindex);

            //获得当前坐标信息
            Point p = _richTextBox.Location;
            int crntFirstIndex = _richTextBox.GetCharIndexFromPosition(p);

            int crntFirstLine = _richTextBox.GetLineFromCharIndex(crntFirstIndex);

            Point crntFirstPos = _richTextBox.GetPositionFromCharIndex(crntFirstIndex);

            p.Y += _richTextBox.Height;

            int crntLastIndex = _richTextBox.GetCharIndexFromPosition(p);

            int crntLastLine = _richTextBox.GetLineFromCharIndex(crntLastIndex) + (foucindex == index ? 1 : 0);

            Point crntLastPos = _richTextBox.GetPositionFromCharIndex(crntLastIndex + (foucindex == index ? 1 : 0));

            crntLastLine = crntLastPos.Y == crntFirstPos.Y ? 0 : crntLastLine;

            //自动列宽
            _control.Width = ((row + 1).ToString().Length + 4) * Convert.ToInt32((_richTextBox.Font.Size - 2));
            _control.Refresh();

            //准备画图
            Graphics g = _control.CreateGraphics();

            Font font = new Font(_richTextBox.Font, _richTextBox.Font.Style);

            SolidBrush brush = new SolidBrush(Color.Green);

            //画图开始

            //刷新画布

            Rectangle rect = _control.ClientRectangle;
            brush.Color = _control.BackColor;

            g.FillRectangle(brush, 0, 0, _control.ClientRectangle.Width, _control.ClientRectangle.Height);

            brush.Color = Color.Navy;//重置画笔颜色

            //绘制行号

            int lineSpace = 0;

            if (crntFirstLine != crntLastLine)
            {
                lineSpace = (crntLastPos.Y - crntFirstPos.Y) / (crntLastLine - crntFirstLine);

            }

            else
            {
                lineSpace = Convert.ToInt32(_richTextBox.Font.Size);

            }

            int brushY = crntLastPos.Y + Convert.ToInt32(font.Size * 0.21f) ;//惊人的算法啊！！
            for (int i = crntLastLine; i >= crntFirstLine; i--)
            {
                int brushX = _control.ClientRectangle.Width - Convert.ToInt32((font.Size - 2) * (i.ToString().Length
                    + ((i + 1) % Math.Pow(10, i.ToString().Length) == 0 ? 1 : 0) + 1));

                g.DrawString((i + 1).ToString(), font, brush, brushX, brushY);
                //绘制箭头图标
                if (icon != null && lineIndex == i)
                {
                    g.DrawIcon(icon, brushX - Convert.ToInt32((font.Size - 2) * 3), brushY);
                }
                brushY -= lineSpace;
            }

            g.Dispose();

            font.Dispose();

            brush.Dispose();
        }

        #endregion

        #region 关键字高亮

        /// <summary>
        /// 设置字体高亮
        /// </summary>
        /// <param name="_richTextBox"></param>
        public void SetHighLight()
        {
            int foucusIndex = _richTextBox.SelectionStart;//当前光标字符索引
            int lineIndex = _richTextBox.GetLineFromCharIndex(foucusIndex);//当前行索引

            int firstIndex = _richTextBox.GetFirstCharIndexOfCurrentLine();//当前行第一个字符索引

            if (_richTextBox.Lines.Length <= 0) return;
            string line = _richTextBox.Lines[lineIndex];
            List<string> names = line.Split(ExcludeList, StringSplitOptions.RemoveEmptyEntries).ToList();

            //添加关键字进去处理
            string listname = string.Join("|", ExcludeList.Select(c => "\\" + c));
            Regex regex = new Regex(@"(?:" + listname + @")");

            //元素中元素累加索引
            int sumindex = 0;
            //插入集合的索引
            int insertindex = 0;
            foreach (Match item in regex.Matches(line))
            {
                //当前字符起始位置
                int index = item.Index;

                //定位
                for (int i = insertindex; i < names.Count; i++)
                {
                    if (index <= sumindex)
                    {
                        break;
                    }
                    sumindex += names.Count > 0 ? names[insertindex].Length : 0;
                    insertindex++;//当前索引加1 就是加后面
                }

                sumindex += item.Length;
                names.Insert(insertindex, item.Value);
                insertindex++;//当前索引加1 就是加后面
            }

            names.Reverse();

            SendMessage(_richTextBox.Handle, WM_SETREDRAW, 0, IntPtr.Zero);

            foreach (string item in names)
            {
                string text = item.ToLower();

                if (MatchingList.Any(P => P.IsDivision ? P.Name.Replace("\\", "").ToLower().Equals(text) : P.Name.ToLower().Equals(text)))
                {
                    int index = line.LastIndexOf(item);
                    //选中元素
                    _richTextBox.SelectionStart = firstIndex + index;
                    _richTextBox.SelectionLength = text.Length;

                    line = line.Remove(index, item.Length);//防止只对最后一个起效

                    var obj = MatchingList.Where(p => p.IsDivision ? p.Name.ToLower().Replace("\\", "").Equals(text) : p.Name.ToLower().Equals(text)).First();
                    _richTextBox.SelectionFont = new Font("Tahoma", 9, obj.IsBold ?
                         FontStyle.Bold : FontStyle.Bold);
                    _richTextBox.SelectionColor = obj.NameColor;
                }
                else
                {
                    int index = line.LastIndexOf(item);
                    //选中元素
                    _richTextBox.SelectionStart = firstIndex + index;
                    _richTextBox.SelectionLength = item.Length;

                    line = line.Remove(index, item.Length);//防止只对最后一个起效

                    _richTextBox.SelectionFont = new Font("Tahoma", 9, FontStyle.Regular);
                    _richTextBox.SelectionColor = Color.Black;
                }
            }

            _richTextBox.SelectionStart = foucusIndex;
            _richTextBox.SelectionLength = 0;
            _richTextBox.SelectedText = "";

            SendMessage(_richTextBox.Handle, WM_SETREDRAW, 1, IntPtr.Zero);

            _richTextBox.Refresh();
        }

        #endregion

        #region 提示

        /// <summary>
        /// 关键字提示
        /// </summary>
        /// <param name="text">高亮</param>
        public void SetTips(List<string> TreeList)
        {
            Control[] c = _richTextBox.Controls.Find("mylb", false);
            if (c.Length > 0)
                ((ListBox)c[0]).Dispose();  //如果被创建则释放

            string data = "";
            int index = _richTextBox.GetFirstCharIndexOfCurrentLine();
            int starindex = _richTextBox.SelectionStart;
            int row = _richTextBox.GetLineFromCharIndex(starindex);
            if (_richTextBox.Lines.Length <= 0) return;
            string line = _richTextBox.Lines[row];
            for (int i = starindex - index - 1; i >= 0; i--)
            {
                if (ExcludeList.Contains(line[i]))
                {
                    //第一个退格是空格不提示
                    if (i == (starindex - index - 1)) return;
                    break;
                }
                data += line[i].ToString();
            }

            if (string.IsNullOrWhiteSpace(data)) return;

            //反转数据
            char[] arr = data.ToCharArray();
            Array.Reverse(arr);
            data = new string(arr);

            //获得筛选后数据
            IEnumerable<string> fiter = TreeList.Where(p => GetTipsRegex(data, p));
            if (fiter.Count() > 0)
            {
                ListBox lb = new ListBox();
                lb.Name = "mylb";
                foreach (var item in fiter)
                    lb.Items.Add(item);
                lb.Show();
                lb.TabIndex = 10000;
                lb.Location = _richTextBox.GetPositionFromCharIndex(_richTextBox.SelectionStart);
                lb.Left += 10;
                lb.MouseClick += new MouseEventHandler(lb_MouseClick);
                lb.KeyDown += new KeyEventHandler(lb_KeyDown);
                _richTextBox.Controls.Add(lb);
            }
        }

        private void lb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ListBox lb = (sender as ListBox);
                if (lb.SelectedItem != null)
                    SetRichText?.Invoke(lb.SelectedItem.ToString());
            }
        }

        private void lb_MouseClick(object sender, MouseEventArgs e)
        {
            ListBox lb = (sender as ListBox);
            if (lb.SelectedItem != null)
                SetRichText?.Invoke(lb.SelectedItem.ToString());

            //旧的引用
            //_richTextBox.Invoke(new Action(() =>
            //{
            //    _richTextBox.Text = ;
            //}));
        }

        #endregion

        #region 其他方法

        [DllImport("user32")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, IntPtr lParam);
        public const int WM_SETREDRAW = 0xB;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        /// <summary>
        /// 返回字符的正则匹配项
        /// </summary>
        public static List<Match> GetRegex(string value)
        {
            string listname = string.Join("|", MatchingList.Select(_ => _.Name));
            string regularExpression = @"(?i)(?:" + listname + @")(?i)";
            Regex rg = new Regex(regularExpression);
            MatchCollection matchs = rg.Matches(value);
            List<Match> match = new List<Match>();
            foreach (Match item in matchs)
                match.Add(item);
            return match;
        }

        /// <summary>
        /// 返回匹配项目
        /// </summary>
        /// <param name="value"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool GetTipsRegex(string value, string data)
        {
            string regularExpression = @"(?i)^" + value + @"\s*(?i)";
            Regex rg = new Regex(regularExpression);
            return rg.IsMatch(data);
        }

        /// <summary>
        /// 转换Image为Icon
        /// </summary>
        /// <param name="image">要转换为图标的Image对象</param>
        /// <param name="nullTonull">当image为null时是否返回null。false则抛空引用异常</param>
        /// <exception cref="ArgumentNullException" />
        public static Icon ConvertToIcon(Image image, bool nullTonull = false)
        {
            if (image == null)
            {
                if (nullTonull) { return null; }
                throw new ArgumentNullException("image");
            }

            using (MemoryStream msImg = new MemoryStream()
                , msIco = new MemoryStream())
            {
                image.Save(msImg, ImageFormat.Png);

                using (var bin = new BinaryWriter(msIco))
                {
                    //写图标头部
                    bin.Write((short)0);           //0-1保留
                    bin.Write((short)1);           //2-3文件类型。1=图标, 2=光标
                    bin.Write((short)1);           //4-5图像数量（图标可以包含多个图像）

                    bin.Write((byte)image.Width);  //6图标宽度
                    bin.Write((byte)image.Height); //7图标高度
                    bin.Write((byte)0);            //8颜色数（若像素位深>=8，填0。这是显然的，达到8bpp的颜色数最少是256，byte不够表示）
                    bin.Write((byte)0);            //9保留。必须为0
                    bin.Write((short)0);           //10-11调色板
                    bin.Write((short)32);          //12-13位深
                    bin.Write((int)msImg.Length);  //14-17位图数据大小
                    bin.Write(22);                 //18-21位图数据起始字节

                    //写图像数据
                    bin.Write(msImg.ToArray());

                    bin.Flush();
                    bin.Seek(0, SeekOrigin.Begin);
                    return new Icon(msIco);
                }
            }
        }

        /// <summary>
        /// 设置某张图片透明度
        /// </summary>
        /// <param name="src"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static Bitmap img_alpha(Bitmap src, int alpha)
        {
            Bitmap bmp = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int h = 0; h < src.Height; h++)
                for (int w = 0; w < src.Width; w++)
                {
                    Color c = src.GetPixel(w, h);
                    bmp.SetPixel(w, h, Color.FromArgb(alpha, c.R, c.G, c.B));//色彩度最大为255，最小为0
                }
            return bmp;
        }

        #endregion
    }
}
