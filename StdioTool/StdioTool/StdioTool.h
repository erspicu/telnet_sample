// StdioTool.h
#pragma once
#include "Stdafx.h"
#include <conio.h>
//#include <windows.h>

using namespace System;

namespace StdioTool 
{
	
	//http://social.msdn.microsoft.com/Forums/en-US/vcgeneral/thread/192c888a-2994-48aa-bb17-ec95f03535b0
	//http://broadcast.oreilly.com/2010/08/understanding-c-text-mode-games.html
	//http://msdn.microsoft.com/zh-tw/library/windows/desktop/ms682073(v=vs.85).aspx
	public ref class stdio
	{
	public:
		//可以支援big5中文分成兩個byte輸入
		char get_char()
		{
			return _getch();
		}
	};
}
