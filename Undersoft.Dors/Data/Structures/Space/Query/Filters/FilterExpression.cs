using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Dors;

namespace System.Dors.Data
{
    public class FilterExpression
    {
        private System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();        

        private Expression<Func<DataTier, bool>> Expression
        { get; set; }

        public Expression<Func<DataTier, bool>> Filter
        { get { return CreateExpression(Stage); } }
        public FilterTerms Conditions;
        public int Stage
        { get; set; }

        public FilterExpression()
        {
            Conditions = new FilterTerms();
            nfi.NumberDecimalSeparator = ".";
            Stage = 1;
        }

        public Expression<Func<DataTier, bool>> this[int stage]
        {
            get
            {
                return CreateExpression(stage);
            }
        }

        public Expression<Func<DataTier, bool>> CreateExpression(int stage = 1)
        {
            Expression<Func<DataTier, bool>> exps = null;
            List<FilterTerm> fcs = Conditions.Get(stage);
            Expression = null;
            LogicType previousLogic = LogicType.And;
            foreach (FilterTerm fc in fcs)
            {
                exps = null;
                if (fc.Operand != OperandType.Contains)
                {
                    if (Expression != null)
                        if (previousLogic != LogicType.Or)
                            Expression = Expression.And(CaseConditioner(fc, exps));
                        else
                            Expression = Expression.Or(CaseConditioner(fc, exps));
                    else
                        Expression = CaseConditioner(fc, exps);
                    previousLogic = fc.Logic;
                }
                else
                {
                    HashSet<int> list = new HashSet<int>((fc.Value.GetType() == typeof(string)) ? fc.Value.ToString().Split(';')
                                                         .Select(p => Convert.ChangeType(p, fc.FilterPylon.DataType).GetHashCode()) :
                                                         (fc.Value.GetType() == typeof(List<object>)) ? ((List<object>)fc.Value)
                                                         .Select(p => Convert.ChangeType(p, fc.FilterPylon.DataType).GetHashCode()) : null);

                    if (list != null && list.Count > 0)
                        exps = (r => list.Contains(r[fc.FilterPylon.PylonName].GetHashCode()));

                    if (Expression != null)
                        if (previousLogic != LogicType.Or)
                            Expression = Expression.And(exps);
                        else
                            Expression = Expression.Or(exps);
                    else
                        Expression = exps;
                    previousLogic = fc.Logic;
                }
            }
            return Expression;
        }
        private Expression<Func<DataTier, bool>> CaseConditioner(FilterTerm fc, Expression<Func<DataTier, bool>> ex)
        {
            if (fc.Value != null)
            {
                object Value = fc.Value;
                OperandType Operand = fc.Operand;
                if (Operand != OperandType.Like && Operand != OperandType.NotLike)
                {
                    switch (Operand)
                    {
                        case OperandType.Equal:
                            ex = (r => r[fc.FilterPylon.Ordinal] != null ? 
                            fc.FilterPylon.DataType == typeof(Quid) || fc.FilterPylon.DataType == typeof(string) || fc.FilterPylon.DataType == typeof(DateTime) ?
                            r[fc.FilterPylon.Ordinal].GetCompareLong(fc.FilterPylon.DataType).Equals(Value.GetCompareLong(fc.FilterPylon.DataType)) :
                            r[fc.FilterPylon.Ordinal].GetCompareDouble(fc.FilterPylon.DataType).Equals(Value.GetCompareDouble(fc.FilterPylon.DataType)) : false);
                            break;
                        case OperandType.EqualOrMore:
                            ex = (r => r[fc.FilterPylon.Ordinal] != null ?
                             fc.FilterPylon.DataType == typeof(Quid) || fc.FilterPylon.DataType == typeof(string) || fc.FilterPylon.DataType == typeof(DateTime) ?
                              r[fc.FilterPylon.Ordinal].GetCompareLong(fc.FilterPylon.DataType) >= (Value.GetCompareLong(fc.FilterPylon.DataType)) :
                            r[fc.FilterPylon.Ordinal].GetCompareDouble(fc.FilterPylon.DataType) >= (Value.GetCompareDouble(fc.FilterPylon.DataType)) : false);
                            break;
                        case OperandType.More:
                            ex = (r => r[fc.FilterPylon.Ordinal] != null ?
                             fc.FilterPylon.DataType == typeof(Quid) || fc.FilterPylon.DataType == typeof(string) || fc.FilterPylon.DataType == typeof(DateTime) ?
                              r[fc.FilterPylon.Ordinal].GetCompareLong(fc.FilterPylon.DataType) > (Value.GetCompareLong(fc.FilterPylon.DataType)) :
                            r[fc.FilterPylon.Ordinal].GetCompareDouble(fc.FilterPylon.DataType) > (Value.GetCompareDouble(fc.FilterPylon.DataType)) : false);
                            break;
                        case OperandType.EqualOrLess:
                            ex = (r => r[fc.FilterPylon.Ordinal] != null ?
                             fc.FilterPylon.DataType == typeof(Quid) || fc.FilterPylon.DataType == typeof(string) || fc.FilterPylon.DataType == typeof(DateTime) ?
                              r[fc.FilterPylon.Ordinal].GetCompareLong(fc.FilterPylon.DataType) <= (Value.GetCompareLong(fc.FilterPylon.DataType)) :
                            r[fc.FilterPylon.Ordinal].GetCompareDouble(fc.FilterPylon.DataType) <= (Value.GetCompareDouble(fc.FilterPylon.DataType)) : false);
                            break;
                        case OperandType.Less:
                            ex = (r => r[fc.FilterPylon.Ordinal] != null ?
                             fc.FilterPylon.DataType == typeof(Quid) || fc.FilterPylon.DataType == typeof(string) || fc.FilterPylon.DataType == typeof(DateTime) ?
                              r[fc.FilterPylon.Ordinal].GetCompareLong(fc.FilterPylon.DataType) < (Value.GetCompareLong(fc.FilterPylon.DataType)) :
                            r[fc.FilterPylon.Ordinal].GetCompareDouble(fc.FilterPylon.DataType) < (Value.GetCompareDouble(fc.FilterPylon.DataType)) : false);
                            break;
                        default:
                            break;
                    }
                }
                else if (Operand != OperandType.NotLike)
                    ex = (r => r[fc.FilterPylon.Ordinal] != null ? Convert.ChangeType(r[fc.FilterPylon.Ordinal], fc.FilterPylon.DataType).ToString().Contains(Convert.ChangeType(Value, fc.FilterPylon.DataType).ToString()) : false);
                else
                    ex = (r => r[fc.FilterPylon.Ordinal] != null ? !Convert.ChangeType(r[fc.FilterPylon.Ordinal], fc.FilterPylon.DataType).ToString().Contains(Convert.ChangeType(Value, fc.FilterPylon.DataType).ToString()): false);
            }
            return ex;
        }
    }
}
