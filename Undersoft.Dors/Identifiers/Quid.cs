using System.Text;
using System.Runtime.InteropServices;

using System.Runtime.CompilerServices; //RR for inline compiling 

namespace System.Dors
{

    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public unsafe struct Quid : IFormattable, IComparable
        , IComparable<Quid>, IEquatable<Quid>, IQuid
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        private byte[] q;

        public long LongValue
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

        private long _LongValue
        {
            get
            {
                return (LongValue << 32) | ((LongValue >> 16) & 0xffff0000) | (LongValue >> 48);
            }
            set
            {
                LongValue = (value >> 32) | (((value & 0x0ffff0000) << 16)) | (value << 48);
            }
        }

        public Quid(long l)
        {
            q = new byte[8];
            LongValue = l;
        }
        //public Quid(string s)
        //{
        //    q = new byte[8];
        //    int i = 0;
        //    q[0] = (byte)(GetHex(s[0]) * 16 + GetHex(s[1]));
        //    q[1] = (byte)(GetHex(s[2]) * 16 + GetHex(s[3]));
        //    if (GetHex(s[4]) > 16) i++;
        //    q[2] = (byte)(GetHex(s[4 + i]) * 16 + GetHex(s[5 + i]));
        //    q[3] = (byte)(GetHex(s[6 + i]) * 16 + GetHex(s[7 + i]));
        //    if (GetHex(s[8 + i]) > 16) i++;
        //    q[4] = (byte)(GetHex(s[8 + i]) * 16 + GetHex(s[9 + i]));
        //    q[5] = (byte)(GetHex(s[10 + i]) * 16 + GetHex(s[11 + i]));
        //    q[6] = (byte)(GetHex(s[12 + i]) * 16 + GetHex(s[13 + i]));
        //    q[7] = (byte)(GetHex(s[14 + i]) * 16 + GetHex(s[15 + i]));
        //}       
        public Quid(string ca)
        {
            q = new byte[8];
            SetPCHexByChar(ca.ToCharArray());
        }                                                  
        public Quid(byte[] b)
        {
            q = b;
        }
        public Quid(ushort a, short b, int c)
        {        
            q = new byte[8];
            q[7] = (byte)(a >> 8);
            q[6] = (byte)(a);
            q[5] = (byte)(b >> 8);
            q[4] = (byte)(b);
            q[3] = (byte)(c >> 24);
            q[2] = (byte)(c >> 16);
            q[1] = (byte)(c >> 8);
            q[0] = (byte)(c);
        }
        
        public byte[] this[int offset]
        {
            get
            {
                byte[] r = new byte[8 - offset];
                fixed (byte* pbyte = &q[0])
                    Marshal.Copy(Marshal.AllocHGlobal(new IntPtr(pbyte)), r, offset, 8 - offset);
                return r;
            }
            set
            {
                fixed (byte* b = &q[offset])
                    Marshal.Copy(value, 0, new IntPtr(b), 8 - offset);
            }
        }

        public int GetIntValue(int pos)
        {
            fixed (byte* pbyte = &q[pos])
                return *((int*)pbyte);
        }
        public void SetIntValue(int pos, int value)
        {
            fixed (byte* b = &q[pos])
                *((int*)b) = value;
        }

        private byte[] HexToByte(String s)
        {
            byte[] r = new byte[8];
            int i = 0;
            r[0] = (byte)(GetHex(s[0]) * 16 + GetHex(s[1]));
            r[1] = (byte)(GetHex(s[2]) * 16 + GetHex(s[3]));
            if (GetHex(s[4]) > 16) i++;
            r[2] = (byte)(GetHex(s[4 + i]) * 16 + GetHex(s[5 + i]));
            r[3] = (byte)(GetHex(s[6 + i]) * 16 + GetHex(s[7 + i]));
            if (GetHex(s[8 + i]) > 16) i++;
            r[4] = (byte)(GetHex(s[8 + i]) * 16 + GetHex(s[9 + i]));
            r[5] = (byte)(GetHex(s[10 + i]) * 16 + GetHex(s[11 + i]));
            r[6] = (byte)(GetHex(s[12 + i]) * 16 + GetHex(s[13 + i]));
            r[7] = (byte)(GetHex(s[14 + i]) * 16 + GetHex(s[15 + i]));
            return r;
        }

        private string ByteToHex(Byte[] bytes)
        {
            StringBuilder s = new StringBuilder();

            s.Append(bytes[0].ToString("x2").ToUpper());
            s.Append(bytes[1].ToString("x2").ToUpper());
            s.Append("-");
            s.Append(bytes[2].ToString("x2").ToUpper());
            s.Append(bytes[3].ToString("x2").ToUpper());
            s.Append("-");
            s.Append(bytes[4].ToString("x2").ToUpper());
            s.Append(bytes[5].ToString("x2").ToUpper());
            s.Append(bytes[6].ToString("x2").ToUpper());
            s.Append(bytes[7].ToString("x2").ToUpper());

            return s.ToString();
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

        public ushort GetClusterId()
        {
            fixed (byte* pbyte = &q[6])
                return *((ushort*)pbyte);
        }

        public short GetDataId()
        {
            fixed (byte* pbyte = &q[4])
                return *((short*)pbyte);
        }

        public int GetConfigId()
        {
            fixed (byte* pbyte = &q[6])
                return *((int*)pbyte);
        }

        public int GetPrimeId()
        {
            fixed (byte* pbyte = &q[0])
                return *((int*)pbyte);
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
            get { return (!IsNull && GetPrimeId() != 0); }
        }

        public bool IsEmpty
        {
            get { return (IsNull || GetPrimeId() == 0); }
        }

        public override int GetHashCode()
        {
            fixed (byte* pbyte = &q.GetShah32()[0])
                return *((int*)pbyte);
        }

        public int CompareTo(object value)
        {
            if (value == null)
                return 1;
            if (!(value is Quid))
                throw new Exception();

            return (int)(LongValue - ((Quid)value).LongValue);
        }

        public int CompareTo(Quid g)
        {
            return (int)(LongValue - g.LongValue);
        }

        public override bool Equals(object value)
        {
            if (value == null || q == null)
                return false;
            if ((value is string))
                return new Quid(value.ToString()).LongValue == LongValue;

            return (LongValue == ((Quid)value).LongValue);
        }

        public bool Equals(long g)
        {
            return (LongValue == g);
        }
        public bool Equals(Quid g)
        {
            return (LongValue == g.LongValue);
        }
        public bool Equals(IQuid g)
        {
            return (LongValue == g.LongValue);
        }
        public bool Equals(string g)
        {
            return (LongValue == new Quid(g).LongValue);
        }
        public bool Int32Equals(IQuid g, int offset)
        {
            return GetIntValue(offset) == g.GetIntValue(offset);
        }
        public bool Int64Equals(IQuid g)
        {
            return (LongValue == g.LongValue);
        }

        public override String ToString()
        {           
            return new string(GetPCHexChar());
        }                                               

        public String ToString(String format)
        {
            return ToString(format, null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {           
            return new string(GetPCHexChar()); 
        }           

        public static bool operator ==(Quid a, Quid b)
        {
            return (a.LongValue == b.LongValue);
        }

        public static bool operator !=(Quid a, Quid b)
        {
            return (a.LongValue != b.LongValue);
        }

        public static Quid Empty
        {
            get { return new Quid(new byte[8]); }
        }

        IQuid IQuid.Empty
        {
            get
            {
                return new Quid(new byte[8]);
            }
        }

        //--------------- RR ----------------//

        public byte[] GetPCHexByte()
        {
            byte[] pchbyte = new byte[10];
            long pchlong;

            long _longValue = _LongValue;
            //56-bit representation: //PrimeId [max 2^30] //DataId [max 2^16] //ClusterId [max 2^10]
            pchlong = ((_longValue & 0x3fffffff00000000L) >> 6) | ((_longValue & 0xffff0000L) >> 6) | (_longValue & 0x03ffL);
            for (int i = 0; i < 10; i++)
            {
                pchbyte[i] = (byte)(pchlong & 0x003fL);
                pchlong = pchlong >> 6;
            }
            return pchbyte;
        }

        public char[] GetPCHexChar()
        {
            char[] pchchar = new char[10];
            long pchlong;
            byte pchbyte;
            int pchlength = 0;
            long _longValue = _LongValue;
            //56-bit representation: //PrimeId [max 2^30] //DataId [max 2^16] //ClusterId [max 2^10]
            //i.e., bits: [Prime: 55..26][DataId: 25..10][ClusterId: 9..0]
            pchlong = ((_longValue & 0x3fffffff00000000L) >> 6) | ((_longValue & 0xffff0000L) >> 6) | (_longValue & 0x03ffL);
            for (int i = 0; i < 5; i++)
            {
                pchbyte = (byte)(pchlong & 0x003fL);
                pchchar[i] = PhexConvertByteToChar(pchbyte);
                pchlong = pchlong >> 6;
            }

            pchlength = 5;

            //Trim PrimeId
            for (int i = 5; i < 10; i++)
            {
                pchbyte = (byte)(pchlong & 0x003fL);
                if (pchbyte != 0x00) pchlength = i + 1;
                pchchar[i] = PhexConvertByteToChar(pchbyte);
                pchlong = pchlong >> 6;
            }

            char[] pchchartrim = new char[pchlength];
            Array.Copy(pchchar, 0, pchchartrim, 0, pchlength);

            return pchchartrim;
        }

        public void SetPCHexByByte(byte[] pchbyte)
        {
            long pchlong = 0;

            //bits: [Prime: 55..26][DataId: 25..10][ClusterId: 9..0]
            pchlong = pchbyte[9] & 0x3fL;
            for (int i = 8; i >= 0; i--)
            {
                pchlong = pchlong << 6;
                pchlong = pchlong | (pchbyte[i] & 0x3fL);
            }

            _LongValue = ((pchlong << 6) & 0x3fffffff00000000L) | ((pchlong << 6) & 0xffff0000L) | (pchlong & 0x03ffL);
        }

        public void SetPCHexByChar(char[] pchchar)
        {
            long pchlong = 0;
            byte pchbyte;
            int pchlength = 0;

            //bits: [Prime: 55..26][DataId: 25..10][ClusterId: 9..0]
            pchlength = pchchar.Length;
            pchbyte = PhexConvertCharToByte(pchchar[pchlength - 1]);
            pchlong = pchbyte & 0x3fL;
            for (int i = pchlength - 2; i >= 0; i--)
            {
                pchbyte = PhexConvertCharToByte(pchchar[i]);
                pchlong = pchlong << 6;
                pchlong = pchlong | (pchbyte & 0x3fL);
            }
            _LongValue = ((pchlong << 6) & 0x3fffffff00000000L) | ((pchlong << 6) & 0xffff0000L) | (pchlong & 0x03ffL);
        }


        public byte[] GetPHexByte()
        {
            byte[] phbyte = new byte[12];
            long phlong;
            long _longValue = _LongValue;
            //ClusterId
            phlong = (_longValue) & 0xffffL;
            phbyte[0] = (byte)(phlong & 0x003fL);
            phbyte[1] = (byte)((phlong >> 6) & 0x003fL);
            phbyte[2] = (byte)((phlong >> 12) & 0x000fL);

            //DataId
            phlong = (_longValue >> 16) & 0xffffL;
            phbyte[3] = (byte)(phlong & 0x003fL);
            phbyte[4] = (byte)((phlong >> 6) & 0x003fL);
            phbyte[5] = (byte)((phlong >> 12) & 0x000fL);

            //PrimeId
            phlong = (_longValue >> 32) & 0xffffffffL;
            phbyte[6] = (byte)((phlong) & 0x003fL);
            phbyte[7] = (byte)((phlong >> 6) & 0x003fL);
            phbyte[8] = (byte)((phlong >> 12) & 0x003fL);
            phbyte[9] = (byte)((phlong >> 18) & 0x003fL);
            phbyte[10] = (byte)((phlong >> 24) & 0x003fL);
            phbyte[11] = (byte)((phlong >> 30) & 0x0003L);

            return phbyte;
        }

        public char[] GetPHexChar()
        {
            char[] phchar = new char[12];
            byte[] phbyte = new byte[12];
            long phlong = LongValue;
            int phlength = 0;

            phbyte = GetPHexByte();
            for (int i = 0; i < 6; i++)
            {
                phchar[i] = PhexConvertByteToChar(phbyte[i]);
            }

            //trim insignificant zeros, e.g. 00101234 => 101234
            phlength = 6;
            for (int i = 6; i < 12; i++)
            {
                if (phbyte[i] != 0x00) phlength = i + 1;
                phchar[i] = PhexConvertByteToChar(phbyte[i]);
            }

            char[] phchartrim = new char[phlength];
            Array.Copy(phchar, 0, phchartrim, 0, phlength);

            return phchartrim;
        }

        public void SetPHexByByte(byte[] phbyte)
        {
            long phlong;

            //Prime [2][6][6][6][6][6]
            phlong = phbyte[11];
            phlong = (phlong << 6) | phbyte[10] & 0x3fL;    //0x3fL
            phlong = (phlong << 6) | phbyte[9] & 0x3fL;     //0x3fL
            phlong = (phlong << 6) | phbyte[8] & 0x3fL;    //0x3fL
            phlong = (phlong << 6) | phbyte[7] & 0x3fL;    //0x3fL
            phlong = (phlong << 6) | phbyte[6] & 0x3fL;    //0x3fL

            //Data [4][6][6]
            phlong = (phlong << 4) | phbyte[5] & 0x0fL;     //0x0fL can be omitted, given just in case
            phlong = (phlong << 6) | phbyte[4] & 0x3fL;             //0x3fL
            phlong = (phlong << 6) | phbyte[3] & 0x3fL;             //0x3fL

            //Cluster [4][6][6]
            phlong = (phlong << 4) | phbyte[2] & 0x0fL;     //0x0fL can be omitted, given just in case
            phlong = (phlong << 6) | phbyte[1] & 0x3fL;             //0x3fL
            phlong = (phlong << 6) | phbyte[0] & 0x3fL;             //0x3fL

            _LongValue = phlong;
        }

        public void SetPHexByChar(char[] phchar)
        {
            byte[] phbyte = new byte[12];
            long phlong;
            int phlength;

            //TODO: Rozwazyc, czy robic to bez tworzenia phbyte tylko bezposrednio konwertowac jak SetPCHexByChar
            phlength = phchar.Length;

            for (int i = 0; i < phlength; i++)
            {
                phbyte[i] = PhexConvertCharToByte(phchar[i]);
            }

            //Prime [2][6][6][6][6][6]
            phlong = phbyte[11];
            phlong = (phlong << 6) | phbyte[10] & 0x3fL;
            phlong = (phlong << 6) | phbyte[9] & 0x3fL;
            phlong = (phlong << 6) | phbyte[8] & 0x3fL;
            phlong = (phlong << 6) | phbyte[7] & 0x3fL;
            phlong = (phlong << 6) | phbyte[6] & 0x3fL;

            //Data [4][6][6]
            phlong = (phlong << 4) | phbyte[5] & 0x0fL;     //0x0fL can be omitted, given just in case
            phlong = (phlong << 6) | phbyte[4] & 0x3fL;
            phlong = (phlong << 6) | phbyte[3] & 0x3fL;

            //Cluster [4][6][6]
            phlong = (phlong << 4) | phbyte[2] & 0x0fL;     //0x0fL can be omitted, given just in case
            phlong = (phlong << 6) | phbyte[1] & 0x3fL;
            phlong = (phlong << 6) | phbyte[0] & 0x3fL;

            _LongValue = phlong;
        }

        public byte[] GetPHexBytePrimeId()
        {
            byte[] phbyte = new byte[6];
            long phlong;

            phlong = (_LongValue >> 32) & 0xffffffffL;
            phbyte[0] = (byte)((phlong) & 0x003fL);
            phbyte[1] = (byte)((phlong >> 6) & 0x003fL);
            phbyte[2] = (byte)((phlong >> 12) & 0x003fL);
            phbyte[3] = (byte)((phlong >> 18) & 0x003fL);
            phbyte[4] = (byte)((phlong >> 24) & 0x003fL);
            phbyte[5] = (byte)((phlong >> 30) & 0x0003L);

            return phbyte;
        }

        public char[] GetPHexCharPrimeId()
        {
            char[] phchar = new char[6];
            byte phbyte;
            long phlong = (_LongValue >> 32) & 0xffffffffL;
            int phlength = 0;


            for (int i = 0; i < 6; i++)
            {
                phbyte = (byte)(phlong & 0x003fL);
                if (phbyte != 0x00) phlength = i;
                phchar[i] = PhexConvertByteToChar((byte)(phbyte));
                phlong = phlong >> 6;
            }
            phlength++;

            char[] phchartrim = new char[phlength];
            Array.Copy(phchar, 0, phchartrim, 0, phlength);

            return phchartrim;

            //without triming insignificant zeros
            //phlong = (LongValue >> 32) & 0xffffffffL
            //phchar[0] = PhexConvertByteToChar((byte)(phlong & 0x003fL));
            //phchar[1] = PhexConvertByteToChar((byte)((phlong >> 6) & 0x003fL));
            //phchar[2] = PhexConvertByteToChar((byte)((phlong >> 12) & 0x003fL));
            //phchar[3] = PhexConvertByteToChar((byte)((phlong >> 18) & 0x003fL));
            //phchar[4] = PhexConvertByteToChar((byte)((phlong >> 24) & 0x003fL));
            //phchar[5] = PhexConvertByteToChar((byte)((phlong >> 30) & 0x0003L));  //by default 0x000 are masked, so 0x3f eqiv 0x03
            //return phchar;
        }

        public byte[] GetPHexByteDataId()
        {
            byte[] phbyte = new byte[3];
            long phlong = (_LongValue >> 16) & 0xffffL;

            phbyte[0] = (byte)(phlong & 0x003fL);
            phbyte[1] = (byte)((phlong >> 6) & 0x003fL);
            phbyte[2] = (byte)((phlong >> 12) & 0x000fL);

            return phbyte;
        }

        public char[] GetPHexCharDataId()
        {
            char[] phchar = new char[3];
            long phlong = (_LongValue >> 16) & 0xffffL;

            phchar[0] = PhexConvertByteToChar((byte)(phlong & 0x003fL));
            phchar[1] = PhexConvertByteToChar((byte)((phlong >> 6) & 0x003fL));
            phchar[2] = PhexConvertByteToChar((byte)((phlong >> 12) & 0x000fL));

            return phchar;
        }

        public byte[] GetPHexByteClusterId()
        {
            byte[] phbyte = new byte[3];
            long phlong = _LongValue & 0xffffL;

            phbyte[0] = (byte)(phlong & 0x003fL);
            phbyte[1] = (byte)((phlong >> 6) & 0x003fL);
            phbyte[2] = (byte)((phlong >> 12) & 0x000fL);

            return phbyte;
        }

        public char[] GetPHexCharClusterId()
        {
            char[] phchar = new char[3];
            long phlong = LongValue & 0xffffL;

            phchar[0] = PhexConvertByteToChar((byte)(phlong & 0x003fL));
            phchar[1] = PhexConvertByteToChar((byte)((phlong >> 6) & 0x003fL));
            phchar[2] = PhexConvertByteToChar((byte)((phlong >> 12) & 0x000fL));

            return phchar;
        }

        // about 10% slower than convert with if
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

        //Use this one to convert, about 10% faster
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
            //TODO: rozwacyz impementacje wersji bez warunkow
            if (phbyte <= 9)
                return (char)(phbyte + 48); //0-9
            else if (phbyte <= 35)
                return (char)(phbyte + 55); //A-Z
            else if (phbyte <= 61)
                return (char)(phbyte + 61); //a-z
            return (char)(phbyte - 17);      //- or .
        }

        //ewentualnie przesunac gdzies indziej
        // nie jest wykorzystywane obecnie, ale pokazuje układ znaków i zasady konwersji
        private static char[] PHexConvertTable = {
            '0', '1', '2', '3', '4','5','6','7','8','9',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z','-','.'};

    }
}
