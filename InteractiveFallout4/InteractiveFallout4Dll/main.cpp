#include "Memory.h"
#include <string>
#include <ctime>

#include <algorithm>
#include <string>

//RVA указывающий на на смещение точки входа в консоль игры относительно начала адресов памяти игры
const int consoleRVA = 0x125B380;

//RVA указывающий на адрес в котором лежит указатель на результат выполнения последней консольной команды
const int consoleLastStringRVA = 0x5ABC398;

//__int64 __fastcall sub_14125B380(char *a1)-место входа в консоль игры, в качестве параметра передается строка с командой
typedef INT64(__fastcall * _Console)(char *consoleCommand);
_Console Console;


//=== Получает указатель на выделенную именованную память =====================================================//
// Имя для выделенной памяти
TCHAR SimpleConsoleCommandMemoryName[] = TEXT("Fallout4SimpleConsoleCommand");

TCHAR ResponseGlobalValueMemoryName[] = TEXT("Fallout4ResponseGlobalValue");
TCHAR RequestGlobalValueMemoryName[] = TEXT("Fallout4RequestGlobalValue");

// Создаст, или подключится к уже созданной памяти с таким именем
HANDLE hFileMapSimpleConsoleCommandMemory = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, 512, SimpleConsoleCommandMemoryName);

HANDLE hFileMapResponseGlobalValueMemory = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, 512, ResponseGlobalValueMemoryName);
HANDLE hFileMapRequestGlobalValueMemory = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, 512, RequestGlobalValueMemoryName);


char* GetConsoleLastString(uintptr_t baseGameAddress)
{
	//Достаем из ячейки памяти указатель на адрес последней строки консоли
	INT64* consoleLastStringPTR = (INT64*)(baseGameAddress + consoleLastStringRVA);

	//получаем адрес ячейки памяти с последней строкой ячейки
	INT64 consoleLastStringAddress = *consoleLastStringPTR;

	//смещение (добавляем смещение т.к. после прохождения всех процедур первый символ строки зануляется, из за чего строка в по данному адресу считается пустой, для чего обходим пустые байты и принимаем адрес строки с учетом смещения.)
	int offsetLastStringAddress = 2;

	//Получаем последнюю строку консоли.
	char* consoleLastString = (char*)(consoleLastStringAddress + offsetLastStringAddress);

	//MessageBoxA(NULL, consoleLastString, "Message title", MB_OK);

	return consoleLastString;
	
	
}


// Функция для поиска подстроки в строке
// + поиск позиции, с которой начинается подстрока
int pos(char* s, char* c, int n)
{
	int i, j;		// Счетчики для циклов
	int lenC, lenS;	// Длины строк

	//Находим размеры строки исходника и искомого
	for (lenC = 0; c[lenC]; lenC++);
	for (lenS = 0; s[lenS]; lenS++);

	for (i = 0; i <= lenS - lenC; i++) // Пока есть возможность поиска
	{
		for (j = 0; s[i + j] == c[j]; j++); // Проверяем совпадение посимвольно
		// Если посимвольно совпадает по длине искомого
		// Вернем из функции номер ячейки, откуда начинается совпадение
		// Учитывать 0-терминатор  ( '\0' )
		if (j - lenC == 1 && i == lenS - lenC && !(n - 1)) return i;
		if (j == lenC)
			if (n - 1) n--;
			else return i;
	}
	//Иначе вернем -1 как результат отсутствия подстроки
	return -1;
}

bool CheckConsoleLastString(uintptr_t baseGameAddress)
{
	//Достаем из ячейки памяти указатель на адрес последней строки консоли
	INT64* consoleLastStringPTR = (INT64*)(baseGameAddress + consoleLastStringRVA);

	//получаем адрес ячейки памяти с последней строкой ячейки
	INT64 consoleLastStringAddress = *consoleLastStringPTR;

	//смещение (добавляем смещение т.к. после прохождения всех процедур первый символ строки зануляется, из за чего строка в по данному адресу считается пустой, для чего обходим пустые байты и принимаем адрес строки с учетом смещения.)
	int offsetLastStringAddress = 1;

	//Получаем последнюю строку консоли.
	char* consoleLastString = (char*)(consoleLastStringAddress + offsetLastStringAddress);


	//оставляем только первую строку, зануляя переход на следующую строку
	std::string s = consoleLastString;
	std::replace(s.begin(), s.end(), '\n', '\0');


	size_t len = strlen(s.c_str());
	size_t spn = strcspn(s.c_str(), ">");

	if (spn == len)
	{
		return true;
	}
	else
	{
		return false;
	}
}


DWORD WINAPI MainThread(LPVOID param)
{
	LPVOID SimpleConsoleCommandMemoryAddres = MapViewOfFile(hFileMapSimpleConsoleCommandMemory, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 512);

	LPVOID ResponseGlobalValueMemoryAddres = MapViewOfFile(hFileMapResponseGlobalValueMemory, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 512);
	LPVOID RequestGlobalValueMemoryAddres = MapViewOfFile(hFileMapRequestGlobalValueMemory, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 512);

	uintptr_t baseGameAddress = (uintptr_t)GetModuleHandle(NULL);

	Console = (_Console)(baseGameAddress + consoleRVA);
	
	if (SimpleConsoleCommandMemoryAddres != NULL && ResponseGlobalValueMemoryAddres != NULL && RequestGlobalValueMemoryAddres != NULL)
	{
		//Если на вход подана команда "end" то завершаем бесконечный цикл
		while ((char*)SimpleConsoleCommandMemoryAddres != "end" || (char*)RequestGlobalValueMemoryAddres != "end")
		{
			//Проверяем что строка по указаному адресу не "занулена"
			if (((char*)SimpleConsoleCommandMemoryAddres)[0] != '\0')
			{

				//Отправляем полученную команду в консоль
				Console((char*)SimpleConsoleCommandMemoryAddres);

				//"Обнуляем" строку в FileMemory отправленную в консоль, что бы данная строка не отправлялась в консоль повторно
				((char*)SimpleConsoleCommandMemoryAddres)[0] = '\0';
			}

			if (((char*)RequestGlobalValueMemoryAddres)[0] != '\0')
			{

				//Отправляем полученную команду в консоль
				Console((char*)RequestGlobalValueMemoryAddres);

				//Обнуляем строку с реквестом
				((char*)RequestGlobalValueMemoryAddres)[0] = '\0';

				char* outStr = "";

				time_t Time = time(NULL);

				//Получаем строку с результатом выполнения консольной команды
				while (CheckConsoleLastString(baseGameAddress))
				{
					//По истечению 5 секунд если небыл получен ответ возвращаем сообщение об ошибке
					if (time(NULL) - Time >= 5)
					{
						outStr = "error_globalvalue \0";
						Time = NULL;//обнуляем таймер.
						break;
					}
					else
					{
						outStr = GetConsoleLastString(baseGameAddress);
					}
				}

				memcpy(ResponseGlobalValueMemoryAddres, outStr, strlen(outStr));
			}
		}

		// Закрывает представление
		UnmapViewOfFile(SimpleConsoleCommandMemoryAddres);
		UnmapViewOfFile(RequestGlobalValueMemoryAddres);
		UnmapViewOfFile(ResponseGlobalValueMemoryAddres);

		// Закрывает указатель на память
		CloseHandle(SimpleConsoleCommandMemoryAddres);
		CloseHandle(RequestGlobalValueMemoryAddres);
		CloseHandle(ResponseGlobalValueMemoryAddres);
	}


	//Отключение библиотеки от игры
	FreeLibraryAndExitThread((HMODULE)param, 0);
	
	return 0;
}

//Место входа в библиотеку.
BOOL WINAPI DllMain(HINSTANCE hModule, DWORD dwReason, LPVOID lpReserved)
{
	switch (dwReason)
	{
	case DLL_PROCESS_ATTACH:
		CreateThread(0, 0, MainThread, hModule, 0, 0);

		break;
	default:
		break;
	}
	return TRUE;
}






		 