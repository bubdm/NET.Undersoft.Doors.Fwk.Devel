using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Dors.Intelops
{
    public class EstimatorObjectCollection : Collection<EstimatorObject>
    {        
        public EstimatorObjectCollection()
        {

        }

        public EstimatorObjectCollection(IList<EstimatorObject> range)  //dowolna lista/kolekcja zawierajaca obiekty DataEstimator - nie musi byc lista
        {
            foreach (EstimatorObject de in range)
            {
                this.Add(de);
            }
        }

        //protected override void InsertItem(int index, EstimatorObject value)
        //{
        //    if (value.Item == typeof(ValueType))
        //    {
        //        IList<double> _value = Convert.ChangeType(value.Item, typeof(double))
        //        base.InsertItem(index, );
        //    }

        //    base.InsertItem(index, value);
        //}

        //protected override void SetItem(int index, EstimatorObject value)
        //{
        //    base.SetItem(index, value);
        //}

        //public new EstimatorObject this[int index]
        //{
        //    get
        //    {
        //        return this.Items[index];
        //    }

        //    set
        //    {
        //        this.Items[index] = value;
        //    }
        //}
    }
}
