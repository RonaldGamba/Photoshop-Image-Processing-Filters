using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photoshop.ViewModels
{
    public class HistogramViewModel
    {
        public HistogramViewModel(int[] grayFrequency)
        {
            this.Data = new List<DataPoint>();

            for (int i = 0; i < grayFrequency.Count(); i++)
            {
                Data.Add(new DataPoint(i, grayFrequency[i]));
            }
        }

        public List<DataPoint> Data { get; set; }
    }
}
