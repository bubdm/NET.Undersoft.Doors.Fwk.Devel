using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace System.Dors
{
    public interface IDataGridStyle
    {
        void RefreshGrid();
        void GetGridStyle(string tablemapping);
        void SetGridStyle(object _dataGrid, string mapping);
        void EnumComboField(Type enumtype, string mapping, string header);
        void PylonComboField(object pylons, string mapping, string header);
        void DataTypeComboField(string mapping, string header);
        void CustomComboField(object _tiers, string mapping, string header, string value, string display);
        void CustomTextField(string mapping, string header);
        void CustomBoolField(string mapping, string header);
    }
}