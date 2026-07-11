using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuizoPlugins
{
    class ChineseCalendar
    {
        #region ChineseCalendarException
        /// <summary>
        /// Chinese calendar exception handling
        /// </summary>
        public class ChineseCalendarException : System.Exception
        {
            public ChineseCalendarException(string msg)
                : base(msg)
            {
            }
        }



        #endregion
        #region Internal structures
        private struct SolarHolidayStruct
        {
            public int Month;
            public int Day;
            public int Recess; //Holiday length
            public string HolidayName;
            public SolarHolidayStruct(int month, int day, int recess, string name)
            {
                Month = month;
                Day = day;
                Recess = recess;
                HolidayName = name;
            }
        }



        private struct LunarHolidayStruct
        {
            public int Month;
            public int Day;
            public int Recess;
            public string HolidayName;



            public LunarHolidayStruct(int month, int day, int recess, string name)
            {
                Month = month;
                Day = day;
                Recess = recess;
                HolidayName = name;
            }
        }



        private struct WeekHolidayStruct
        {
            public int Month;
            public int WeekAtMonth;
            public int WeekDay;
            public string HolidayName;



            public WeekHolidayStruct(int month, int weekAtMonth, int weekDay, string name)
            {
                Month = month;
                WeekAtMonth = weekAtMonth;
                WeekDay = weekDay;
                HolidayName = name;
            }
        }
        #endregion



        #region Internal variables
        private DateTime _date;



        private int _cYear;
        private int _cMonth;
        private int _cDay;
        private bool _cIsLeapMonth; //whether the current month is a leap month
        private bool _cIsLeapYear; //whether the current year has a leap month
        #endregion



        #region Base data
        #region Basic constants
        private const int MinYear = 1900;
        private const int MaxYear = 2050;
        private static DateTime MinDay = new DateTime(1900, 1, 30);
        private static DateTime MaxDay = new DateTime(2049, 12, 31);
        private const int GanZhiStartYear = 1864; //Ganzhi calculation start year
        private static DateTime GanZhiStartDay = new DateTime(1899, 12, 22);//start day
        private const string HZNum = "○一二三四五六七八九";
        private const int AnimalStartYear = 1900; //1900 is the Year of the Rat
        #endregion



        #region Lunar calendar data
        /// <summary>
        /// Lunar calendar data sourced from the internet
        /// </summary>
        /// <remarks>
        /// Data structure as follows, using 17 digits total
        /// Digit 17: leap month day count, 0 = 29 days, 1 = 30 days
        /// Digits 16-5 (12 digits) represent the 12 months; digit 16 is month 1 - 1 if that month has 30 days, 0 if 29
        /// Digits 4-1 (4 digits) indicate which month is the leap month; 0 if the year has no leap month
        ///</remarks>
        private static int[] LunarDateArray = new int[]{
                0x04BD8,0x04AE0,0x0A570,0x054D5,0x0D260,0x0D950,0x16554,0x056A0,0x09AD0,0x055D2,
                0x04AE0,0x0A5B6,0x0A4D0,0x0D250,0x1D255,0x0B540,0x0D6A0,0x0ADA2,0x095B0,0x14977,
                0x04970,0x0A4B0,0x0B4B5,0x06A50,0x06D40,0x1AB54,0x02B60,0x09570,0x052F2,0x04970,
                0x06566,0x0D4A0,0x0EA50,0x06E95,0x05AD0,0x02B60,0x186E3,0x092E0,0x1C8D7,0x0C950,
                0x0D4A0,0x1D8A6,0x0B550,0x056A0,0x1A5B4,0x025D0,0x092D0,0x0D2B2,0x0A950,0x0B557,
                0x06CA0,0x0B550,0x15355,0x04DA0,0x0A5B0,0x14573,0x052B0,0x0A9A8,0x0E950,0x06AA0,
                0x0AEA6,0x0AB50,0x04B60,0x0AAE4,0x0A570,0x05260,0x0F263,0x0D950,0x05B57,0x056A0,
                0x096D0,0x04DD5,0x04AD0,0x0A4D0,0x0D4D4,0x0D250,0x0D558,0x0B540,0x0B6A0,0x195A6,
                0x095B0,0x049B0,0x0A974,0x0A4B0,0x0B27A,0x06A50,0x06D40,0x0AF46,0x0AB60,0x09570,
                0x04AF5,0x04970,0x064B0,0x074A3,0x0EA50,0x06B58,0x055C0,0x0AB60,0x096D5,0x092E0,
                0x0C960,0x0D954,0x0D4A0,0x0DA50,0x07552,0x056A0,0x0ABB7,0x025D0,0x092D0,0x0CAB5,
                0x0A950,0x0B4A0,0x0BAA4,0x0AD50,0x055D9,0x04BA0,0x0A5B0,0x15176,0x052B0,0x0A930,
                0x07954,0x06AA0,0x0AD50,0x05B52,0x04B60,0x0A6E6,0x0A4E0,0x0D260,0x0EA65,0x0D530,
                0x05AA0,0x076A3,0x096D0,0x04BD7,0x04AD0,0x0A4D0,0x1D0B6,0x0D250,0x0D520,0x0DD45,
                0x0B5A0,0x056D0,0x055B2,0x049B0,0x0A577,0x0A4B0,0x0AA50,0x1B255,0x06D20,0x0ADA0,
                0x14B63       
                };



        #endregion



        #region Zodiac (Western) sign names
        private static string[] _constellationName =
                    {
                    "白羊座", "金牛座", "双子座",
                    "巨蟹座", "狮子座", "处女座",
                    "天秤座", "天蝎座", "射手座",
                    "摩羯座", "水瓶座", "双鱼座"
                    };
        #endregion



        #region The 24 solar terms
        private static string[] _lunarHolidayName =
                    {
                    "小寒", "大寒", "立春", "雨水",
                    "惊蛰", "春分", "清明", "谷雨",
                    "立夏", "小满", "芒种", "夏至",
                    "小暑", "大暑", "立秋", "处暑",
                    "白露", "秋分", "寒露", "霜降",
                    "立冬", "小雪", "大雪", "冬至"
                    };
        #endregion



        #region Solar term data
        private static string[] SolarTerm = new string[] { "小寒", "大寒", "立春", "雨水", "惊蛰", "春分", "清明", "谷雨", "立夏", "小满", "芒种", "夏至", "小暑", "大暑", "立秋", "处暑", "白露", "秋分", "寒露", "霜降", "立冬", "小雪", "大雪", "冬至" };
        private static int[] sTermInfo = new int[] { 0, 21208, 42467, 63836, 85337, 107014, 128867, 150921, 173149, 195551, 218072, 240693, 263343, 285989, 308563, 331033, 353350, 375494, 397447, 419210, 440795, 462224, 483532, 504758 };
        #endregion



        #region Lunar-calendar-related data
        private static string ganStr = "甲乙丙丁戊己庚辛壬癸";
        private static string zhiStr = "子丑寅卯辰巳午未申酉戌亥";
        private static string animalStr = "鼠牛虎兔龙蛇马羊猴鸡狗猪";
        private static string nStr1 = "日一二三四五六七八九";
        private static string nStr2 = "初十廿卅";
        private static string[] _monthString =
                {
                    "出错","正月","二月","三月","四月","五月","六月","七月","八月","九月","十月","十一月","腊月"
                };
        private static string[] _dayString = {"*","初一","初二","初三","初四","初五",
                                                 "初六","初七","初八","初九","初十",
                                                 "十一","十二","十三","十四","十五",
                                                 "十六","十七","十八","十九","二十",
                                                 "廿一","廿二","廿三","廿四","廿五",
                                                 "廿六","廿七","廿八","廿九","三十"};
        #endregion



        #region Holidays calculated by the solar calendar
        private static SolarHolidayStruct[] sHolidayInfo = new SolarHolidayStruct[]{
            new SolarHolidayStruct(1, 1, 1, "元旦"),
            new SolarHolidayStruct(2, 2, 0, "世界湿地日"),
            new SolarHolidayStruct(2, 10, 0, "国际气象节"),
            new SolarHolidayStruct(2, 14, 0, "情人节"),
            new SolarHolidayStruct(3, 1, 0, "国际海豹日"),
            new SolarHolidayStruct(3, 5, 0, "学雷锋纪念日"),
            new SolarHolidayStruct(3, 8, 0, "妇女节"),
            new SolarHolidayStruct(3, 12, 0, "植树节 孙中山逝世纪念日"),
            new SolarHolidayStruct(3, 14, 0, "国际警察日"),
            new SolarHolidayStruct(3, 15, 0, "消费者权益日"),
            new SolarHolidayStruct(3, 17, 0, "中国国医节 国际航海日"),
            new SolarHolidayStruct(3, 21, 0, "世界森林日 消除种族歧视国际日 世界儿歌日"),
            new SolarHolidayStruct(3, 22, 0, "世界水日"),
            new SolarHolidayStruct(3, 24, 0, "世界防治结核病日"),
            new SolarHolidayStruct(4, 1, 0, "愚人节"),
            new SolarHolidayStruct(4, 5, 0, "清明节"),
            new SolarHolidayStruct(4, 7, 0, "世界卫生日"),
            new SolarHolidayStruct(4, 22, 0, "世界地球日"),
            new SolarHolidayStruct(5, 1, 1, "劳动节"),
            new SolarHolidayStruct(5, 2, 1, "劳动节假日"),
            new SolarHolidayStruct(5, 3, 1, "劳动节假日"),
            new SolarHolidayStruct(5, 4, 0, "青年节"),
            new SolarHolidayStruct(5, 8, 0, "世界红十字日"),
            new SolarHolidayStruct(5, 12, 0, "国际护士节"),
            new SolarHolidayStruct(5, 31, 0, "世界无烟日"),
            new SolarHolidayStruct(6, 1, 0, "国际儿童节"),
            new SolarHolidayStruct(6, 5, 0, "世界环境保护日"),
            new SolarHolidayStruct(6, 26, 0, "国际禁毒日"),
            new SolarHolidayStruct(7, 1, 0, "建党节 香港回归纪念 世界建筑日"),
            new SolarHolidayStruct(7, 11, 0, "世界人口日"),
            new SolarHolidayStruct(8, 1, 0, "建军节"),
            new SolarHolidayStruct(8, 8, 0, "中国男子节 父亲节"),
            new SolarHolidayStruct(8, 15, 0, "抗日战争胜利纪念"),
            new SolarHolidayStruct(9, 9, 0, "maozedong逝世纪念"),
            new SolarHolidayStruct(9, 10, 0, "教师节"),
            new SolarHolidayStruct(9, 18, 0, "九·一八事变纪念日"),
            new SolarHolidayStruct(9, 20, 0, "国际爱牙日"),
            new SolarHolidayStruct(9, 27, 0, "世界旅游日"),
            new SolarHolidayStruct(9, 28, 0, "孔子诞辰"),
            new SolarHolidayStruct(10, 1, 1, "国庆节 国际音乐日"),
            new SolarHolidayStruct(10, 2, 1, "国庆节假日"),
            new SolarHolidayStruct(10, 3, 1, "国庆节假日"),
            new SolarHolidayStruct(10, 6, 0, "老人节"),
            new SolarHolidayStruct(10, 24, 0, "联合国日"),
            new SolarHolidayStruct(11, 10, 0, "世界青年节"),
            new SolarHolidayStruct(11, 12, 0, "孙中山诞辰纪念"),
            new SolarHolidayStruct(12, 1, 0, "世界艾滋病日"),
            new SolarHolidayStruct(12, 3, 0, "世界残疾人日"),
            new SolarHolidayStruct(12, 20, 0, "澳门回归纪念"),
            new SolarHolidayStruct(12, 24, 0, "平安夜"),
            new SolarHolidayStruct(12, 25, 0, "圣诞节"),
            new SolarHolidayStruct(12, 26, 0, "maozedong诞辰纪念")
           };
        #endregion



        #region Holidays calculated by the lunar calendar
        private static LunarHolidayStruct[] lHolidayInfo = new LunarHolidayStruct[]{
            new LunarHolidayStruct(1, 1, 1, "春节"),
            new LunarHolidayStruct(1, 15, 0, "元宵节"),
            new LunarHolidayStruct(5, 5, 0, "端午节"),
            new LunarHolidayStruct(7, 7, 0, "七夕情人节"),
            new LunarHolidayStruct(7, 15, 0, "中元节 盂兰盆节"),
            new LunarHolidayStruct(8, 15, 0, "中秋节"),
            new LunarHolidayStruct(9, 9, 0, "重阳节"),
            new LunarHolidayStruct(12, 8, 0, "腊八节"),
            new LunarHolidayStruct(12, 23, 0, "北方小年(扫房)"),
            new LunarHolidayStruct(12, 24, 0, "南方小年(掸尘)"),
            //new LunarHolidayStruct(12, 30, 0, "除夕")  //注意除夕需要其它方法进行计算
        };
        #endregion



        #region Holidays by nth weekday of month
        private static WeekHolidayStruct[] wHolidayInfo = new WeekHolidayStruct[]{
            new WeekHolidayStruct(5, 2, 1, "母亲节"),
            new WeekHolidayStruct(5, 3, 1, "全国助残日"),
            new WeekHolidayStruct(6, 3, 1, "父亲节"),
            new WeekHolidayStruct(9, 3, 3, "国际和平日"),
            new WeekHolidayStruct(9, 4, 1, "国际聋人节"),
            new WeekHolidayStruct(10, 1, 2, "国际住房日"),
            new WeekHolidayStruct(10, 1, 4, "国际减轻自然灾害日"),
            new WeekHolidayStruct(11, 4, 5, "感恩节")
        };
        #endregion



        #endregion



        #region Constructors
        #region ChinaCalendar <Solar date initialization>
        /// <summary>
        /// Initialize using a standard solar (Gregorian) date
        /// </summary>
        /// <param name="dt"></param>
        public ChineseCalendar(DateTime dt)
        {
            int i;
            int leap;
            int temp;
            int offset;



            CheckDateLimit(dt);



            _date = dt.Date;



            //Lunar date calculation section
            leap = 0;
            temp = 0;



            TimeSpan ts = _date - ChineseCalendar.MinDay;//Calculate the basic difference between the two dates
            offset = ts.Days;



            for (i = MinYear; i <= MaxYear; i++)
            {
                temp = GetChineseYearDays(i);  //Get the number of days in that lunar year
                if (offset - temp < 1)
                    break;
                else
                {
                    offset = offset - temp;
                }
            }
            _cYear = i;



            leap = GetChineseLeapMonth(_cYear);//Calculate which month is the leap month that year
            //Set whether the year has a leap month
            if (leap > 0)
            {
                _cIsLeapYear = true;
            }
            else
            {
                _cIsLeapYear = false;
            }



            _cIsLeapMonth = false;
            for (i = 1; i <= 12; i++)
            {
                //Leap month
                if ((leap > 0) && (i == leap + 1) && (_cIsLeapMonth == false))
                {
                    _cIsLeapMonth = true;
                    i = i - 1;
                    temp = GetChineseLeapMonthDays(_cYear); //Calculate the leap month's day count
                }
                else
                {
                    _cIsLeapMonth = false;
                    temp = GetChineseMonthDays(_cYear, i);//Calculate a non-leap month's day count
                }



                offset = offset - temp;
                if (offset <= 0) break;
            }



            offset = offset + temp;
            _cMonth = i;
            _cDay = offset;
        }
        #endregion



        #region ChinaCalendar <Lunar date initialization>
        /// <summary>
        /// Initialize using a lunar date
        /// </summary>
        /// <param name="cy">Lunar year</param>
        /// <param name="cm">Lunar month</param>
        /// <param name="cd">Lunar day</param>
        /// <param name="LeapFlag">Leap month flag</param>
        public ChineseCalendar(int cy, int cm, int cd, bool leapMonthFlag)
        {
            int i, leap, Temp, offset;



            CheckChineseDateLimit(cy, cm, cd, leapMonthFlag);



            _cYear = cy;
            _cMonth = cm;
            _cDay = cd;



            offset = 0;



            for (i = MinYear; i < cy; i++)
            {
                Temp = GetChineseYearDays(i); //Get the number of days in that lunar year
                offset = offset + Temp;
            }



            leap = GetChineseLeapMonth(cy);// Calculate which month should be the leap month that year
            if (leap != 0)
            {
                this._cIsLeapYear = true;
            }
            else
            {
                this._cIsLeapYear = false;
            }



            if (cm != leap)
            {
                _cIsLeapMonth = false;  //The current date isn't a leap month
            }
            else
            {
                _cIsLeapMonth = leapMonthFlag;  //Use the user-provided leap-month flag
            }




            if ((_cIsLeapYear == false) || //The year has no leap month
                 (cm < leap)) //The month being calculated is before the leap month
            {
                #region ...
                for (i = 1; i < cm; i++)
                {
                    Temp = GetChineseMonthDays(cy, i);//Calculate a non-leap month's day count
                    offset = offset + Temp;
                }



                //Check whether the date exceeds the maximum day
                if (cd > GetChineseMonthDays(cy, cm))
                {
                    //throw new ChineseCalendarException("Invalid lunar date");
                }
                offset = offset + cd; //Add the current month's day count
                #endregion
            }
            else   //It's a leap year, and the month being calculated is >= the leap month
            {
                #region ...
                for (i = 1; i < cm; i++)
                {
                    Temp = GetChineseMonthDays(cy, i); //Calculate a non-leap month's day count
                    offset = offset + Temp;
                }



                if (cm > leap) //The month is after the leap month
                {
                    Temp = GetChineseLeapMonthDays(cy);   //Calculate the leap month's day count
                    offset = offset + Temp;               //Add the leap month's day count



                    if (cd > GetChineseMonthDays(cy, cm))
                    {
                        throw new ChineseCalendarException("Invalid lunar date");
                    }
                    offset = offset + cd;
                }
                else  //The month being calculated equals the leap month
                {
                    //If calculating the leap month itself, first add the day count of its corresponding regular month
                    if (this._cIsLeapMonth == true) //The month being calculated is the leap month
                    {
                        Temp = GetChineseMonthDays(cy, cm); //Calculate a non-leap month's day count
                        offset = offset + Temp;
                    }



                    if (cd > GetChineseLeapMonthDays(cy))
                    {
                        throw new ChineseCalendarException("Invalid lunar date");
                    }
                    offset = offset + cd;
                }
                #endregion
            }




            _date = MinDay.AddDays(offset);
        }
        #endregion
        #endregion



        #region Private functions



        #region GetChineseMonthDays
        //Returns the total days in lunar year y, month m
        private int GetChineseMonthDays(int year, int month)
        {
            if (BitTest32((LunarDateArray[year - MinYear] & 0x0000FFFF), (16 - month)))
            {
                return 30;
            }
            else
            {
                return 29;
            }
        }
        #endregion



        #region GetChineseLeapMonth
        //Returns which month (1-12) is the leap month in lunar year y; 0 if none
        private int GetChineseLeapMonth(int year)
        {



            return LunarDateArray[year - MinYear] & 0xF;



        }
        #endregion



        #region GetChineseLeapMonthDays
        //Returns the day count of the leap month in lunar year y
        private int GetChineseLeapMonthDays(int year)
        {
            if (GetChineseLeapMonth(year) != 0)
            {
                if ((LunarDateArray[year - MinYear] & 0x10000) != 0)
                {
                    return 30;
                }
                else
                {
                    return 29;
                }
            }
            else
            {
                return 0;
            }
        }
        #endregion



        #region GetChineseYearDays
        /// <summary>
        /// Get the total days in a lunar year
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        private int GetChineseYearDays(int year)
        {
            int i, f, sumDay, info;



            sumDay = 348; //29 days x 12 months
            i = 0x8000;
            info = LunarDateArray[year - MinYear] & 0x0FFFF;



            //Calculate how many of the 12 months have 30 days
            for (int m = 0; m < 12; m++)
            {
                f = info & i;
                if (f != 0)
                {
                    sumDay++;
                }
                i = i >> 1;
            }
            return sumDay + GetChineseLeapMonthDays(year);
        }
        #endregion



        #region CheckDateLimit
        /// <summary>
        /// Check whether the solar date meets requirements
        /// </summary>
        /// <param name="dt"></param>
        private void CheckDateLimit(DateTime dt)
        {
            if ((dt < MinDay) || (dt > MaxDay))
            {
                throw new ChineseCalendarException("Date out of convertible range");
            }
        }
        #endregion



        #region CheckChineseDateLimit
        /// <summary>
        /// Check whether the lunar date is valid
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="leapMonth"></param>
        private void CheckChineseDateLimit(int year, int month, int day, bool leapMonth)
        {
            if ((year < MinYear) || (year > MaxYear))
            {
                throw new ChineseCalendarException("Invalid lunar date");
            }
            if ((month < 1) || (month > 12))
            {
                throw new ChineseCalendarException("Invalid lunar date");
            }
            if ((day < 1) || (day > 30)) //A Chinese calendar month has at most 30 days
            {
                throw new ChineseCalendarException("Invalid lunar date");
            }



            int leap = GetChineseLeapMonth(year);// Calculate which month should be the leap month that year
            if ((leapMonth == true) && (month != leap))
            {
                throw new ChineseCalendarException("Invalid lunar date");
            }




        }
        #endregion



        #region ConvertNumToChineseNum
        /// <summary>
        /// Convert 0-9 into Chinese numeral form
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private string ConvertNumToChineseNum(char n)
        {
            if ((n < '0') || (n > '9')) return "";
            switch (n)
            {
                case '0':
                    return HZNum[0].ToString();
                case '1':
                    return HZNum[1].ToString();
                case '2':
                    return HZNum[2].ToString();
                case '3':
                    return HZNum[3].ToString();
                case '4':
                    return HZNum[4].ToString();
                case '5':
                    return HZNum[5].ToString();
                case '6':
                    return HZNum[6].ToString();
                case '7':
                    return HZNum[7].ToString();
                case '8':
                    return HZNum[8].ToString();
                case '9':
                    return HZNum[9].ToString();
                default:
                    return "";
            }
        }
        #endregion



        #region BitTest32
        /// <summary>
        /// Test whether a given bit is true
        /// </summary>
        /// <param name="num"></param>
        /// <param name="bitpostion"></param>
        /// <returns></returns>
        private bool BitTest32(int num, int bitpostion)
        {



            if ((bitpostion > 31) || (bitpostion < 0))
                throw new Exception("Error Param: bitpostion[0-31]:" + bitpostion.ToString());



            int bit = 1 << bitpostion;



            if ((num & bit) == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
        #region ConvertDayOfWeek
        /// <summary>
        /// Convert a weekday into its numeric representation
        /// </summary>
        /// <param name="dayOfWeek"></param>
        /// <returns></returns>
        private int ConvertDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return 1;
                case DayOfWeek.Monday:
                    return 2;
                case DayOfWeek.Tuesday:
                    return 3;
                case DayOfWeek.Wednesday:
                    return 4;
                case DayOfWeek.Thursday:
                    return 5;
                case DayOfWeek.Friday:
                    return 6;
                case DayOfWeek.Saturday:
                    return 7;
                default:
                    return 0;
            }
        }
        #endregion



        #region CompareWeekDayHoliday
        /// <summary>
        /// Compare whether the day is the specified nth weekday
        /// </summary>
        /// <param name="date"></param>
        /// <param name="month"></param>
        /// <param name="week"></param>
        /// <param name="day"></param>
        /// <returns></returns>

        private bool CompareWeekDayHoliday(DateTime date, int month, int week, int day)
        {
            bool ret = false;

            if (date.Month == month) //Same month
            {
                if (ConvertDayOfWeek(date.DayOfWeek) == day) //Same weekday
                {
                    DateTime firstDay = new DateTime(date.Year, date.Month, 1);//Generate the first day of the month
                    int i = ConvertDayOfWeek(firstDay.DayOfWeek);
                    int firWeekDays = 7 - ConvertDayOfWeek(firstDay.DayOfWeek) + 1; //Calculate the remaining days in the first week

                    if (i > day)
                    {
                        if ((week - 1) * 7 + day + firWeekDays == date.Day)
                        {
                            ret = true;
                        }
                    }
                    else
                    {
                        if (day + firWeekDays + (week - 2) * 7 == date.Day)
                        {
                            ret = true;
                        }
                    }
                }
            }

            return ret;
        }
        #endregion
        #endregion

        #region  Properties

        #region Holidays
        #region ChineseCalendarHoliday
        /// <summary>
        /// Calculate Chinese lunar calendar holidays
        /// </summary>
        public string ChineseCalendarHoliday
        {
            get
            {
                string tempStr = "";
                if (this._cIsLeapMonth == false) //Don't calculate holidays for a leap month
                {
                    foreach (LunarHolidayStruct lh in lHolidayInfo)
                    {
                        if ((lh.Month == this._cMonth) && (lh.Day == this._cDay))
                        {

                            tempStr = lh.HolidayName;
                            break;

                        }
                    }

                    //Special handling for Chinese New Year's Eve
                    if (this._cMonth == 12)
                    {
                        int i = GetChineseMonthDays(this._cYear, 12); //Calculate the total days in lunar month 12 of that year
                        if (this._cDay == i) //If it's the last day
                        {
                            tempStr = "除夕";
                        }
                    }
                }
                return tempStr;
            }
        }
        #endregion

        #region WeekDayHoliday
        /// <summary>
        /// Holidays calculated by the nth weekday of a given month
        /// </summary>
        public string WeekDayHoliday
        {
            get
            {
                string tempStr = "";
                foreach (WeekHolidayStruct wh in wHolidayInfo)
                {
                    if (CompareWeekDayHoliday(_date, wh.Month, wh.WeekAtMonth, wh.WeekDay))
                    {
                        tempStr = wh.HolidayName;
                        break;
                    }
                }
                return tempStr;
            }
        }
        #endregion

        #region DateHoliday
        /// <summary>
        /// Holidays calculated by the solar calendar day
        /// </summary>
        public string DateHoliday
        {
            get
            {
                string tempStr = "";

                foreach (SolarHolidayStruct sh in sHolidayInfo)
                {
                    if ((sh.Month == _date.Month) && (sh.Day == _date.Day))
                    {
                        tempStr = sh.HolidayName;
                        break;
                    }
                }
                return tempStr;
            }
        }
        #endregion
        #endregion

        #region Solar date
        #region Date
        /// <summary>
        /// Get the corresponding solar date
        /// </summary>
        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }
        #endregion

        #region WeekDay
        /// <summary>
        /// Get the day of the week
        /// </summary>
        public DayOfWeek WeekDay
        {
            get { return _date.DayOfWeek; }
        }
        #endregion

        #region WeekDayStr
        /// <summary>
        /// Weekday as text
        /// </summary>
        public string WeekDayStr
        {
            get
            {
                switch (_date.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        return "星期日";
                    case DayOfWeek.Monday:
                        return "星期一";
                    case DayOfWeek.Tuesday:
                        return "星期二";
                    case DayOfWeek.Wednesday:
                        return "星期三";
                    case DayOfWeek.Thursday:
                        return "星期四";
                    case DayOfWeek.Friday:
                        return "星期五";
                    default:
                        return "星期六";
                }
            }
        }
        #endregion

        #region DateString
        /// <summary>
        /// Solar date in Chinese numeral form, e.g. 一九九七年七月一日
        /// </summary>
        public string DateString
        {
            get
            {
                return "公元" + this._date.ToLongDateString();
            }
        }
        #endregion

        #region IsLeapYear
        /// <summary>
        /// Whether the current year is a solar (Gregorian) leap year
        /// </summary>
        public bool IsLeapYear
        {
            get
            {
                return DateTime.IsLeapYear(this._date.Year);
            }
        }
        #endregion
        #endregion

        #region Lunar date
        #region IsChineseLeapMonth
        /// <summary>
        /// Whether it's a leap month
        /// </summary>
        public bool IsChineseLeapMonth
        {
            get { return this._cIsLeapMonth; }
        }
        #endregion

        #region IsChineseLeapYear
        /// <summary>
        /// Whether the year has a leap month
        /// </summary>
        public bool IsChineseLeapYear
        {
            get
            {
                return this._cIsLeapYear;
            }
        }
        #endregion

        #region ChineseDay
        /// <summary>
        /// Lunar day
        /// </summary>
        public int ChineseDay
        {
            get { return this._cDay; }
        }
        #endregion

        #region ChineseDayString
        /// <summary>
        /// Lunar day in Chinese numeral form
        /// </summary>
        public string ChineseDayString
        {
            get
            {
                switch (this._cDay)
                {
                    case 0:
                        return "";
                    case 10:
                        return "初十";
                    case 20:
                        return "二十";
                    case 30:
                        return "三十";
                    default:
                        return nStr2[(int)(_cDay / 10)].ToString() + nStr1[_cDay % 10].ToString();

                }
            }
        }
        #endregion

        #region ChineseMonth
        /// <summary>
        /// Lunar month
        /// </summary>
        public int ChineseMonth
        {
            get { return this._cMonth; }
        }
        #endregion

        #region ChineseMonthString
        /// <summary>
        /// Lunar month in Chinese numeral form
        /// </summary>
        public string ChineseMonthString
        {
            get
            {
                return _monthString[this._cMonth];
            }
        }
        #endregion

        #region ChineseYear
        /// <summary>
        /// Get the lunar year
        /// </summary>
        public int ChineseYear
        {
            get { return this._cYear; }
        }
        #endregion

        #region ChineseYearString
        /// <summary>
        /// Get the lunar year string, e.g. 一九九七年
        /// </summary>
        public string ChineseYearString
        {
            get
            {
                string tempStr = "";
                string num = this._cYear.ToString();
                for (int i = 0; i < 4; i++)
                {
                    tempStr += ConvertNumToChineseNum(num[i]);
                }
                return tempStr + "年";
            }
        }
        #endregion

        #region ChineseDateString
        /// <summary>
        /// Get the lunar date representation: e.g. 农历一九九七年正月初五
        /// </summary>
        public string ChineseDateString
        {
            get
            {
                if (this._cIsLeapMonth == true)
                {
                    return "农历" + ChineseYearString + "闰" + ChineseMonthString + ChineseDayString;
                }
                else
                {
                    return "农历" + ChineseYearString + ChineseMonthString + ChineseDayString;
                }
            }
        }
        #endregion

        #region ChineseTwentyFourDay
        /// <summary>
        /// Calculates the 24 solar terms using the constant-solar-term method; the 24 solar terms are calculated from the Earth's revolution, not the lunar calendar
        /// </summary>
        /// <remarks>
        /// There are two methods for defining solar terms. The ancient calendar method, called "constant qi", divides the year into 24 equal parts,
        /// with each solar term averaging just over 15 days, hence also called "average qi". The modern lunar calendar uses the method called "true qi", which
        /// uses the Earth's position in its orbit as the standard - one revolution is 360 degrees, with 15 degrees between consecutive solar terms. Because the Earth is near
        /// perihelion around the winter solstice and moving faster, the time for the sun to move 15 degrees along the ecliptic is just under 15 days.
        /// Around the summer solstice it's the opposite - the sun moves more slowly along the ecliptic, and a solar term can last up to 16 days. Using
        /// the true-qi method ensures the spring and autumn equinoxes always fall on days when day and night are equal.
        /// </remarks>
        public string ChineseTwentyFourDay
        {
            get
            {
                DateTime baseDateAndTime = new DateTime(1900, 1, 6, 2, 5, 0); //#1/6/1900 2:05:00 AM#
                DateTime newDate;
                double num;
                int y;
                string tempStr = "";

                y = this._date.Year;

                for (int i = 1; i <= 24; i++)
                {
                    num = 525948.76 * (y - 1900) + sTermInfo[i - 1];

                    newDate = baseDateAndTime.AddMinutes(num);//Calculated in minutes
                    if (newDate.DayOfYear == _date.DayOfYear)
                    {
                        tempStr = SolarTerm[i - 1];
                        break;
                    }
                }
                return tempStr;
            }
        }
        #endregion
        #endregion

        #region Zodiac (Western) sign
        #region Constellation
        /// <summary>
        /// Calculate the zodiac sign index for a given date
        /// </summary>
        /// <returns></returns>
        public string Constellation
        {
            get
            {
                int index = 0;
                int y, m, d;
                y = _date.Year;
                m = _date.Month;
                d = _date.Day;
                y = m * 100 + d;

                if (((y >= 321) && (y <= 419))) { index = 0; }
                else if ((y >= 420) && (y <= 520)) { index = 1; }
                else if ((y >= 521) && (y <= 620)) { index = 2; }
                else if ((y >= 621) && (y <= 722)) { index = 3; }
                else if ((y >= 723) && (y <= 822)) { index = 4; }
                else if ((y >= 823) && (y <= 922)) { index = 5; }
                else if ((y >= 923) && (y <= 1022)) { index = 6; }
                else if ((y >= 1023) && (y <= 1121)) { index = 7; }
                else if ((y >= 1122) && (y <= 1221)) { index = 8; }
                else if ((y >= 1222) || (y <= 119)) { index = 9; }
                else if ((y >= 120) && (y <= 218)) { index = 10; }
                else if ((y >= 219) && (y <= 320)) { index = 11; }
                else { index = 0; }

                return _constellationName[index];
            }
        }
        #endregion
        #endregion

        #region Chinese zodiac (animal sign)
        #region Animal
        /// <summary>
        /// Calculate the Chinese zodiac index. Note: although the zodiac is traditionally distinguished by lunar year, in practice it is commonly calculated using the solar calendar.
        /// Year of the Rat = 1, and so on
        /// </summary>
        public int Animal
        {
            get
            {
                int offset = _date.Year - AnimalStartYear;
                return (offset % 12) + 1;
            }
        }
        #endregion

        #region AnimalString
        /// <summary>
        /// Get the Chinese zodiac string
        /// </summary>
        public string AnimalString
        {
            get
            {
                int offset = _date.Year - AnimalStartYear;
                return animalStr[offset % 12].ToString();
            }
        }
        #endregion
        #endregion

        #region Heavenly Stems and Earthly Branches (Ganzhi)
        #region GanZhiYearString
        /// <summary>
        /// Get the Ganzhi representation of the lunar year, e.g. 乙丑年
        /// </summary>
        public string GanZhiYearString
        {
            get
            {
                string tempStr;
                int i = (this._cYear - GanZhiStartYear) % 60; //Calculate the Ganzhi
                tempStr = ganStr[i % 10].ToString() + zhiStr[i % 12].ToString() + "年";
                return tempStr;
            }
        }
        #endregion

        #region GanZhiMonthString
        /// <summary>
        /// Get the Ganzhi month string; note leap months don't get their own Ganzhi designation
        /// </summary>
        public string GanZhiMonthString
        {
            get
            {
                //Each month's Earthly Branch is always fixed, and always starts from the month of Yin
                int zhiIndex;
                string zhi;
                if (this._cMonth > 10)
                {
                    zhiIndex = this._cMonth - 10;
                }
                else
                {
                    zhiIndex = this._cMonth + 2;
                }
                zhi = zhiStr[zhiIndex - 1].ToString();

                //Calculate the first month's Heavenly Stem based on the year's Heavenly Stem
                int ganIndex = 1;
                string gan;
                int i = (this._cYear - GanZhiStartYear) % 60; //Calculate the Ganzhi
                switch (i % 10)
                {
                    #region ...
                    case 0: //甲
                        ganIndex = 3;
                        break;
                    case 1: //乙
                        ganIndex = 5;
                        break;
                    case 2: //丙
                        ganIndex = 7;
                        break;
                    case 3: //丁
                        ganIndex = 9;
                        break;
                    case 4: //戊
                        ganIndex = 1;
                        break;
                    case 5: //己
                        ganIndex = 3;
                        break;
                    case 6: //庚
                        ganIndex = 5;
                        break;
                    case 7: //辛
                        ganIndex = 7;
                        break;
                    case 8: //壬
                        ganIndex = 9;
                        break;
                    case 9: //癸
                        ganIndex = 1;
                        break;
                    #endregion
                }
                gan = ganStr[(ganIndex + this._cMonth - 2) % 10].ToString();

                return gan + zhi + "月";
            }
        }
        #endregion

        #region GanZhiDayString
        /// <summary>
        /// Get the Ganzhi day representation
        /// </summary>
        public string GanZhiDayString
        {
            get
            {
                int i, offset;
                TimeSpan ts = this._date - GanZhiStartDay;
                offset = ts.Days;
                i = offset % 60;
                return ganStr[i % 10].ToString() + zhiStr[i % 12].ToString() + "日";
            }
        }
        #endregion

        #region GanZhiDateString
        /// <summary>
        /// Get the Ganzhi representation of the current date, e.g. 甲子年乙丑月丙庚日
        /// </summary>
        public string GanZhiDateString
        {
            get
            {
                return GanZhiYearString + GanZhiMonthString + GanZhiDayString;
            }
        }
        #endregion
        #endregion
        #endregion

        #region Methods
        #region NextDay
        /// <summary>
        /// Get the next day
        /// </summary>
        /// <returns></returns>
        public ChineseCalendar NextDay()
        {
            DateTime nextDay = _date.AddDays(1);
            return new ChineseCalendar(nextDay);
        }
        #endregion

        #region PervDay
        /// <summary>
        /// Get the previous day
        /// </summary>
        /// <returns></returns>
        public ChineseCalendar PervDay()
        {
            DateTime pervDay = _date.AddDays(-1);
            return new ChineseCalendar(pervDay);
        }
        #endregion
        #endregion
    }
}
