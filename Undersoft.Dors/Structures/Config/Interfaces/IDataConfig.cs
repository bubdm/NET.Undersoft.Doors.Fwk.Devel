using System;
using System.Collections.Generic;

namespace System.Dors
{
    public interface IDataConfig
    {
        DataState State { get; set; }
        DataConfig Config { get; set; }
        bool Checked { get; set; }
        bool Saved { get; set; }
        bool Synced { get; set; }
    }
}