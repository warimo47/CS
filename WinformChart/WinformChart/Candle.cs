using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinformChart
{
    class Candle
    {
        public string time { get; set; }
        public int open { get; set; }
        public int close { get; set; }
        public int min { get; set; }
        public int max { get; set; }

        public Candle(int _day)
        {
            time = "2020-12-" + (24 - _day).ToString() + "";
            Random r = new Random(_day);
            int temp = r.Next(1000, 10000);
            open = temp + 100;
            min = temp;
            close = temp + 200;
            max = temp + 300;
        }
    }
}
