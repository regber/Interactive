#include "Memory.h"
#include <string>
#include <ctime>

#include <algorithm>
#include <string>

//RVA ����������� �� �� �������� ����� ����� � ������� ���� ������������ ������ ������� ������ ����
const int consoleRVA = 0x125B380;

//RVA ����������� �� ����� � ������� ����� ��������� �� ��������� ���������� ��������� ���������� �������
const int consoleLastStringRVA = 0x5ABC398;

//__int64 __fastcall sub_14125B380(char *a1)-����� ����� � ������� ����, � �������� ��������� ���������� ������ � ��������
typedef INT64(__fastcall * _Console)(char *consoleCommand);
_Console Console;


//=== �������� ��������� �� ���������� ����������� ������ =====================================================//
// ��� ��� ���������� ������
TCHAR SimpleConsoleCommandMemoryName[] = TEXT("Fallout4SimpleConsoleCommand");

TCHAR ResponseGlobalValueMemoryName[] = TEXT("Fallout4ResponseGlobalValue");
TCHAR RequestGlobalValueMemoryName[] = TEXT("Fallout4RequestGlobalValue");

// �������, ��� ����������� � ��� ��������� ������ � ����� ������
HANDLE hFileMapSimpleConsoleCommandMemory = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, 512, SimpleConsoleCommandMemoryName);

HANDLE hFileMapResponseGlobalValueMemory = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, 512, ResponseGlobalValueMemoryName);
HANDLE hFileMapRequestGlobalValueMemory = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, 512, RequestGlobalValueMemoryName);


char* GetConsoleLastString(uintptr_t baseGameAddress)
{
	//������� �� ������ ������ ��������� �� ����� ��������� ������ �������
	INT64* consoleLastStringPTR = (INT64*)(baseGameAddress + consoleLastStringRVA);

	//�������� ����� ������ ������ � ��������� ������� ������
	INT64 consoleLastStringAddress = *consoleLastStringPTR;

	//�������� (��������� �������� �.�. ����� ����������� ���� �������� ������ ������ ������ ����������, �� �� ���� ������ � �� ������� ������ ��������� ������, ��� ���� ������� ������ ����� � ��������� ����� ������ � ������ ��������.)
	int offsetLastStringAddress = 2;

	//�������� ��������� ������ �������.
	char* consoleLastString = (char*)(consoleLastStringAddress + offsetLastStringAddress);

	//MessageBoxA(NULL, consoleLastString, "Message title", MB_OK);

	return consoleLastString;
	
	
}


// ������� ��� ������ ��������� � ������
// + ����� �������, � ������� ���������� ���������
int pos(char* s, char* c, int n)
{
	int i, j;		// �������� ��� ������
	int lenC, lenS;	// ����� �����

	//������� ������� ������ ��������� � ��������
	for (lenC = 0; c[lenC]; lenC++);
	for (lenS = 0; s[lenS]; lenS++);

	for (i = 0; i <= lenS - lenC; i++) // ���� ���� ����������� ������
	{
		for (j = 0; s[i + j] == c[j]; j++); // ��������� ���������� �����������
		// ���� ����������� ��������� �� ����� ��������
		// ������ �� ������� ����� ������, ������ ���������� ����������
		// ��������� 0-����������  ( '\0' )
		if (j - lenC == 1 && i == lenS - lenC && !(n - 1)) return i;
		if (j == lenC)
			if (n - 1) n--;
			else return i;
	}
	//����� ������ -1 ��� ��������� ���������� ���������
	return -1;
}

bool CheckConsoleLastString(uintptr_t baseGameAddress)
{
	//������� �� ������ ������ ��������� �� ����� ��������� ������ �������
	INT64* consoleLastStringPTR = (INT64*)(baseGameAddress + consoleLastStringRVA);

	//�������� ����� ������ ������ � ��������� ������� ������
	INT64 consoleLastStringAddress = *consoleLastStringPTR;

	//�������� (��������� �������� �.�. ����� ����������� ���� �������� ������ ������ ������ ����������, �� �� ���� ������ � �� ������� ������ ��������� ������, ��� ���� ������� ������ ����� � ��������� ����� ������ � ������ ��������.)
	int offsetLastStringAddress = 1;

	//�������� ��������� ������ �������.
	char* consoleLastString = (char*)(consoleLastStringAddress + offsetLastStringAddress);


	//��������� ������ ������ ������, ������� ������� �� ��������� ������
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
		//���� �� ���� ������ ������� "end" �� ��������� ����������� ����
		while ((char*)SimpleConsoleCommandMemoryAddres != "end" || (char*)RequestGlobalValueMemoryAddres != "end")
		{
			//��������� ��� ������ �� ��������� ������ �� "��������"
			if (((char*)SimpleConsoleCommandMemoryAddres)[0] != '\0')
			{

				//���������� ���������� ������� � �������
				Console((char*)SimpleConsoleCommandMemoryAddres);

				//"��������" ������ � FileMemory ������������ � �������, ��� �� ������ ������ �� ������������ � ������� ��������
				((char*)SimpleConsoleCommandMemoryAddres)[0] = '\0';
			}

			if (((char*)RequestGlobalValueMemoryAddres)[0] != '\0')
			{

				//���������� ���������� ������� � �������
				Console((char*)RequestGlobalValueMemoryAddres);

				//�������� ������ � ���������
				((char*)RequestGlobalValueMemoryAddres)[0] = '\0';

				char* outStr = "";

				time_t Time = time(NULL);

				//�������� ������ � ����������� ���������� ���������� �������
				while (CheckConsoleLastString(baseGameAddress))
				{
					//�� ��������� 5 ������ ���� ����� ������� ����� ���������� ��������� �� ������
					if (time(NULL) - Time >= 5)
					{
						outStr = "error_globalvalue \0";
						Time = NULL;//�������� ������.
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

		// ��������� �������������
		UnmapViewOfFile(SimpleConsoleCommandMemoryAddres);
		UnmapViewOfFile(RequestGlobalValueMemoryAddres);
		UnmapViewOfFile(ResponseGlobalValueMemoryAddres);

		// ��������� ��������� �� ������
		CloseHandle(SimpleConsoleCommandMemoryAddres);
		CloseHandle(RequestGlobalValueMemoryAddres);
		CloseHandle(ResponseGlobalValueMemoryAddres);
	}


	//���������� ���������� �� ����
	FreeLibraryAndExitThread((HMODULE)param, 0);
	
	return 0;
}

//����� ����� � ����������.
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






		 