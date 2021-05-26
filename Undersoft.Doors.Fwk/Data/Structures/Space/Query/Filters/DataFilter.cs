using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using System.Doors.Data;
using System.Doors;

namespace System.Doors.Data
{
    [JsonIgnore]
    [Serializable]
    public class DataFilter
    {
        [NonSerialized] private FilterTerms termsBuffer;
        [NonSerialized] private FilterTerms termsReducer;
        [NonSerialized] private FilterExpression expression;
        [NonSerialized] private DataTrellis trell;
        [NonSerialized] public Func<DataTier, bool> Evaluator;

        public DataTrellis Trell
        { get { return trell; } set { trell = value; } }
        public FilterTerms Reducer
        { get; set; }
        public FilterTerms Terms
        { get; set; }

        public DataFilter(DataTrellis trell)
        {
            Trell = trell;
            expression = new FilterExpression();
            Reducer = new FilterTerms(trell);
            Terms = new FilterTerms(trell);
            termsBuffer = expression.Conditions;
            termsReducer = new FilterTerms(Trell);
        }
        
        public Expression<Func<DataTier, bool>> GetExpression(int stage = 1)
        {
            termsReducer.Clear();
            termsReducer.AddRange(Reducer.AsEnumerable().Concat(Terms.AsEnumerable()).ToArray());
            expression.Conditions = termsReducer;
            termsBuffer = termsReducer;
            return expression.CreateExpression(stage);
        }

        public DataTier[] Filter(int stage = 1)
        {
            termsReducer.Clear();
            termsReducer.AddRange(Reducer.AsEnumerable().Concat(Terms.AsEnumerable()).ToArray());
            expression.Conditions = termsReducer;
            termsBuffer = termsReducer;
            return Trell.Tiers.AsEnumerable().AsQueryable().Where(expression.CreateExpression(stage).Compile()).ToArray();
        }
        public DataTier[] Filter(ICollection<DataTier> toFilter, int stage = 1)
        {
            termsReducer.Clear();
            termsReducer.AddRange(Reducer.AsEnumerable().Concat(Terms.AsEnumerable()).ToArray());
            expression.Conditions = termsReducer;
            termsBuffer = termsReducer;
            return toFilter.AsQueryable().Where(expression.CreateExpression(stage).Compile()).ToArray();
        }       
    }
}
