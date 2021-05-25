using System;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace System.Dors.Intelops
{

    //kazdy estymator ma swoje input - wtedy zmiana estymatora wymaga tez zmiany input - komplikacje to powoduje

    public class Statistics : Estimator
    {
        private Estimator estimator;    //run Estimator Methods
        private EstimatorMethod defaultMethod;

        //konstruktor Statistics uzyty w IStatiscics -> implikuje zainicjowanie Input i (dodatkowo) określenie metody domyslnej estymowania
        //public Statistics(EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> input, EstimatorMethod method = EstimatorMethod.Empty)
        public Statistics(EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> input, EstimatorMethod method)
        {
            //try catch, czy input moze byc dla danej metody !!! - zapytac Darka
            estimator = resolveMethod(method);
            defaultMethod = method;
            Prepare(input);
            
            //defaultowo ustawic Empty jak catch exception
        }
        //public Statistics(EstimatorObjectCollection x, EstimatorObjectCollection y, EstimatorMethod method = EstimatorMethod.Empty)
        public Statistics(EstimatorObjectCollection x, EstimatorObjectCollection y, EstimatorMethod method)
        {
            estimator = resolveMethod(method);
            defaultMethod = method;
            Prepare(x, y);
        }

        //wywolywane przez konstruktory
        public override void Prepare(EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> input)
        {
            try
            {
                Input = input;
                estimator.Prepare(input);       //can thow exception if input is not, e.g., 1D for LinearRegression, but input can be still valid for other estimators
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                
            }
        }

        public override void Prepare(EstimatorObjectCollection x, EstimatorObjectCollection y)
        {
            EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> _input =
                new EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection>(x, y);

            Prepare(_input);
        }

        public override void Create()
        {
            try
            {
                estimator.Create();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

            }
        }

        public override void Update(EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> input)
        {
            try
            {
                //Input = input;  //???? czy rozszerzyć input???? czy nie???
                estimator.Update(input);       //can thow exception if input is not, e.g., 1D for LinearRegression, but input can be still valid for other estimators
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

            }

        }

        public override void Update(EstimatorObjectCollection x, EstimatorObjectCollection y)
        {
            EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> _input =
                new EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection>(x, y);

            Update(_input);
        }


        public override EstimatorObject Evaluate(EstimatorObject x)
        {
            return estimator.Evaluate(x);
        }

        public override EstimatorObject Evaluate(object x)
        {
            return estimator.Evaluate(new EstimatorObject(x));
        }

        private Estimator resolveMethod(EstimatorMethod method)
        {
            return (Estimator)CallEstimatorInstance.ActivateMethod("System.Dors.Intelops." +  method.ToString());
        }

        public Estimator SetDefaultMethod(EstimatorMethod method)
        {            
            if (estimator != null && defaultMethod == method) return estimator;

            try
            {
                Estimator newEstimator = resolveMethod(method);                
                newEstimator.Prepare(Input);        //can change to this estimator for given input data
                estimator = newEstimator;
                defaultMethod = method;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return estimator;
        }

        public new void SetAdvancedParameters(IList<object> advParameters = null)
        {
            estimator.SetAdvancedParameters(advParameters);
        }

        public override double[][] GetParameters()
        {
            return estimator.GetParameters();
        }

        public EstimatorMethod GetDefaultMethod()
        {
            return defaultMethod;
        }
    }

    public enum EstimatorMethod
    {
        EmptyEstimator,
        LinearRegressionEstimator,
        LagrangeEstimator,
        LinearLastSquareEstimator,
        RecursiveLastSquareEstimator,
        KalmanEstimator
    }

}
