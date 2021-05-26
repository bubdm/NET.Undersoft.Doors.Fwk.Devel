using System;

namespace System.Doors
{   
    public interface IQuid
    {
        long LongValue { get; set; }

        Int32 GetIntValue(int pos);
        void SetIntValue(int pos, int value);

        bool Int32Equals(IQuid obj, int pos);
        bool Int64Equals(IQuid obj);

        bool Equals(IQuid obj);
        bool Equals(long obj);

        IQuid Empty { get; }

        byte[] GetBytes();
    }
}