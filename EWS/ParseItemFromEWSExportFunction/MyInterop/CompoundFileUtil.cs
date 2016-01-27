﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MyInterop
{
    public class CompoundFileUtil
    {
        [DllImport("ole32.dll")]
        public static extern int StgIsStorageFile([MarshalAs(UnmanagedType.LPWStr)] string pwcsName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalAlloc(int uFlags, int dwBytes);

        [DllImport("kernel32.dll")]
        public static extern UIntPtr GlobalSize(IntPtr hMem);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalFree(IntPtr hMem);

        [DllImport("ole32.dll")]
        public static extern int StgOpenStorage(
            [MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
            IStorage pstgPriority,
            STGM grfMode,
            IntPtr snbExclude,
            uint reserved,
            out IStorage ppstgOpen);

        /// <summary>
        /// The StgCreateDocfileOnILockBytes function creates and opens a new compound file 
        /// storage object on top of a byte-array object provided by the caller. The storage 
        /// object supports the COM-provided, compound-file implementation for the IStorage interface.
        /// </summary>
        /// <param name="plkbyt">A pointer to the ILockBytes interface on the underlying byte-array object on which to create a compound file.</param>
        /// <param name="grfMode">Specifies the access mode to use when opening the new compound file. For more information, see STGM Constants.</param>
        /// <param name="reserved">Reserved for future use; must be zero.</param>
        /// <param name="ppstgOpen">A pointer to the location of the IStorage pointer on the new storage object.</param>
        /// <returns>
        /// S_OK
        ///    Indicates that the compound file was successfully created.
        ///STG_E_ACCESSDENIED
        ///    Access denied because the caller does not have enough permissions, or another caller has the file open and locked.
        ///STG_E_FILEALREADYEXISTS
        ///    Indicates that the compound file already exists and the grfMode parameter is set to STGM_FAILIFTHERE.
        ///STG_E_INSUFFICIENTMEMORY
        ///    Indicates that the storage object was not created due to inadequate memory.
        ///STG_E_INVALIDPOINTER
        ///    Indicates that a non-valid pointer was in the pLkbyt parameter or the ppStgOpen parameter.
        ///STG_E_INVALIDFLAG
        ///    Indicates that a non-valid flag combination was in the grfMode parameter.
        ///STG_E_TOOMANYOPENFILES
        ///    Indicates that the storage object was not created due to a lack of file handles.
        ///STG_E_LOCKVIOLATION
        ///    Access denied because another caller has the file open and locked.
        ///STG_E_SHAREVIOLATION
        ///    Access denied because another caller has the file open and locked.
        ///STG_S_CONVERTED
        ///    Indicates that the compound file was successfully converted. The original byte-array object was successfully converted to IStorage format.
        /// </returns>
        [DllImport("ole32.dll")]
        public extern static int StgCreateDocfileOnILockBytes(ILockBytes plkbyt, STGM grfMode, int reserved, out IStorage ppstgOpen);

        /// <summary>
        /// The CreateILockBytesOnHGlobal function creates a byte array object, 
        /// using global memory as the physical device, which is intended to be the 
        /// compound file foundation. This object supports a COM implementation of the 
        /// ILockBytes interface.
        /// </summary>
        /// 
        /// <param name="hGlobal">The memory handle allocated by the GlobalAlloc function. 
        /// The handle must be allocated as moveable and nondiscardable. If the handle is 
        /// shared between processes, it must also be allocated as shared. New handles should 
        /// be allocated with a size of zero. If hGlobal is NULL, CreateILockBytesOnHGlobal 
        /// internally allocates a new shared memory block of size zero.</param>
        /// 
        /// <param name="fDeleteOnRelease">A flag that specifies whether the underlying handle 
        /// for this byte array object should be automatically freed when the object is released. 
        /// If set to FALSE, the caller must free the hGlobal after the final release. If set 
        /// to TRUE, the final release will automatically free the hGlobal parameter.</param>
        /// 
        /// <param name="ppLkbyt">The address of ILockBytes pointer variable that receives the 
        /// interface pointer to the new byte array object.</param>
        /// 
        /// <returns>This function supports the standard return values E_INVALIDARG and E_OUTOFMEMORY, as well as the following:
        /// S_OK: The byte array object was created successfully.</returns>
        [DllImport("ole32.dll")]
        public extern static int CreateILockBytesOnHGlobal(IntPtr hGlobal, [MarshalAs(UnmanagedType.Bool)] bool fDeleteOnRelease, out ILockBytes ppLkbyt);

        [DllImport("ole32.dll", ExactSpelling = true, PreserveSig = false)]
        static extern void WriteClassStg(
            IStorage pStg,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid rclsid);

        //public CompoundFileUtil(string filePathForRead)
        //{

        //}

        public CompoundFileUtil(string filePathForCreate, Guid rclsid)
        {
            _fileName = filePathForCreate;
            _openFileErrorCode = StgOpenStorage(filePathForCreate, null, STGM.READWRITE | STGM.TRANSACTED | STGM.CREATE, IntPtr.Zero, 0, out _rootStorage);
            if (_openFileErrorCode != 0)
                _rootStorage = null;
            else
            {
                RootStorage.SetClass(rclsid);
            }
        }

        //public CompoundFileUtil(string fileName, int stgmMode)
        //{

        //}


        private readonly int _openFileErrorCode;
        private readonly string _fileName;
        private readonly IStorage _rootStorage;
        public IStorage RootStorage
        {
            get
            {
                if (_openFileErrorCode != 0)
                    throw new FileLoadException(string.Format("File [{0}] open failed with error code [{1}]", _fileName, _openFileErrorCode));
                return _rootStorage;
            }
        }

        public IStorage CreateStorage(string storageName, IStorage parentStorage = null)
        {
            var storage = RootStorage;
            if (parentStorage != null)
            {
                storage = parentStorage;
            }

            IStorage childStorage;
            storage.CreateStorage(storageName, (uint)(STGM.CREATE | STGM.SHARE_EXCLUSIVE | STGM.READWRITE), 0, 0, out childStorage);
            return childStorage;
        }

        public IStream CreateStream(string streamName, IStorage parentStorage = null)
        {
            var storage = RootStorage;
            if (parentStorage != null)
            {
                storage = parentStorage;
            }

            IStream childStream;
            storage.CreateStream(streamName, (uint)(STGM.CREATE | STGM.SHARE_EXCLUSIVE | STGM.READWRITE), 0, 0, out childStream);
            return childStream;
        }


    }

    [ComVisible(false)]
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000000A-0000-0000-C000-000000000046")]
    public interface ILockBytes
    {
        //Note: These two by(reference 32-bit integers (ULONG) could be used as return values instead,
        //      but they are not tagged [retval] in the IDL, so for consitency's sake...
        void ReadAt(long ulOffset, System.IntPtr pv, int cb, out System.UInt32 pcbRead);
        void WriteAt(long ulOffset, System.IntPtr pv, int cb, out System.UInt32 pcbWritten);
        void Flush();
        void SetSize(long cb);
        void LockRegion(long libOffset, long cb, int dwLockType);
        void UnlockRegion(long libOffset, long cb, int dwLockType);
        void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag);
    }

    [Flags]
    public enum GMEMFlag
    {
        GMEM_FIXED = 0x0000,
        GHND = 0x0042,
        GMEM_MOVEABLE = 0x0002,
        GMEM_ZEROINIT = 0x0040,
        GPTR = 0x0040
    }

    [Flags]
    public enum STGM : int
    {
        DIRECT = 0x00000000,
        TRANSACTED = 0x00010000,
        SIMPLE = 0x08000000,
        READ = 0x00000000,
        WRITE = 0x00000001,
        READWRITE = 0x00000002,
        SHARE_DENY_NONE = 0x00000040,
        SHARE_DENY_READ = 0x00000030,
        SHARE_DENY_WRITE = 0x00000020,
        SHARE_EXCLUSIVE = 0x00000010,
        PRIORITY = 0x00040000,
        DELETEONRELEASE = 0x04000000,
        NOSCRATCH = 0x00100000,
        CREATE = 0x00001000,
        CONVERT = 0x00020000,
        FAILIFTHERE = 0x00000000,
        NOSNAPSHOT = 0x00200000,
        DIRECT_SWMR = 0x00400000,
    }

    [ComImport]
    [Guid("0000000d-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumSTATSTG
    {
        // The user needs to allocate an STATSTG array whose size is celt.
        [PreserveSig]
        uint Next(
            uint celt,
            [MarshalAs(UnmanagedType.LPArray), Out]
            System.Runtime.InteropServices.ComTypes.STATSTG[] rgelt,
            out uint pceltFetched
        );

        void Skip(uint celt);

        void Reset();

        [return: MarshalAs(UnmanagedType.Interface)]
        IEnumSTATSTG Clone();
    }

    public enum STATFLAG : uint
    {
        STATFLAG_DEFAULT = 0,
        STATFLAG_NONAME = 1,
        STATFLAG_NOOPEN = 2
    }

    public enum STGTY : int
    {
        STGTY_STORAGE = 1,
        STGTY_STREAM = 2,
        STGTY_LOCKBYTES = 3,
        STGTY_PROPERTY = 4
    }

    public enum STREAM_SEEK : int
    {
        STREAM_SEEK_SET = 0,
        STREAM_SEEK_CUR = 1,
        STREAM_SEEK_END = 2
    };

    [ComImport]
    [Guid("0000000b-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStorage
    {
        void CreateStream(
            /* [string][in] */ string pwcsName,
            /* [in] */ uint grfMode,
            /* [in] */ uint reserved1,
            /* [in] */ uint reserved2,
            /* [out] */ out IStream ppstm);

        void OpenStream(
            /* [string][in] */ string pwcsName,
            /* [unique][in] */ IntPtr reserved1,
            /* [in] */ uint grfMode,
            /* [in] */ uint reserved2,
            /* [out] */ out IStream ppstm);

        void CreateStorage(
            /* [string][in] */ string pwcsName,
            /* [in] */ uint grfMode,
            /* [in] */ uint reserved1,
            /* [in] */ uint reserved2,
            /* [out] */ out IStorage ppstg);

        void OpenStorage(
            /* [string][unique][in] */ string pwcsName,
            /* [unique][in] */ IStorage pstgPriority,
            /* [in] */ uint grfMode,
            /* [unique][in] */ IntPtr snbExclude,
            /* [in] */ uint reserved,
            /* [out] */ out IStorage ppstg);

        void CopyTo(
            /* [in] */ uint ciidExclude,
            /* [size_is][unique][in] */ Guid[] rgiidExclude, // should this be an array?
            /* [unique][in] */ IntPtr snbExclude,
            /* [unique][in] */ IStorage pstgDest);

        void MoveElementTo(
            /* [string][in] */ string pwcsName,
            /* [unique][in] */ IStorage pstgDest,
            /* [string][in] */ string pwcsNewName,
            /* [in] */ uint grfFlags);

        void Commit(
            /* [in] */ uint grfCommitFlags);

        void Revert();

        void EnumElements(
            /* [in] */ uint reserved1,
            /* [size_is][unique][in] */ IntPtr reserved2,
            /* [in] */ uint reserved3,
            /* [out] */ out IEnumSTATSTG ppenum);

        void DestroyElement(
            /* [string][in] */ string pwcsName);

        void RenameElement(
            /* [string][in] */ string pwcsOldName,
            /* [string][in] */ string pwcsNewName);

        void SetElementTimes(
            /* [string][unique][in] */ string pwcsName,
            /* [unique][in] */ System.Runtime.InteropServices.ComTypes.FILETIME pctime,
            /* [unique][in] */ System.Runtime.InteropServices.ComTypes.FILETIME patime,
            /* [unique][in] */ System.Runtime.InteropServices.ComTypes.FILETIME pmtime);

        void SetClass(
            /* [in] */ Guid clsid);

        void SetStateBits(
            /* [in] */ uint grfStateBits,
            /* [in] */ uint grfMask);

        void Stat(
            /* [out] */ out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg,
            /* [in] */ uint grfStatFlag);

    }
}