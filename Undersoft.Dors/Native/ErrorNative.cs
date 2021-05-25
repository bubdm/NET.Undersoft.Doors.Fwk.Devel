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

namespace System.Dors
{
 
    public unsafe struct String255 : IEquatable<String255>, IEquatable<string>
    {
        private const string MsgStrMustBeLessThanMax = "The string must be less than 256 characters in length.";
        private const string MsgNoMoreSpace = "There is not enough storage space.";
        public const int MaximumLength = 255;

        public fixed char Buffer[256];
        public byte Length;

        public String255(string str)
        {
            if (str.Length > MaximumLength)
                throw new ArgumentException(MsgStrMustBeLessThanMax);

            fixed (char* buffer = this.Buffer)
            {
                for (int i = 0; i < str.Length; i++)
                    buffer[i] = str[i];

                buffer[str.Length] = '\0';
            }

            this.Length = (byte)str.Length;
        }

        public String255(String255 str, int startIndex, int length)
        {
            if (startIndex >= str.Length)
                throw new ArgumentException("The start index is too large.");
            if (length > str.Length - startIndex)
                throw new ArgumentException("The length is too large.");
            if (length > MaximumLength)
                throw new ArgumentException(MsgStrMustBeLessThanMax);

            fixed (char* buffer = this.Buffer)
            {
                LibC.WMemCpy(buffer, &str.Buffer[startIndex], length);
                buffer[length] = '\0';
            }

            this.Length = (byte)length;
        }

        public String255(char* str, int length)
        {
            if (length > MaximumLength)
                throw new ArgumentException(MsgStrMustBeLessThanMax);

            fixed (char* buffer = this.Buffer)
            {
                LibC.WMemCpy(buffer, str, length);
                buffer[length] = '\0';
            }

            this.Length = (byte)length;
        }

        public char this[int index]
        {
            get
            {
                fixed (char* buffer = this.Buffer)
                    return buffer[index];
            }
            set
            {
                fixed (char* buffer = this.Buffer)
                    buffer[index] = value;
            }
        }

        public void Append(char c)
        {
            if ((this.Length + 1) > MaximumLength)
                throw new InvalidOperationException(MsgNoMoreSpace);

            fixed (char* buffer = this.Buffer)
            {
                buffer[this.Length++] = c;
                buffer[this.Length] = '\0';
            }
        }

        public void Append(String255 str)
        {
            if ((this.Length + str.Length) > MaximumLength)
                throw new InvalidOperationException(MsgNoMoreSpace);

            fixed (char* buffer = this.Buffer)
            {
                LibC.WMemCpyUnaligned(&buffer[this.Length], str.Buffer, str.Length);
                this.Length += str.Length;
                buffer[this.Length] = '\0';
            }
        }

        public void Append(string str)
        {
            if ((this.Length + str.Length) > MaximumLength)
                throw new InvalidOperationException(MsgNoMoreSpace);

            fixed (char* buffer = this.Buffer)
            {
                for (int i = 0; i < str.Length; i++)
                    buffer[this.Length++] = str[i];

                buffer[this.Length] = '\0';
            }
        }

        public int CompareTo(String255 str)
        {
            fixed (char* buffer = this.Buffer)
            {
                int result;

                result = LibC.WMemCmp(buffer, str.Buffer, this.Length < str.Length ? this.Length : str.Length);

                if (result == 0)
                    return this.Length - str.Length;
                else
                    return result;
            }
        }

        public bool EndsWith(String255 str)
        {
            if (str.Length > this.Length)
                return false;

            fixed (char* buffer = this.Buffer)
                return LibC.WMemCmp(&buffer[this.Length - str.Length], str.Buffer, str.Length) == 0;
        }

        public override bool Equals(object other)
        {
            if (other is String255)
                return this.Equals((String255)other);
            else if (other is string)
                return this.Equals((string)other);
            else
                return false;
        }

        public bool Equals(String255 other)
        {
            if (this.Length != other.Length)
                return false;

            fixed (char* buffer = this.Buffer)
            {
                for (int i = 0; i < other.Length; i++)
                {
                    if (buffer[i] != other.Buffer[i])
                        return false;
                }
            }

            return true;
        }

        public bool Equals(string other)
        {
            if (this.Length != other.Length)
                return false;

            fixed (char* buffer = this.Buffer)
            {
                for (int i = 0; i < other.Length; i++)
                {
                    if (buffer[i] != other[i])
                        return false;
                }
            }

            return true;
        }

        public int IndexOf(char c)
        {
            char* ptr;

            fixed (char* buffer = this.Buffer)
            {
                ptr = LibC.WMemChr(buffer, c, this.Length);

                if (ptr != null)
                    return (int)(ptr - buffer);
                else
                    return -1;
            }
        }

        public override int GetHashCode()
        {
            int hashCode = 0x15051505;

            fixed (char* buffer = this.Buffer)
            {
                for (int i = 0; i < this.Length; i += 4)
                {
                    hashCode += hashCode ^ (hashCode << ((i % 4) * 8));
                }
            }

            return hashCode;
        }

        public bool StartsWith(String255 str)
        {
            if (str.Length > this.Length)
                return false;

            fixed (char* buffer = this.Buffer)
                return LibC.WMemCmp(buffer, str.Buffer, str.Length) == 0;
        }

        public String255 Substring(int startIndex)
        {
            return this.Substring(startIndex, this.Length - startIndex);
        }

        public String255 Substring(int startIndex, int length)
        {
            return new String255(this, startIndex, length);
        }

        public override string ToString()
        {
            fixed (char* buffer = this.Buffer)
            {
                return new string(buffer, 0, this.Length);
            }
        }
    }

    /// <summary>
    /// Represents a Win32 or Native exception.
    /// </summary>
    /// <remarks>
    /// Unlike the System.ComponentModel.Win32Exception class, 
    /// this class does not get the error's associated 
    /// message unless it is requested.
    /// </remarks>
    public class WindowsException : Exception
    {
        private bool _isNtStatus = false;
        private Win32Error _errorCode = 0;
        private NtStatus _status;
        private string _message = null;

        /// <summary>
        /// Creates an exception with no error.
        /// </summary>
        public WindowsException()
        { }

        /// <summary>
        /// Creates an exception from a Win32 error code.
        /// </summary>
        /// <param name="errorCode">The Win32 error code.</param>
        public WindowsException(Win32Error errorCode)
        {
            _errorCode = errorCode;
        }

        /// <summary>
        /// Creates an exception from a NT status value.
        /// </summary>
        /// <param name="status">The NT status value.</param>
        public WindowsException(NtStatus status)
        {
            _status = status;
            _errorCode = status.ToDosError();
            _isNtStatus = true;
        }

        /// <summary>
        /// Gets whether the NT status value is valid.
        /// </summary>
        public bool IsNtStatus
        {
            get { return _isNtStatus; }
        }

        /// <summary>
        /// Gets a Win32 error code which represents the exception.
        /// </summary>
        public Win32Error ErrorCode
        {
            get { return _errorCode; }
        }

        /// <summary>
        /// Gets a NT status value which represents the exception.
        /// </summary>
        public NtStatus Status
        {
            get { return _status; }
        }

        /// <summary>
        /// Gets a message describing the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                // No locking, for performance reasons. Getting the 
                // message doesn't have any side-effects anyway.
                if (_message == null)
                {
                    // We prefer native status messages because they are usually 
                    // more detailed. However, for some status values we do 
                    // prefer the shorter Win32 error message.

                    if (
                        _isNtStatus &&
                        _status != NtStatus.AccessDenied &&
                        _status != NtStatus.AccessViolation
                        )
                    {
                        string message = _status.GetMessage();

                        if (message == null)
                            message = "Could not retrieve the error message (0x" + ((int)_status).ToString("x") + ").";

                        _message = message;
                    }
                    else
                    {
                        //_message = _errorCode.GetMessage();
                    }
                }

                return _message;
            }
        }

        /// <summary>
        /// Converts the NT status value to a DOS/Windows error code.
        /// </summary>
        /// <param name="status">The NT status value.</param>
        /// <returns>A DOS/Windows error code.</returns>       
    }

    /// <summary>
    /// A Win32 error code.
    /// </summary>
    public enum Win32Error : uint
    {
        Success = 0x0,
        InvalidFunction = 0x1,
        FileNotFound = 0x2,
        PathNotFound = 0x3,
        TooManyOpenFiles = 0x4,
        AccessDenied = 0x5,
        InvalidHandle = 0x6,
        ArenaTrashed = 0x7,
        NotEnoughMemory = 0x8,
        InvalidBlock = 0x9,
        BadEnvironment = 0xa,
        BadFormat = 0xb,
        InvalidAccess = 0xc,
        InvalidData = 0xd,
        OutOfMemory = 0xe,
        InvalidDrive = 0xf,
        CurrentDirectory = 0x10,
        NotSameDevice = 0x11,
        NoMoreFiles = 0x12,
        WriteProtect = 0x13,
        BadUnit = 0x14,
        NotReady = 0x15,
        BadCommand = 0x16,
        Crc = 0x17,
        BadLength = 0x18,
        Seek = 0x19,
        NotDosDisk = 0x1a,
        SectorNotFound = 0x1b,
        OutOfPaper = 0x1c,
        WriteFault = 0x1d,
        ReadFault = 0x1e,
        GenFailure = 0x1f,
        SharingViolation = 0x20,
        LockViolation = 0x21,
        WrongDisk = 0x22,
        SharingBufferExceeded = 0x24,
        HandleEof = 0x26,
        HandleDiskFull = 0x27,
        NotSupported = 0x32,
        RemNotList = 0x33,
        DupName = 0x34,
        BadNetPath = 0x35,
        NetworkBusy = 0x36,
        DevNotExist = 0x37,
        TooManyCmds = 0x38,
        FileExists = 0x50,
        CannotMake = 0x52,
        AlreadyAssigned = 0x55,
        InvalidPassword = 0x56,
        InvalidParameter = 0x57,
        NetWriteFault = 0x58,
        NoProcSlots = 0x59,
        TooManySemaphores = 0x64,
        ExclSemAlreadyOwned = 0x65,
        SemIsSet = 0x66,
        TooManySemRequests = 0x67,
        InvalidAtInterruptTime = 0x68,
        SemOwnerDied = 0x69,
        SemUserLimit = 0x6a,
        Cancelled = 0x4c7
    }

    public static class Win32ErrorExtensions
    {
        public static HResult GetHResult(this Win32Error errorCode)
        {
            int error = (int)errorCode;

            if ((error & 0x80000000) == 0x80000000)
                return (HResult)error;

            return (HResult)(0x80070000 | (uint)(error & 0xffff));
        }

        //public static string GetMessage(this Win32Error errorCode)
        //{
        //    String255 buffer = new String255();

        //    unsafe
        //    {
        //        if ((buffer.Length = (byte)Win32.FormatMessage(
        //            0x3200,
        //            IntPtr.Zero,
        //            (int)errorCode,
        //            0,
        //            new IntPtr(buffer.Buffer),
        //            String255.MaximumLength,
        //            IntPtr.Zero
        //            )) == 0)
        //            return "Unknown error (0x" + ((int)errorCode).ToString("x") + ")";

        //        String255 result = new String255();

        //        for (int i = 0; i < buffer.Length; i++)
        //        {
        //            char c = buffer.Buffer[i];

        //            if (!char.IsLetterOrDigit(c) &&
        //                !char.IsPunctuation(c) &&
        //                !char.IsSymbol(c) &&
        //                !char.IsWhiteSpace(c))
        //                break;

        //            result.Append(c);
        //        }

        //        return result.ToString().Replace("\r\n", "");
        //    }
        //}
    }

    public enum HResult : uint
    {
        False = 0x0001,
        OK = 0x0000,
        Cancelled = 1223,

        Error = 0x80000000,
        NoInterface = 0x80004002,
        Fail = 0x80004005,
        TypeElementNotFound = 0x8002802b,
        NoObject = 0x800401e5,
        OutOfMemory = 0x8007000e,
        InvalidArgument = 0x80070057,
        ResourceInUse = 0x800700aa,
        ElementNotFound = 0x80070490
    }

    public static class HResultExtensions
    {
        public static bool IsError(this HResult result)
        {
            //Return != OK because there are come errors with lower values than HResult.False
            return result != HResult.OK;
        }

        public static void ThrowIf(this HResult result)
        {
            if (result.IsError())
                throw Marshal.GetExceptionForHR((int)result);
        }
    }
}
