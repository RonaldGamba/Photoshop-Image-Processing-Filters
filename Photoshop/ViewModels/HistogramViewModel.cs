using OxyPlot;
using Photoshop.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photoshop.ViewModels
{
    public class HistogramViewModel
    {
        public HistogramViewModel(float[] histogram, float[] equalizedHistogram )
        {
            NormalHistogramData = new List<DataPoint>();
            EqualizedHistogramData = new List<DataPoint>();

            for (int i = 0; i < histogram.Count(); i++)
                NormalHistogramData.Add(new DataPoint(i, histogram[i]));

            for (int i = 0; i < equalizedHistogram.Count(); i++)
                EqualizedHistogramData.Add(new DataPoint(i, equalizedHistogram[i]));
        }

        public List<DataPoint> NormalHistogramData { get; set; }
        public List<DataPoint> EqualizedHistogramData { get; set; }
    }
}
