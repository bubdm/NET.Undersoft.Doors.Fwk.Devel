using System.Doors.Data;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace System.Doors.Intelops
{
    //praktycznie taki sam jak RLS - inny jest dopiero Extended Kalman Filter
    public class KalmanEstimator : Estimator
    {
        private bool validParameters;
        private double[][] parameterK;
        private double[][] parameterP;
        private double[][] parameterTheta;

        private List<double> advancedParameters;

        //przyspieszyc estymatory - bez nieustannego alokowania, tylko operacje na juz istniejacych elementach !!!!

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
            // prediction step: 
            // P = P + Rw; Rw - related with noise-error
            // correction step:
            // K = P*XX*inv(Rv+XX'*P*XX) //Rv - error-noise
            // theta = theta + K * (YY - XX'*theta)
            // P = P - K*XX'*P
            if (validInput == false)
            {
                throw new StatisticsExceptions(StatisticsExceptionList.DataType);
            }

            int m = Input.X.Count;
            int nx = Input.X[0].Item.Length;
            int ny = Input.Y[0].Item.Length;
            double[][] xx = MathOperations.MatrixCreate(nx, 1);
            double[][] yy = MathOperations.MatrixCreate(1, ny);

            double[][] K = MathOperations.MatrixCreate(nx, 1);
            double[][] P = MathOperations.MatrixDiagonal(nx, 10000); //nx x nx small confidence in initial theta (which is 0 0 0 0)
            double[][] theta = MathOperations.MatrixCreate(nx, ny);

            double[][] Rw = MathOperations.MatrixDiagonal(nx, 1);
            double[][] Rv = MathOperations.MatrixDiagonal(1, 1);

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


            if (advancedParameters != null)
            {
                Rv[0][0] = advancedParameters[0];
                Rw = MathOperations.MatrixDiagonal(nx, advancedParameters[1]);
            }

            for (int i = 0; i < m; i++)
            {
                xx = MathOperations.MatrixCreateColumn(Input.X[i].Item, xx);
                xxT = MathOperations.MatrixTranpose(xx, xxT);
                yy = MathOperations.MatrixCreateRow(Input.Y[i].Item, yy);
                P = MathOperations.MatrixSum(P, Rw, P);
                P_XX = MathOperations.MatrixProduct(P, xx, P_XX);
                XXT_P = MathOperations.MatrixProduct(xxT, P, XXT_P);                
                XXT_P_XX = MathOperations.MatrixProduct(XXT_P, xx, XXT_P_XX);
                XXT_P_XX = MathOperations.MatrixSum(XXT_P_XX, Rv, XXT_P_XX);                
                inv_XXT_P_XX = MathOperations.MatrixInverse(XXT_P_XX, inv_XXT_P_XX);
                K = MathOperations.MatrixProduct(P_XX, inv_XXT_P_XX, K);
                XXT_theta = MathOperations.MatrixProduct(xxT, theta, XXT_theta);
                YY_XXT_theta = MathOperations.MatrixSub(yy, XXT_theta, YY_XXT_theta);
                K_YY_XXT_theta = MathOperations.MatrixProduct(K, YY_XXT_theta, K_YY_XXT_theta);
                theta = MathOperations.MatrixSum(theta, K_YY_XXT_theta, theta);
                K_XXT_P = MathOperations.MatrixProduct(K, XXT_P, K_XXT_P);
                P = MathOperations.MatrixSub(P, K_XXT_P, P);
            }

            parameterK = K;
            parameterP = P;
            parameterTheta = theta;

            validParameters = true;
        }

        public override void Update(EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> input)
        {
            if((input == null || input.X.Count == 0 || input.X.Count == 0)
                || (parameterTheta != null) 
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

            double[][] Rw = MathOperations.MatrixDiagonal(nx, 1);
            double[][] Rv = MathOperations.MatrixDiagonal(1, 1);

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
                Rv[0][0] = advancedParameters[0];
                Rw = MathOperations.MatrixDiagonal(nx, advancedParameters[1]);
            }
            
            for (int i = 0; i < m; i++)
            {
                xx = MathOperations.MatrixCreateColumn(Input.X[i].Item, xx);
                xxT = MathOperations.MatrixTranpose(xx, xxT);
                yy = MathOperations.MatrixCreateRow(Input.Y[i].Item, yy);
                P = MathOperations.MatrixSum(P, Rw, P);
                P_XX = MathOperations.MatrixProduct(P, xx, P_XX);
                XXT_P = MathOperations.MatrixProduct(xxT, P, XXT_P);
                XXT_P_XX = MathOperations.MatrixProduct(XXT_P, xx, XXT_P_XX);
                XXT_P_XX = MathOperations.MatrixSum(XXT_P_XX, Rv, XXT_P_XX);
                inv_XXT_P_XX = MathOperations.MatrixInverse(XXT_P_XX, inv_XXT_P_XX);
                K = MathOperations.MatrixProduct(P_XX, inv_XXT_P_XX, K);
                XXT_theta = MathOperations.MatrixProduct(xxT, theta, XXT_theta);
                YY_XXT_theta = MathOperations.MatrixSub(yy, XXT_theta, YY_XXT_theta);
                K_YY_XXT_theta = MathOperations.MatrixProduct(K, YY_XXT_theta, K_YY_XXT_theta);
                theta = MathOperations.MatrixSum(theta, K_YY_XXT_theta, theta);
                K_XXT_P = MathOperations.MatrixProduct(K, XXT_P, K_XXT_P);
                P = MathOperations.MatrixSub(P, K_XXT_P, P);
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
            //exception ... or not double
            if (advParameters == null || advParameters.Count < 2)
            {
                advancedParameters = null;
                return;
            }
            advancedParameters = new List<double>();
            advancedParameters.Add(Convert.ToDouble(advParameters[0]));
            advancedParameters.Add(Convert.ToDouble(advParameters[1]));
        }

        public override double[][] GetParameters()
        {            
            return MathOperations.MatrixDuplicate(parameterTheta);
        }

        public void WriteToTrellis(DataTrellis trell)
        {

        }

        public void ReadFromTrellis(DataTrellis trell)
        {

        }
    }

}
