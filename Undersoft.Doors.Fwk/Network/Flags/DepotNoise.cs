using System.IO;

namespace System.Doors
{
    public static class DataDepotNoise
    {
        public static byte[] Initialize(this byte[] array, byte defaultValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = defaultValue;
            }
            return array;
        }
        public static byte[] Initialize(this byte[] array, char defaultValue)
        {
            byte[] charbyte = BitConverter.GetBytes(defaultValue);
            int charlength = charbyte.Length;
            int length = (array.Length % charlength) + array.Length;
            int counter = 0;
            for (int i = 0; i < length; i++)
            {
                array[i] = charbyte[counter];
                counter = (charlength > counter) ? counter++ : 0;
            }
            return array;
        }
        //public static Span<byte> Initialize(this Span<byte> array, byte defaultValue)
        //{
        //    array.Fill(defaultValue);
        //    return array;
        //}
        //public static Span<byte> Initialize(this Span<byte> array, char defaultValue)
        //{
        //    array.Fill((byte)defaultValue.ToBytes(CharCoding.UTF8)[0]);         
        //    return array;
        //}

        public static Stream Noise(this Stream stream, long blocksize, NoiseType bytenoise)
        {
            long blockSize = blocksize;
            long blockLeft = stream.Length % blockSize;
            long resize = (blockSize - blockLeft >= 28) ? blockSize - blockLeft - 16 : blockSize + (blockSize - blockLeft) - 16;
            byte[] byteNoise = new byte[resize].Initialize((byte)bytenoise);
            stream.Write(byteNoise, 0, (int)resize);
            return stream;
        }
        public static Stream Noise(this Stream stream, int blocksize, NoiseType bytenoise)
        {
            int blockSize = blocksize;
            long blockLeft = stream.Length % blockSize;
            long resize = (blockSize - blockLeft >= 28) ? blockSize - blockLeft - 16 : blockSize + (blockSize - blockLeft) - 16;
            byte[] byteNoise = new byte[resize].Initialize((byte)bytenoise);
            stream.Write(byteNoise, 0, (int)resize);
            return stream;
        }
        public static byte[] Noise(this byte[] array, long blocksize, NoiseType bytenoise, int written = 0, int byteprefix = 0)
        {
            int arrayLength = (written > 0) ? written : array.Length;
            long blockSize = blocksize;
            long blockLeft = arrayLength % blockSize;
            long resize = (blockSize - blockLeft >= 28) ? blockSize - blockLeft - byteprefix : blockSize + (blockSize - blockLeft) - byteprefix;
            byte[] byteNoise = new byte[resize].Initialize((byte)bytenoise);
            Array.Resize<byte>(ref array, arrayLength + (int)resize);
            byteNoise.CopyTo(array, arrayLength);
            byteNoise = null;
            return array;
        }
        public static byte[] Noise(this byte[] array, int blocksize, NoiseType bytenoise, int written = 0, int byteprefix = 0)
        {
            int arrayLength = (written > 0) ? written : array.Length;
            int blockSize = blocksize;
            long blockLeft = arrayLength % blockSize;
            long resize = (blockSize - blockLeft >= 28) ? blockSize - blockLeft - byteprefix : blockSize + (blockSize - blockLeft) - byteprefix;
            byte[] byteNoise = new byte[resize].Initialize((byte)bytenoise);
            Array.Resize<byte>(ref array, arrayLength + (int)resize);
            byteNoise.CopyTo(array, arrayLength);
            byteNoise = null;
            return array;
        }
        //public unsafe static Memory<byte> Noise(this Memory<byte> array, long blocksize, NoiseType bytenoise, int written = 0, int byteprefix = 0)
        //{
        //    int arrayLength = (written > 0) ? written : array.Length;
        //    long blockSize = blocksize;
        //    long blockLeft = arrayLength % blockSize;
        //    long resize = (blockSize - blockLeft >= 28) ? blockSize - blockLeft - byteprefix : blockSize + (blockSize - blockLeft) - byteprefix;
        //    Memory<byte> resizedarray = new byte[arrayLength + (int)resize];
        //    resizedarray.Slice(arrayLength).Span.Fill((byte)bytenoise);
        //    MemoryNative.Copy((byte*)resizedarray.Pin().Pointer, (byte*)array.Pin().Pointer, (uint)arrayLength); 
        //    array = resizedarray;
        //    return array;
        //}
        //public unsafe static Memory<byte> Noise(this Memory<byte> array, int blocksize, NoiseType bytenoise, int written = 0, int byteprefix = 0)
        //{
        //    int arrayLength = (written > 0) ? written : array.Length;
        //    long blockSize = blocksize;
        //    long blockLeft = arrayLength % blockSize;
        //    long resize = (blockSize - blockLeft >= 28) ? blockSize - blockLeft - byteprefix : blockSize + (blockSize - blockLeft) - byteprefix;
        //    Memory<byte> resizedarray = new byte[arrayLength + (int)resize];
        //    resizedarray.Slice(arrayLength).Span.Fill((byte)bytenoise);
        //    MemoryNative.Copy((byte*)resizedarray.Pin().Pointer, (byte*)array.Pin().Pointer, (uint)arrayLength);
        //    array = resizedarray;
        //    return array;
        //}


        public static NoiseType SeekNoise(this Stream stream, SeekOrigin seekorigin = SeekOrigin.Begin, SeekDirection direction = SeekDirection.Forward, int offset = 0, int _length = -1)
        {
            bool isFwd = (direction != SeekDirection.Forward) ? false : true;
            short noiseFlag = 0;
            NoiseType noiseKind = NoiseType.None;
            NoiseType lastKind = NoiseType.None;
            if (stream.Length > 0)
            {
                long length = (_length <= 0) ? stream.Length : _length;
                long saveposition = stream.Position;
                offset += (!isFwd) ? 1 : 0;
                length -= ((!isFwd) ? 0 : 1);

                for (int i = offset; i < length; i++)
                {
                    if (!isFwd)
                        stream.Seek(-i, seekorigin);
                    else
                        stream.Seek(i, seekorigin);

                    byte checknoise = (byte)stream.ReadByte();

                    NoiseType tempKind = NoiseType.None;
                    if (checknoise.IsNoise(out tempKind))
                    {
                        lastKind = tempKind;
                        noiseFlag++;
                    }
                    else if (noiseFlag >= 16)
                    {
                        noiseKind = lastKind;
                        return noiseKind;
                    }
                    else
                    {
                        lastKind = tempKind;
                        noiseFlag = 0;
                    }
                }
                stream.Position = saveposition;
            }
            return lastKind;
        }
        public static NoiseType SeekNoise(this byte[] array, out long position, SeekDirection direction = SeekDirection.Forward, int offset = 0, int _length = -1)
        {
            bool isFwd = (direction != SeekDirection.Forward) ? false : true;
            short noiseFlag = 0;
            NoiseType noiseKind = NoiseType.None;
            NoiseType lastKind = NoiseType.None;
            if (array.Length > 0)
            {
                long length = (_length <= 0 || _length > array.Length) ? array.Length : _length;
                int arraylength = array.Length;
                offset += (!isFwd) ? 1 : 0;
                length -= ((!isFwd) ? 0 : 1);

                for (int i = offset; i < length; i++)
                {
                    byte checknoise = 0;
                    if (!isFwd)
                        checknoise = array[arraylength - i];
                    else
                        checknoise = array[i];

                    NoiseType tempKind = NoiseType.None;
                    if (checknoise.IsNoise(out tempKind))
                    {
                        lastKind = tempKind;
                        noiseFlag++;
                    }
                    else if (noiseFlag >= 16)
                    {
                        noiseKind = lastKind;
                        position = (!isFwd) ? arraylength - i + 1 : i;
                        return noiseKind;
                    }
                    else
                    {
                        lastKind = tempKind;                 
                        noiseFlag = 0; 
                    }
                }
            }
            position = (!isFwd && noiseFlag != 0) ? array.Length - noiseFlag + 1 : 0;
            return lastKind;
        }
        //public static NoiseType SeekNoise(this Span<byte> array, out long position, SeekDirection direction = SeekDirection.Forward, int offset = 0, int _length = -1)
        //{
        //    bool isFwd = (direction != SeekDirection.Forward) ? false : true;
        //    short noiseFlag = 0;
        //    NoiseType noiseKind = NoiseType.None;
        //    NoiseType lastKind = NoiseType.None;
        //    int arraylength = array.Length;
        //    if (arraylength > 0)
        //    {
        //        long length = (_length <= 0 || _length > arraylength) ? arraylength : _length;
        //        offset += (!isFwd) ? 1 : 0;
        //        length -= ((!isFwd) ? 0 : 1);

        //        for (int i = offset; i < length; i++)
        //        {
        //            byte checknoise = 0;
        //            if (!isFwd)
        //                checknoise = array[arraylength - i];
        //            else
        //                checknoise = array[i];

        //            NoiseType tempKind = NoiseType.None;
        //            if (checknoise.IsNoise(out tempKind))
        //            {
        //                lastKind = tempKind;
        //                noiseFlag++;
        //            }
        //            else if (noiseFlag >= 16)
        //            {
        //                noiseKind = lastKind;
        //                position = (!isFwd) ? arraylength - i + 1 : i;
        //                return noiseKind;
        //            }
        //            else
        //            {
        //                lastKind = tempKind;
        //                noiseFlag = 0;
        //            }
        //        }
        //    }
        //    position = (!isFwd && noiseFlag != 0) ? array.Length - noiseFlag + 1 : 0;
        //    return lastKind;
        //}

        public static bool IsNoise(this byte checknoise, out NoiseType noisekind)
        {
            switch (checknoise)
            { 
                case (byte)NoiseType.Block:
                    noisekind = NoiseType.Block;
                    return true;
                case (byte)NoiseType.End:
                    noisekind = NoiseType.End;
                    return true;
                case (byte)NoiseType.Empty:
                    noisekind = NoiseType.Empty;
                    return false;
                default:
                    noisekind = NoiseType.None;
                    return false;
            }
        }
        public static bool IsSpliter(this byte checknoise, out NoiseType spliterkind)
        {
            switch (checknoise)
            {
                case (byte)NoiseType.Empty:
                    spliterkind = NoiseType.Empty;
                    return true;
                case (byte)NoiseType.Line:
                    spliterkind = NoiseType.Line;
                    return true;
                case (byte)NoiseType.Space:
                    spliterkind = NoiseType.Space;
                    return true;
                case (byte)NoiseType.Semi:
                    spliterkind = NoiseType.Semi;
                    return true;
                case (byte)NoiseType.Coma:
                    spliterkind = NoiseType.Coma;
                    return true;
                case (byte)NoiseType.Colon:
                    spliterkind = NoiseType.Colon;
                    return true;
                case (byte)NoiseType.Dot:
                    spliterkind = NoiseType.Dot;
                    return true;
                default:
                    spliterkind = NoiseType.None;
                    return false;
            }
        }

    }

    public enum NoiseType
    {
        None = (byte)0xFF,        
        Block = (byte)0x17,
        End = (byte)0x04,
        Empty = (byte)0x00,
        Line = (byte)0x10,
        Space = (byte)0x32,
        Semi = (byte)0x59,
        Coma = (byte)0x44,
        Colon = (byte)0x58,
        Dot = (byte)0x46,
        Cancel = (byte)0x18,
    }

    public enum SeekDirection
    {
        Forward,
        Backward
    }

}