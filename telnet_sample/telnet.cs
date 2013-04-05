using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using StdioTool; // 有些功能暫時可能需要靠c++ lib & win32api才能處理


namespace telnet_sample
{
    enum Verbs
    {
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,
        IAC = 255
    }

    class minimal_telnet
    {
        public TcpClient tcpSocket;

        public stdio std = new stdio(); //討厭的東西,先替帶著用,最終希望直接靠主程式全部解決
        public Stream stdout = Console.OpenStandardOutput();

        public minimal_telnet(string ip, int port, int size)
        {
            tcpSocket = new TcpClient(ip, port);

            tcpSocket.NoDelay = true;

            Console.WindowWidth = 90;
            Console.WindowHeight = 0x18 + 1; // 多一行來放置console末行輸入法
            Console.Title = "Sample BBS - " + ip;
            char[] style = "細明體".ToCharArray();
            unsafe
            {
                fixed (char* style_p = style)
                {
                    if (size == 0)
                        std.set_font_style(16, 16, style_p);
                    else if (size == 1)
                        std.set_font_style(20, 20, style_p);
                    else if (size == 2)
                        std.set_font_style(24, 24, style_p);
                    else if (size == 3)
                        std.set_font_style(28, 28, style_p);
                    else if (size == 4)
                        std.set_font_style(32, 32, style_p);


                }
            }

            if (!tcpSocket.Connected)
            {
                Console.WriteLine("連接失敗!");
                return;
            }
            //防閒置踢出
            Thread ko = new Thread(avoid_kickout);
            ko.IsBackground = true;
            ko.Start();
            //啟動寫入獨立執行緒
            new Thread(readkey).Start();
        }

        public void avoid_kickout()
        {
            while (true)
            {
                Thread.Sleep(180000);
                tcpSocket.GetStream().WriteByte(0x00);
            }
        }

        //讀取處理thread獨立出來
        public void readkey()
        {
            bool virtualkey = false;
            while ( connect_fails == false )
            {

                if (tcpSocket.Connected == false) return;

                byte keychar;
                keychar = (byte)std.get_char();

                if (keychar == 0xE0) virtualkey = true;

                if (virtualkey == true && keychar == 0x48 && keychar != 0xe0) // 上
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, 0x4f, 0x41 }, 0, 3);
                    virtualkey = false; //結束virtualkey讀取狀態
                    continue;
                }
                else if (virtualkey == true && keychar == 0x50) //下
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, 0x4f, 0x42 }, 0, 3);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && keychar == 0x4b)//左
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, 0x4f, 0x44 }, 0, 3);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && keychar == 0x4d)//右
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, 0x4f, 0x43 }, 0, 3);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && keychar == 0x53)//delete
                {
                    tcpSocket.GetStream().Write(new byte[] { 127 }, 0, 1);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && keychar == 0x47)//home
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, (byte)'[', (byte)'5', (byte)'1' }, 0, 4);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && keychar == 0x49)//pageup
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, (byte)'[', (byte)'2' }, 0, 3);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && keychar == 0x51)//pagedown
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, (byte)'[', (byte)'5', (byte)'~' }, 0, 4);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && keychar == 0x4f)//end
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, (byte)'[', (byte)'4' }, 0, 3);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && keychar == 0x52)//ins
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, (byte)'[', (byte)'2' }, 0, 3);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && keychar != 0xe0) // 0xe0開頭的中文字碼 或是 可能沒處理到的特殊雙碼按鍵
                {
                    virtualkey = false;
                    tcpSocket.GetStream().Write(new byte[] { 0xe0, (byte)keychar }, 0, 2);
                    continue;
                }


                if (keychar != 0xe0 && virtualkey != true && tcpSocket.Connected == true)
                    tcpSocket.GetStream().WriteByte((byte)keychar);



            }
        }

        bool connect_fails = false;
        public void start()
        {
            int revice_byte = 0; ;
            List<byte> revcice_byte_list = new List<byte>();
            do
            {
                do
                {
                    try
                    {
                        revice_byte = tcpSocket.GetStream().ReadByte(); //等待主機新資料送出
                    }
                    catch
                    {
                        Console.WriteLine("\n\n按任意鍵離開 Sample BBS Browser.");
                        connect_fails = true;
                        return;
                    }
                    switch (revice_byte)
                    {
                        case -1:
                            break;

                        case (int)(Verbs.IAC):
                            int inputverb = tcpSocket.GetStream().ReadByte();
                            if (inputverb == -1)
                                break;
                            switch (inputverb)
                            {
                                case (int)Verbs.IAC:
                                    break;
                                case (int)Verbs.DO:
                                case (int)Verbs.DONT:
                                case (int)Verbs.WILL:
                                case 0xfa:
                                case (int)Verbs.WONT:
                                    int inputoption = tcpSocket.GetStream().ReadByte();
                                    if (inputoption == -1) break;
                                    byte res = 0;

                                    if (inputverb == 0xfd) res = 0xfb;
                                    if (inputverb == 0xfb) res = 0xfd;
                                    if (inputverb == 0xfa) res = 0xfa;

                                    if (inputoption == 0)
                                    {
                                        if (inputverb == 0xfb) res = 0xfe;
                                        if (inputverb == 0xfd) res = 0xfc;
                                    }

                                    if (inputverb != 0xfa)
                                    {
                                        tcpSocket.GetStream().Write(new byte[] { (byte)Verbs.IAC, res, (byte)inputoption }, 0, 3);
                                        Thread.Sleep(50);
                                        if (inputoption == 0x1f)
                                        {
                                            tcpSocket.GetStream().Write(new byte[] { 0xff, 0xfa, 0x1f, 0x00, 0x50, 0x00, 0x18, 0xff, 0xf0 }, 0, 9);
                                            Console.WindowWidth = 0x50;
                                        }
                                        Thread.Sleep(50);
                                    }
                                    if (inputverb == 0xfa)
                                    {
                                        tcpSocket.GetStream().Read(new byte[3], 0, 3);
                                        tcpSocket.GetStream().Write(new byte[] { (byte)Verbs.IAC, res, (byte)inputoption, 0x00, 0x56, 0x54, 0x31, 0x30, 0x30, 0xff, 0xf0 }, 0, 11);
                                        Thread.Sleep(50);
                                    }
                                    break;
                                default:
                                    MessageBox.Show("未處理到的指令操作" + inputverb.ToString("X"));
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    revcice_byte_list.Add((byte)revice_byte);
                } while (tcpSocket.Available > 0);

                //-- 列印回傳
                revcice_byte_list.Remove(255); // remove iac
                if (revcice_byte_list.Count != 0) print_asii(revcice_byte_list);
                revcice_byte_list.Clear();
            } while (tcpSocket.Connected);
        }

        //參考 
        //http://www2.gar.no/glinkj/help/cmds/ansa.htm
        //http://www.ibiblio.org/pub/historic-linux/ftp-archives/tsx-11.mit.edu/Oct-07-1996/info/vt102.codes
        //http://www.comptechdoc.org/os/linux/howlinuxworks/linux_hlvt100.html
        //http://www5c.biglobe.ne.jp/~ecb/assembler2/b_2.html
        //http://www.handshake.de/infobase/dfue/prgrmmer/t322.htm
        public bool hilight = false;
        public bool fbchange = false;

        public ConsoleColor fcolorbackup = Console.ForegroundColor;
        public ConsoleColor bcolorbackup = Console.BackgroundColor;
        public void print_asii(List<byte> asii_seq)
        {
            bool cond_code = false;
            List<byte> cond_token = new List<byte>();
            List<byte> byte_str = new List<byte>();
            bool has_SquareBracket = false;


            // MessageBox.Show("s");

            foreach (byte c in asii_seq)
            {
                //判斷是否讀取到控制碼開頭
                if (c == 0x1b) cond_code = true;

                //非控制碼內容,直接記錄到一般字串
                if (cond_code == false)
                {
                    if (c == 0x0a) // need check
                    {
                        if ((Console.CursorTop + 1 - Console.WindowTop) > Console.WindowHeight - 2)
                            Console.WindowTop++;
                        Console.CursorTop++;
                    }
                    if (c == 0x0d) Console.CursorLeft = 0;
                    if (c == 0x08) Console.CursorLeft--;
                    if (c != 0x08 && c != 0x0a && c != 0x0d) stdout.WriteByte(c);
                }

                if (cond_code == true && c != 0x1b)
                {
                    cond_token.Add(c);
                    if (c == '[') has_SquareBracket = true;

                    //讀取到整組控制碼token,進行處理,離開控制碼
                    if (c == 'm')
                    {
                        string token = Encoding.Default.GetString(cond_token.ToArray());
                        if (token == "[;m" || token == "[m")
                        {
                            hilight = false;
                            fbchange = false;
                            Console.ResetColor();
                        }

                        List<string> asii_tokens = new List<string>();
                        token = token.Replace("[", "").Replace("m", "");
                        asii_tokens = token.Split(new char[] { ';' }).ToList();

                        bool exist_clear_attr = false;
                        if (asii_tokens.Contains("") && token != "")
                            exist_clear_attr = true;

                        bool exist_f_attr = false;
                        bool exist_b_attr = false;

                        //色彩控制處理需要再修正
                        foreach (string asii_t in asii_tokens)
                        {
                            switch (asii_t)
                            {
                                case "0":
                                    hilight = false;
                                    fbchange = false;
                                    Console.ResetColor();
                                    break;
                                case "30":
                                    Console.ForegroundColor = ConsoleColor.Black; //DarkGray ; //DarkGray  ;
                                    exist_f_attr = true;
                                    break;
                                case "31":
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    exist_f_attr = true;
                                    break;
                                case "32":
                                    Console.ForegroundColor = ConsoleColor.DarkGreen; // DarkGreen;
                                    exist_f_attr = true;
                                    break;
                                case "33":
                                    Console.ForegroundColor = ConsoleColor.DarkYellow; //Yellow;
                                    exist_f_attr = true;
                                    break;
                                case "34":
                                    Console.ForegroundColor = ConsoleColor.DarkBlue; //DarkBlue; //Blue;
                                    exist_f_attr = true;
                                    break;
                                case "35":
                                    Console.ForegroundColor = ConsoleColor.DarkMagenta; //Magenta;
                                    exist_f_attr = true;
                                    break;
                                case "36":
                                    Console.ForegroundColor = ConsoleColor.DarkCyan; //Cyan;
                                    exist_f_attr = true;
                                    break;
                                case "37":
                                    Console.ForegroundColor = ConsoleColor.White; //白色以淺灰代表
                                    exist_f_attr = true;
                                    break;
                                //背景比前景暗一度
                                case "40":
                                    Console.BackgroundColor = ConsoleColor.Black;
                                    exist_b_attr = true;
                                    break;
                                case "41":
                                    Console.BackgroundColor = ConsoleColor.DarkRed;//ok
                                    exist_b_attr = true;
                                    break;
                                case "42":
                                    Console.BackgroundColor = ConsoleColor.DarkGreen; //ok
                                    exist_b_attr = true;
                                    break;
                                case "43":
                                    Console.BackgroundColor = ConsoleColor.DarkYellow; //ok
                                    exist_b_attr = true;
                                    break;
                                case "44":
                                    Console.BackgroundColor = ConsoleColor.DarkBlue; //ok
                                    exist_b_attr = true;
                                    break;
                                case "45":
                                    Console.BackgroundColor = ConsoleColor.DarkMagenta; //ok
                                    exist_b_attr = true;
                                    break;
                                case "46":
                                    Console.BackgroundColor = ConsoleColor.DarkCyan;//Cyan;
                                    exist_b_attr = true;
                                    break;
                                case "47":
                                    Console.BackgroundColor = ConsoleColor.Gray; //??
                                    exist_b_attr = true;
                                    break;
                            }
                        }

                        if (exist_b_attr && !exist_f_attr && exist_clear_attr)
                        {
                            fbchange = false;
                            hilight = false;
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }

                        else if (!exist_b_attr && exist_f_attr && exist_clear_attr)
                        {
                            fbchange = false;
                            hilight = false;
                            Console.BackgroundColor = ConsoleColor.Black;
                        }

                        if (exist_b_attr && exist_f_attr && exist_clear_attr || asii_tokens.Contains("1")) fbchange = false;
                        if (exist_b_attr && exist_f_attr && exist_clear_attr || asii_tokens.Contains("7")) hilight = false;

                        if (asii_tokens.Contains("1") || hilight == true)//前景加亮
                        {
                            hilight = true;
                            if (Console.ForegroundColor == ConsoleColor.Gray)
                                Console.ForegroundColor = ConsoleColor.White;
                            if (Console.ForegroundColor == ConsoleColor.Black)
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                            else if (Console.ForegroundColor == ConsoleColor.DarkGreen)
                                Console.ForegroundColor = ConsoleColor.Green;
                            else if (Console.ForegroundColor == ConsoleColor.DarkRed)
                                Console.ForegroundColor = ConsoleColor.Red;
                            else if (Console.ForegroundColor == ConsoleColor.DarkMagenta)
                                Console.ForegroundColor = ConsoleColor.Magenta;
                            else if (Console.ForegroundColor == ConsoleColor.DarkYellow)
                                Console.ForegroundColor = ConsoleColor.Yellow;
                            else if (Console.ForegroundColor == ConsoleColor.DarkBlue)
                                Console.ForegroundColor = ConsoleColor.Blue;
                            else if (Console.ForegroundColor == ConsoleColor.DarkCyan)
                                Console.ForegroundColor = ConsoleColor.Cyan;
                        }

                        if (asii_tokens.Contains("7") || fbchange == true) //前後背景色彩交換
                        {
                            fbchange = true;
                            ConsoleColor backup = Console.ForegroundColor;
                            Console.ForegroundColor = Console.BackgroundColor;
                            Console.BackgroundColor = backup;
                        }

                        //---------------------------------------------------------------------------------------------
                        cond_code = false;
                        has_SquareBracket = false;
                        cond_token.Clear();
                    }
                    if (c == 'H' || c == 'f')
                    {
                        string token = Encoding.Default.GetString(cond_token.ToArray());
                        if (token == "[H" || token == "[;H" || token == "[f" || token == "[;f")
                            Console.SetCursorPosition(Console.WindowLeft, Console.WindowTop);
                        else
                        {
                            try
                            {
                                List<string> c_TopRight = new List<string>();
                                token = token.Replace("[", "").Replace("H", "");
                                c_TopRight = token.Split(new char[] { ';' }).ToList();
                                if (int.Parse(c_TopRight[1]) - 1 < 0) c_TopRight[1] = "1";
                                if (int.Parse(c_TopRight[0]) - 1 < 0) c_TopRight[0] = "1";

                                Console.CursorLeft = Console.WindowLeft + int.Parse(c_TopRight[1]) - 1;
                                Console.CursorTop = Console.WindowTop + int.Parse(c_TopRight[0]) - 1;
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show("fail paser in H cond : " + e.Message);
                            }
                        }
                        cond_code = false;
                        has_SquareBracket = false;
                        cond_token.Clear();
                    }
                    if (c == 'J')
                    {
                        string token = Encoding.Default.GetString(cond_token.ToArray());
                        if (token == "[2J")
                            Console.Clear();
                        else
                            MessageBox.Show("尚未處理到的部分 J 控制碼");//沒遇到過...
                        cond_code = false;
                        has_SquareBracket = false;
                        cond_token.Clear();
                    }
                    if (c == 'K')
                    {
                        string token = Encoding.Default.GetString(cond_token.ToArray());
                        if (token == "[K" || token == "[0K")
                        {
                            int org = Console.CursorLeft;
                            for (int th = Console.CursorLeft; th < Console.WindowWidth - 1; th++) //應該正確了
                                Console.Write(" ");
                            Console.CursorLeft = org;
                        }
                        else
                            MessageBox.Show("尚未處理到的部分 K 控制碼"); //好像用不到 沒出現過
                        cond_code = false;
                        has_SquareBracket = false;
                        cond_token.Clear();
                    }
                    if (c == 'r') //ColaBBS 發表編輯或讀取文章會用到的控制屬性  set scroll region ?
                    {
                        string token = Encoding.Default.GetString(cond_token.ToArray());
                        token = token.Replace("[", "").Replace("r", "");
                        List<string> c_TopRight = token.Split(new char[] { ';' }).ToList();

                        if (token == "[;r")
                            MessageBox.Show("unfinish [;r");
                        else
                            MessageBox.Show(token);

                        cond_code = false;
                        has_SquareBracket = false;
                        cond_token.Clear();
                    }
                    if (c == 'D') //ColaBBS 發表編輯或讀取文章會用到的控制屬性 index ??
                    {
                        //&& has_SquareBracket == false
                        MessageBox.Show("unfinish cond D");
                        cond_code = false;
                        has_SquareBracket = false;
                        cond_token.Clear();
                    }
                    //Inser line (<n> lines)  ; Esc  [ <n> L
                    if (c == 'L') //fix 2013.04.04
                    {
                        Console.WindowTop--;
                        Console.CursorTop--;
                        int org = Console.CursorLeft;
                        for (int th = Console.CursorLeft; th < Console.WindowWidth - 1; th++)
                            Console.Write(" ");
                        Console.CursorLeft = org;
                        cond_code = false;
                        has_SquareBracket = false;
                        cond_token.Clear();
                    }
                    if (c == 'M' && has_SquareBracket == false)
                    {
                        if (Console.WindowTop - 1 > 0) Console.WindowTop--;
                        if (Console.CursorTop - 1 > 0) Console.CursorTop--;
                        cond_code = false;
                        has_SquareBracket = false;
                        cond_token.Clear();
                    }
                }
            }
            //if (cond_code == true) MessageBox.Show("沒找到正確控制碼結束對應字元");
        }
    }
}
