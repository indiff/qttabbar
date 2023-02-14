//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2021  Quizo, Paul Accisano
//
//    QTTabBar is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    QTTabBar is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with QTTabBar.  If not, see <http://www.gnu.org/licenses/>.

#include <windows.h>
#include <msi.h>
#include <msiquery.h>
#include <shlobj.h>
#include <exdisp.h>
#include <Shlwapi.h>
#include <psapi.h>
#include <tchar.h>
#include <vector>
#include "../QTHookLib/CComPtr.h"

#define WIXAPI __stdcall

#pragma comment(lib, "msi.lib")
#pragma comment(lib, "shlwapi.lib")
#pragma comment(lib, "psapi.lib")

UINT WM_SHOWHIDEBARS = RegisterWindowMessageA("QTTabBar_ShowHideBars");

struct PairHwndPath {
    HWND hwnd;
    TCHAR path[MAX_PATH];
};

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved) {
	switch (ul_reason_for_call) {
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
    return TRUE;
}

void GetExplorerWindows(std::vector<PairHwndPath>& windows, BOOL needPaths) {
    CComPtr<IShellWindows> psw;
    if(!psw.Create(CLSID_ShellWindows, CLSCTX_ALL)) return;
    VARIANT v;
    V_VT(&v) = VT_I4;
    CComPtr<IDispatch> pdisp;
    for(V_I4(&v) = 0; psw->Item(v, &pdisp) == S_OK; V_I4(&v)++) {
        CComPtr<IWebBrowserApp> pwba;
        if(!pwba.QueryFrom(pdisp)) continue;
        PairHwndPath pair;
        if(!SUCCEEDED(pwba->get_HWND((LONG_PTR*)&pair.hwnd))) continue;
        pair.path[0] = 0;

        CComPtr<IServiceProvider> psp;
        CComPtr<IShellBrowser> psb;
        CComPtr<IShellView> psv;
        CComPtr<IFolderView> pfv;
        CComPtr<IPersistFolder2> ppf2;
        CComPtr<IShellFolder> psf;
        LPITEMIDLIST pidlFolder = NULL;
        LPCITEMIDLIST pidlLast = NULL;
        STRRET strret;
        if(needPaths
                && psp.QueryFrom(pwba)
                && SUCCEEDED(psp->QueryService(SID_STopLevelBrowser, IID_IShellBrowser, (void**)&psb))
                && SUCCEEDED(psb->QueryActiveShellView(&psv))
                && pfv.QueryFrom(psv)
                && SUCCEEDED(pfv->GetFolder(IID_IPersistFolder2, (void**)&ppf2))
                && SUCCEEDED(ppf2->GetCurFolder(&pidlFolder))
                && !SHGetPathFromIDList(pidlFolder, pair.path)
                && SUCCEEDED(SHBindToParent(pidlFolder, IID_IShellFolder, (void**)&psf, &pidlLast))
                && SUCCEEDED(psf->GetDisplayNameOf(pidlLast, 0x8000, &strret))) {
            StrRetToBuf(&strret, pidlLast, pair.path, MAX_PATH);
        }
        if(pidlFolder != NULL) CoTaskMemFree(pidlFolder);
        windows.push_back(pair);
    }
}

UINT WIXAPI HideBars(MSIHANDLE hInstaller) {
    std::vector<PairHwndPath> windows;
    BOOL rollback = MsiGetMode(hInstaller, MSIRUNMODE_ROLLBACK);
    GetExplorerWindows(windows, false);
    for(UINT i = 0; i < windows.size(); ++i) {
        HWND hwnd = GetParent(windows[i].hwnd);
        if(hwnd == 0) hwnd = windows[i].hwnd;
        SendMessage(hwnd, WM_SHOWHIDEBARS, rollback ? 1 : 0, 0);
    }
    return ERROR_SUCCESS;
}

UINT WIXAPI CloseAndReopen(MSIHANDLE hInstaller) {
    std::vector<PairHwndPath> windows;
    BOOL rollback = MsiGetMode(hInstaller, MSIRUNMODE_ROLLBACK);
    GetExplorerWindows(windows, true);
    if(windows.size() == 0) return ERROR_SUCCESS;
    int length = 0;
    for(UINT i = 0; i < windows.size(); ++i) {
        HWND hwnd = GetParent(windows[i].hwnd);
        if(hwnd == 0) hwnd = windows[i].hwnd;
        SendMessage(hwnd, WM_CLOSE, 0, 0);
        int l = _tcslen(windows[i].path);
        if(l > 0 && i > 0) length += l + 1;
    }
    TCHAR* build = new TCHAR[length + 1];
    build[0] = 0;
    for(UINT i = 1; i < windows.size(); ++i) {
        UINT l = _tcslen(windows[i].path);
        _tcscat(build, windows[i].path);
        _tcscat(build, _T(";"));
    }
    HKEY key;
    REGSAM access = KEY_SET_VALUE | KEY_CREATE_SUB_KEY | KEY_WOW64_64KEY;
    if(RegOpenKeyEx(HKEY_CURRENT_USER, _T("Software\\QTTabBar\\"), 0, access, &key) == ERROR_SUCCESS) {
        RegSetValueEx(key, _T("TabsOnLastClosedWindow"), 0, REG_SZ, (LPBYTE)build, length + 1);
    }
    RegCloseKey(key);
    delete[] build;
    ShellExecute(NULL, NULL, windows[0].path, NULL, NULL, SW_SHOWNORMAL);
    return ERROR_SUCCESS;
}


UINT WIXAPI CloseAndReopenAndDeletePlugins(MSIHANDLE hInstaller) {
    std::vector<PairHwndPath> windows;
    BOOL rollback = MsiGetMode(hInstaller, MSIRUNMODE_ROLLBACK);
    GetExplorerWindows(windows, true);
    if(windows.size() == 0) return ERROR_SUCCESS;
    int length = 0;
    for(UINT i = 0; i < windows.size(); ++i) {
        HWND hwnd = GetParent(windows[i].hwnd);
        if(hwnd == 0) hwnd = windows[i].hwnd;
        SendMessage(hwnd, WM_CLOSE, 0, 0);
        int l = _tcslen(windows[i].path);
        if(l > 0 && i > 0) length += l + 1;
    }
    TCHAR* build = new TCHAR[length + 1];
    build[0] = 0;
    for(UINT i = 1; i < windows.size(); ++i) {
        UINT l = _tcslen(windows[i].path);
        _tcscat(build, windows[i].path);
        _tcscat(build, _T(";"));
    }
    HKEY key;
    // REGSAM access = KEY_SET_VALUE | KEY_CREATE_SUB_KEY | KEY_WOW64_64KEY | KEY_DELETE;
    REGSAM access = KEY_ALL_ACCESS;
    if(RegOpenKeyEx(HKEY_CURRENT_USER, _T("Software\\QTTabBar\\"), 0, access, &key) == ERROR_SUCCESS) {
        RegSetValueEx(key, _T("TabsOnLastClosedWindow"), 0, REG_SZ, (LPBYTE)build, length + 1);
		RegDeleteKey(key,_T("Plugins\\Paths"));
    }
    RegCloseKey(key);
    delete[] build;
    ShellExecute(NULL, NULL, windows[0].path, NULL, NULL, SW_SHOWNORMAL);
    return ERROR_SUCCESS;
}



UINT WIXAPI CheckOldVersion(MSIHANDLE hInstaller) {
    HKEY key;
    REGSAM access = KEY_QUERY_VALUE | KEY_WOW64_64KEY;
	// 计算机\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall
    if(RegOpenKeyEx(HKEY_LOCAL_MACHINE, _T("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{DAD20769-75D8-4C1D-80E3-D545563FE9EF}_is1"), 0, access, &key) == ERROR_SUCCESS) {
       MsiSetProperty(hInstaller, _T("OBSOLETEVERSION"), _T("1"));
       RegCloseKey(key);
       return ERROR_SUCCESS;
    }
	
    RegCloseKey(key);
	// 遍历注册表的路径检测老版本 计算机\HKEY_CLASSES_ROOT\Installer\Products
	HKEY hKey = NULL; //保存注册表的句柄 
	DWORD dwIndexs = 0; //需要返回子项的索引 
	TCHAR keyName[MAX_PATH] = { 0 }; //保存子键的名称 
	DWORD charLength = 256;  //想要读取多少字节并返回实际读取到的字符长度
	// auto subKey = _T("SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall");
	auto subKey = _T("Installer\\Products");
	// if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, subKey, 0, KEY_READ, &hKey) == ERROR_SUCCESS)
	if (RegOpenKeyEx(HKEY_CLASSES_ROOT, subKey, 0, KEY_READ, &hKey) == ERROR_SUCCESS)
	{
		while (RegEnumKeyEx(hKey, dwIndexs, keyName, &charLength, NULL, NULL, NULL, NULL) == ERROR_SUCCESS)
		{
			//wprintf(_T("%d : %s\n"), dwIndexs, keyName);
			// char logfilename[MAX_PATH];
			// sprintf(logfilename, "$NTUninstKB%d$.log", unpkinfo.nKBID);
			TCHAR data_Set[500] = { 0 };
			_tcscat(data_Set, _T("Installer\\Products\\"));
			_tcscat(data_Set, keyName);
			HKEY hSubKey;
			TCHAR lpszValue[1024];
			DWORD dwSize = sizeof(lpszValue);
			DWORD dwType = REG_SZ;
			
			if (RegOpenKeyEx(HKEY_CLASSES_ROOT, data_Set, NULL, KEY_READ, &hSubKey) == ERROR_SUCCESS)
			{
				delete data_Set;
				if (RegQueryValueEx(hSubKey, _T("ProductName"), NULL, &dwType, (LPBYTE)&lpszValue, &dwSize) == ERROR_SUCCESS)
				{
					CharLower(lpszValue);
					if (_tcsstr(lpszValue, _T("qttabbar")) )
					{
						RegCloseKey(hSubKey);
						MsiSetProperty(hInstaller, _T("OBSOLETEVERSION"), _T("1"));
						if (hKey != NULL)
						{
							RegCloseKey(hKey);
						}
					    return ERROR_SUCCESS;
					} 
					RegCloseKey(hSubKey);
				}
			}
			++dwIndexs;
			charLength = 256; // 数据必须要重置一下， 不然数据长度会有问题
		}
	}
	if (hKey != NULL)
	{
		RegCloseKey(hKey);
	}
	
    // Check if it's uninstalled, but the user hasn't restarted Explorer yet.
    // Do this by making sure explorer.exe does not have our dll loaded.
    DWORD guess = 1024;
    DWORD* aProcesses;
    DWORD cbNeeded, cProcesses;
    BOOL fFound = false;
    
    // Get a list of processes
    while(true) {
        aProcesses = new DWORD[guess];
        if(!EnumProcesses(aProcesses, sizeof(DWORD) * guess, &cbNeeded)){
            return GetLastError();
        }
        cProcesses = cbNeeded / sizeof(DWORD);
        if(cProcesses < guess) break;
        delete[] aProcesses;
        guess *= 2;
    }

    for(DWORD i = 0; i < cProcesses && !fFound; ++i) {
        TCHAR szProcessName[MAX_PATH] = _T("");
        HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, aProcesses[i]);

        // Get process basename, look for explorer.exe
        if(hProcess != NULL) {
            HMODULE hMod;
            DWORD cbNeeded;
            if(EnumProcessModules(hProcess, &hMod, sizeof(hMod), &cbNeeded)) {
                GetModuleBaseName(hProcess, hMod, szProcessName, sizeof(szProcessName) / sizeof(TCHAR));
            }
        }

        if(_tcscmp(szProcessName, _T("explorer.exe")) == 0) {
            HMODULE* aMods;
            DWORD cMods;
            guess = 1024;

            // Get a list of loaded modules
            while(true) {
                aMods = new HMODULE[guess];
                if(!EnumProcessModules(hProcess, aMods, sizeof(HMODULE) * guess, &cbNeeded)){
                    cMods = 0;
                    break;
                }
                cMods = cbNeeded / sizeof(DWORD);
                if(cMods < guess) break;
                delete[] aMods;
                guess *= 2;
            }
            
            // Look for QTTabBar.dll or QTTabBar.ni.dll (native image)
            for(DWORD j = 0; j < cMods; ++j) {
                TCHAR szModName[MAX_PATH];
                if(GetModuleBaseName(hProcess, aMods[j], szModName, sizeof(szModName) / sizeof(TCHAR))) {
                    if(_tcscmp(szModName, _T("QTTabBar.dll")) == 0 || _tcscmp(szModName, _T("QTTabBar.ni.dll")) == 0) {
                        fFound = true;
                        break;
                    }
                }
            }
            delete[] aMods;
        }
        CloseHandle(hProcess);
    }    
    delete[] aProcesses;
    
    if(fFound) {
        MsiSetProperty(hInstaller, _T("OBSOLETEVERSION"), _T("2"));
    }

    return ERROR_SUCCESS;
}