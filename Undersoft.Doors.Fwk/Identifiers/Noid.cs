using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Runtime.CompilerServices; //RR for inline compiling 

namespace System.Doors
{

    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public unsafe struct Noid : IFormattable, IComparable
        , IComparable<Noid>, IEquatable<Noid>, IQuid
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        private byte[] q;

        public int  GetIntValue(int pos)
        {
            fixed (byte* pbyte = &q[pos])
                return *((int*)pbyte);
        }
        public void SetIntValue(int pos, int value)
        {
            fixed (byte* b = &q[pos])
                *((int*)b) = value;
        }

        public long LongValue
        {
            get { return KeyLongValue; }
            set { KeyLongValue = value; }
        }

        public long    KeyLongValue
        {
            get
            {
                fixed (byte* pbyte = q)
                    return *((long*)pbyte);
            }
            set
            {
                fixed (byte* b = q)
                    *((long*)b) = value;
            }
        }
        public ushort  DriveShortValue
        {
            get
            {
                fixed (byte* pbyte = &q[8])
                    return *((ushort*)pbyte);
            }
            set
            {
                fixed (byte* b = &q[8])
                    *((ushort*)b) = value;
            }
        }
        public ushort  SectorShortValue
        {
            get
            {
                fixed (byte* pbyte = &q[10])
                    return *((ushort*)pbyte);
            }
            set
            {
                fixed (byte* b = &q[10])
                    *((ushort*)b) = value;
            }
        }
        public ushort  LineShortValue
        {
            get
            {
                fixed (byte* pbyte = &q[12])
                    return *((ushort*)pbyte);
            }
            set
            {
                fixed (byte* b = &q[12])
                    *((ushort*)b) = value;
            }
        }
        public ushort  StateShortValue
        {
            get
            {
                fixed (byte* pbyte = &q[14])
                    return *((ushort*)pbyte);
            }
            set
            {
                fixed (byte* b = &q[14])
                    *((ushort*)b) = value;
            }
        }
        public long    ClockLongValue
        {
            get
            {
                fixed (byte* pbyte = &q[16])
                    return *((long*)pbyte);
            }
            set
            {
                fixed (byte* b = &q[16])
                    *((long*)b) = value;
            }
        }

        public Noid(long l)
        {
            q = new byte[24];
            KeyLongValue = l;
            fixed (byte* b = &q[16])
                *((long*)b) = DateTime.Now.ToBinary();

        }
        public Noid(string s)
        {
            //q = s.FromHex();

            q = new byte[24];
            SetPCHexByChar(s.ToCharArray());    //RR
        }
        public Noid(byte[] b)
        {
            q = b;
        }

        //RR: to nie dzaialalo, literowka byla, zabraklo "&" przy n
        public Noid(byte[] a, short b, short c, short d, short e, long f)
        {          
            q = new byte[24];
            fixed (byte* n = q)
            {
                fixed (byte* s = a)
                    *((long*)n) = *((long*)s);
                *((short*)(n + 8)) = b;            
                *((short*)(n + 10)) = c;
                *((short*)(n + 12)) = d;
                *((short*)(n + 14)) = e;
                *((long*)(n + 16)) = f;
            }
        }

        //RR: to nie dzaialalo, literowka byla, zabraklo "&" przy n
        public Noid(object[] a, short b, short c, short d, BitArray e, DateTime f)
        {
            q = new byte[24];
            byte[] shah = a.GetShah();
            byte[] state = new byte[2];
            e.CopyTo(state, 0);

            fixed (byte* n = q)
            {
                fixed(byte* s = shah)
                    *((long*)n) = *((long*)s);
                *((short*)(n + 8)) = b;
                *((short*)(n + 10)) = c;
                *((short*)(n + 12)) = d;
                fixed (byte* s = state)
                    *((long*)(n + 14)) = *((ushort*)s);
                *((long*)(n + 16)) = f.ToBinary();    //TODO: f.Tick - rok 2018.01.01 w tikach
            }
        }
        
        public byte[] this[int offset]
        {
            get
            {
                if (offset == 0)
                    return q;
                else
                {
                    byte[] r = new byte[24 - offset];
                    fixed (byte* pbyte = q)
                    fixed (byte* rbyte = r)
                    {
                        MemoryNative.Copy(rbyte, (pbyte + offset), (uint)(24 - offset));
                    }
                    return r;
                }
            }
            set
            {
                int l = value.Length;
                int s = 24 - offset;
                if (offset == 0 && l == 24)
                    q = value;
                else
                    fixed (byte* b = value)
                    fixed (byte* pbyte = q)
                    {
                        MemoryNative.Copy(pbyte + offset, b, (uint)(s > l ? l : s));
                    }
            }
        }
        public byte[] this[int offset, int length]
        {
            get
            {
                if (offset == 0 && length == 24)
                    return q;
                else
                {
                    byte[] r = new byte[24 - offset];
                    fixed (byte* pbyte = q)
                        Marshal.Copy((IntPtr)(pbyte + offset), r, 0, length);
                    return r;
                }
            }
            set
            {
                int l = value.Length;
                int s = 24 - offset;
                if (offset == 0 && length == 24 && l == 24)
                    q = value;
                else
                    fixed (byte* b = value)
                        Marshal.Copy((IntPtr)b, q, offset, s > l ? l < length ? l : length : s < length ? s : length);
            }
        }

        private byte[] HexToByte(String s)
        {         
            return s.FromHex();
        }

        private string ByteToHex(Byte[] bytes)
        {
            return bytes.ToHex();
        }

        private byte GetHex(Char x)
        {
            if (x <= '9' && x >= '0')
                return (byte)(x - '0');
            else if (x <= 'Z' && x >= 'A')
                return (byte)(x - 'A' + 10);
            else if (x <= 'z' && x >= 'a')
                return (byte)(x - 'a' + 10);
            return 0xFF;
        }

        public byte[] GetBytes()
        {
            return q;
        }

        public int    GetPrimeId(int drivesize, int sectorsize)
        {
            return (GetDriveId() * drivesize * sectorsize) + (GetSectorId() * sectorsize) + GetLineId();
        }
        public ushort GetDriveId()
        {
            fixed (byte* pbyte = &q[8])
                return *((ushort*)pbyte);
        }
        public short  GetSectorId()
        {
            fixed (byte* pbyte = &q[10])
                return *((short*)pbyte);
        }
        public short  GetLineId()
        {
            fixed (byte* pbyte = &q[12])
                return *((short*)pbyte);
        }

        public short    GetStateShort()
        {
            fixed (byte* pbyte = &q[14])
                return *((short*)pbyte);
        }
        public BitArray GetStateBits()
        {
            fixed (byte* pbyte = &q[14])
                return new BitArray(new byte[] { *((byte*)pbyte), *((byte*)(pbyte + 1)) });
        }

        public long     GetDateLong()
        {
            fixed (byte* pbyte = &q[16])
                return *((long*)pbyte);
        }
        public DateTime GetDateTime()
        {            
            fixed (byte* pbyte = &q[16])
                return DateTime.FromBinary(*((long*)pbyte));
        }

        public bool IsNull
        {
            get
            {
                if (q == null)
                    return true;
                return false;
            }
            set
            {
                if (value) q = null;
            }
        }

        public bool IsNotEmpty
        {
            get { return (!IsNull && KeyLongValue != 0); }
        }

        public bool IsEmpty
        {
            get { return (IsNull || KeyLongValue == 0); }
        }

        public override int GetHashCode()
        {
            fixed (byte* pbyte = &this[0,8].GetShah32()[0])
                return *((int*)pbyte);
        }

        public int CompareTo(object value)
        {
            if (value == null)
                return 1;
            if (!(value is Noid))
                throw new Exception();

            return (int)(KeyLongValue - ((Noid)value).KeyLongValue);
        }

        public int CompareTo(Noid g)
        {
            return (int)(KeyLongValue - g.KeyLongValue);
        }

        public bool Equals(long g)
        {
            return (LongValue == g);
        }

        public override bool Equals(object value)
        {
            if (value == null || q == null)
                return false;
            if ((value is string))
                return new Noid(value.ToString()).KeyLongValue == KeyLongValue;

            return (KeyLongValue == ((Noid)value).KeyLongValue);
        }

        //czy porownanie ma tylko porownywac klucze, czy wszystkie pola q?
        //Dodalem nizej metode EqualsContent, gdzie porownuje zawartosc
        public bool Equals(Noid g)
        {
            return (KeyLongValue == g.KeyLongValue);
        }
        public bool Equals(IQuid g)
        {
            return (KeyLongValue == g.LongValue);
        }
        public bool Int32Equals(IQuid g, int offset)
        {
            return GetIntValue(offset) == g.GetIntValue(offset);        
        }
        public bool Int64Equals(IQuid g)
        {
            return (KeyLongValue == g.LongValue);
        }

        public override String ToString()
        {
            //return ByteToHex(q);
            if (q == null)
                q = new byte[24];
            return new string(GetPCHexChar());  //RR
        }

        public String ToString(String format)
        {
            return ToString(format, null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            //return ByteToHex(q);
            if (q == null)
                q = new byte[24];
            return new string(GetPCHexChar());  //RR
        }

        public static bool operator ==(Noid a, Noid b)
        {
            return (a.KeyLongValue == b.KeyLongValue);
        }

        public static bool operator !=(Noid a, Noid b)
        {
            return (a.KeyLongValue != b.KeyLongValue);
        }

        public static Noid Empty
        {
            get { return new Noid(new byte[24]); }
        }      

        IQuid IQuid.Empty
        {
            get
            {
                return new Noid(new byte[24]);
            }
        }

        //-------------- RR -----------------//
        //TODO: mapowanie q na 3 long -> operacje bitowe
        //przyciecie zer nieznaczacych (Clock)
        //Jezeli ostatni 
        public Noid(long a, short b, short c, short d, short e, long f)
        {
            q = new byte[24];
                                    
            fixed (byte* n = q)
            {
                *((long*)n) = a;
                *((short*)&n[8]) = b;
                *((short*)&n[10]) = c;
                *((short*)&n[12]) = d;
                *((short*)&n[14]) = e;
                *((long*)&n[16]) = f;
            }
        }

        /* standard:             3 longs  x 8 bytes x 8 bits = 192 bits - each byte into 2 chars, i.e., 48 chars         
         * PCHex representation: 4 blocks x 8 PCHex x 6 bits = 192 bits - full utilization of bits; 
         * each PCHex into one char, i.e., max 32 chars
         */
        public char[] GetPCHexChar()
        {
            char[] pchchar = new char[32];
            long pchblock;  
            int pchlength = 32;
            byte pchbyte;
            int idx = 0;

            for (int j = 0; j < 4; j++)
            {
                fixed (byte* pbyte = &q[j * 6])
                {
                    pchblock = *((long*)pbyte);
                }
                pchblock = pchblock & 0x0000ffffffffffffL;  //each block has 6 bytes
                for (int i = 0; i < 8; i++)
                {
                    pchbyte = (byte)(pchblock & 0x3fL);                    
                    pchchar[idx] = PhexConvertByteToChar(pchbyte);
                    idx++;                    
                    pchblock = pchblock >> 6;
                    if (pchbyte != 0x00) pchlength = idx;
                }
            }
                        
            char[] pchchartrim = new char[pchlength];
            Array.Copy(pchchar, 0, pchchartrim, 0, pchlength);

            return pchchartrim;            
        }

        public void SetPCHexByChar(char[] pchchar)
        {
            int pchlength = pchchar.Length;
            int idx = 0;
            byte pchbyte;
            long pchblock = 0;
            int blocklength = 8;
            int pchblock_int;
            short pchblock_short;

            for (int j = 0; j < 4; j++)
            {
                pchblock = 0x00L;
                blocklength = Math.Min(8, Math.Max(0, pchlength - 8 * j));        //required if trimmed zeros, length < 32
                idx = Math.Min(pchlength, 8*(j+1)) - 1;                           //required if trimmed zeros, length <32

                for (int i = 0; i < blocklength; i++)     //8 chars per block, each 6 bits
                {
                    pchbyte = PhexConvertCharToByte(pchchar[idx]);
                    pchblock = pchblock << 6;
                    pchblock = pchblock | (pchbyte & 0x3fL);
                    idx--;
                }
                fixed (byte* pbyte = q)
                {
                    if (j == 3) //ostatnie nalozenie - block3 przekracza o 2 bajty rozmiar q!!!! tych 2 bajty sa 0, ale uniknac ewentualne wejscia w pamiec poza q
                    {
                        pchblock_short = (short)(pchblock & 0x00ffffL);
                        pchblock_int = (int)(pchblock >> 16);
                        *((long*)&pbyte[18]) = pchblock_short;
                        *((long*)&pbyte[20]) = pchblock_int;
                        break;
                    }
                    *((long*)&pbyte[j * 6]) = pchblock;

                }
            }                                    
        }

        public bool EqualsContent(Noid g)
        {
            long pchblockA, pchblockB, pchblockC;
            bool result;

            if (g == null) return false;
            fixed (byte* pbyte = q)
            {
                pchblockA = *((long*)&pbyte[0]);
                pchblockB = *((long*)&pbyte[8]);
                pchblockC = *((long*)&pbyte[16]);
            }

            fixed (byte* pbyte = g.q)
            {
                result = (pchblockA  == * ((long*)&pbyte[0]))
                && (pchblockB == *((long*)&pbyte[8]))
                && (pchblockC == *((long*)&pbyte[16]));
            }
            
            return result;
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte PhexConvertCharToByte(char phchar)
        {
            if (phchar <= '.')
                return (byte)(phchar + 17); //0-9
            else if (phchar <= '9')
                return (byte)(phchar - 48); //A-Z
            else if (phchar <= 'Z')
                return (byte)(phchar - 55); //a-z
            return (byte)(phchar - 61);      //- or .
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char PhexConvertByteToChar(byte phbyte)
        {
            if (phbyte <= 9)
                return (char)(phbyte + 48); //0-9
            else if (phbyte <= 35)
                return (char)(phbyte + 55); //A-Z
            else if (phbyte <= 61)
                return (char)(phbyte + 61); //a-z
            return (char)(phbyte - 17);      //- or .
        }

        // about 60% slower than Convert using "if-else" in for(i=...) tests
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char PhexConvertByteToCharNoIf(byte phbyte)
        {
            char phchar;
            byte a, b, c, d, e, f;
            a = (byte)((phbyte) & 0x01);
            b = (byte)((phbyte >> 1) & 0x01);
            c = (byte)((phbyte >> 2) & 0x01);
            d = (byte)((phbyte >> 3) & 0x01);
            e = (byte)((phbyte >> 4) & 0x01);
            f = (byte)((phbyte >> 5) & 0x01);

            phchar =
                (char)(phbyte +
                ((1 - f) & (1 - e) & ( (1 - d) | d & (1 - c) & (1 - b))) * (48)    //0-9
                + ((1 - f) & (1 - e) & d & ( (1 - c) & b | c) | (1 - f) & e | f & (1 - e) & (1 - d) & (1 - c)) * (55)  //A-Z
                + (f & (1 - e) & ( c | d) | f & e & ((1 - d) | (1 - c)) | f & e & d & c & (1 - b)) * (61)  //a-z
                + (f & e & d & c & b) * (-17));  //- or .

            return phchar;        
        }

        // about 10% slower than convert using "if-else" in for(i=...) tests
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte PhexConvertCharToByteNoIf(char phchar)
        {
            byte phbyte;
            byte a, b, c;

            //it is based on the following rule:
            //                              cba
            //- . => 2Dh, 2Eh => [2]    => [010]
            //0-9 => 30h..39h => [3]    => [011]
            //A-Z => 41h..5Ah => [4,5]  => [100] v [101]
            //a-z => 61h..7Ah => [6, 7] => [110] v [111]

            a = (byte)((phchar >> 4) & 0x01);
            b = (byte)((phchar >> 5) & 0x01);
            c = (byte)((phchar >> 6) & 0x01);

            phbyte = (byte)(phchar
                + (byte)((1 - c) * b * (1 - a) * (17) //- or .
                + (1 - c) * b * a * (-48)               //0-9
                + c * (1 - b) * (-55)                   //A-Z
                + c * b * (-61)));                      //a-z

            return phbyte;
        }
    }

    //TODO: 
    //rozdzielczosc milisekundy, a rok zerowy to 2000 r., ewentualnie 2018.01.01
    //RR zastanowic sie nad tym czasem ile bajtow 

    public enum NoidSegment
    {
        LongKey,
        FirstKey,
        SecondKey,
        PrimeId,
        DriveId,
        SectorId,
        LineId,
        State,
        Clock
    }
}
