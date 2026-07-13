// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

// This was an attempt for AOT using CsWin32
/*
global using Windows.Win32.Foundation;
global using Windows.Win32.System.Com;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;
*/


// Source: David Anson, Microsoft. https://dlaa.me/blog/post/9923072

/*
Portions of this software's code not covered by another author's or entity's copyright are released under the Creative Commons Zero (CC0) public domain license.

To the extent possible under law, Shendare (Jon D. Jackson) has waived all copyright and related or neighboring rights to this EQ-Zip application. This work is published from: The United States.

You may copy, modify, and distribute the work, even for commercial purposes, without asking permission.

For more information, read the CC0 summary and full legal text here:

https://creativecommons.org/publicdomain/zero/1.0/
*/

/*
#pragma warning disable CA1416 // Validate platform compatibility

namespace ArcExplorer;

/// <summary>
/// Class implementing drag/drop and clipboard support for virtual files.
/// Also offers an alternate interface to the IDataObject interface.
/// </summary>
[GeneratedComClass]
public partial class VirtualFileDataObject : IDataObject, IAsyncOperation
{
    /// <summary>
    /// Gets or sets a value indicating whether the data object can be used asynchronously.
    /// </summary>
    public bool IsAsynchronous { get; set; }

    private static readonly ushort FILECONTENTS = (ushort)PInvoke.RegisterClipboardFormat(NativeMethods.CFSTR_FILECONTENTS);
    private static readonly ushort FILEDESCRIPTORW = (ushort)PInvoke.RegisterClipboardFormat(NativeMethods.CFSTR_FILEDESCRIPTORW);
    private static readonly ushort PASTESUCCEEDED = (ushort)PInvoke.RegisterClipboardFormat(NativeMethods.CFSTR_PASTESUCCEEDED);
    private static readonly ushort PERFORMEDDROPEFFECT = (ushort)PInvoke.RegisterClipboardFormat(NativeMethods.CFSTR_PERFORMEDDROPEFFECT);
    private static readonly ushort PREFERREDDROPEFFECT = (ushort)PInvoke.RegisterClipboardFormat(NativeMethods.CFSTR_PREFERREDDROPEFFECT);

    /// <summary>
    /// In-order list of registered data objects.
    /// </summary>
    private readonly List<DataObject> _dataObjects = [];

    /// <summary>
    /// Tracks whether an asynchronous operation is ongoing.
    /// </summary>
    private bool _inOperation;

    /// <summary>
    /// Stores the user-specified start action.
    /// </summary>
    private readonly Action<VirtualFileDataObject> _startAction;

    /// <summary>
    /// Stores the user-specified end action.
    /// </summary>
    private readonly Action<VirtualFileDataObject> _endAction;

    /// <summary>
    /// Initializes a new instance of the VirtualFileDataObject class.
    /// </summary>
    public VirtualFileDataObject()
    {
        IsAsynchronous = true;
    }

    private static StrategyBasedComWrappers _wrappers;

    static VirtualFileDataObject()
    {
        _wrappers = new StrategyBasedComWrappers();
    }

    /// <summary>
    /// Initializes a new instance of the VirtualFileDataObject class.
    /// </summary>
    /// <param name="startAction">Optional action to run at the start of the data transfer.</param>
    /// <param name="endAction">Optional action to run at the end of the data transfer.</param>
    public VirtualFileDataObject(Action<VirtualFileDataObject> startAction, Action<VirtualFileDataObject> endAction)
        : this()
    {
        _startAction = startAction;
        _endAction = endAction;
    }

    #region IDataObject Members
    // Explicit interface implementation hides the technical details from users of VirtualFileDataObject.

    /// <summary>
    /// Creates a connection between a data object and an advisory sink.
    /// </summary>
    /// <param name="pFormatetc">A FORMATETC structure that defines the format, target device, aspect, and medium that will be used for future notifications.</param>
    /// <param name="advf">One of the ADVF values that specifies a group of flags for controlling the advisory connection.</param>
    /// <param name="adviseSink">A pointer to the IAdviseSink interface on the advisory sink that will receive the change notification.</param>
    /// <param name="connection">When this method returns, contains a pointer to a DWORD token that identifies this connection.</param>
    /// <returns>HRESULT success code.</returns>
    unsafe int IDataObject.DAdvise(FORMATETC* pFormatetc, uint advf, IAdviseSink* adviseSink, out int connection)
    {
        Marshal.ThrowExceptionForHR(NativeMethods.OLE_E_ADVISENOTSUPPORTED);
        throw new NotImplementedException();
    }

    /// <summary>
    /// Destroys a notification connection that had been previously established.
    /// </summary>
    /// <param name="connection">A DWORD token that specifies the connection to remove.</param>
    int IDataObject.DUnadvise(int connection)
    {
        Marshal.ThrowExceptionForHR(NativeMethods.OLE_E_ADVISENOTSUPPORTED);
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates an object that can be used to enumerate the current advisory connections.
    /// </summary>
    /// <param name="enumAdvise">When this method returns, contains an IEnumSTATDATA that receives the interface pointer to the new enumerator object.</param>
    /// <returns>HRESULT success code.</returns>
    unsafe int IDataObject.EnumDAdvise(IEnumSTATDATA* enumAdvise)
    {
        Marshal.ThrowExceptionForHR(NativeMethods.OLE_E_ADVISENOTSUPPORTED);
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates an object for enumerating the FORMATETC structures for a data object.
    /// </summary>
    /// <param name="direction">One of the DATADIR values that specifies the direction of the data.</param>
    /// <returns>IEnumFORMATETC interface.</returns>
    unsafe IEnumFORMATETC* IDataObject.EnumFormatEtc(int direction)
    {
        if ((DATADIR)direction == DATADIR.DATADIR_GET)
        {
            if (_dataObjects.Count == 0)
            {
                // Note: SHCreateStdEnumFmtEtc fails for a count of 0; throw helpful exception
                throw new InvalidOperationException("VirtualFileDataObject requires at least one data object to enumerate.");
            }

            // Create enumerator and return it
            IEnumFORMATETC* enumerator;
            if (PInvoke.SHCreateStdEnumFmtEtc(_dataObjects.Select(d => d.FORMATETC).ToArray(), &enumerator).Succeeded)
                return enumerator;

            // Returning null here can cause an AV in the caller; throw instead
            Marshal.ThrowExceptionForHR(NativeMethods.E_FAIL);
        }
        throw new NotImplementedException();
    }

    /// <summary>
    /// Provides a standard FORMATETC structure that is logically equivalent to a more complex structure.
    /// </summary>
    /// <param name="formatIn">A pointer to a FORMATETC structure that defines the format, medium, and target device that the caller would like to use to retrieve data in a subsequent call such as GetData.</param>
    /// <param name="formatOut">When this method returns, contains a pointer to a FORMATETC structure that contains the most general information possible for a specific rendering, making it canonically equivalent to formatetIn.</param>
    /// <returns>HRESULT success code.</returns>
    unsafe int IDataObject.GetCanonicalFormatEtc(FORMATETC* formatIn, FORMATETC* formatOut)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Obtains data from a source data object.
    /// </summary>
    /// <param name="format">A pointer to a FORMATETC structure that defines the format, medium, and target device to use when passing the data.</param>
    /// <param name="medium">When this method returns, contains a pointer to the STGMEDIUM structure that indicates the storage medium containing the returned data through its tymed member, and the responsibility for releasing the medium through the value of its pUnkForRelease member.</param>
    unsafe int IDataObject.GetData(FORMATETC* format, STGMEDIUM* medium)
    {
        *medium = new STGMEDIUM();
        HRESULT hr = (HRESULT)((IDataObject)this).QueryGetData(format);
        if (hr.Succeeded)
        {
            // Find the best match
            var formatCopy = format; // Cannot use ref or out parameter inside an anonymous method, lambda expression, or query expression
            var dataObject = _dataObjects.FirstOrDefault(d =>
                    (d.FORMATETC.cfFormat == formatCopy->cfFormat) &&
                    (d.FORMATETC.dwAspect == formatCopy->dwAspect) &&
                    ((d.FORMATETC.tymed & formatCopy->tymed) != 0 && (d.FORMATETC.lindex == formatCopy->lindex || formatCopy->lindex == -1)));

            if (dataObject != null)
            {
                if (!IsAsynchronous && (FILEDESCRIPTORW == dataObject.FORMATETC.cfFormat) && !_inOperation)
                {
                    // Enter the operation and call the start action
                    _inOperation = true;
                    _startAction?.Invoke(this);
                }

                // Populate the STGMEDIUM
                medium->tymed = (TYMED)dataObject.FORMATETC.tymed;
                var result = dataObject.GetData(); // Possible call to user code
                hr = result.Item2;

                if (hr.Succeeded)
                {
                    if ((TYMED)dataObject.FORMATETC.tymed == TYMED.TYMED_HGLOBAL)
                        medium->u.hGlobal = new HGLOBAL(result.Item1);
                    else if ((TYMED)dataObject.FORMATETC.tymed == TYMED.TYMED_ISTREAM)
                        medium->u.pstm = (IStream*)result.Item1;
                }
            }
            else
            {
                // Couldn't find a match
                hr = NativeMethods.DV_E_FORMATETC;
            }
        }

        if (hr.Failed)
            hr.ThrowOnFailure();

        return hr;
    }

    /// <summary>
    /// Obtains data from a source data object.
    /// </summary>
    /// <param name="format">A pointer to a FORMATETC structure that defines the format, medium, and target device to use when passing the data.</param>
    /// <param name="medium">A STGMEDIUM that defines the storage medium containing the data being transferred.</param>
    unsafe int IDataObject.GetDataHere(FORMATETC* format, STGMEDIUM* medium)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Determines whether the data object is capable of rendering the data described in the FORMATETC structure.
    /// </summary>
    /// <param name="format">A pointer to a FORMATETC structure that defines the format, medium, and target device to use for the query.</param>
    /// <returns>HRESULT success code.</returns>
    unsafe int IDataObject.QueryGetData(FORMATETC* format)
    {
        FORMATETC formatCopy = *format; // Cannot use ref or out parameter inside an anonymous method, lambda expression, or query expression
        var formatMatches = _dataObjects.Where(d => d.FORMATETC.cfFormat == formatCopy.cfFormat);
        if (!formatMatches.Any())
            return NativeMethods.DV_E_FORMATETC;

        var tymedMatches = formatMatches.Where(d => 0 != (d.FORMATETC.tymed & formatCopy.tymed));
        if (!tymedMatches.Any())
            return NativeMethods.DV_E_TYMED;

        var aspectMatches = tymedMatches.Where(d => d.FORMATETC.dwAspect == formatCopy.dwAspect);
        if (!aspectMatches.Any())
            return NativeMethods.DV_E_DVASPECT;

        return NativeMethods.S_OK;
    }

    /// <summary>
    /// Transfers data to the object that implements this method.
    /// </summary>
    /// <param name="formatIn">A FORMATETC structure that defines the format used by the data object when interpreting the data contained in the storage medium.</param>
    /// <param name="medium">A STGMEDIUM structure that defines the storage medium in which the data is being passed.</param>
    /// <param name="release">true to specify that the data object called, which implements SetData, owns the storage medium after the call returns.</param>
    unsafe void IDataObject.SetData(FORMATETC* formatIn, STGMEDIUM* medium, bool release)
    {
        var handled = false;
        if (((DVASPECT)formatIn->dwAspect == DVASPECT.DVASPECT_CONTENT) &&
            ((TYMED)formatIn->tymed == TYMED.TYMED_HGLOBAL) &&
            (medium->tymed == (TYMED)formatIn->tymed))
        {
            // Supported format; capture the data
            var ptr = (nint)PInvoke.GlobalLock(medium->u.hGlobal);
            if (ptr != nint.Zero)
            {
                try
                {
                    var length = NativeMethods.GlobalSize(ptr).ToInt32();
                    var data = new byte[length];
                    Marshal.Copy(ptr, data, 0, length);
                    // Store it in our own format
                    SetData(formatIn->cfFormat, data);
                    handled = true;
                }
                finally
                {
                    PInvoke.GlobalUnlock(medium->u.hGlobal);
                }
            }

            // Release memory if we now own it
            if (release)
                PInvoke.GlobalFree(medium->u.hGlobal);
        }

        // Handle synchronous mode
        if (!IsAsynchronous && (PERFORMEDDROPEFFECT == formatIn->cfFormat) && _inOperation)
        {
            // Call the end action and exit the operation
            _endAction?.Invoke(this);

            _inOperation = false;
        }

        // Throw if unhandled
        if (!handled)
            throw new NotImplementedException();
    }

    #endregion

    /// <summary>
    /// Provides data for the specified data format (HGLOBAL).
    /// </summary>
    /// <param name="dataFormat">Data format.</param>
    /// <param name="data">Sequence of data.</param>
    public void SetData(ushort dataFormat, IEnumerable<byte> data)
    {
        _dataObjects.Add(
            new DataObject
            {
                FORMATETC = new FORMATETC
                {
                    cfFormat = dataFormat,
                    ptd = null,
                    dwAspect = (uint)DVASPECT.DVASPECT_CONTENT,
                    lindex = -1,
                    tymed = (uint)TYMED.TYMED_HGLOBAL
                },
                GetData = () =>
                {
                    var dataArray = data.ToArray();
                    var ptr = Marshal.AllocHGlobal(dataArray.Length);
                    Marshal.Copy(dataArray, 0, ptr, dataArray.Length);
                    return (ptr, (HRESULT)NativeMethods.S_OK);
                },
            });
    }

    /// <summary>
    /// Provides data for the specified data format and index (ISTREAM).
    /// </summary>
    /// <param name="dataFormat">Data format.</param>
    /// <param name="index">Index of data.</param>
    /// <param name="streamData">Action generating the data.</param>
    /// <remarks>
    /// Uses Stream instead of IEnumerable(T) because Stream is more likely
    /// to be natural for the expected scenarios.
    /// </remarks>
    public void SetData(ushort dataFormat, int index, FileDescriptor fileDescriptor, Action<FileDescriptor, Stream> streamData)
    {
        _dataObjects.Add(
            new DataObject
            {
                FORMATETC = new FORMATETC
                {
                    cfFormat = dataFormat,
                    ptd = null,
                    dwAspect = (uint)DVASPECT.DVASPECT_CONTENT,
                    lindex = index,
                    tymed = (uint)TYMED.TYMED_ISTREAM
                },
                GetData = () =>
                {
                    // Create IStream for data
                    var ptr = IntPtr.Zero;
                    var iStream = NativeMethods.CreateStreamOnHGlobal(IntPtr.Zero, true);
                    if (streamData != null)
                    {
                        // Wrap in a .NET-friendly Stream and call provided code to fill it
                        using var stream = new IStreamWrapper(iStream);
                        streamData(fileDescriptor, stream);
                    }
                    // Return an IntPtr for the IStream
                    ptr = _wrappers.GetOrCreateComInterfaceForObject(iStream, CreateComInterfaceFlags.None);
                    return (ptr, NativeMethods.S_OK);
                },
            });
    }

    /// <summary>
    /// Provides data for the specified data format (FILEGROUPDESCRIPTOR/FILEDESCRIPTOR)
    /// </summary>
    /// <param name="fileDescriptors">Collection of virtual files.</param>
    public void SetData(IEnumerable<FileDescriptor> fileDescriptors)
    {
        // Prepare buffer
        var bytes = new List<byte>();
        // Add FILEGROUPDESCRIPTOR header
        bytes.AddRange(StructureBytes(new NativeMethods.FILEGROUPDESCRIPTOR { cItems = (uint)(fileDescriptors.Count()) }));
        // Add n FILEDESCRIPTORs
        foreach (var fileDescriptor in fileDescriptors)
        {
            // Set required fields
            var FILEDESCRIPTOR = new NativeMethods.FILEDESCRIPTOR
            {
                cFileName = fileDescriptor.Name,
            };

            if (fileDescriptor.IsDirectory)
            {
                FILEDESCRIPTOR.dwFlags |= NativeMethods.FD_ATTRIBUTES;
                FILEDESCRIPTOR.dwFileAttributes = NativeMethods.FILE_ATTRIBUTE_DIRECTORY;
            }

            // Set optional timestamp
            if (fileDescriptor.ChangeTimeUtc.HasValue)
            {
                FILEDESCRIPTOR.dwFlags |= NativeMethods.FD_CREATETIME | NativeMethods.FD_WRITESTIME;
                var changeTime = fileDescriptor.ChangeTimeUtc.Value.ToLocalTime().ToFileTime();
                var changeTimeFileTime = new FILETIME
                {
                    dwLowDateTime = (uint)(changeTime & 0xffffffff),
                    dwHighDateTime = (uint)(changeTime >> 32),
                };
                FILEDESCRIPTOR.ftLastWriteTime = changeTimeFileTime;
                FILEDESCRIPTOR.ftCreationTime = changeTimeFileTime;
            }
            // Set optional length
            if (fileDescriptor.Length.HasValue)
            {
                FILEDESCRIPTOR.dwFlags |= NativeMethods.FD_FILESIZE;
                FILEDESCRIPTOR.nFileSizeLow = (uint)(fileDescriptor.Length & 0xffffffff);
                FILEDESCRIPTOR.nFileSizeHigh = (uint)(fileDescriptor.Length >> 32);
            }
            // Add structure to buffer
            bytes.AddRange(StructureBytes(FILEDESCRIPTOR));
        }

        // Set CFSTR_FILEDESCRIPTORW
        SetData(FILEDESCRIPTORW, bytes);
        // Set n CFSTR_FILECONTENTS

        var index = 0;
        foreach (var fileDescriptor in fileDescriptors)
        {
            if (!fileDescriptor.IsDirectory)
                SetData(FILECONTENTS, index, fileDescriptor, fileDescriptor.StreamContents);
            index++;
        }
    }

    /// <summary>
    /// Gets or sets the CFSTR_PASTESUCCEEDED value for the object.
    /// </summary>
    public DragDropEffects? PasteSucceeded
    {
        get { return GetDropEffect(PASTESUCCEEDED); }
        set { SetData(PASTESUCCEEDED, BitConverter.GetBytes((uint)value)); }
    }

    /// <summary>
    /// Gets or sets the CFSTR_PERFORMEDDROPEFFECT value for the object.
    /// </summary>
    public DragDropEffects? PerformedDropEffect
    {
        get { return GetDropEffect(PERFORMEDDROPEFFECT); }
        set { SetData(PERFORMEDDROPEFFECT, BitConverter.GetBytes((uint)value)); }
    }

    /// <summary>
    /// Gets or sets the CFSTR_PREFERREDDROPEFFECT value for the object.
    /// </summary>
    public DragDropEffects? PreferredDropEffect
    {
        get { return GetDropEffect(PREFERREDDROPEFFECT); }
        set { SetData(PREFERREDDROPEFFECT, BitConverter.GetBytes((uint)value)); }
    }

    /// <summary>
    /// Gets the DragDropEffects value (if any) previously set on the object.
    /// </summary>
    /// <param name="format">Clipboard format.</param>
    /// <returns>DragDropEffects value or null.</returns>
    private DragDropEffects? GetDropEffect(ushort format)
    {
        // Get the most recent setting
        var dataObject = _dataObjects.LastOrDefault(d =>
                (format == d.FORMATETC.cfFormat) &&
                (DVASPECT.DVASPECT_CONTENT == (DVASPECT)d.FORMATETC.dwAspect) &&
                (TYMED.TYMED_HGLOBAL == (TYMED)d.FORMATETC.tymed));

        if (dataObject != null)
        {
            // Read the value and return it
            var result = dataObject.GetData();
            if (result.Item2.Succeeded)
            {
                var ptr = NativeMethods.GlobalLock(result.Item1);
                if (ptr != nint.Zero)
                {
                    try
                    {
                        var length = NativeMethods.GlobalSize(ptr).ToInt32();
                        if (4 == length)
                        {
                            var data = new byte[length];
                            Marshal.Copy(ptr, data, 0, length);
                            return (DragDropEffects)(BitConverter.ToUInt32(data, 0));
                        }
                    }
                    finally
                    {
                        NativeMethods.GlobalUnlock(result.Item1);
                    }
                }
            }
        }
        return null;
    }

    #region IAsyncOperation Members
    // Explicit interface implementation hides the technical details from users of VirtualFileDataObject.

    /// <summary>
    /// Called by a drop source to specify whether the data object supports asynchronous data extraction.
    /// </summary>
    /// <param name="fDoOpAsync">A Boolean value that is set to VARIANT_TRUE to indicate that an asynchronous operation is supported, or VARIANT_FALSE otherwise.</param>
    void IAsyncOperation.SetAsyncMode(int fDoOpAsync)
    {
        IsAsynchronous = !(NativeMethods.VARIANT_FALSE == fDoOpAsync);
    }

    /// <summary>
    /// Called by a drop target to determine whether the data object supports asynchronous data extraction.
    /// </summary>
    /// <param name="pfIsOpAsync">A Boolean value that is set to VARIANT_TRUE to indicate that an asynchronous operation is supported, or VARIANT_FALSE otherwise.</param>
    void IAsyncOperation.GetAsyncMode(out int pfIsOpAsync)
    {
        pfIsOpAsync = IsAsynchronous ? NativeMethods.VARIANT_TRUE : NativeMethods.VARIANT_FALSE;
    }

    /// <summary>
    /// Called by a drop target to indicate that asynchronous data extraction is starting.
    /// </summary>
    /// <param name="pbcReserved">Reserved. Set this value to NULL.</param>
    void IAsyncOperation.StartOperation(nint pbcReserved)
    {
        _inOperation = true;
        _startAction?.Invoke(this);
    }

    /// <summary>
    /// Called by the drop source to determine whether the target is extracting data asynchronously.
    /// </summary>
    /// <param name="pfInAsyncOp">Set to VARIANT_TRUE if data extraction is being handled asynchronously, or VARIANT_FALSE otherwise.</param>
    void IAsyncOperation.InOperation(out int pfInAsyncOp)
    {
        pfInAsyncOp = _inOperation ? NativeMethods.VARIANT_TRUE : NativeMethods.VARIANT_FALSE;
    }

    /// <summary>
    /// Notifies the data object that that asynchronous data extraction has ended.
    /// </summary>
    /// <param name="hResult">An HRESULT value that indicates the outcome of the data extraction. Set to S_OK if successful, or a COM error code otherwise.</param>
    /// <param name="pbcReserved">Reserved. Set to NULL.</param>
    /// <param name="dwEffects">A DROPEFFECT value that indicates the result of an optimized move. This should be the same value that would be passed to the data object as a CFSTR_PERFORMEDDROPEFFECT format with a normal data extraction operation.</param>
    void IAsyncOperation.EndOperation(int hResult, nint pbcReserved, uint dwEffects)
    {
        _endAction?.Invoke(this);
        _inOperation = false;
    }

    #endregion

    /// <summary>
    /// Returns the in-memory representation of an interop structure.
    /// </summary>
    /// <param name="source">Structure to return.</param>
    /// <returns>In-memory representation of structure.</returns>
    private static byte[] StructureBytes<T>(T source)
    {
        // Set up for call to StructureToPtr
        var size = Marshal.SizeOf(source);
        var ptr = Marshal.AllocHGlobal(size);
        var bytes = new byte[size];
        try
        {
            Marshal.StructureToPtr(source, ptr, false);
            // Copy marshalled bytes to buffer
            Marshal.Copy(ptr, bytes, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return bytes;
    }

    /// <summary>
    /// Class representing a virtual file for use by drag/drop or the clipboard.
    /// </summary>
    public class FileDescriptor
    {
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the (optional) length of the file.
        /// </summary>
        public long? Length { get; set; }

        /// <summary>
        /// Gets or sets the (optional) change time of the file.
        /// </summary>
        public DateTime? ChangeTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets an Action that returns the contents of the file.
        /// </summary>
        public Action<FileDescriptor, Stream> StreamContents { get; set; }

        public object UserData { get; set; }

        /// <summary>
        /// Whether this is a directory.
        /// </summary>
        public bool IsDirectory { get; set; }

        public bool TryGetUserData<T>(out T value)
        {
            if (UserData is T t)
            {
                value = t;
                return true;
            }

            value = default;
            return false;
        }
    }

    /// <summary>
    /// Class representing the result of a SetData call.
    /// </summary>
    private class DataObject
    {
        /// <summary>
        /// FORMATETC structure for the data.
        /// </summary>
        public FORMATETC FORMATETC { get; set; }

        /// <summary>
        /// Func returning the data as an IntPtr and an HRESULT success code.
        /// </summary>
        public Func<(nint, HRESULT)> GetData { get; set; }
    }

    /// <summary>
    /// Simple class that exposes a write-only IStream as a Stream.
    /// </summary>
    private class IStreamWrapper : Stream
    {
        /// <summary>
        /// IStream instance being wrapped.
        /// </summary>
        private readonly IStream _iStream;

        /// <summary>
        /// Initializes a new instance of the IStreamWrapper class.
        /// </summary>
        /// <param name="iStream">IStream instance to wrap.</param>
        public IStreamWrapper(IStream iStream)
        {
            _iStream = iStream;
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _iStream.Write(buffer.AsSpan(offset, count), out _);
        }
    }

    /// <summary>
    /// Initiates a drag-and-drop operation.
    /// </summary>
    /// <param name="dragSource">A reference to the dependency object that is the source of the data being dragged.</param>
    /// <param name="dataObject">A data object that contains the data being dragged.</param>
    /// <param name="allowedEffects">One of the DragDropEffects values that specifies permitted effects of the drag-and-drop operation.</param>
    /// <returns>One of the DragDropEffects values that specifies the final effect that was performed during the drag-and-drop operation.</returns>
    /// <remarks>
    /// Call this method instead of System.Windows.DragDrop.DoDragDrop because this method handles IDataObject better.
    /// </remarks>
    public static unsafe DragDropEffects DoDragDrop(VirtualFileDataObject dataObject, DragDropEffects allowedEffects)
    {
        try
        {
            var dropSource = new DropSource();
            nint dataObjectPtr = _wrappers.GetOrCreateComInterfaceForObject(dataObject, CreateComInterfaceFlags.None);
            nint dropSourcePtr = _wrappers.GetOrCreateComInterfaceForObject(dropSource, CreateComInterfaceFlags.None);

            PInvoke.DoDragDrop((Windows.Win32.System.Com.IDataObject*)dataObjectPtr, (IDropSource*)dropSourcePtr, (DROPEFFECT)allowedEffects, out DROPEFFECT finalEffect);
            return (DragDropEffects)finalEffect;
        }
        finally
        {
            if ((dataObject is VirtualFileDataObject virtualFileDataObject) && !virtualFileDataObject.IsAsynchronous && virtualFileDataObject._inOperation)
            {
                // Call the end action and exit the operation
                virtualFileDataObject._endAction?.Invoke(virtualFileDataObject);
                virtualFileDataObject._inOperation = false;
            }
        }
        
    }
}

/// <summary>
/// Provides access to Win32-level constants, structures, and functions.
/// </summary>
public static partial class NativeMethods
{
    public const int DRAGDROP_S_DROP = 0x00040100;
    public const int DRAGDROP_S_CANCEL = 0x00040101;
    public const int DRAGDROP_S_USEDEFAULTCURSORS = 0x00040102;
    public static readonly HRESULT DV_E_DVASPECT = (HRESULT)(-2147221397);
    public static readonly HRESULT DV_E_FORMATETC = (HRESULT)(-2147221404);
    public static readonly HRESULT DV_E_TYMED = (HRESULT)(-2147221399);
    public static readonly HRESULT E_FAIL = (HRESULT)(-2147467259);
    public const uint FD_CREATETIME = 0x00000008;
    public const uint FD_WRITESTIME = 0x00000020;
    public const uint FD_FILESIZE = 0x00000040;
    public static readonly HRESULT OLE_E_ADVISENOTSUPPORTED = (HRESULT)(-2147221501);
    public static readonly HRESULT S_OK = (HRESULT)(0);
    public static readonly HRESULT S_FALSE = (HRESULT)(1);
    public const int VARIANT_FALSE = 0;
    public const int VARIANT_TRUE = -1;

    public const string CFSTR_FILECONTENTS = "FileContents";
    public const string CFSTR_FILEDESCRIPTORW = "FileGroupDescriptorW";
    public const string CFSTR_PASTESUCCEEDED = "Paste Succeeded";
    public const string CFSTR_PERFORMEDDROPEFFECT = "Performed DropEffect";
    public const string CFSTR_PREFERREDDROPEFFECT = "Preferred DropEffect";

    public const uint FD_ATTRIBUTES = 0x00000004;
    public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

    [StructLayout(LayoutKind.Sequential)]
    public struct FILEGROUPDESCRIPTOR
    {
        public uint cItems;
        // Followed by 0 or more FILEDESCRIPTORs
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct FILEDESCRIPTOR
    {
        public uint dwFlags;
        public Guid clsid;
        public int sizelcx;
        public int sizelcy;
        public int pointlx;
        public int pointly;
        public uint dwFileAttributes;
        public FILETIME ftCreationTime;
        public FILETIME ftLastAccessTime;
        public FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
    }

    [GeneratedComInterface]
    [Guid("00000121-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface IDropSource
    {
        [PreserveSig]
        int QueryContinueDrag(int fEscapePressed, uint grfKeyState);
        [PreserveSig]
        int GiveFeedback(uint dwEffect);
    }

    [return: MarshalAs(UnmanagedType.Interface)]
    [DllImport("ole32.dll", PreserveSig = false)]
    public static extern IStream CreateStreamOnHGlobal(IntPtr hGlobal, [MarshalAs(UnmanagedType.Bool)] bool fDeleteOnRelease);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GlobalLock(IntPtr hMem);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll")]
    public static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GlobalSize(IntPtr handle);
}

/// <summary>
/// Contains the methods for generating visual feedback to the end user and for canceling or completing the drag-and-drop operation.
/// </summary>
[GeneratedComClass]
public partial class DropSource : NativeMethods.IDropSource
{
    /// <summary>
    /// Determines whether a drag-and-drop operation should continue.
    /// </summary>
    /// <param name="fEscapePressed">Indicates whether the Esc key has been pressed since the previous call to QueryContinueDrag or to DoDragDrop if this is the first call to QueryContinueDrag. A TRUE value indicates the end user has pressed the escape key; a FALSE value indicates it has not been pressed.</param>
    /// <param name="grfKeyState">The current state of the keyboard modifier keys on the keyboard. Possible values can be a combination of any of the flags MK_CONTROL, MK_SHIFT, MK_ALT, MK_BUTTON, MK_LBUTTON, MK_MBUTTON, and MK_RBUTTON.</param>
    /// <returns>This method returns S_OK/DRAGDROP_S_DROP/DRAGDROP_S_CANCEL on success.</returns>
    public int QueryContinueDrag(int fEscapePressed, uint grfKeyState)
    {
        var escapePressed = (0 != fEscapePressed);

        var keyStates = (DragDropKeyStates)grfKeyState;
        if (escapePressed)
            return NativeMethods.DRAGDROP_S_CANCEL;
        else if (DragDropKeyStates.None == (keyStates & DragDropKeyStates.LeftMouseButton))
            return NativeMethods.DRAGDROP_S_DROP;

        return NativeMethods.S_OK;
    }

    /// <summary>
    /// Gives visual feedback to an end user during a drag-and-drop operation.
    /// </summary>
    /// <param name="dwEffect">The DROPEFFECT value returned by the most recent call to IDropTarget::DragEnter, IDropTarget::DragOver, or IDropTarget::DragLeave. </param>
    /// <returns>This method returns S_OK on success.</returns>
    public int GiveFeedback(uint dwEffect)
    {
        return NativeMethods.DRAGDROP_S_USEDEFAULTCURSORS;
    }
}

/// <summary>
/// Definition of the IAsyncOperation COM interface.
/// </summary>
/// <remarks>
/// Pseudo-public because VirtualFileDataObject implements it.
/// </remarks>
[GeneratedComInterface]
[Guid("3D8B0590-F691-11d2-8EA9-006097DF5BD4")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal partial interface IAsyncOperation
{
    void SetAsyncMode(int fDoOpAsync);
    void GetAsyncMode(out int pfIsOpAsync);
    void StartOperation(nint pbcReserved);
    void InOperation(out int pfInAsyncOp);
    void EndOperation(int hResult, nint pbcReserved, uint dwEffects);
}

[Flags]
public enum DragDropEffects
{
    None = 0,
    Copy = 1,
    Move = 2,
    Link = 4,
    Scroll = unchecked((int)0x80000000),
    All = unchecked((int)0x80000003)
}

[Flags]
public enum DragDropKeyStates
{
    None = 0,
    LeftMouseButton = 0x0001,
    RightMouseButton = 0x0002,
    ShiftKey = 0x0004,
    ControlKey = 0x0008,
    MiddleMouseButton = 0x0010,
    AltKey = 0x0020
}

[GeneratedComInterface]
[Guid("0000010E-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public unsafe partial interface IDataObject
{
    [PreserveSig] int GetData(FORMATETC* format, STGMEDIUM* medium);
    [PreserveSig] int GetDataHere(FORMATETC* format, STGMEDIUM* medium);
    [PreserveSig] int QueryGetData(FORMATETC* format);
    [PreserveSig] int GetCanonicalFormatEtc(FORMATETC* formatIn, FORMATETC* formatOut);
    [PreserveSig] void SetData(FORMATETC* formatIn, STGMEDIUM* medium, [MarshalAs(UnmanagedType.Bool)] bool release);
    [PreserveSig] IEnumFORMATETC* EnumFormatEtc(int direction);
    [PreserveSig] int DAdvise(FORMATETC* pFormatetc, uint advf, IAdviseSink* adviseSink, out int connection);
    [PreserveSig] int DUnadvise(int connection);
    [PreserveSig] int EnumDAdvise(IEnumSTATDATA* enumAdvise);
}
*/