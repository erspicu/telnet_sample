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
	//http://faq.cprogramming.com/cgi-bin/smartfaq.cgi?answer=1045691686&id=1043284392 getchar win32api實作

	public ref class stdio
	{
	public:
		//可以支援big5中文分成兩個byte輸入
		char get_char()
		{
			return _getch();
		}

		//

		/*bool getChar(TCHAR &ch) 
		{
			bool    ret = false;
			HANDLE  stdIn = GetStdHandle(STD_INPUT_HANDLE);
			DWORD   saveMode;
			GetConsoleMode(stdIn, &saveMode);
			SetConsoleMode(stdIn, ENABLE_PROCESSED_INPUT);
			if (WaitForSingleObject(stdIn, INFINITE) == WAIT_OBJECT_0)
			{
				DWORD num;
				ReadConsole(stdIn, &ch, 1, &num, NULL);
				if (num == 1) ret = true;
			}
			SetConsoleMode(stdIn, saveMode);
			return(ret);
		}
		TCHAR  getChar_native (void)
		{
			TCHAR ch = 0;
			getChar(ch);
			return(ch);
		}*/


		//
	};
}
