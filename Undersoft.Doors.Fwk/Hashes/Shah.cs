using System.Text;
using System.Linq;
using System.Collections;
using System.Runtime.InteropServices;

namespace System.Doors
{
    public unsafe static class Shah
    {
        private static xxHash xHash = new xxHash(64);
        private static xxHash yHash = new xxHash(32);

        public static ulong ToUInt64(this Byte[] shah)
        {
            fixed (byte* pbyte = &shah[0])
                return *((ulong*)pbyte);
        }
        public static long ToInt64(this Byte[] shah)
        {
            fixed (byte* pbyte = &shah[0])
                return *((long*)pbyte);
        }

        public static byte[] ToNoid(this ulong shah)
        {
            byte[] q = new byte[8];
            fixed (byte* pbyte = q)
                *((ulong*)pbyte[0]) = shah;
            return q;
        }
        public static byte[] ToNoid(this long shah)
        {
            byte[] q = new byte[8];
            fixed (byte* pbyte = q)
                *((long*)pbyte[0]) = shah;
            return q;
        }

        public static Byte[] GetShah(this Byte[] shah)
        {
            return xHash.ComputeHash(shah); ;
        }
        public static Byte[] GetShah(this Object obj)
        {
            return xHash.ComputeHash(obj.SetMarshal());
        }
        public static Byte[] GetShah(this Object[] obj)
        {
            return xHash.ComputeHash(obj.SetMarshal());
        }
        public static Byte[] GetShah(this String obj)
        {
            return xHash.ComputeHash(obj.SetMarshal());
        }
        public static Byte[] GetShah(this IQuid shah)
        {
            return shah.GetBytes();
        }

        public static Byte[] GetPCHexShah(this Object obj)
        {
            byte[] shah = xHash.ComputeHash(obj.SetMarshal());
            fixed (byte* pbyte = &shah[0])
            {
                *((ushort*)(pbyte + 6)) = (ushort)(Math.Abs(*((short*)(pbyte + 6))) / 64);
                *((int*)(pbyte)) = Math.Abs(*((int*)(pbyte))) / 2;
            }
            return shah;
        }
        public static Byte[] GetPCHexShah(this Object[] obj)
        {
            byte[] shah = xHash.ComputeHash(obj.SetMarshal());
            fixed (byte* pbyte = &shah[0])
            {
                *((ushort*)(pbyte + 6)) = (ushort)(Math.Abs(*((short*)(pbyte + 6))) / 64);
                *((int*)(pbyte)) = Math.Abs(*((int*)(pbyte))) / 2;
            }
            return shah;
        }
        public static Byte[] GetPCHexShah(this String obj)
        {
            byte[] shah = xHash.ComputeHash(obj.SetMarshal());
            fixed (byte* pbyte = &shah[0])
            {
                *((ushort*)(pbyte+6)) = (ushort)(Math.Abs(*((short*)(pbyte + 6))) / 64);
                *((int*)(pbyte)) = Math.Abs(*((int*)(pbyte))) / 2;
            }
            return shah;
        }

        public static Byte[] ShahToPCHex(this Byte[] shah)
        {
            fixed (byte* pbyte = &shah[0])
            {
                *((ushort*)(pbyte + 6)) = (ushort)(Math.Abs(*((short*)(pbyte + 6))) / 64);
                *((int*)(pbyte)) = Math.Abs(*((int*)(pbyte))) / 2;
            }
            return shah;
        }

        public static Noid GetNoid(this IDataNative n, Int32 noid)
        {
            return ((Noid)n[noid]);
        }

        public static Byte[] GetShah(this IDataNative n, Int32 noid, Int32 cell = -1)
        {
            if (cell > -1)
                return n[cell].GetShah();
            else
                return ((Noid)n[noid]).GetBytes();
        }
        public static Byte[] GetShah(this IDataNative n, Int32[] keyids)
        {
            return keyids.Select(k => n[k]).ToArray().GetShah();
        }

        public static Byte[] GetShah32(this Byte[] shah)
        {
            return yHash.ComputeHash(shah); ;
        }
        public static Byte[] GetShah32(this Object obj)
        {
            return yHash.ComputeHash(obj.SetMarshal());
        }
        public static Byte[] GetShah32(this Object[] obj)
        {
            return yHash.ComputeHash(obj.SetMarshal());
        }
        public static Byte[] GetShah32(this String obj)
        {
            return yHash.ComputeHash(obj.SetMarshal());
        }
        public static Byte[] GetShah32(this IQuid shah)
        {
            return yHash.ComputeHash(shah.GetBytes());
        }

        public static Byte[] GetShah32(this IDataNative n, Int32 noid, Int32 cell)
        {
            if (cell >= 0)
                return n[cell].GetShah32();
            else
                return ((byte[])n[noid]);
        }

        public static Int32 GetShahCode32(this Object obj)
        {
            if (obj is Quid)
            {
                return ((Quid)obj).GetShahCode32();
            }
            byte[] buff = xHash.ComputeHash(obj.SetMarshal());
            fixed (byte* pbyte = buff)           
            {
                return ((((17 + *(int*)pbyte) * 23) + *(int*)(pbyte + 4)) * 23);
            }
        }
        public static Int32 GetShahCode32(this Object[] obj)
        {
            if (obj.Length == 1)
            {
                return obj[0].GetShahCode32();
            }
            byte[] buff = xHash.ComputeHash(obj.SetMarshal());
            fixed (byte* pbyte = buff)
            {
                return ((((17 + *(int*)pbyte) * 23) + *(int*)(pbyte + 4)) * 23);
            }
        }
        public static Int32 GetShahCode32(this String obj)
        {
            byte[] buff = xHash.ComputeHash(obj.SetMarshal());
            return new int[] { ((int)buff[3] << 24) | ((int)buff[2] << 16) | ((int)buff[1] << 8) | buff[0] ,
                               ((int)buff[7] << 24) | ((int)buff[6] << 16) | ((int)buff[5] << 8) | buff[4] }
                                .Aggregate(17, (a, b) => (a + b) * 23);
        }
        public static Int32 GetShahCode32(this IQuid obj)
        {
            byte[] buff = obj.GetBytes();
            fixed (byte* pbyte = buff)
            {
                return ((((17 + *(int*)pbyte) * 23) + *(int*)(pbyte + 4)) * 23);
            }
        }

        public static Int64 GetCompareLong(this Object obj, Type type)
        {
            if (obj is string)
            {
                if (type != typeof(string))
                {
                    if (type == typeof(Quid))
                        return new Quid((string)obj).LongValue;
                    if (type == typeof(DateTime))
                        return ((DateTime)Convert.ChangeType(obj, type)).Ticks;
                    return Convert.ToInt64(Convert.ChangeType(obj, type));
                }
                return obj.GetShahCode64();
            }

            if (type == typeof(Quid))
                return ((Quid)obj).LongValue;
            if (type == typeof(DateTime))
                return ((DateTime)obj).Ticks;
            if (obj is ValueType)
                return Convert.ToInt64(obj);
            return obj.GetShahCode64();
        }

        public static Double GetCompareCode(this Object obj)
        {
            if (obj is IQuid)
                return Convert.ToDouble(((IQuid)obj).LongValue);
            if (obj is DateTime)
                return ((DateTime)obj).ToOADate();
            if (obj is ValueType)
                    return Convert.ToDouble(obj);
            return Convert.ToDouble(obj.GetShahCode64());
        }

        public static Double GetCompareDouble(this Object obj, Type type)
        {
            if (obj is string)
            {
                if (type != typeof(string))
                {
                    if (type == typeof(Quid))
                        return Convert.ToDouble(new Quid((string)obj).LongValue);
                    if (type == typeof(DateTime))
                        return ((DateTime)Convert.ChangeType(obj, type)).ToOADate();
                    return Convert.ToDouble(Convert.ChangeType(obj, type));
                }
                return Convert.ToDouble(obj.GetShahCode64());
            }

            if (type == typeof(Quid))
                return Convert.ToDouble(((Quid)obj).LongValue);            
            if (type == typeof(DateTime))
                return ((DateTime)obj).ToOADate();
            if (obj is ValueType)
                return Convert.ToDouble(obj);
            return Convert.ToDouble(obj.GetShahCode64());
        }

        public static Int64 GetShahCode64(this Object obj)
        {
            return BitConverter.ToInt64(xHash.ComputeHash(obj.SetMarshal()), 0);
        }
        public static Int64 GetShahCode64(this Object[] obj)
        {
            fixed (byte* b = xHash.ComputeHash(obj.SetMarshal()))
                return *((long*)b);
        }
        public static Int64 GetShahCode64(this String obj)
        {
            fixed (byte* b = xHash.ComputeHash(obj.SetMarshal()))
                return *((long*)b);
        }
        public static Int64 GetShahCode64(this IQuid obj)
        {
            return obj.LongValue;
        }

        public static Byte[] GetMsShah(this Object obj)
        {
            return BitConverter.GetBytes(obj.GetHashCode())
                                  .Concat(BitConverter.GetBytes(
                                  obj.ToString()
                                  .Reverse().Select(c => (int)c).Aggregate(17, (x, y) => 23 * (x + y)).GetHashCode()))
                                  .ToArray();
        }
        public static Byte[] GetMsShah(this Object[] obj)
        {
            return BitConverter.GetBytes(obj.Select(h => h.GetHashCode()).Aggregate(17, (x, y) => 23 * (x + y)))
                                  .Concat(BitConverter.GetBytes(obj.Select(h => h.ToString()
                                      .Reverse().Select(c => (int)c).Aggregate(17, (x, y) => 23 * (x + y)).GetHashCode())
                                          .Aggregate(17, (x, y) => 23 * (x + y)))).ToArray();
        }
        public static Byte[] GetMsShah(this String obj)
        {
            return BitConverter.GetBytes(obj.GetHashCode())
                                  .Concat(BitConverter.GetBytes(
                                      obj.Reverse()
                                           .Select(c => (int)c).Aggregate(17, (x, y) => 23 * (x + y)).GetHashCode()))
                                               .ToArray();
        }

        public static UInt16[] GetDrive(this Byte[] b)
        {
            return new ushort[] { BitConverter.ToUInt16(b, 8), BitConverter.ToUInt16(b, 10), BitConverter.ToUInt16(b, 12) };
        }
        public static UInt16[] GetDrive(this IDataNative n, Int32 noid)
        {
            byte[] b = ((byte[])n[noid]);
            return new ushort[] { BitConverter.ToUInt16(b, 8), BitConverter.ToUInt16(b, 10), BitConverter.ToUInt16(b, 12) };
        }

        public static Byte[] FromHex(this String hex)
        {
            return HexToByte(hex);
        }
        public static String ToHex(this Byte[] ba, bool trim = false)
        {
            return ByteToHex(ba, trim);
        }

        public static Char[] ToChars(this Byte[] ba, CharCoding tf = CharCoding.ASCII)
        {
            switch (tf)
            {
                case CharCoding.ASCII:
                    return Encoding.ASCII.GetChars(ba);
                case CharCoding.UTF8:
                    return Encoding.UTF8.GetChars(ba);
                case CharCoding.Unicode:
                    return Encoding.Unicode.GetChars(ba);
                default:
                    return Encoding.ASCII.GetChars(ba);
            }
        }
        public static Char[] ToChars(this Byte ba, CharCoding tf = CharCoding.ASCII)
        {
            switch (tf)
            {
                case CharCoding.ASCII:
                    return Encoding.ASCII.GetChars(new byte[] { ba });
                case CharCoding.UTF8:
                    return Encoding.UTF8.GetChars(new byte[] { ba });
                case CharCoding.Unicode:
                    return Encoding.Unicode.GetChars(new byte[] { ba });
                default:
                    return Encoding.ASCII.GetChars(new byte[] { ba });
            }
        }

        public static Byte[] ToBytes(this String ca, CharCoding tf = CharCoding.ASCII)
        {
            switch (tf)
            {
                case CharCoding.ASCII:
                    return Encoding.ASCII.GetBytes(ca);
                case CharCoding.UTF8:
                    return Encoding.UTF8.GetBytes(ca);
                case CharCoding.Unicode:
                    return Encoding.Unicode.GetBytes(ca);
                default:
                    return Encoding.ASCII.GetBytes(ca);
            }
        }
        public static Byte[] ToBytes(this Char[] ca, CharCoding tf = CharCoding.ASCII)
        {
            switch (tf)
            {
                case CharCoding.ASCII:
                    return Encoding.ASCII.GetBytes(ca);
                case CharCoding.UTF8:
                    return Encoding.UTF8.GetBytes(ca);
                case CharCoding.Unicode:
                    return Encoding.Unicode.GetBytes(ca);
                default:
                    return Encoding.ASCII.GetBytes(ca);
            }
        }
        public static Byte[] ToBytes(this Char ca, CharCoding tf = CharCoding.ASCII)
        {
            switch (tf)
            {
                case CharCoding.ASCII:
                    return Encoding.ASCII.GetBytes(new char[] { ca });
                case CharCoding.UTF8:
                    return Encoding.UTF8.GetBytes(new char[] { ca });
                case CharCoding.Unicode:
                    return Encoding.Unicode.GetBytes(new char[] { ca });
                default:
                    return Encoding.ASCII.GetBytes(new char[] { ca });
            }
        }

        private static Byte[] SetMarshal(this Object objvalue)
        {
            if (objvalue is Quid)
            {
                return ((Quid)objvalue).GetBytes();
            }
            else if (objvalue is Enum)
            {
                return SetMarshalAsString(objvalue);
            }
            else if (objvalue is ValueType)
            {
                if(objvalue is DateTime)
                    return SetMarshalAsStruct(((DateTime)objvalue).ToBinary());
                else
                    return SetMarshalAsStruct(objvalue);
            }
            else
            {
                return SetMarshalAsString(objvalue);
            }
        }
        private static Byte[] SetMarshal(this Object[] objvalue)
        {
            int[] sizes = objvalue.Select(o => (o is Quid) ? 8 :
                                                    (o is Enum) ?
                                                      o.ToString().Length :                                                         
                                                         (o is ValueType) ?
                                                         (o is DateTime) ? 8 :
                                                           Marshal.SizeOf(o) :
                                                            (o != null) ?
                                                                o.ToString().Length : 0).ToArray();
            int offset = 0;
            int length = objvalue.Length;
            byte[] r = new byte[sizes.Sum()];
            GCHandle gc = GCHandle.Alloc(r, GCHandleType.Pinned);
            IntPtr ptr = gc.AddrOfPinnedObject();
            for (int i = 0; i < length; i++)
            {
                object o = objvalue[i];
                int size = sizes[i];
                if (size > 0)
                {
                    if (o is Quid)
                    {
                        *((long*)(ptr + offset).ToPointer()) = ((Quid)o).LongValue;
                    }
                    else if (o is Enum)
                    {
                        SetMarshalAsString(ptr, o, offset, (uint)size);
                    }
                    else if (o is ValueType)
                    {
                        if(o is DateTime)
                            Marshal.StructureToPtr(((DateTime)o).ToBinary(), ptr + offset, true);
                        else
                            Marshal.StructureToPtr(o, ptr + offset, true);
                    }
                    else
                    {
                        SetMarshalAsString(ptr, o, offset, (uint)size);
                    }
                    offset += size;
                }
            }
            gc.Free();
            return r;
        }

        private static void SetMarshalAsStruct(IntPtr dest, object o, int offset)
        {
            Marshal.StructureToPtr(o, dest + offset, true);
        }
        private static void SetMarshalAsString(IntPtr dest, object o, int offset, uint size)
        {
            IntPtr rawpointer = Marshal.StringToHGlobalAnsi(o.ToString());
            MemoryNative.Copy(dest + offset, rawpointer, (uint)size);
            //Marshal.Copy(rawpointer, r, offset, size);
            Marshal.FreeHGlobal(rawpointer);
        }
        private static byte[] SetMarshalAsStruct(object o)
        {
            byte[] rawvalue = new byte[Marshal.SizeOf(o)];
            GCHandle gc = GCHandle.Alloc(rawvalue, GCHandleType.Pinned);
            Marshal.StructureToPtr(o, gc.AddrOfPinnedObject(), true);
            gc.Free();
            return rawvalue;
        }
        private static byte[] SetMarshalAsString(object o)
        {
            string temp = o.ToString();
            int rawsize = temp.Length;
            byte[] rawvalue = new byte[rawsize];
            IntPtr rawpointer = Marshal.StringToHGlobalAnsi(temp);
            Marshal.Copy(rawpointer, rawvalue, 0, rawsize);
            Marshal.FreeHGlobal(rawpointer);
            return rawvalue;
        }

        private static Byte[] SetMarshal(this String objvalue)
        {
            int rawsize = objvalue.Length;
            IntPtr rawpointer = Marshal.StringToHGlobalAnsi((string)objvalue);
            byte[] rawvalue = new byte[rawsize];
            Marshal.Copy(rawpointer, rawvalue, 0, rawsize);
            Marshal.FreeHGlobal(rawpointer);
            return rawvalue;
        }
        //private static Byte[] SetMarshal(this Object objvalue)
        //{
        //    int rawsize = 0;
        //    IntPtr rawpointer = new IntPtr();
        //    if (objvalue.GetType() == typeof(string))
        //    {
        //        rawsize = ((string)objvalue).Length;
        //        rawpointer = Marshal.StringToHGlobalAnsi((string)objvalue);
        //    }
        //    else if (objvalue is Enum || objvalue is Type)
        //    {
        //        string temp = objvalue.ToString();
        //        rawsize = temp.Length;
        //        rawpointer = Marshal.StringToHGlobalAnsi(temp);
        //    }
        //    else
        //    {
        //        rawsize = Marshal.SizeOf(objvalue);
        //        rawpointer = Marshal.AllocHGlobal(rawsize);
        //        Marshal.StructureToPtr(objvalue, rawpointer, true);
        //    }
        //    byte[] rawvalue = new byte[rawsize];
        //    Marshal.Copy(rawpointer, rawvalue, 0, rawsize);
        //    Marshal.FreeHGlobal(rawpointer);
        //    return rawvalue;
        //}
        //private static Byte[] SetMarshal(this Object[] objvalue)
        //{
        //    int size = 0;
        //    int offset = 0;
        //    int length = objvalue.Length;
        //    IntPtr ptr = new IntPtr();
        //    byte[] r = new byte[0];
        //    for (int i = 0; i < length; i++)
        //    {
        //        if (objvalue[i] != null)
        //        {
        //            if (objvalue[i] is string)
        //            {
        //                size = ((string)objvalue[i]).Length;
        //                ptr = Marshal.StringToHGlobalAnsi((string)objvalue[i]);
        //            }
        //            else if (objvalue[i] is Enum || objvalue[i] is Type)
        //            {
        //                string temp = objvalue[i].ToString();
        //                size = temp.Length;
        //                ptr = Marshal.StringToHGlobalAnsi(temp);
        //            }
        //            else
        //            {
        //                size = Marshal.SizeOf(objvalue[i]);
        //                ptr = Marshal.AllocHGlobal(size);
        //                Marshal.StructureToPtr(objvalue[i], ptr, true);
        //            }
        //            Array.Resize(ref r, size + offset);
        //            Marshal.Copy(ptr, r, offset, size);
        //            offset += size;
        //        }
        //    }
        //    Marshal.FreeHGlobal(ptr);
        //    return r;
        //}
        private static String ByteToHex(Byte[] bytes, bool trim = false)
        {
            StringBuilder s = new StringBuilder();
            int length = bytes.Length;
            if (trim)
            {
                foreach (byte b in bytes)
                    if (b == 0)
                        length--;
                    else break;
            }
            for (int i = 0; i < length; i++)
                s.Append(bytes[i].ToString("x2").ToUpper());
            return s.ToString();
        }
        private static Byte[] HexToByte(String hex, int length = -1)
        {
            if (length < 0)
                length = hex.Length;
            byte[] r = new byte[length / 2];
            for (int i = 0; i < length - 1; i += 2)
            {
                byte a = GetHex(hex[i]);
                byte b = GetHex(hex[i + 1]);
                r[i / 2] = (byte)(a * 16 + b);
            }
            return r;
        }
        private static Byte GetHex(Char x)
        {
            if (x <= '9' && x >= '0')
            {
                return (byte)(x - '0');
            }
            else if (x <= 'z' && x >= 'a')
            {
                return (byte)(x - 'a' + 10);
            }
            else if (x <= 'Z' && x >= 'A')
            {
                return (byte)(x - 'A' + 10);
            }
            return 0;
        }

        public static bool Equals(this Byte[] b, Byte[] c)
        {
            if (b == null || c == null)
                return false;
            if (ReferenceEquals(b, c) || b.SequenceEqual(c))
                return true;
            return false;
        }

        public static bool IsSame(this Object obj, Object value)
        {
            if (obj != null)
                return obj.Equals(value);
            return (obj == null && value == null);
        }
        public static bool IsSameOrNull(this Object obj, Object value)
        {
            if (obj != null)
                return obj.Equals(value);
            return (obj == null && value == null);
        }

        public static bool TryGetValue(this Hashtable table, ref IQuid key, out object result)
        {
            if (!table.Contains(key))
            {
                result = null;
                return false;
            }
            result = table[key];
            return true;
        }

        public static Noid GetRelayNoid(this IDataCells tier, int[] keyOrdinal)
        {
            return new Noid(keyOrdinal.Select(x => tier.iN[x]).ToArray().GetShah());
        }
        public static Noid GetCubeRelayNoid(this IDataTier tier, int[] keyOrdinal)
        {
            return new Noid(keyOrdinal.Select(x => tier[x]).ToArray().GetShah());
        }

        public static Int64 GetMapKey64(this IDataTier tier, int[] keyOrdinal)
        {
            if (!tier.IsCube)
                return keyOrdinal.Select(x => tier.iN[x]).ToArray().GetShahCode64();
            return keyOrdinal.Select(x => tier[x]).ToArray().GetShahCode64();
        }       

        public static Int64 GetShahKey64(this IDataTier tier, int[] keyOrdinal)
        {
            return keyOrdinal.Select(x => tier.iN[x]).ToArray().GetShahCode64();
        }
        public static Int64 GetCubeShahKey64(this IDataTier tier, int[] keyOrdinal)
        {
            return keyOrdinal.Select(x => tier[x]).ToArray().GetShahCode64();
        }

        public static Int32 GetShahKey32(this IDataTier tier, int[] keyOrdinal)
        {
            return keyOrdinal.Select(x => tier.iN[x]).ToArray().GetShahCode32();
        }
        public static Int32 GetCubeShahKey32(this IDataTier tier, int[] keyOrdinal)
        {
            return keyOrdinal.Select(x => tier[x]).ToArray().GetShahCode32();
        }

        public static int NumberOfTrailingZeros(this int i)
        {            
            int y;
            if (i == 0) return 32;
            int n = 31;
            y = i << 16; if (y != 0) { n -= 16; i = y; }
            y = i << 8; if (y != 0) { n = n - 8; i = y; }
            y = i << 4; if (y != 0) { n = n - 4; i = y; }
            y = i << 2; if (y != 0) { n = n - 2; i = y; }
            return n - ((i << 1) >> 31);
        }

        public static uint HighestOneBit(this uint i)
        {
            i |= (i >> 1);
            i |= (i >> 2);
            i |= (i >> 4);
            i |= (i >> 8);
            i |= (i >> 16);
            return i - (i >> 1);
        }

    }

    public static class ParamsTool
    {
        public static Object[] Params(this Object[] obj, params object[] parameters)
        {
            if (obj == null || obj.Length == 0)
                obj = parameters;
            else if (parameters.Length > 0)
                obj = obj.Concat(parameters).ToArray();
            return obj;
        }

        public static Object[] Params(this Object obj, params object[] parameters)
        {
            if (obj == null)
                obj = parameters;
            else if (parameters.Length > 0)
                obj = new object[] { obj }.Concat(parameters).ToArray();
            return (object[])obj;
        }

        public static Object[] New(params object[] parameters)
        {
            return parameters;
        }
    }


        public enum CharCoding
    {
        ASCII,
        UTF8,
        Unicode
    }
}
