// QTCpp.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include <iostream>
#include <vector>
#include <ostream>
#include <algorithm>

using namespace std;

class Animal {
private :
	char i;
public:
	inline Animal() {}

	virtual void eat() {}
	virtual void sleep() {}
	virtual void run() {}
 
	virtual ~Animal() {}


	void i_can_fly_away() {}
};
int _tmain(int argc, _TCHAR* argv[])
{
	Animal a ;


	cout << sizeof( a ) << endl;

	cout << sizeof( Animal ) << endl;

	vector<int> v;


	for ( int i = 0; i < 8 ; i++ ) {
		v.push_back( i );
	}

	//for_each( v.begin(); v.end(); );
	return 0;
}

