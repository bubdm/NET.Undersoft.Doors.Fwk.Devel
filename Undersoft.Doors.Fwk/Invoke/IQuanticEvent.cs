﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.Doors
{ 
    public interface IDorsEvent
    {
        DorsInvokeInfo InvokeInfo { get; set; }

        object Execute(params object[] parameters);
    }
}
