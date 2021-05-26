using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;

namespace System.Doors
{
    public interface IDataGridBinder
    {
        object BindingSource { get; }

        IDepotSync DepotSync { get; set; }
        IDataGridBinder BoundedGrid { get; set; }
        IDataGridStyle GridStyle { get; set; }
    }
}