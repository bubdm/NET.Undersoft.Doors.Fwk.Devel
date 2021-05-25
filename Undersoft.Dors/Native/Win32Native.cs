using System;
using System.Runtime.InteropServices;

namespace System.Dors
{

    public static class Win32
    {       
        #region Consts

        public const int DontResolveDllReferences = 0x1;
        public const int ErrorNoMoreItems = 259;
        public const int SeeMaskInvokeIdList = 0xc;
        public const uint ServiceNoChange = 0xffffffff;
        public const uint ShgFiIcon = 0x100;
        public const uint ShgFiLargeIcon = 0x0;
        public const uint ShgFiSmallIcon = 0x1;

        #endregion          

        public static Win32Error GetLastErrorCode()
        {
            return (Win32Error)Marshal.GetLastWin32Error();
        }

        /// <summary>
        /// Gets the error message associated with the last error that occured.
        /// </summary>
        /// <returns>An error message.</returns>
        public static string GetLastErrorMessage()
        {
            return null; // GetLastErrorCode().GetMessage();
        }

        /// <summary>
        /// Throws a WindowsException with the last error that occurred.
        /// </summary>
        public static void Throw()
        {
            Throw(GetLastErrorCode());
        }

        public static void Throw(NtStatus status)
        {
            throw new WindowsException(status);
        }

        public static void Throw(int error)
        {
            Throw((Win32Error)error);
        }

        public static void Throw(Win32Error error)
        {
            throw new WindowsException(error);
        }

        #region Strings

        #region ANSI

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlAnsiStringToUnicodeString(
            ref UnicodeString DestinationString,
            [In] ref AnsiString SourceString,
            [In] bool AllocateDestinationString
            );

        [DllImport("ntdll.dll")]
        public static extern void RtlFreeAnsiString(
            [In] ref AnsiString AnsiString
            );

        #endregion

        #region Unicode

        [DllImport("ntdll.dll")]
        public static extern int RtlCompareUnicodeString(
            [In] ref UnicodeString String1,
            [In] ref UnicodeString String2,
            [In] bool CaseInSensitive
            );

        [DllImport("ntdll.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RtlCreateUnicodeString(
            [Out] out UnicodeString DestinationString,
            [MarshalAs(UnmanagedType.LPWStr)]
            [In] string SourceString
            );

        [DllImport("ntdll.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RtlCreateUnicodeStringFromAsciiz(
            [Out] out UnicodeString DestinationString,
            [MarshalAs(UnmanagedType.LPStr)]
            [In] string SourceString
            );

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlDuplicateUnicodeString(
            [In] RtlDuplicateUnicodeStringFlags Flags,
            [In] ref UnicodeString StringIn,
            [Out] out UnicodeString StringOut
            );

        [DllImport("ntdll.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RtlEqualUnicodeString(
            [In] ref UnicodeString String1,
            [In] ref UnicodeString String2,
            [In] bool CaseInSensitive
            );

        [DllImport("ntdll.dll")]
        public static extern void RtlFreeUnicodeString(
            [In] ref UnicodeString UnicodeString
            );     

        [DllImport("ntdll.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RtlPrefixUnicodeString(
            [In] ref UnicodeString String1,
            [In] ref UnicodeString String2,
            [In] bool CaseInSensitive
            );

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlUnicodeStringToAnsiString(
            ref AnsiString DestinationString,
            [In] ref UnicodeString SourceString,
            [In] bool AllocateDestinationString
            );

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlUpcaseUnicodeStringToAnsiString(
            ref AnsiString DestinationString,
            [In] ref UnicodeString SourceString,
            [In] bool AllocateDestinationString
            );

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlValidateUnicodeString(
            [In] int Flags,
            [In] ref UnicodeString String
            );

        #endregion

        #endregion

        #region Files

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int SearchPath(
            [In] [Optional] string Path,
            [In] string FileName,
            [In] [Optional] string Extension,
            [In] int BufferLength,
            [In] IntPtr Buffer,
            [Out] out IntPtr FilePart
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetFileInformationByHandleEx(
            [In] IntPtr FileHandle,
            [In] int FileInformationClass,
            [In] IntPtr FileInformation,
            [In] int FileInformationLength
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetFileSizeEx(
            [In] IntPtr FileHandle,
            [Out] out long FileSize
            );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int QueryDosDevice(
            [In] [Optional] string DeviceName,
            [In] IntPtr TargetPath,
            [In] int MaxLength
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFile(
            [In] string FileName,
            [In] FileAccess DesiredAccess,
            [In] FileShareMode ShareMode,
            [In] [Optional] int SecurityAttributes,
            [In] FileCreationDispositionWin32 CreationDisposition,
            [In] int FlagsAndAttributes,
            [In] [Optional] IntPtr TemplateFile
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(
            [In] IntPtr FileHandle,
            [Out] byte[] Buffer,
            [In] int Bytes,
            [Out] [Optional] out int ReadBytes,
            [Optional] IntPtr Overlapped
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteFile(
            [In] IntPtr FileHandle,
            [In] byte[] Buffer,
            [In] int Bytes,
            [Out] [Optional] out int WrittenBytes,
            [Optional] IntPtr Overlapped
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public unsafe static extern bool WriteFile(
            [In] IntPtr FileHandle,
            [In] void* Buffer,
            [In] int Bytes,
            [Out] [Optional] out int WrittenBytes,
            [Optional] IntPtr Overlapped
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            [In] IntPtr FileHandle,
            [In] int IoControlCode,
            [In] [Optional] byte[] InBuffer,
            [In] int InBufferLength,
            [Out] [Optional] byte[] OutBuffer,
            [In] int OutBufferLength,
            [Out] [Optional]out int BytesReturned,
            [Optional] IntPtr Overlapped
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public unsafe static extern bool DeviceIoControl(
            [In] IntPtr FileHandle,
            [In] int IoControlCode,
            [In] [Optional] byte* InBuffer,
            [In] int InBufferLength,
            [Out] [Optional] byte* OutBuffer,
            [In] int OutBufferLength,
            [Out] [Optional] out int BytesReturned,
            [Optional] IntPtr Overlapped
            );

        #endregion

        #region Shell

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int ShellAbout(
            [In] [Optional] IntPtr hWnd,
            [In] string App,
            [In] [Optional] string OtherStuff,
            [In] [Optional] IntPtr IconHandle
            );

        [DllImport("shell32.dll", EntryPoint = "#61", CharSet = CharSet.Unicode)]
        public static extern int RunFileDlg(
            [In] IntPtr hWnd,
            [In] IntPtr Icon,
            [In] string Path,
            [In] string Title,
            [In] string Prompt,
            [In] RunFileDialogFlags Flags
            );

        [DllImport("shell32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShellExecuteEx(
            [MarshalAs(UnmanagedType.Struct)] ref ShellExecuteInfo s
            );

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(
            [In] int HookId,
            [In] IntPtr HookFunction,
            [In] IntPtr Module,
            [In] int ThreadId
            );

        [DllImport("shell32.dll")]
        public extern static int ExtractIconEx(
            [In] string libName,
            [In] int iconIndex,
            [Out] IntPtr[] largeIcon,
            [Out] IntPtr[] smallIcon,
            [In] int nIcons
            );

        [DllImport("shell32.dll")]
        public static extern int SHGetFileInfo(
            [In] string pszPath,
            [In] uint dwFileAttributes,
            [Out] out ShFileInfo psfi,
            [In] uint cbSizeFileInfo,
            [In] uint uFlags);

        [DllImport("shell32.dll", EntryPoint = "#660")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FileIconInit([In] bool RestoreCache);

        #endregion

        #region Processes and Threads

        [DllImport("ntdll.dll")]
        public static extern void RtlAcquirePebLock();

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlAllocateFromPeb(
            [In] int Size,
            [Out] out IntPtr Block
            );

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlCreateEnvironment(
            [In] bool CloneCurrentEnvironment,
            [Out] out IntPtr Environment
            );

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlCreateProcessParameters(
            [Out] out IntPtr ProcessParameters,
            [In] ref UnicodeString ImagePathName,
            [In] ref UnicodeString DllPath,
            [In] ref UnicodeString CurrentDirectory,
            [In] ref UnicodeString CommandLine,
            [In] IntPtr Environment,
            [In] ref UnicodeString WindowTitle,
            [In] ref UnicodeString DesktopInfo,
            [In] ref UnicodeString ShellInfo,
            [In] ref UnicodeString RuntimeData
            );        

        [DllImport("ntdll.dll")]
        public static extern IntPtr RtlDeNormalizeProcessParameters(
            [In] IntPtr ProcessParameters
            );

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlDestroyEnvironment(
            [In] IntPtr Environment
            );

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlDestroyProcessParameters(
            [In] IntPtr ProcessParameters
            );

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlExitUserProcess(
            [In] NtStatus ExitStatus
            );

        [DllImport("ntdll.dll")]
        public static extern void RtlExitUserThread(
            [In] NtStatus ExitStatus
            );

        [DllImport("ntdll.dll")]
        public static extern void RtlFreeUserThreadStack(
            [In] IntPtr Process,
            [In] IntPtr Thread
            );

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlFreeToPeb(
            [In] IntPtr Block,
            [In] int Size
            );   

        [DllImport("ntdll.dll")]
        public static extern IntPtr RtlNormalizeProcessParameters(
            [In] IntPtr ProcessParameters
            );

        [DllImport("ntdll.dll")]
        public static extern Win32Error RtlNtStatusToDosError(
            [In] NtStatus Status
            );

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlQueryEnvironmentVariable_U(
            [In] [Optional] IntPtr Environment,
            [In] ref UnicodeString Name,
            ref UnicodeString Value
            );

        [DllImport("ntdll.dll")]
        public static extern void RtlReleasePebLock();

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlRemoteCall(
            [In] IntPtr Process,
            [In] IntPtr Thread,
            [In] IntPtr CallSite,
            [In] int ArgumentCount,
            [In] IntPtr[] Arguments,
            [In] bool PassContext,
            [In] bool AlreadySuspended
            );

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlSetCurrentEnvironment(
            [In] IntPtr Environment,
            [Out] out IntPtr PreviousEnvironment
            );

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlSetEnvironmentVariable(
            ref IntPtr Environment,
            [In] ref UnicodeString Name,
            [In] [Optional] ref UnicodeString Value
            );

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlSetEnvironmentVariable(
            [In] [Optional] IntPtr Environment,
            [In] ref UnicodeString Name,
            [In] [Optional] ref UnicodeString Value
            );

        #endregion

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int VirtualQueryEx(
         [In] IntPtr Process,
         [In] [Optional] IntPtr Address,
         [Out] [MarshalAs(UnmanagedType.Struct)] out MemoryBasicInformation Buffer,
         [In] int Size
         );

        [DllImport("ntdll.dll")]
        public static extern NtStatus NtQueryVirtualMemory(
           [In] IntPtr ProcessHandle,
           [In] IntPtr BaseAddress,
           [In] MemoryInformationClass MemoryInformationClass,
           [In] IntPtr Buffer,
           [In] IntPtr MemoryInformationLength,
           [Out] [Optional] out IntPtr ReturnLength
           );

        [DllImport("ntdll.dll")]
        public static extern NtStatus NtQueryVirtualMemory(
            [In] IntPtr ProcessHandle,
            [In] IntPtr BaseAddress,
            [In] MemoryInformationClass MemoryInformationClass,
            [Out] out MemoryBasicInformation Buffer,
            [In] IntPtr MemoryInformationLength,
            [Out] [Optional] out IntPtr ReturnLength
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AllocateUserPhysicalPages(
            [In] IntPtr ProcessHandle,
            ref IntPtr NumberOfPages,
            IntPtr[] UserPfnArray
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeUserPhysicalPages(
            [In] IntPtr ProcessHandle,
            ref IntPtr NumberOfPages,
            IntPtr[] UserPfnArray
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool MapUserPhysicalPages(
            [In] IntPtr Address,
            IntPtr NumberOfPages,
            IntPtr[] UserPfnArray
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LocalAlloc(
            [In] AllocFlags Flags,
            [In] int Bytes
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LocalReAlloc(
            [In] IntPtr Memory,
            [In] AllocFlags Flags,
            [In] int Bytes
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LocalFree(
            [In] IntPtr Memory
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int GetProcessHeaps(
            [In] int NumberOfHeaps,
            [Out] IntPtr[] Heaps
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int HeapCompact(
            [In] IntPtr Heap,
            [In] bool NoSerialize
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr HeapCreate(
            [In] HeapFlags Flags,
            [In] IntPtr InitialSize,
            [In] IntPtr MaximumSize
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool HeapDestroy(
            [In] IntPtr Heap
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool HeapFree(
            [In] IntPtr Heap,
            [In] HeapFlags Flags,
            [In] IntPtr Memory
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr HeapAlloc(
            [In] IntPtr Heap,
            [In] HeapFlags Flags,
            [In] IntPtr Bytes
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr HeapReAlloc(
            [In] IntPtr Heap,
            [In] HeapFlags Flags,
            [In] IntPtr Memory,
            [In] IntPtr Bytes
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcessHeap();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VirtualProtectEx(
            [In] IntPtr Process,
            [In] IntPtr Address,
            [In] int Size,
            [In] MemoryProtection NewProtect,
            [Out] out MemoryProtection OldProtect
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(
            [In] IntPtr Process,
            [In] [Optional] IntPtr Address,
            [In] int Size,
            [In] MemoryState Type,
            [In] MemoryProtection Protect
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VirtualFreeEx(
            [In] IntPtr Process,
            [In] IntPtr Address,
            [In] int Size,
            [In] MemoryState FreeType
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadProcessMemory(
            [In] IntPtr Process,
            [In] IntPtr BaseAddress,
            [Out] byte[] Buffer,
            [In] int Size,
            [Out] out int BytesRead
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public unsafe static extern bool ReadProcessMemory(
            [In] IntPtr Process,
            [In] IntPtr BaseAddress,
            [Out] void* Buffer,
            [In] int Size,
            [Out] out int BytesRead
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteProcessMemory(
            [In] IntPtr Process,
            [In] IntPtr BaseAddress,
            [In] byte[] Buffer,
            [In] int Size,
            [Out] out int BytesWritten
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public unsafe static extern bool WriteProcessMemory(
            [In] IntPtr Process,
            [In] IntPtr BaseAddress,
            [In] void* Buffer,
            [In] int Size,
            [Out] out int BytesWritten
            );

        [DllImport("ntdll.dll")]
        public static extern IntPtr RtlCompareMemory(
           [In] IntPtr Source1,
           [In] IntPtr Source2,
           [In] IntPtr Length
           );

        [DllImport("ntdll.dll")]
        public static extern void RtlFillMemory(
            [In] IntPtr Destination,
            [In] IntPtr Length,
            [In] byte Fill
            );

        [DllImport("ntdll.dll")]
        public static extern void RtlMoveMemory(
            [In] IntPtr Destination,
            [In] IntPtr Source,
            [In] IntPtr Length
            );

        [DllImport("ntdll.dll")]
        public static extern void RtlZeroMemory(
            [In] IntPtr Destination,
            [In] IntPtr Length
            );

        [DllImport("ntdll.dll")]
        public static extern IntPtr RtlAllocateHeap(
            [In] IntPtr HeapHandle,
            [In] HeapFlags Flags,
            [In] IntPtr Size
            );

        [DllImport("ntdll.dll")]
        public static extern IntPtr RtlCompactHeap(
            [In] IntPtr HeapHandle,
            [In] HeapFlags Flags
            );

        [DllImport("ntdll.dll")]
        public static extern IntPtr RtlCreateHeap(
            [In] HeapFlags Flags,
            [In] [Optional] IntPtr HeapBase,
            [In] [Optional] IntPtr ReserveSize,
            [In] [Optional] IntPtr CommitSize,
            [In] [Optional] IntPtr Lock,
            [In] [Optional] IntPtr Parameters
            );

        [DllImport("ntdll.dll")]
        public static extern IntPtr RtlDestroyHeap(
            [In] IntPtr HeapHandle
            );

        [DllImport("ntdll.dll")]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool RtlFreeHeap(
            [In] IntPtr HeapHandle,
            [In] HeapFlags Flags,
            [In] IntPtr BaseAddress
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlGetProcessHeaps(
            [In] int NumberOfHeaps,
            IntPtr[] ProcessHeaps
            );

        [DllImport("ntdll.dll")]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool RtlLockHeap(
            [In] IntPtr HeapHandle
            );

        [DllImport("ntdll.dll")]
        public static extern void RtlProtectHeap(
            [In] IntPtr HeapHandle,
            [In] bool MakeReadOnly
            );

        [DllImport("ntdll.dll")]
        public static extern IntPtr RtlReAllocateHeap(
            [In] IntPtr HeapHandle,
            [In] HeapFlags Flags,
            [In] IntPtr BaseAddress,
            [In] IntPtr Size
            );

        [DllImport("ntdll.dll")]
        public static extern IntPtr RtlSizeHeap(
            [In] IntPtr HeapHandle,
            [In] HeapFlags Flags,
            [In] IntPtr BaseAddress
            );

        [DllImport("ntdll.dll")]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool RtlUnlockHeap(
            [In] IntPtr HeapHandle
            );

        [DllImport("ntdll.dll")]
        public static extern NtStatus RtlZeroHeap(
            [In] IntPtr HeapHandle,
            [In] HeapFlags Flags
            );
    }

    [Flags]
    public enum RunFileDialogFlags : uint
    {
        /// <summary>
        /// Don't use any of the flags (only works alone)
        /// </summary>
        None = 0x0000,
        /// <summary>
        /// Removes the browse button
        /// </summary>
        NoBrowse = 0x0001,
        /// <summary>
        /// No default item selected
        /// </summary>
        NoDefault = 0x0002,
        /// <summary>
        /// Calculates the working directory from the file name
        /// </summary>
        CalcDirectory = 0x0004,
        /// <summary>
        /// Removes the edit box label
        /// </summary>
        NoLabel = 0x0008,
        /// <summary>
        /// Removes the separate memory space checkbox (Windows NT only)
        /// </summary>
        NoSeparateMemory = 0x0020
    }

    [Flags]
    public enum FileAccess : uint
    {
        ReadData = 0x0001, // File, Named Pipe
        ListDirectory = 0x0001, // Directory

        WriteData = 0x0002, // File, Named Pipe
        AddFile = 0x0002, // Directory

        AppendData = 0x0004, // File
        AddSubdirectory = 0x0004, // Directory
        CreatePipeInstance = 0x0004, // Named Pipe

        ReadEa = 0x0008, // File, Directory

        WriteEa = 0x0010, // File, Directory

        Execute = 0x0020, // File
        Traverse = 0x0020, // Directory

        DeleteChild = 0x0040, // Directory

        ReadAttributes = 0x0080, // All

        WriteAttributes = 0x0100, // All

        All = StandardRights.Required | StandardRights.Synchronize | 0x1ff,
        GenericRead = StandardRights.Read | ReadData | ReadAttributes | ReadEa |
            StandardRights.Synchronize,
        GenericWrite = StandardRights.Write | WriteData | WriteAttributes | WriteEa |
            AppendData | StandardRights.Synchronize,
        GenericExecute = StandardRights.Execute | ReadAttributes | Execute |
            StandardRights.Synchronize
    }

    [Flags]
    public enum StandardRights : uint
    {
        Delete = 0x00010000,
        ReadControl = 0x00020000,
        WriteDac = 0x00040000,
        WriteOwner = 0x00080000,
        Synchronize = 0x00100000,
        Required = 0x000f0000,
        Read = ReadControl,
        Write = ReadControl,
        Execute = ReadControl,
        All = 0x001f0000,

        SpecificRightsAll = 0x0000ffff,
        AccessSystemSecurity = 0x01000000,
        MaximumAllowed = 0x02000000,
        GenericRead = 0x80000000,
        GenericWrite = 0x40000000,
        GenericExecute = 0x20000000,
        GenericAll = 0x10000000
    }


    [Flags]
    public enum EnlistmentOptions : int
    {
        Superior = 0x1,
        MaximumOption = 0x1
    }

    public enum EventInformationClass : int
    {
        EventBasicInformation
    }

    public enum EventType : int
    {
        NotificationEvent,
        SynchronizationEvent
    }

    public enum FileAlignment : int
    {
        Byte = 0x0,
        Word = 0x1,
        Long = 0x3,
        Quad = 0x7,
        Octa = 0xf,
        ThirtyTwoByte = 0x1f,
        SixtyFourByte = 0x3f,
        OneHundredAndTwentyEightByte = 0x7f,
        TwoHundredAndFiftySixByte = 0xff,
        FiveHundredAndTwelveByte = 0x1ff
    }

    [Flags]
    public enum FileAttributes : uint
    {
        ReadOnly = 0x1,
        Hidden = 0x2,
        System = 0x4,

        Directory = 0x10,
        Archive = 0x20,
        Device = 0x40,
        Normal = 0x80,

        Temporary = 0x100,
        SparseFile = 0x200,
        ReparsePoint = 0x400,
        Compressed = 0x800,

        Offline = 0x1000,
        NotContextIndexed = 0x2000,
        Encrypted = 0x4000
    }

    [Flags]
    public enum FileCreateOptions : uint
    {
        DirectoryFile = 0x1,
        WriteThrough = 0x2,
        SequentialOnly = 0x4,
        NoIntermediateBuffering = 0x8,

        SynchronousIoAlert = 0x10,
        SynchronousIoNonAlert = 0x20,
        NonDirectoryFile = 0x40,
        CreateTreeConnection = 0x80,

        CompleteIfOpLocked = 0x100,
        NoEaKnowledge = 0x200,
        OpenForRecovery = 0x400,
        RandomAccess = 0x800,

        DeleteOnClose = 0x1000,
        OpenByFileId = 0x2000,
        OpenForBackupIntent = 0x4000,
        NoCompression = 0x8000,

        ReserveOpFilter = 0x100000,
        OpenReparsePoint = 0x200000,
        OpenNoRecall = 0x400000,
        OpenForFreeSpaceQuery = 0x800000,

        CopyStructuredStorage = 0x41,
        StructuredStorage = 0x441,

        ValidOptionFlags = 0xffffff,
        ValidPipeOptionFlags = 0x32,
        ValidMailslotOptionFlags = 0x32,
        ValidSetFlags = 0x36
    }

    public enum FileCreationDisposition : int
    {
        Supersede = 0x0,
        Open = 0x1,
        Create = 0x2,
        OpenIf = 0x3,
        Overwrite = 0x4,
        OverwriteIf = 0x5
    }

    public enum FileInformationClass : int
    {
        FileDirectoryInformation = 1, // dir
        FileFullDirectoryInformation, // dir
        FileBothDirectoryInformation, // dir
        FileBasicInformation,
        FileStandardInformation,
        FileInternalInformation,
        FileEaInformation,
        FileAccessInformation,
        FileNameInformation,
        FileRenameInformation, // 10
        FileLinkInformation,
        FileNamesInformation, // dir
        FileDispositionInformation,
        FilePositionInformation,
        FileFullEaInformation,
        FileModeInformation,
        FileAlignmentInformation,
        FileAllInformation,
        FileAllocationInformation,
        FileEndOfFileInformation, // 20
        FileAlternateNameInformation,
        FileStreamInformation,
        FilePipeInformation,
        FilePipeLocalInformation,
        FilePipeRemoteInformation,
        FileMailslotQueryInformation,
        FileMailslotSetInformation,
        FileCompressionInformation,
        FileObjectIdInformation, // dir
        FileCompletionInformation, // 30
        FileMoveClusterInformation,
        FileQuotaInformation,
        FileReparsePointInformation,
        FileNetworkOpenInformation,
        FileAttributeTagInformation,
        FileTrackingInformation,
        FileIdBothDirectoryInformation, // dir
        FileIdFullDirectoryInformation, // dir
        FileValidDataLengthInformation,
        FileShortNameInformation, // 40
        FileIoCompletionNotificationInformation,
        FileIoStatusBlockRangeInformation,
        FileIoPriorityHintInformation,
        FileSfioReserveInformation,
        FileSfioVolumeInformation,
        FileHardLinkInformation,
        FileProcessIdsUsingFileInformation,
        FileNormalizedNameInformation,
        FileNetworkPhysicalNameInformation,
        FileIdGlobalTxDirectoryInformation, // 50
        FileMaximumInformation
    }

    public enum FileIoStatus : int
    {
        Superseded = 0,
        Opened = 1,
        Created = 2,
        Overwritten = 3,
        Exists = 4,
        DoesNotExist = 5
    }

    public enum FileNotifyAction : int
    {
        Added = 0x1,
        Removed = 0x2,
        Modified = 0x3,
        RenamedOldName = 0x4,
        RenamedNewName = 0x5,
        AddedStream = 0x6,
        RemovedStream = 0x7,
        ModifiedStream = 0x8,
        RemovedByDelete = 0x9,
        IdNotTunnelled = 0xa,
        TunnelledIdCollision = 0xb
    }

    [Flags]
    public enum FileNotifyFlags : int
    {
        FileName = 0x1,
        DirName = 0x2,
        Name = 0x3,
        Attributes = 0x4,
        Size = 0x8,
        LastWrite = 0x10,
        LastAccess = 0x20,
        Creation = 0x40,
        Ea = 0x80,
        Security = 0x100,
        StreamName = 0x200,
        StreamSize = 0x400,
        StreamWrite = 0x800,
        Valid = 0xfff
    }

    [Flags]
    public enum FileObjectFlags : int
    {
        FileOpen = 0x00000001,
        SynchronousIo = 0x00000002,
        AlertableIo = 0x00000004,
        NoIntermediateBuffering = 0x00000008,
        WriteThrough = 0x00000010,
        SequentialOnly = 0x00000020,
        CacheSupported = 0x00000040,
        NamedPipe = 0x00000080,
        StreamFile = 0x00000100,
        MailSlot = 0x00000200,
        GenerateAuditOnClose = 0x00000400,
        QueueIrpToThread = GenerateAuditOnClose,
        DirectDeviceOpen = 0x00000800,
        FileModified = 0x00001000,
        FileSizeChanged = 0x00002000,
        CleanupComplete = 0x00004000,
        TemporaryFile = 0x00008000,
        DeleteOnClose = 0x00010000,
        OpenedCaseSensitivity = 0x00020000,
        HandleCreated = 0x00040000,
        FileFastIoRead = 0x00080000,
        RandomAccess = 0x00100000,
        FileOpenCancelled = 0x00200000,
        VolumeOpen = 0x00400000,
        RemoteOrigin = 0x01000000,
        SkipCompletionPort = 0x02000000,
        SkipSetEvent = 0x04000000,
        SkipSetFastIo = 0x08000000
    }

    [Flags]
    public enum FileShareMode : uint
    {
        Exclusive = 0x0,
        Read = 0x1,
        Write = 0x2,
        Delete = 0x4,

        ReadWrite = Read | Write,
        ReadWriteDelete = Read | Write | Delete
    }

    public enum FsInformationClass : int
    {
        FileFsVolumeInformation = 1,
        FileFsLabelInformation,
        FileFsSizeInformation,
        FileFsDeviceInformation,
        FileFsAttributeInformation,
        FileFsControlInformation,
        FileFsFullSizeInformation,
        FileFsObjectIdInformation,
        FileFsDriverPathInformation,
        FileFsVolumeFlagsInformation, // 10
        FileFsMaximumInformation
    }

    public enum DepFlags : uint
    {
        Disable = 0x00000000,
        Enable = 0x00000001,
        DisableAtlThunkEmulation = 0x00000002
    }

    public enum DepSystemStrategyType : int
    {
        AlwaysOff = 0,
        AlwaysOn,
        OptIn,
        OptOut
    }

    [Flags]
    public enum ExitWindowsFlags : uint
    {
        Logoff = 0x0,
        Poweroff = 0x8,
        Reboot = 0x2,
        RestartApps = 0x40,
        Shutdown = 0x1,
        Force = 0x4,
        ForceIfHung = 0x10
    }

    public enum FileCreationDispositionWin32 : uint
    {
        /// <summary>
        /// Creates a new file. The function fails if the specified file already exists.
        /// </summary>
        CreateNew = 1,
        /// <summary>
        /// Creates a new file. If the file exists, the function overwrites the file and clears the existing attributes.
        /// </summary>
        CreateAlways = 2,
        /// <summary>
        /// Opens the file. The function fails if the file does not exist. 
        /// </summary>
        OpenExisting = 3,
        /// <summary>
        /// Opens the file, if it exists. If the file does not exist, the function creates the file.
        /// </summary>
        OpenAlways = 4,
        /// <summary>
        /// Opens the file. Once opened, the file is truncated so that its size is zero bytes. 
        /// The function fails if the file does not exist.
        /// </summary>
        TruncateExisting = 5
    }

    [Flags]
    public enum RtlDuplicateUnicodeStringFlags : int
    {
        NullTerminate = 0x1,
        AllocateNullString = 0x2
    }

    [Flags]
    public enum HandleFlags : byte
    {
        ProtectFromClose = 0x1,
        Inherit = 0x2,
        AuditObjectClose = 0x4
    }

    public enum HandleTraceType : int
    {
        Open = 1,
        Close = 2,
        BadRef = 3
    }
}
