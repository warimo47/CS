using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WinformChart
{
    public partial class Form1 : Form
    {
        private Series chartSeries;
        private List<Candle> priceInfoList;

        public int max = 100;
        public int min = 0;

        public Form1()
        {
            InitializeComponent();

            chartSeries = chart1.Series["Candle"];
            // chartSeries["PriceUpColor"] = "Red";
            // chartSeries["PriceDownColor"] = "Blue";
            // chart1.AxisViewChanged += chart1_AxisViewChanged;

            priceInfoList = new List<Candle>();

            for (int i = 0; i < 10; ++i)
            {
                priceInfoList.Add(new Candle(i));
            }

            for(int i = 0; i < priceInfoList.Count; ++i)
            {
                chartSeries.Points.AddXY(priceInfoList[i].time, priceInfoList[i].max);
                chartSeries.Points[i].YValues[1] = priceInfoList[i].min;
                chartSeries.Points[i].YValues[2] = priceInfoList[i].open;
                chartSeries.Points[i].YValues[3] = priceInfoList[i].close;
            }
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void chart1_AxisViewChanged(object sender, ViewEventArgs e)
        {
            if (sender.Equals(chart1) == false) return;
            
            int startPosition = (int)e.Axis.ScaleView.ViewMinimum;
            int endPosition = (int)e.Axis.ScaleView.ViewMaximum;

            int max = (int)e.ChartArea.AxisY.ScaleView.ViewMinimum;
            int min = (int)e.ChartArea.AxisY.ScaleView.ViewMaximum;

            for (int i = startPosition - 1; i < endPosition; ++i)
            {
                if (i < 0) i = 0;
                if (i >= priceInfoList.Count) break;
                if (priceInfoList[i].max > max) max = priceInfoList[i].max;
                if (priceInfoList[i].min < min) min = priceInfoList[i].min;
            }

            chart1.ChartAreas[0].AxisY.Maximum = max;
            chart1.ChartAreas[0].AxisY.Minimum = min;
        }
    }
}
