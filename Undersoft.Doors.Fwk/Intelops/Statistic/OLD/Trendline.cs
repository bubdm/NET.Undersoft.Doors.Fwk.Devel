using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Doors.Intelops
{
    public class InvalidTrendException : Exception
    {
        public void InvalidTrendlineException(string message)
        { }

        public class Trendline
        {
            public Trendline(IList<decimal> yAxisValues, IList<decimal> xAxisValues) : this(yAxisValues.Select((t, i) => new decimal[] { xAxisValues[i], t }).ToArray())
            { }

            public Trendline(decimal[][] data)
            {
                try
                {
                    decimal[][] cachedData = data.ToArray();

                    int n = cachedData.Length; if (n <= 0) return;
                    decimal sumX = cachedData.Sum(x => x[0]); if (sumX <= 0) return;
                    decimal sumX2 = cachedData.Sum(x => x[0] * x[0]); if (sumX2 <= 0) return;
                    decimal sumY = cachedData.Sum(x => x[1]); if (sumY <= 0) return;
                    decimal sumXY = cachedData.Sum(x => x[0] * x[1]); if (sumXY <= 0) return;

                    Slope = (sumXY - ((sumX * sumY) / n)) / (sumX2 - (sumX * sumX / n));        //b = (sum(x*y) - ((sum(x)*sum(y))/n) / (sum(x^2) - sum(x)^2/n)
                    Intercept = (sumY / n) - (Slope * (sumX / n));                              //a = sum(y)/n - b^(sum(x)/n)

                    Start = GetYValue(cachedData.Min(a => a[0]));
                    End = GetYValue(cachedData.Max(a => a[0]));
                }
                catch (Exception ex)
                {

                }
            }

            public decimal Slope { get; private set; }
            public decimal Intercept { get; private set; }
            public decimal Start { get; private set; }
            public decimal End { get; private set; }

            public decimal GetYValue(decimal xValue)
            {
                return Intercept + Slope * xValue;
            }       
        }
    }


}
