using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace System.Doors.Intelops
{
    public class Lagrange
    {
        public double Interpolation(double[] xs, double[] ys, double x)
        {
            double t;
            double y = 0.0;

            for (int k = 0; k < xs.Length; k++)
            {
                t = 1.0;
                for (int j = 0; j < xs.Length; j++)
                    if (j != k)
                        t = t * ((x - xs[j]) / (xs[k] - xs[j]));

                y += t * ys[k];
            }
            return y;
        }
    }    
}
