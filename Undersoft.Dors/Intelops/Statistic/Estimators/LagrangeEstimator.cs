using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace System.Dors.Intelops
{
    public class LagrangeEstimator : Estimator
    {
        public override void Prepare(EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> input)
        {
            validInput = false;
            //only one dimension
            if (input.X.Count > 0 && input.X[0].Item.Length > 1 ||
                input.Y.Count > 0 && input.Y[0].Item.Length > 1) throw new StatisticsExceptions(StatisticsExceptionList.DataTypeSingle);
            Input = input;
            validInput = true;
        }

        public override void Prepare(EstimatorObjectCollection x, EstimatorObjectCollection y)
        {
            Prepare(new EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection>(x, y));
        }

        public override EstimatorObject Evaluate(object x)
        {
            return Evaluate(new EstimatorObject(x));
        }

        public override void Create()
        {
            
        }

        public override EstimatorObject Evaluate(EstimatorObject x)
        {
            if (!validInput) throw new StatisticsExceptions(StatisticsExceptionList.DataType);

            double t;
            double y = 0.0;

            var a = Input.X.Select((x0, k) => Input.X.Select((x1, j) => x1).ToList()).ToList();

            for (int k = 0; k < Input.X.Count; k++)
            {
                t = 1.0;
                for (int j = 0; j < Input.X.Count; j++)
                {
                    if (j != k)
                    {
                        t = t * ((x.Item[0] - Input.X[j].Item[0]) / (Input.X[k].Item[0] - Input.X[j].Item[0]));
                    }
                }

                y += t * Input.Y[k].Item[0];
            }

            return new EstimatorObject(y);
        }

        public override void Update(EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> input)
        {
            throw new StatisticsExceptions(StatisticsExceptionList.MethodCannotBeProceeded);
        }

        public override void Update(EstimatorObjectCollection x, EstimatorObjectCollection y)
        {
            throw new StatisticsExceptions(StatisticsExceptionList.MethodCannotBeProceeded);
        }

        public override double[][] GetParameters()
        {
            return null;
        }

    }
}
