using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using System.IO;
using StdioTool;

namespace telnet_sample
{
    class samplebbs
    {
        static void Main(string[] args)
        {
            //目前不到500行的最簡易telnet bbs瀏覽器實作
            //參考1 http://140.134.131.145/upload/paper_uni/922pdf/4d/922014.pdf 概略觀念..懶得慢慢k rfc
            //直接看人家整理好的東西.我目前很多觀念還是懵懵懂懂...有些是靠Wireshark反推控制原理
            //參考2 http://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library
            //參考2基本上根本不能用的東西,不過概略架構是正確的(但應該不是好方式),本架構核心是來至於這支程式

            string site = "";
            int port = 23;

            if (args.Length == 0)
            {
                Console.WriteLine("連入預設站台 ptt.cc , port 23");
                Thread.Sleep(1000);
                site = "ptt.cc";
            }
            if (args.Length == 1)
            {
                site = args[0];
                try
                {
                    port = int.Parse(args[1]);
                    Console.WriteLine("連入站台 " + site + " , port " + port.ToString());
                    Thread.Sleep(1000);
                }
                catch
                {
                    port = 23;
                    Console.WriteLine("連入站台 " + site + " , port " + port.ToString());
                    Thread.Sleep(1000);
                }
            }

            minimal_telnet mytenlet = new minimal_telnet(site, port);

            try
            {
                mytenlet.start();
            }
            finally
            {
                Console.WriteLine("\n按任意鍵離開.");
            }
        }
    }
}
