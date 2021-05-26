/*
 * Process Hacker - 
 *   file-related utility functions
 *
 * Copyright (C) 2009 wj32
 * 
 * This file is part of Process Hacker.
 * 
 * Process Hacker is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Process Hacker is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Process Hacker.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Doors
{
    public static class LibC
    {
        public unsafe static byte* MemChr(byte* buf, byte c, int length)
        {
            while (length-- > 0)
            {
                if (*buf == c)
                    return buf;

                buf++;
            }

            return null;
        }

        public unsafe static int MemCmp(byte* buf1, byte* buf2, int length)
        {
            while (length-- > 0)
            {
                if (*buf1 != *buf2)
                    return (*buf1 > *buf2) ? 1 : -1;

                buf1++;
                buf2++;
            }

            return 0;
        }

        public unsafe static void MemCpy(byte* dest, byte* src, int length)
        {
            // Copy in groups of 8 bytes (2 ints).
            while (length >= 8)
            {
                *((int*)dest) = *((int*)src);
                *((int*)dest + 1) = *((int*)src + 1);
                dest += 8;
                src += 8;
                length -= 8;
            }

            // Copy remaining 4 bytes.
            if ((length & 4) != 0)
            {
                *(int*)dest = *(int*)src;
                dest += 4;
                src += 4;
            }

            // Copy remaining 2 bytes.
            if ((length & 2) != 0)
            {
                *(short*)dest = *(short*)src;
                dest += 2;
                src += 2;
            }

            // Copy remaining byte.
            if ((length & 1) != 0)
                *dest = *src;
        }

        public unsafe static void MemSet(byte* dest, byte c, int length)
        {
            int p = (c << 24) | (c << 16) | (c << 8) | c;

            // Copy in groups of 8 bytes (2 ints).
            while (length >= 8)
            {
                *((int*)dest) = p;
                *((int*)dest + 1) = p;
                dest += 8;
                length -= 8;
            }

            // Copy remaining 4 bytes.
            if ((length & 4) != 0)
            {
                *(int*)dest = p;
                dest += 4;
            }

            // Copy remaining 2 bytes.
            if ((length & 2) != 0)
            {
                *(short*)dest = (short)p;
                dest += 2;
            }

            // Copy remaining byte.
            if ((length & 1) != 0)
                *dest = (byte)p;
        }

        public unsafe static char* WMemChr(char* buf, char c, int length)
        {
            while (length-- > 0)
            {
                if (*buf == c)
                    return buf;

                buf++;
            }

            return null;
        }

        public unsafe static int WMemCmp(char* buf1, char* buf2, int length)
        {
            while (length-- > 0)
            {
                if (*buf1 != *buf2)
                    return (*buf1 > *buf2) ? 1 : -1;

                buf1++;
                buf2++;
            }

            return 0;
        }

        public unsafe static void WMemCpy(char* dest, char* src, int length)
        {
            // Copy in groups of 16 bytes (4 ints).
            while (length >= 8)
            {
                *((int*)dest) = *((int*)src);
                *((int*)dest + 1) = *((int*)src + 1);
                *((int*)dest + 2) = *((int*)src + 2);
                *((int*)dest + 3) = *((int*)src + 3);
                dest += 8;
                src += 8;
                length -= 8;
            }

            // Copy remaining 8 bytes.
            if ((length & 4) != 0)
            {
                *((int*)dest) = *((int*)src);
                *((int*)dest + 1) = *((int*)src + 1);
                dest += 4;
                src += 4;
            }

            // Copy remaining 4 bytes.
            if ((length & 2) != 0)
            {
                *(int*)dest = *(int*)src;
                dest += 2;
                src += 2;
            }

            // Copy remaining 2 bytes.
            if ((length & 1) != 0)
                *dest = *src;
        }

        public unsafe static void WMemCpyUnaligned(char* dest, char* src, int length)
        {
            // Align destination to 4 bytes if needed.
            if (((int)dest & 2) != 0)
            {
                *dest = *src;
                dest++;
                src++;
                length--;
            }

            // Copy in groups of 16 bytes (4 ints).
            while (length >= 8)
            {
                *((int*)dest) = *((int*)src);
                *((int*)dest + 1) = *((int*)src + 1);
                *((int*)dest + 2) = *((int*)src + 2);
                *((int*)dest + 3) = *((int*)src + 3);
                dest += 8;
                src += 8;
                length -= 8;
            }

            // Copy remaining 8 bytes.
            if ((length & 4) != 0)
            {
                *((int*)dest) = *((int*)src);
                *((int*)dest + 1) = *((int*)src + 1);
                dest += 4;
                src += 4;
            }

            // Copy remaining 4 bytes.
            if ((length & 2) != 0)
            {
                *(int*)dest = *(int*)src;
                dest += 2;
                src += 2;
            }

            // Copy remaining 2 bytes.
            if ((length & 1) != 0)
                *dest = *src;
        }

        public unsafe static void WMemSet(char* dest, char c, int length)
        {
            int p = (c << 16) | c;

            // Copy in groups of 16 bytes (4 ints).
            while (length >= 8)
            {
                *((int*)dest) = p;
                *((int*)dest + 1) = p;
                *((int*)dest + 2) = p;
                *((int*)dest + 3) = p;
                dest += 8;
                length -= 8;
            }

            // Copy remaining 8 bytes.
            if ((length & 4) != 0)
            {
                *((int*)dest) = p;
                *((int*)dest + 1) = p;
                dest += 4;
            }

            // Copy remaining 4 bytes.
            if ((length & 2) != 0)
            {
                *(int*)dest = p;
                dest += 2;
            }

            // Copy remaining 2 bytes.
            if ((length & 1) != 0)
                *(short*)dest = (short)p;
        }
    }
}
