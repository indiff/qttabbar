//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2010  Quizo, Paul Accisano
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

#pragma once
#include <Windows.h>

template<class T>
class CComPtr {
public:
    CComPtr() {
        m_ptr = NULL;
    }

    CComPtr(T* ptr) {
        m_ptr = ptr;
        if(m_ptr != NULL) m_ptr->AddRef();
    }

    CComPtr(const CComPtr<T>& lptr) {
        m_ptr = lptr.m_ptr;
        if(m_ptr != NULL) m_ptr->AddRef();
    }

    ~CComPtr() {
        if(m_ptr != NULL) {
            m_ptr->Release();
            m_ptr = NULL;
        }
    }

    operator T*() const {
        return m_ptr;
    }

    T* operator->() const {
        return m_ptr;
    }

    T** operator&() {
        return &m_ptr;
    }

    CComPtr<T>& operator=(const CComPtr<T> lPtr) {
        m_ptr = lPtr.m_ptr;
        if(m_ptr != NULL) m_ptr->AddRef();
        return *this;
    }

    void Attach(T* ptr) {
        if(m_ptr) m_ptr->Release();
        ptr = m_ptr;
    }

    T* Detach() {
        T* ptr = m_ptr;
        m_ptr = NULL;
        return ptr;
    }

    bool IsSameObject(IUnknown* pOther) {
        if(pOther == NULL) return m_ptr == NULL;
        T* ptr;
        if(!SUCCEEDED(pOther->QueryInterface(__uuidof(T), (void**)&ptr))) return false;
        ptr->Release();    
        return ptr == m_ptr;
    }

    bool Implements(REFIID iid) {
        IUnknown* ptr;
        if(SUCCEEDED(m_ptr->QueryInterface(iid, (void**)&ptr))) {
            ptr->Release();
            return true;
        }
        return false;
    }

    template <class U>
    bool Implements() {
        return Implements(__uuidof(U));
    }

    CComPtr<T>& QueryFrom(IUnknown* punk) {
        if(m_ptr != NULL) m_ptr->Release();
        punk->QueryInterface(__uuidof(T), (void**)&m_ptr);
        return *this;
    }
    
    CComPtr<T>& Create(REFCLSID clsid, DWORD context) {
        if(m_ptr != NULL) m_ptr->Release();
        CoCreateInstance(clsid, NULL, context, __uuidof(T), (void**)&m_ptr);
        return *this;
    }

private:
    T* m_ptr;
};
