using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Dors;

namespace System.Dors.Intelops
{

    public class TestRR
    {

        private Statistics st;
        private Sales sls;
        private EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> inputTest;

        public TestRR()
        {

            sls = new Sales();
            sls.List.AddRange(new Sale[] {
                new Sale() { price = 1.0, date = DateTime.Now, quantity = 1, rank = 1.0 },
                new Sale() { price = 2.0, date = DateTime.Now.AddDays(12), quantity = 1, rank = 4.0 },
                new Sale() { price = 4.0, date = DateTime.Now.AddDays(12), quantity = 1, rank = 16.0 },
                new Sale() { price = 6.0, date = DateTime.Now.AddDays(12), quantity = 1, rank = 36.0 }
                });

            List<Sale> listTest = new List<Sale>();
            listTest.AddRange(new Sale[] {
                new Sale() { price = 1.0, date = DateTime.Now, quantity = 4, rank = 1.0 },
                new Sale() { price = 2.0, date = DateTime.Now.AddDays(12), quantity = 8, rank = 4.0 },
                new Sale() { price = 4.0, date = DateTime.Now.AddDays(12), quantity = 10, rank = 16.0 },
                new Sale() { price = 6.0, date = DateTime.Now.AddDays(12), quantity = 15, rank = 36.0 }
                });


            inputTest =
    new EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection>(

        new EstimatorObjectCollection()
        {
                                new EstimatorObject(new double[3]{ 1, 2, 3 }),
                                new EstimatorObject(new double[3]{ 3, 4, 5 }),
                                new EstimatorObject(new double[3]{ 5, 6, 7 }),
                                new EstimatorObject(new double[3]{ 7, 8, 9 }),
                                new EstimatorObject(new double[3]{ 9, 10, 11 }),
                                new EstimatorObject(new double[3]{ 11, 12, 13 }),
                                new EstimatorObject(new double[3]{ 13, 14, 15 })
        },
        new EstimatorObjectCollection()
        {
                                new EstimatorObject(new double[2]{   1, 10 }),
                                new EstimatorObject(new double[2]{   2, 20 }),
                                new EstimatorObject(new double[2]{   3, 30 }),
                                new EstimatorObject(new double[2]{   4, 40 }),
                                new EstimatorObject(new double[2]{   5, 50 }),
                                new EstimatorObject(new double[2]{   6, 60 }),
                                new EstimatorObject(new double[2]{   7, 70 })}
    );

            st = sls.CreateEstimator(EstimatorMethod.EmptyEstimator);
        }


        public void RunTest(IDorsEvent WriteEcho, EstimatorMethod method)
        {
            EstimatorObject yObj;
            double y;
            double x = 5;
            double[] xx = new double[3] { 13, 14, 15 };
            try
            {
                st.SetDefaultMethod(method);
                //st.Prepare(inputTest);
                //st.Update(inputTest);

                yObj = st.Evaluate(x);
                y = yObj.Item[0];   //do testow, mozna pozniej rozszerzyc na multi
                WriteEcho.Execute(method.ToString() + " f("+x.ToString()+") = " + y.ToString());
            }
            catch (Exception ex)
            {
                WriteEcho.Execute(ex.ToString());
            }
        }
    }

    public class Sales : IStatistics
    {
        public List<Sale> List = new List<Sale>();

        private EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> MultiInput
        {
            get
            {
                return new EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection>(
                             new EstimatorObjectCollection(List.Select(f =>
                                 new EstimatorObject(f.price)).ToArray()),
                             new EstimatorObjectCollection(List.Select(f =>
                                 new EstimatorObject(f.rank)).ToArray()));
            }
        }

        private EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> SingleInput
        {
            get
            {
                return new EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection>(
                             new EstimatorObjectCollection(List.Select(f =>
                                 new EstimatorObject(f.price)).ToArray()),
                             new EstimatorObjectCollection(List.Select(f =>
                                 new EstimatorObject(f.rank)).ToArray()));
            }
        }

        // initialize Input
        public Statistics CreateEstimator(EstimatorMethod method)
        {
            EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> input;
           switch (method)
            {
                case EstimatorMethod.LinearRegressionEstimator:
                    input = SingleInput;
                    break;
                case EstimatorMethod.LagrangeEstimator:
                    input = SingleInput;
                    break;
                default:
                    input = MultiInput;
                    break;
            }

            return new Statistics(input, method);
        }

        //public TrendEstimator Estimator(DataEstimator x, EstimatorMethod method)
        //{
        //    TrendEstimator te = new TrendEstimator();
        //    return te;
        //}

        //public DataEstimatorInput<DataEstimatorCollection, DataEstimatorCollection> Input
        //{
        //    get
        //    {
        //        return new DataEstimatorInput<DataEstimatorCollection, DataEstimatorCollection>(
        //                    new DataEstimatorCollection(List.Select(s =>
        //                        new DataEstimator(s.price)).ToArray()),
        //                    new DataEstimatorCollection(List.Select(s =>
        //                        new DataEstimator(s.rank)).ToArray()));

        //        //    return new DataEstimatorInput<DataEstimatorCollection, DataEstimatorCollection>(
        //        //new DataEstimatorCollection(List.Select(s =>
        //        //    new DataEstimator(new List<double>() { s.price, s.quantity })).ToArray()),
        //        //new DataEstimatorCollection(List.Select(s =>
        //        //    new DataEstimator(s.rank)).ToArray()));

        //        //return new DataEstimatorPoint<DataEstimators, DataEstimators>(
        //        //            new DataEstimators(List.Select(s =>
        //        //                     new EstimatorItem() { Item = s.price }).ToArray()),
        //        //            new DataEstimators(List.Select(s =>
        //        //                     new EstimatorItem() { Item = s.rank }).ToArray()));
        //    }
        //}
    }

    public class Sale
    {
        public double price;
        public DateTime date;
        public double rank;
        public double quantity;
    }


}
