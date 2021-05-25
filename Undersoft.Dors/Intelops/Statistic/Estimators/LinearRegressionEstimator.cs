using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;



namespace System.Dors.Intelops
{

    public class LinearRegressionEstimator : Estimator
    {
        //parameters can be done as a matrix

        private bool validParameters;
        private double parameterSumX = 0;
        private double parameterSumXX = 0;
        private double parameterSumY = 0;
        private double parameterSumYY = 0;
        private double parameterSumXY = 0;
        private double parameterN = 0;
        private double parameterA = 0;
        private double parameterB = 0;
        

        public override void Prepare(EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> input)
        {
            validInput = false;
            
            if (input.X.Count > 0 && input.X[0].Mode != EstimatorObjectMode.Single ||
                input.Y.Count > 0 && input.Y[0].Mode != EstimatorObjectMode.Single)
            {
                throw new StatisticsExceptions(StatisticsExceptionList.DataTypeSingle);
            }
            Input = input;
            validParameters = false;
            validInput = true;
        }

        public override void Prepare(EstimatorObjectCollection x, EstimatorObjectCollection y)
        {
            Prepare(new EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection>(x, y));
        }

        public override void Create()
        {
            if (!validInput) throw new StatisticsExceptions(StatisticsExceptionList.DataType);

            parameterN = Input.X.Count;
            parameterSumX = Input.X.Sum(v => v.Item[0]);
            parameterSumXX = Input.X.Sum(v => v.Item[0] * v.Item[0]);
            parameterSumY = Input.Y.Sum(v => v.Item[0]);
            parameterSumYY = Input.Y.Sum(v => v.Item[0] * v.Item[0]);
            parameterSumXY = Input.X.Select((v, j) => v.Item[0] * Input.Y[j].Item[0]).Sum();

            double delta = parameterN * parameterSumXX - parameterSumX * parameterSumX;

            parameterA = (parameterN * parameterSumXY - parameterSumX * parameterSumY) / delta;
            parameterB = (parameterSumXX * parameterSumY - parameterSumX * parameterSumXY) / delta;
            validParameters = true;
        }

        public override EstimatorObject Evaluate(object x)
        {
            return Evaluate(new EstimatorObject(x));
        }
       
        public override EstimatorObject Evaluate(EstimatorObject x)
        {
            if (validParameters == false)
            {
                Create();

                validParameters = true;
            }
            
            return new EstimatorObject(parameterB + parameterA * x.Item[0]);
        }

        public override void Update(EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> input)
        {
            if (input.X.Count > 0 && input.X[0].Mode != EstimatorObjectMode.Single ||
                input.Y.Count > 0 && input.Y[0].Mode != EstimatorObjectMode.Single)
            {
                throw new StatisticsExceptions(StatisticsExceptionList.DataTypeSingle);
            }

            parameterN += input.X.Count;
            parameterSumX += input.X.Sum(v => v.Item[0]);
            parameterSumXX += input.X.Sum(v => v.Item[0] * v.Item[0]);
            parameterSumY = input.Y.Sum(v => v.Item[0]);
            parameterSumYY += input.Y.Sum(v => v.Item[0] * v.Item[0]);
            parameterSumXY += input.X.Select((v, j) => v.Item[0] * Input.Y[j].Item[0]).Sum();

            double delta = parameterN * parameterSumXX - parameterSumX * parameterSumX;

            parameterA = (parameterN * parameterSumXY - parameterSumX * parameterSumY) / delta;
            parameterB = (parameterSumXX * parameterSumY - parameterSumX * parameterSumXY) / delta;
            validParameters = true;
        }

        public override void Update(EstimatorObjectCollection x, EstimatorObjectCollection y)
        {
            Update(new EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection>(x, y));
        }

        public override double[][] GetParameters()
        {
            //can be done as a matrix

            return null;
        }


    }

}
