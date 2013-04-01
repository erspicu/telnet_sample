using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using StdioTool; // 有些功能需要靠c++ lib & win32api才能處理

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

    enum Options
    {
        SGA = 3
    }

    class minimal_telnet
    {
        public TcpClient tcpSocket;
        public string server_ip = "";
        public int server_port;

        public stdio std = new stdio(); //討厭的東西,先替帶著用,最終希望直接靠主程式全部解決

        public minimal_telnet(string ip, int port)
        {
            server_ip = ip;
            server_port = port;
            tcpSocket = new TcpClient(server_ip, server_port);
            Console.WindowWidth = 90;
            Console.WindowHeight = 0x1A;
            Console.Title = "Sample BBS - " + ip;
            if (!tcpSocket.Connected)
            {
                Console.WriteLine("連接失敗!");
                return;
            }
            //防閒置踢出
            Thread ko = new Thread(avoid_kickout );
            ko.IsBackground = true;
            ko.Start();
            //啟動寫入獨力執行緒
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
            while (true)
            {
                if (tcpSocket.Connected == false)
                    return;

                byte cmd;
                cmd = (byte)std.get_char();

                if (cmd == 0xE0)
                    virtualkey = true;

                if (virtualkey == true && cmd == 0x48 && cmd != 0xe0) // 上
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, 0x4f, 0x41 }, 0, 3);
                    virtualkey = false; //結束virtualkey讀取狀態
                    continue;
                }
                else if (virtualkey == true && cmd == 0x50) //下
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, 0x4f, 0x42 }, 0, 3);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && cmd == 0x4b)//左
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, 0x4f, 0x44 }, 0, 3);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && cmd == 0x4d)//右
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, 0x4f, 0x43 }, 0, 3);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && cmd == 0x53)//delete
                {
                    tcpSocket.GetStream().Write(new byte[] { 127 }, 0, 1);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && cmd == 0x47)//home
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, (byte)'[', (byte)'5', (byte)'1' }, 0, 4);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && cmd == 0x49)//pageup
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, (byte)'[', (byte)'2' }, 0, 3);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && cmd == 0x51)//pagedown
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, (byte)'[', (byte)'5', (byte)'~' }, 0, 4);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && cmd == 0x4f)//end
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, (byte)'[', (byte)'4' }, 0, 3);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && cmd == 0x52)//ins
                {
                    tcpSocket.GetStream().Write(new byte[] { 0x1b, (byte)'[', (byte)'2' }, 0, 3);
                    virtualkey = false;
                    continue;
                }
                else if (virtualkey == true && cmd != 0xe0)
                {
                    virtualkey = false;
                    MessageBox.Show("未處理到的virtualkey");
                    continue;
                }

                if (cmd != 0xe0 && virtualkey != true && tcpSocket.Connected == true)
                    tcpSocket.GetStream().WriteByte((byte)cmd);
            }
        }

        public void start()
        {
            int rc;
            List<byte> rcb = new List<byte>();
            do
            {
                do
                {
                    try
                    {
                        rc = tcpSocket.GetStream().ReadByte();
                    }
                    catch
                    {
                        return;
                    }
                    switch (rc)
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

                                    if (inputverb == 0xfd)
                                        res = 0xfb;

                                    if (inputverb == 0xfb)
                                        res = 0xfd;

                                    if (inputverb == 0xfa)
                                        res = 0xfa;

                                    if (inputoption == 0)
                                    {
                                        if (inputverb == 0xfb)
                                            res = 0xfe;
                                        if (inputverb == 0xfd)
                                            res = 0xfc;
                                    }

                                    if (inputverb != 0xfa)
                                    {

                                        tcpSocket.GetStream().Write(new byte[] { (byte)Verbs.IAC, res, (byte)inputoption }, 0, 3);
                                        Thread.Sleep(50);
                                        if (inputoption == 0x1f)
                                        {
                                            tcpSocket.GetStream().Write(new byte[] { 0xff, 0xfa, 0x1f, 0x00, 0x50, 0x00, 0x18, 0xff, 0xf0 }, 0, 9);
                                            Console.WindowWidth = 0x50;
                                            Console.WindowHeight = 0x18+1;
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
                                    MessageBox.Show("未處理到的指令操作");
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    rcb.Add((byte)rc);
                } while (tcpSocket.Available > 0);

                //-- 列印回傳
                rcb.Remove(255); // remove iac
                if (rcb.Count != 0)
                    print_asii(rcb);
                rcb.Clear();

            } while (tcpSocket.Connected);
        }


        //ref http://www2.gar.no/glinkj/help/cmds/ansa.htm
        public void print_asii(List<byte> asii_seq)
        {
            bool cond_code = false;
            List<byte> cond_token = new List<byte>();
            List<byte> byte_str = new List<byte>();
            bool has_c = false;

            foreach (byte c in asii_seq)
            {
                //判斷是否讀取到控制碼開頭
                if (c == 0x1b)
                    cond_code = true;

                //非控制碼內容,直接記錄到一般字串
                if (cond_code == false)
                {
                    if (c == 0x0a)
                    {
                        int left_org = Console.CursorLeft;
                        Console.CursorTop++;
                        Console.CursorLeft = left_org;
                    }
                    if (c == 0x08)
                        Console.CursorLeft--;

                    if (c != 0x08 && c != 0x0a)
                        std.print_asii((sbyte)c);
                }

                if (cond_code == true && c != 0x1b)
                {
                    cond_token.Add(c);
                    if (c == '[')
                        has_c = true;

                    //讀取到整組控制碼token,進行處理,離開控制碼
                    if (c == 'm')
                    {
                        string token = Encoding.Default.GetString(cond_token.ToArray());

                        if (token == "[;m" || token == "[m")
                            Console.ResetColor();

                        List<string> asii_tokens = new List<string>();
                        token = token.Replace("[", "").Replace("m", "");
                        asii_tokens = token.Split(new char[] { ';' }).ToList();

                        foreach (string asii_t in asii_tokens)
                        {
                            switch (asii_t)
                            {
                                case "0":
                                    Console.ResetColor();
                                    break;
                                case "7":
                                    ConsoleColor ccb = Console.ForegroundColor;
                                    Console.ForegroundColor = Console.BackgroundColor;
                                    Console.BackgroundColor = ccb;
                                    break;
                                case "8": //不顯示 unfinished 
                                    MessageBox.Show("色彩控制8未支援");
                                    break;
                                case "30":
                                    Console.ForegroundColor = ConsoleColor.DarkGray ;
                                    break;
                                case "31":
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    break;
                                case "32":
                                    Console.ForegroundColor = ConsoleColor.Green; // DarkGreen;
                                    break;
                                case "33":
                                    Console.ForegroundColor = ConsoleColor.Yellow; //Yellow;
                                    break;
                                case "34":
                                    Console.ForegroundColor = ConsoleColor.Blue; //DarkBlue; //Blue;
                                    break;
                                case "35":
                                    Console.ForegroundColor = ConsoleColor.Magenta; //Magenta;
                                    break;
                                case "36":
                                    Console.ForegroundColor = ConsoleColor.Cyan; //Cyan;
                                    break;
                                case "37":
                                    Console.ForegroundColor = ConsoleColor.White ; //白色以淺灰代表
                                    break;
                                //背景以暗色為主
                                case "40":
                                    Console.BackgroundColor = ConsoleColor.Black;
                                    break;
                                case "41":
                                    Console.BackgroundColor = ConsoleColor.DarkRed;
                                    break;
                                case "42":
                                    Console.BackgroundColor = ConsoleColor.DarkGreen; //Green;
                                    break;
                                case "43":
                                    Console.BackgroundColor = ConsoleColor.DarkYellow; //Yellow;
                                    break;
                                case "44":
                                    Console.BackgroundColor = ConsoleColor.DarkBlue; //Blue;
                                    break;
                                case "45":
                                    Console.BackgroundColor = ConsoleColor.DarkMagenta; // Magenta;
                                    break;
                                case "46":
                                    Console.BackgroundColor = ConsoleColor.Cyan ;  //Cyan;
                                    break;
                                case "47":
                                    Console.BackgroundColor = ConsoleColor.Gray;
                                    break;
                            }
                        }
                        //---------------------------------------------------------------------------------------------
                        cond_code = false;
                        has_c = false;
                        cond_token.Clear();
                    }
                    if (c == 'H' || c == 'f')
                    {

                    string token = Encoding.Default.GetString(cond_token.ToArray());

                        if (token == "[H" || token == "[;H" || token == "[f" || token == "[;f")
                            Console.SetCursorPosition(0, 0);
                        else
                        {
                            try
                            {
                                List<string> c_TopRight = new List<string>();
                                token = token.Replace("[", "").Replace("H", "");
                                c_TopRight = token.Split(new char[] { ';' }).ToList();
                                if (int.Parse(c_TopRight[1]) - 1 < 0)
                                    c_TopRight[1] = "1";
                                if (int.Parse(c_TopRight[0]) - 1 < 0)
                                    c_TopRight[0] = "1";
                                Console.SetCursorPosition(int.Parse(c_TopRight[1]) - 1, int.Parse(c_TopRight[0]) - 1);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show("fail paser in H cond : " + e.Message);
                            }
                        }

                        cond_code = false;
                        has_c = false;
                        cond_token.Clear();
                    }
                    if (c == 'J')
                    {
                        string token = Encoding.Default.GetString(cond_token.ToArray());

                        if (token == "[J" || token == "[0J")//Erasing Text From cursor to end of screen
                            MessageBox.Show("[J");
                        
                        if (token == "[2J")
                            Console.Clear();
                        if (token == "[1J") // From beginning of screen to cursor
                        {
                            MessageBox.Show("[1J");
                        }
                        cond_code = false;
                        has_c = false;
                        cond_token.Clear();
                    }
                    if (c == 'K')
                    {
                        string token = Encoding.Default.GetString(cond_token.ToArray());
                        if (token == "[K" || token == "[0K") //須要確認
                        {
                            int org = Console.CursorLeft;
                            int ns = Console.WindowWidth - Console.CursorLeft;
                            for (int ii = 0; ii < ns; ii++)
                                Console.Write(" ");
                            Console.CursorLeft = org;
                        }

                        if (token == "[2K")
                        {
                            MessageBox.Show("[2K");
                        }

                        if (token == "[1K")
                        {
                            MessageBox.Show("[1K");
                        }
                        cond_code = false;
                        has_c = false;
                        cond_token.Clear();
                    }
                    if (c == 'r') //發表編輯文章會用到的控制屬性
                    {
                        MessageBox.Show("unfinish cond r");
                        cond_code = false;
                        has_c = false;
                        cond_token.Clear();
                    }
                    //if (c == 'E') MessageBox.Show("unfinish cond D");
                    //if (c == 'D') MessageBox.Show("unfinish cond D");  //似乎不太會出現
                    if (c == 'M') //似乎不太會出現 
                    {
                        MessageBox.Show("unfinish cond M");
                        cond_code = false;
                        has_c = false;
                        cond_token.Clear();
                    }
                    if (c == '7' && has_c == false) //記錄光標位置,與色彩屬性 似乎不太會出現
                        MessageBox.Show("unfinish cond C");
                    if (c == '8' && has_c == false) //恢復光標位置,與色彩屬性 似乎不太會出現
                        MessageBox.Show("unfinish cond C");
                    if (c == 'A') MessageBox.Show("unfinish cond A");
                    if (c == 'B') MessageBox.Show("unfinish cond B");
                    if (c == 'C') MessageBox.Show("unfinish cond C");
                }
            }
            if (cond_code == true)
                MessageBox.Show("沒找到正確控制碼結束對應字元");
        }
    }
}
