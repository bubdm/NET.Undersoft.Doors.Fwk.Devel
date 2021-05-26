using System;
using System.Collections.Generic;

namespace System.Doors
{
    public interface IDataNative
    {
        object this[int index] { get; set; }

        object this[string key] { get; set; }

        object[] PrimeArray { get; set; }

        //object ByteArray { get; }

        //T Cell<T>(int index);

        //object Deserialize(byte[] binary, Type type);

        //void CopyTo(IntPtr dest);
        //void CopyTo(byte[] dest);

        //void CopyFrom(IntPtr from);
        //void CopyFrom(byte[] from);

        //byte[] Serialize(object structure, Type t);
        //object Deserialize(byte[] binary, Type t);
    }
}