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
    public class RecursiveLastSquareEstimator : Estimator
    {

        private bool validParameters;
        private double[][] parameterK;
        private double[][] parameterP;
        private double[][] parameterTheta;

        private List<double> advancedParameters;

       
        public override void Prepare(EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> input)
        {
            //verification etc....
            Input = input;
            validInput = true;
            validParameters = false;
        }

        public override void Prepare(EstimatorObjectCollection x, EstimatorObjectCollection y)
        {
            Prepare(new EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection>(x, y));
        }

        public override EstimatorObject Evaluate(object x)
        {
            return Evaluate(new EstimatorObject(x));
        }
        
        public override EstimatorObject Evaluate(EstimatorObject x)
        {
            if (validParameters == false) //to aviod recalculations of systemParameters
            {
                Create();
            }

            //eval create, gdyz moga miec rozne rozmiary danych wejsciowych

            return new EstimatorObject(MathOperations.MatrixVectorProduct(MathOperations.MatrixTranpose(parameterTheta), x.Item));
        }

        public override void Create()
        {
            // RLS Canonical form:
            // 
            // P initial values:
            //    a) P>>0 (10^3) small confidence in initial theta
            //    b) P~10 confidence in initial theta
            // theta = column vector
            // P = eye(nx,nx)*value
            // X = [[x1..xn];[x1,...,xn]; [x1,..., xn]]
            // Y =[
            // XX = [x1...xn]' column vector
            // K = P*XX*inv(1+XX'*P*XX) //inv == ^(-1)
            // theta = theta + K * (YY - XX'*theta)
            // P = P - K*XX'*P
            // with forgetting factor FF (usually 0.95, 0.995)
            // K = P*XX*inv(FF+XX'*P*XX) //inv == ^(-1)
            // theta = theta + K * (YY - XX'*theta)
            // P = (P - K*XX'*P)/FF


            //throw exception if covariance matrix is invertible

            if (validInput == false)
            {
                throw new StatisticsExceptions(StatisticsExceptionList.DataType);
            }

            int m = Input.X.Count;
            int nx = Input.X[0].Item.Length;
            int ny = Input.Y[0].Item.Length;
        
            double[][] xx = MathOperations.MatrixCreate(nx,1);            
            double[][] yy = MathOperations.MatrixCreate(1,ny);

            double[][] K = MathOperations.MatrixCreate(nx,1);
            double[][] P = MathOperations.MatrixDiagonal(nx, 10000); //nx x nx small confidence in initial theta (which is 0 0 0 0)
            double[][] theta = MathOperations.MatrixCreate(nx, ny);
            double[][] ff = MathOperations.MatrixDiagonal(1, 0.95);

            //auxuliary calculations
            double[][] xxT = MathOperations.MatrixCreate(1, nx);    //xx'
            double[][] P_XX = MathOperations.MatrixCreate(nx, 1);   //P*xx
            double[][] XXT_P = MathOperations.MatrixCreate(1, nx);
            double[][] XXT_P_XX = MathOperations.MatrixCreate(1, 1); //XX'*P*XX -> scalar, later + ff
            double[][] inv_XXT_P_XX = MathOperations.MatrixCreate(1, 1);
            double[][] XXT_theta = MathOperations.MatrixCreate(1, ny);
            double[][] YY_XXT_theta = MathOperations.MatrixCreate(1, ny);
            double[][] K_YY_XXT_theta = MathOperations.MatrixCreate(nx, ny);
            double[][] K_XXT_P = MathOperations.MatrixCreate(nx, nx);

            xxT = MathOperations.MatrixTest(xxT);

            if (advancedParameters != null)
            {
                ff[0][0] = advancedParameters[0];
            }

            for (int i = 0; i < m; i++)
            {                
                xx = MathOperations.MatrixCreateColumn(Input.X[i].Item, xx);               
                xxT = MathOperations.MatrixTranpose(xx, xxT);
                yy = MathOperations.MatrixCreateRow(Input.Y[i].Item, yy);
                P_XX = MathOperations.MatrixProduct(P, xx, P_XX);
                XXT_P = MathOperations.MatrixProduct(xxT, P, XXT_P);
                XXT_P_XX = MathOperations.MatrixProduct(XXT_P, xx, XXT_P_XX);
                XXT_P_XX = MathOperations.MatrixSum(XXT_P_XX, ff, XXT_P_XX);
                inv_XXT_P_XX = MathOperations.MatrixInverse(XXT_P_XX, inv_XXT_P_XX);
                K = MathOperations.MatrixProduct(P_XX, inv_XXT_P_XX, K);
                XXT_theta = MathOperations.MatrixProduct(xxT, theta, XXT_theta);
                YY_XXT_theta = MathOperations.MatrixSub(yy, XXT_theta, YY_XXT_theta);
                K_YY_XXT_theta = MathOperations.MatrixProduct(K, YY_XXT_theta, K_YY_XXT_theta);
                theta = MathOperations.MatrixSum(theta, K_YY_XXT_theta, theta);
                K_XXT_P = MathOperations.MatrixProduct(K, XXT_P, K_XXT_P);
                P = MathOperations.MatrixSub(P, K_XXT_P, P);
                P = MathOperations.MatrixProduct(P, 1/ff[0][0], P);
            }
            parameterK = K;
            parameterP = P;
            parameterTheta = theta;

            validParameters = true;
        }

        public override void Update(EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> input)
        {
            if ((input == null || input.X.Count == 0 || input.X.Count == 0)
                || (validParameters == true)
                    && (input.X[0].Item.Length != parameterTheta.Length || input.Y[0].Item.Length != parameterTheta[0].Length))
            {
                throw new StatisticsExceptions(StatisticsExceptionList.InputParameterInconsistent);
            }

            int m = Input.X.Count;
            int nx = Input.X[0].Item.Length;
            int ny = Input.Y[0].Item.Length;

            double[][] xx = MathOperations.MatrixCreate(nx, 1);
            double[][] yy = MathOperations.MatrixCreate(1, ny);
            double[][] K = MathOperations.MatrixCreate(nx, 1);
            double[][] P = MathOperations.MatrixDiagonal(nx, 10000); //nx x nx small confidence in initial theta (which is 0 0 0 0)
            double[][] theta = MathOperations.MatrixCreate(nx, ny);
            double[][] ff = MathOperations.MatrixDiagonal(1, 0.95);

            //auxuliary calculations
            double[][] xxT = MathOperations.MatrixCreate(1, nx);    //xx'
            double[][] P_XX = MathOperations.MatrixCreate(nx, 1);   //P*xx
            double[][] XXT_P = MathOperations.MatrixCreate(1, nx);
            double[][] XXT_P_XX = MathOperations.MatrixCreate(1, 1); //XX'*P*XX -> scalar, later + ff
            double[][] inv_XXT_P_XX = MathOperations.MatrixCreate(1, 1);
            double[][] XXT_theta = MathOperations.MatrixCreate(1, ny);
            double[][] YY_XXT_theta = MathOperations.MatrixCreate(1, ny);
            double[][] K_YY_XXT_theta = MathOperations.MatrixCreate(nx, ny);
            double[][] K_XXT_P = MathOperations.MatrixCreate(nx, nx);

            if (validParameters != false) //update run
            {
                K = parameterK;
                P = parameterP;
                theta = parameterTheta;
            }

            if (advancedParameters != null)
            {
                ff[0][0] = advancedParameters[0];
            }

            for (int i = 0; i < m; i++)
            {
                xx = MathOperations.MatrixCreateColumn(Input.X[i].Item, xx);
                xxT = MathOperations.MatrixTranpose(xx, xxT);
                yy = MathOperations.MatrixCreateRow(Input.Y[i].Item, yy);
                P_XX = MathOperations.MatrixProduct(P, xx, P_XX);
                XXT_P = MathOperations.MatrixProduct(xxT, P, XXT_P);
                XXT_P_XX = MathOperations.MatrixProduct(XXT_P, xx, XXT_P_XX);
                XXT_P_XX = MathOperations.MatrixSum(XXT_P_XX, ff, XXT_P_XX);
                inv_XXT_P_XX = MathOperations.MatrixInverse(XXT_P_XX, inv_XXT_P_XX);
                K = MathOperations.MatrixProduct(P_XX, inv_XXT_P_XX, K);
                XXT_theta = MathOperations.MatrixProduct(xxT, theta, XXT_theta);
                YY_XXT_theta = MathOperations.MatrixSub(yy, XXT_theta, YY_XXT_theta);
                K_YY_XXT_theta = MathOperations.MatrixProduct(K, YY_XXT_theta, K_YY_XXT_theta);
                theta = MathOperations.MatrixSum(theta, K_YY_XXT_theta, theta);
                K_XXT_P = MathOperations.MatrixProduct(K, XXT_P, K_XXT_P);
                P = MathOperations.MatrixSub(P, K_XXT_P, P);
                P = MathOperations.MatrixProduct(P, 1 / ff[0][0], P);
            }
            
            parameterK = K;
            parameterP = P;
            parameterTheta = theta;

            validParameters = true;
        }

        public override void Update(EstimatorObjectCollection x, EstimatorObjectCollection y)
        {
            Update(new EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection>(x, y));
        }

        public override void SetAdvancedParameters(IList<object> advParameters = null)
        {
            //exception if zero or not double
            if (advParameters == null)
            {
                advancedParameters = null;
                return;
            }
            advancedParameters = new List<double>();
            advancedParameters.Add(Convert.ToDouble(advParameters[0]));
        }

        public override double[][] GetParameters()
        {
            return MathOperations.MatrixDuplicate(parameterTheta);
        }


    }

}
