using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace telnet_sample
{
    class samplebbs
    {
        static void Main(string[] args)
        {

            //目前不到600行的最簡易telnet bbs瀏覽器實作
            //參考1 http://140.134.131.145/upload/paper_uni/922pdf/4d/922014.pdf 概略觀念..懶得慢慢k rfc
            //直接看人家整理好的東西.我目前很多觀念還是懵懵懂懂...有些是靠Wireshark反推控制原理
            //參考2 http://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library
            //參考2基本上根本不能用的東西,不過概略架構是正確的(但應該不是好方式),本架構核心是來至於這支程式

            //啟動改由GUI介面前導進入
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new loader());
        }
    }
}
