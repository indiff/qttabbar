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

using System.Globalization;
using System.Text;

namespace QTTabBarLib {
    internal static class TxtEnc {
        private const int CD_JPN_EUC = 2;
        private const int CD_JPN_ISO2022 = 0;
        private const int CD_JPN_SJIS = 1;
        private const int CD_UTF_16 = 4;
        private const int CD_UTF_32 = 5;
        private const int CD_UTF_7 = 6;
        private const int CD_UTF_8 = 3;
        private const int CDEX_EUC_INFO_EXK = 0x24;
        private const int CDEX_FLG_ERROR = 1;
        private const int CDEX_FLG_FIX = 2;
        private const int CDEX_FLG_INFO = 4;
        private const int CDEX_ISO2022_INFO_ASCII = 0x2004;
        private const int CDEX_ISO2022_INFO_ESC = 0x24;
        private const int CDEX_ISO2022_INFO_JIS0201_1976_K = 0x1004;
        private const int CDEX_ISO2022_INFO_JIS0201_1976_RS1 = 0x4004;
        private const int CDEX_ISO2022_INFO_JIS0201_1976_RS2 = 0x8004;
        private const int CDEX_ISO2022_INFO_JIS0208_1978 = 0x84;
        private const int CDEX_ISO2022_INFO_JIS0208_1983 = 260;
        private const int CDEX_ISO2022_INFO_JIS0208_1990_1 = 0x204;
        private const int CDEX_ISO2022_INFO_JIS0208_1990_2 = 0x10004;
        private const int CDEX_ISO2022_INFO_JIS0213_2000_A1 = 0x404;
        private const int CDEX_ISO2022_INFO_JIS0213_2000_A2 = 0x804;
        private const int CDEX_ISO2022_INFO_SISO = 0x44;
        private const int CDEX_UTF_7_ERROR_BASE64 = 0x21;
        private const int CDEX_UTF_7_FIX_RFC2060 = 0x42;
        private const int CDEX_UTF_8_ERROR_0XC0 = 0x41;
        private const int CDEX_UTF_8_ERROR_DOUBLE = 0x21;
        private const int CDEX_UTF_8_INFO_BOM = 0x84;
        private const int CDEX_UTF_8_INFO_UCS4 = 260;
        private const int CDEX_UTF_INFO_BOM_BE = 0x24;
        private const int CDEX_UTF_INFO_BOM_LE = 0x44;
        private const int NumCode = 7;
        private static bool srcCodeBreak = true;
        private static string srcCodec;
        private static int srcMaxRead = 0x800;

        private static int Chk_JPN_ISO2022(ref byte[] txt, ref int RP, int AS, ref int[] code, ref int[] excode, ref int code2, ref int code3, ref bool[] code4) {
            if((code2 == -1) || (code2 == 0)) {
                if(code4[0]) {
                    return 1;
                }
                if((txt[RP] == 14) || (txt[RP] == 15)) {
                    excode[0] |= 0x44;
                }
                else if(txt[RP] == 0x1b) {
                    if(AS >= (RP + 2)) {
                        if(txt[RP + 1] == 0x24) {
                            if(txt[RP + 2] == 0x40) {
                                code[0]++;
                                excode[0] |= 0x84;
                                code2 = 0;
                                code3 = -2;
                                RP += 3;
                                return 2;
                            }
                            if(txt[RP + 2] == 0x42) {
                                code[0]++;
                                excode[0] |= 260;
                                code2 = 0;
                                code3 = -2;
                                RP += 3;
                                return 2;
                            }
                            if((AS >= (RP + 3)) && (txt[RP + 2] == 40)) {
                                if(txt[RP + 3] == 0x44) {
                                    code[0]++;
                                    excode[0] |= 0x204;
                                    code2 = 0;
                                    code3 = -2;
                                    RP += 4;
                                    return 2;
                                }
                                if(txt[RP + 3] == 0x4f) {
                                    code[0]++;
                                    excode[0] |= 0x404;
                                    code2 = 0;
                                    code3 = -2;
                                    RP += 4;
                                    return 2;
                                }
                                if(txt[RP + 3] == 80) {
                                    code[0]++;
                                    excode[0] |= 0x804;
                                    code2 = 0;
                                    code3 = -2;
                                    RP += 4;
                                    return 2;
                                }
                            }
                        }
                        else if(txt[RP + 1] == 40) {
                            if(txt[RP + 2] == 0x49) {
                                code[0]++;
                                excode[0] |= 0x1004;
                                code2 = 0;
                                code3 = -2;
                                RP += 3;
                                return 2;
                            }
                            if(txt[RP + 2] == 0x42) {
                                code[0]++;
                                excode[0] |= 0x2004;
                                code2 = 0;
                                code3 = -2;
                                RP += 3;
                                return 2;
                            }
                            if(txt[RP + 2] == 0x4a) {
                                code[0]++;
                                excode[0] |= 0x4004;
                                code2 = 0;
                                code3 = -2;
                                RP += 3;
                                return 2;
                            }
                            if(txt[RP + 2] == 0x48) {
                                code[0]++;
                                excode[0] |= 0x8004;
                                code2 = 0;
                                code3 = -2;
                                RP += 3;
                                return 2;
                            }
                        }
                        else if((((AS >= (RP + 5)) && (txt[RP + 1] == 0x26)) && ((txt[RP + 2] == 0x40) && (txt[RP + 3] == 0x1b))) && ((txt[RP + 4] == 0x24) && (txt[RP + 5] == 0x42))) {
                            code[0]++;
                            excode[0] |= 0x10004;
                            code2 = 0;
                            code3 = -2;
                            RP += 6;
                            return 2;
                        }
                    }
                    for(int i = RP + 1; i < (AS + 1); i++) {
                        if((txt[i] > 0x2f) && (txt[i] < 0x7f)) {
                            code[0]++;
                            excode[0] |= 0x24;
                            code2 = 0;
                            code3 = -2;
                            RP += i - RP;
                            return 2;
                        }
                        if((txt[i] < 0x20) || (txt[i] > 0x7e)) {
                            code4[0] = true;
                            return 0;
                        }
                    }
                }
            }
            return 1;
        }

        private static int Chk_JPN_SJIS_EUC(ref byte[] txt, ref int RP, int AS, ref int[] code, ref int[] excode, ref int code2, ref int code3, ref bool[] code4) {
            if(((code2 != -1) && (code2 != 1)) && (code2 != 2)) {
                return 1;
            }
            if(code4[2] && code4[1]) {
                return 1;
            }
            if((((txt[RP] == 0x8e) && ((RP + 1) <= AS)) && ((code2 == 2) || (code2 == -1))) && !code4[2]) {
                if(((txt[RP + 1] > 160) && (txt[RP + 1] < 0xe0)) && (((code[1] == 0) || (code3 != -2)) || (code3 == 2))) {
                    code[2]++;
                    code3 = -2;
                    RP += 2;
                    return 2;
                }
                if((txt[RP + 1] > 0x7f) && (txt[RP + 1] < 0xa1)) {
                    code[1]++;
                    code2 = 1;
                    code3 = -2;
                    RP += 2;
                    return 2;
                }
                if(((txt[RP + 1] > 0x3f) && (txt[RP + 1] < 0x7f)) || ((txt[RP + 1] > 0x7f) && (txt[RP + 1] < 0xfd))) {
                    code[1]++;
                    code3 = -2;
                    RP += 2;
                    return 2;
                }
                code4[2] = true;
                return 0;
            }
            if((((txt[RP] == 0x8f) && ((RP + 2) <= AS)) && ((code2 == 2) || (code2 == -1))) && !code4[2]) {
                if((((txt[RP + 1] > 160) && (txt[RP + 1] < 0xff)) && ((txt[RP + 2] > 160) && (txt[RP + 2] < 0xff))) && (((code[1] == 0) || (code3 != -2)) || (code3 == 2))) {
                    code[2]++;
                    code3 = -2;
                    excode[2] |= 0x24;
                    if(((txt[RP + 1] == 0xfd) || (txt[RP + 1] == 0xfe)) || ((txt[RP + 2] == 0xfd) || (txt[RP + 2] == 0xfe))) {
                        RP += 3;
                        code2 = 2;
                        return 2;
                    }
                    RP += 3;
                    return 2;
                }
                if((txt[RP + 1] > 0x7f) && (txt[RP + 1] < 0xa1)) {
                    code[1]++;
                    code2 = 1;
                    code3 = -2;
                    RP += 2;
                    return 2;
                }
                if(((txt[RP + 1] > 0x3f) && (txt[RP + 1] < 0x7f)) || ((txt[RP + 1] > 0x7f) && (txt[RP + 1] < 0xfd))) {
                    code[1]++;
                    code3 = -2;
                    RP += 2;
                    return 2;
                }
                code4[2] = true;
                return 0;
            }
            if((((txt[RP] > 0x7f) && (txt[RP] < 0xa1)) && ((code2 == 1) || (code2 == -1))) && !code4[1]) {
                code[1]++;
                code2 = 1;
                code3 = -2;
                RP++;
                return 2;
            }
            if(((((txt[RP] > 160) && (txt[RP] < 0xe0)) && ((code2 == -1) || (code2 == 1))) && (((code[2] == 0) || (code3 != -2)) || (code3 == 1))) && !code4[1]) {
                if((RP + 1) <= AS) {
                    if((((txt[RP] == 0xa4) && (txt[RP + 1] > 160)) && (txt[RP + 1] < 0xf4)) || (((txt[RP] == 0xa5) && (txt[RP + 1] > 160)) && ((txt[RP + 1] < 0xf7) && !code4[2]))) {
                        code[2] += 2;
                        code3 = 2;
                        RP += 2;
                        return 2;
                    }
                    if(((txt[RP + 1] > 0xdf) && (txt[RP + 1] < 0xff)) && !code4[2]) {
                        code[2]++;
                        code3 = -2;
                        if(((RP + 2) <= AS) && (((txt[RP + 1] == 0xfd) || (txt[RP + 1] == 0xfe)) || ((txt[RP + 2] == 0xfd) || (txt[RP + 2] == 0xfe)))) {
                            RP += 2;
                            code2 = 2;
                            return 2;
                        }
                        RP++;
                        return 2;
                    }
                    code[1]++;
                    code3 = -2;
                    RP++;
                    return 2;
                }
                code[1]++;
                code3 = -2;
                RP++;
                return 2;
            }
            if((((txt[RP] <= 160) || (txt[RP] >= 0xff)) || ((code2 != 2) && (code2 != -1))) || code4[2]) {
                return 1;
            }
            code[2]++;
            code3 = -2;
            if((txt[RP] == 0xfd) || (txt[RP] == 0xfe)) {
                code2 = 2;
            }
            RP++;
            return 2;
        }

        private static int Chk_UTF_16_32(ref byte[] txt, ref int RP, int AS, ref int[] code, ref int[] excode, ref int code2, ref int code3) {
            if((RP + 1) <= AS) {
                if((((txt[RP] == 0) && (txt[RP + 1] == 0)) && (((RP + 3) <= AS) && (txt[RP + 2] == 0xfe))) && (txt[RP + 3] == 0xff)) {
                    code[5]++;
                    code2 = 5;
                    code3 = -2;
                    excode[5] |= 0x24;
                    RP += 4;
                    return 2;
                }
                if((txt[RP] == 0xfe) && (txt[RP + 1] == 0xff)) {
                    if(((RP > 1) && (txt[RP - 2] == 0)) && (txt[RP - 1] == 0)) {
                        code[5]++;
                        code2 = 5;
                        code3 = -2;
                        excode[5] |= 0x24;
                        RP += 2;
                        return 2;
                    }
                    code[4]++;
                    code2 = 4;
                    code3 = -2;
                    excode[4] |= 0x24;
                    RP += 2;
                    return 2;
                }
                if((txt[RP] == 0xff) && (txt[RP + 1] == 0xfe)) {
                    if((((RP + 3) <= AS) && (txt[RP + 2] == 0)) && (txt[RP + 3] == 0)) {
                        code[5]++;
                        code2 = 5;
                        code3 = -2;
                        excode[5] |= 0x44;
                        RP += 4;
                        return 2;
                    }
                    code[4]++;
                    code2 = 4;
                    code3 = -2;
                    excode[4] |= 0x44;
                    RP += 2;
                    return 2;
                }
            }
            return 1;
        }

        private static int Chk_UTF_7(ref byte[] txt, ref int RP, int AS, ref int[] code, ref int[] excode, ref int code2, ref bool[] code4) {
            if((code2 == -1) || (code2 == 6)) {
                if(code4[6]) {
                    return 1;
                }
                if((RP + 2) <= AS) {
                    int num;
                    if((txt[RP] == 0x2b) && (txt[RP + 1] != 0x2d)) {
                        for(num = RP + 1; num < (AS + 1); num++) {
                            if(((txt[num] == 0x2d) || ((txt[num] >= 0) && (txt[num] < 0x20))) || (txt[num] == 0x7f)) {
                                RP = num + 1;
                                code[6]++;
                                return 2;
                            }
                            if(txt[num] == 0x3d) {
                                excode[6] |= 0x21;
                                if((num + 1) <= AS) {
                                    if(txt[num + 1] == 0x3d) {
                                        if(((num + 2) <= AS) && (((txt[num + 2] == 0x2d) || ((txt[num + 2] >= 0) && (txt[num + 2] < 0x20))) || (txt[num + 2] == 0x7f))) {
                                            RP = num + 3;
                                            code[6]++;
                                            return 2;
                                        }
                                    }
                                    else if(((txt[num + 1] == 0x2d) || ((txt[num + 1] >= 0) && (txt[num + 1] < 0x20))) || (txt[num + 1] == 0x7f)) {
                                        RP = num + 2;
                                        code[6]++;
                                        return 2;
                                    }
                                }
                            }
                            if((((txt[num] != 0x2b) && ((txt[num] <= 0x2e) || (txt[num] >= 0x40))) && ((txt[num] <= 0x40) || (txt[num] >= 0x5b))) && ((txt[num] <= 0x60) || (txt[num] >= 0x7b))) {
                                code4[6] = true;
                                return 0;
                            }
                        }
                        code4[6] = true;
                        return 0;
                    }
                    if(txt[RP] == 0x26) {
                        for(num = RP + 1; num < (AS + 1); num++) {
                            if(((txt[num] == 0x2d) || ((txt[num] >= 0) && (txt[num] < 0x20))) || (txt[num] == 0x7f)) {
                                RP = num + 1;
                                code[6]++;
                                excode[6] |= 0x42;
                                return 2;
                            }
                            if(txt[num] == 0x3d) {
                                excode[6] |= 0x21;
                                if((num + 1) <= AS) {
                                    if(txt[num + 1] == 0x3d) {
                                        if(((num + 2) <= AS) && (((txt[num + 2] == 0x2d) || ((txt[num + 2] >= 0) && (txt[num + 2] < 0x20))) || (txt[num + 2] == 0x7f))) {
                                            RP = num + 3;
                                            code[6]++;
                                            excode[6] |= 0x42;
                                            return 2;
                                        }
                                    }
                                    else if(((txt[num + 1] == 0x2d) || ((txt[num + 1] >= 0) && (txt[num + 1] < 0x20))) || (txt[num + 1] == 0x7f)) {
                                        RP = num + 2;
                                        code[6]++;
                                        excode[6] |= 0x42;
                                        return 2;
                                    }
                                }
                            }
                            if(((((txt[num] != 0x2b) && (txt[num] != 0x2c)) && ((txt[num] <= 0x2f) || (txt[num] >= 0x40))) && ((txt[num] <= 0x40) || (txt[num] >= 0x5b))) && ((txt[num] <= 0x60) || (txt[num] >= 0x7b))) {
                                code4[6] = true;
                                return 0;
                            }
                        }
                        code4[6] = true;
                        return 0;
                    }
                }
            }
            return 1;
        }

        private static int Chk_UTF_8(ref byte[] txt, ref int RP, int AS, ref int[] code, ref int[] excode, ref int code2, ref int code3, ref bool[] code4) {
            if((code2 != -1) && (code2 != 3)) {
                return 1;
            }
            if(code4[3]) {
                return 1;
            }
            int num = 0;
            bool flag = false;
            if((txt[RP] <= 0xbf) || (txt[RP] >= 0xfe)) {
                return 1;
            }
            if((txt[RP] & 0xfc) == 0xfc) {
                num = 5;
                excode[3] |= 260;
            }
            else if((txt[RP] & 0xf8) == 0xf8) {
                num = 4;
                excode[3] |= 260;
            }
            else if((txt[RP] & 240) == 240) {
                num = 3;
            }
            else if((txt[RP] & 0xe0) == 0xe0) {
                num = 2;
                if((((RP + 2) <= AS) && (txt[RP] == 0xef)) && ((txt[RP + 1] == 0xbb) && (txt[RP + 2] == 0xbf))) {
                    excode[3] |= 0x84;
                    code[3] += 3;
                    RP += 3;
                    if(RP == 3) {
                        return 3;
                    }
                    return 2;
                }
            }
            else if((txt[RP] == 0xc0) || (txt[RP] == 0xc1)) {
                excode[3] |= 0x41;
                num = 1;
            }
            else if((txt[RP] & 0xc0) == 0xc0) {
                num = 1;
            }
            if((RP + num) > AS) {
                code4[3] = true;
                return 0;
            }
            if(txt[RP + 1] == txt[RP]) {
                num++;
                if((RP + num) > AS) {
                    code4[3] = true;
                    return 0;
                }
                flag = true;
            }
            for(int i = RP + 1; i <= (RP + num); i++) {
                if((txt[i] <= 0x7f) || (txt[i] >= 0xc0)) {
                    code4[3] = true;
                    return 0;
                }
            }
            if(flag) {
                excode[3] |= 0x21;
                code3 = 3;
            }
            else {
                code[3]++;
                code3 = -2;
            }
            RP += num + 1;
            return 2;
        }

        public static Encoding GetEncoding(ref byte[] txt) {
            int aS = txt.Length - 1;
            int rP = 0;
            int srcMaxRead = TxtEnc.srcMaxRead;
            int[] code = new int[7];
            int[] excode = new int[7];
            int num4 = -1;
            int num5 = -1;
            bool[] flagArray = new bool[7];
            if(aS == -1) {
                return null;
            }
            if(srcMaxRead == 0) {
                srcMaxRead = aS;
            }
            else if(srcMaxRead > aS) {
                srcMaxRead = aS;
            }
            if(Chk_UTF_16_32(ref txt, ref rP, aS, ref code, ref excode, ref num4, ref num5) == 1) {
                while(rP <= srcMaxRead) {
                    int num6 = 0x400;
                    if((txt[rP] == 0x1b) && !flagArray[0]) {
                        num6 = Chk_JPN_ISO2022(ref txt, ref rP, aS, ref code, ref excode, ref num4, ref num5, ref flagArray);
                    }
                    else if(txt[rP] > 0x7f) {
                        if(((txt[rP] > 0xbf) && (txt[rP] < 0xfe)) && !flagArray[3]) {
                            switch(Chk_UTF_8(ref txt, ref rP, aS, ref code, ref excode, ref num4, ref num5, ref flagArray)) {
                                case 0:
                                    num6 = 0;
                                    goto Label_0164;

                                case 1:
                                    num6 = Chk_JPN_SJIS_EUC(ref txt, ref rP, aS, ref code, ref excode, ref num4, ref num5, ref flagArray);
                                    goto Label_0164;

                                case 2:
                                    num6 = 2;
                                    goto Label_0164;

                                case 3:
                                    num6 = 2;
                                    num4 = 3;
                                    goto Label_01BE;
                            }
                        }
                        else if(!flagArray[3]) {
                            flagArray[3] = true;
                            if(code[3] != 0) {
                                num6 = 0;
                            }
                            else {
                                num6 = Chk_JPN_SJIS_EUC(ref txt, ref rP, aS, ref code, ref excode, ref num4, ref num5, ref flagArray);
                            }
                        }
                        else {
                            num6 = Chk_JPN_SJIS_EUC(ref txt, ref rP, aS, ref code, ref excode, ref num4, ref num5, ref flagArray);
                        }
                    }
                    else {
                        num6 = Chk_UTF_7(ref txt, ref rP, aS, ref code, ref excode, ref num4, ref flagArray);
                    }
                Label_0164:
                    switch(num6) {
                        case 0: {
                                rP = 0;
                                code = new int[7];
                                excode = new int[7];
                                num4 = -1;
                                num5 = -1;
                                continue;
                            }
                        case 1: {
                                rP++;
                                continue;
                            }
                        case 2:
                            if(!srcCodeBreak || (num4 == -1)) {
                                continue;
                            }
                            goto Label_01BE;

                        case 0x400:
                            break;

                        default: {
                                continue;
                            }
                    }
                    rP++;
                }
            }
        Label_01BE:
            if(num4 == -1) {
                if(num5 > -1) {
                    num4 = num5;
                }
                else {
                    num4 = 0;
                }
                for(int i = 1; i < 7; i++) {
                    if(code[i] > code[num4]) {
                        num4 = i;
                    }
                }
            }
            switch(num4) {
                case 0:
                    if((excode[0] & 0x1004) != 0x1004) {
                        if((excode[0] & 0x44) == 0x44) {
                            srcCodec = "50222";
                        }
                        else {
                            srcCodec = "iso-2022-jp";
                        }
                        break;
                    }
                    srcCodec = "csISO2022JP";
                    break;

                case 1:
                    srcCodec = "shift_jis";
                    break;

                case 2:
                    if((excode[2] & 0x24) != 0x24) {
                        srcCodec = "euc-jp";
                        break;
                    }
                    srcCodec = "20932";
                    break;

                case 3:
                    srcCodec = "utf-8";
                    break;

                case 4:
                    if((excode[4] & 0x24) != 0x24) {
                        srcCodec = "utf-16";
                        break;
                    }
                    srcCodec = "unicodeFFFE";
                    break;

                case 5:
                    if((excode[5] & 0x24) != 0x24) {
                        srcCodec = "utf-32";
                        break;
                    }
                    srcCodec = "utf-32BE";
                    break;

                case 6:
                    srcCodec = "utf-7";
                    break;

                default:
                    srcCodec = "iso-2022-jp";
                    break;
            }
            try {
                double num8;
                if(num4 == 3) {
                    if((excode[3] & 0x84) == 0x84) {
                        return new UTF8Encoding(true);
                    }
                    return new UTF8Encoding(false);
                }
                if(double.TryParse(srcCodec, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out num8)) {
                    return Encoding.GetEncoding(int.Parse(srcCodec));
                }
                return Encoding.GetEncoding(srcCodec);
            }
            catch {
                return null;
            }
        }
    }
}
