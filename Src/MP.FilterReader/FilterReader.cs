using MP.FilterReader.COMTypeDef;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace MP.FilterReader
{
    /// <summary>
    /// <para>
    /// TextReader implementation based on IFilters. Implemets Read and Peek, other methods are inherited from parent. 
    /// This class depends on IFilters installed on machine.
    /// </para>
    /// </summary>
    public class FilterReader : TextReader
    {
        private IFilter filter;
        private Queue<char> internalBuffer = new Queue<char>();
        private bool hasMoreChunks = true;
        // buffer is quite big in hope to prohibit errors when reading large PDF files
        private uint sizeToRead = 8192;

        /// <summary>
        /// Open new FilterReader from stream. Requires extension.
        /// </summary>
        /// <param name="stream">Stream that contains file contents.</param>
        /// <param name="extension">File extension with leading dot</param>
        public FilterReader(Stream stream, string extension)
        {
            try
            {
                this.filter = FilterLoader.LoadForStream(stream, extension);
            }
            catch
            {
                if (this.filter != null)
                {
                    Marshal.ReleaseComObject(this.filter);
                }
                throw;
            }
        }

        /// <summary>
        /// Open new FilterReader from specific path
        /// </summary>
        /// <param name="path">Path to file on HDD.</param>
        public FilterReader(string path)
        {
            try
            {
                this.filter = FilterLoader.LoadForFile(path);
            }
            catch
            {
                if (this.filter != null)
                {
                    Marshal.ReleaseComObject(this.filter);
                }
                throw;
            }
        }

        public override int Read()
        {
            if (this.internalBuffer.Count == 0)
            {
                if (this.ReadToBuffer() == false)
                {
                    return -1;
                }
            }

            return this.internalBuffer.Dequeue();
        }

        public override int Peek()
        {
            if (this.internalBuffer.Count == 0)
            {
                if (this.ReadToBuffer() == false)
                {
                    return -1;
                }
            }

            return this.internalBuffer.Peek();
        }

        /// <summary>
        /// Read from underlying filter into queue based buffer. Implementations of Read and Peak then check buffer directly.
        /// </summary>
        /// <returns></returns>
        private bool ReadToBuffer()
        {
            STAT_CHUNK statChunk;

            // Outer loop will read chunks from the document at a time.  For those
            // chunks that have text, the contents will be pulled and put into the
            // return buffer.                    
            while (this.hasMoreChunks && this.internalBuffer.Count <= 0)
            {
                var rtn = this.filter.GetChunk(out statChunk);
                if (rtn == IFilterReturnCodes.S_OK)
                {
                    // Ignore all non-text chunks.
                    if (statChunk.flags != CHUNKSTATE.CHUNK_TEXT)
                    {
                        continue;
                    }

                    // Check for white space items and add the appropriate breaks.
                    switch (statChunk.breakType)
                    {
                        case CHUNK_BREAKTYPE.CHUNK_NO_BREAK:
                            break;

                        case CHUNK_BREAKTYPE.CHUNK_EOW:
                            this.internalBuffer.Enqueue(' ');
                            break;

                        case CHUNK_BREAKTYPE.CHUNK_EOC:
                        case CHUNK_BREAKTYPE.CHUNK_EOP:
                        case CHUNK_BREAKTYPE.CHUNK_EOS:
                            var newline = Environment.NewLine.ToCharArray();
                            foreach (var @char in newline)
                            {
                                this.internalBuffer.Enqueue(@char);
                            }
                            break;
                    }

                    // At this point we have a text chunk.  The following code will pull out
                    // all of it and add it to the buffer.
                    bool bMoreText = true;
                    while (bMoreText)
                    {
                        // create temporary buffer
                        var secondaryBuffer = new char[this.sizeToRead];
                        uint readSize = this.sizeToRead;
                        // read piece of text
                        var handle = GCHandle.Alloc(secondaryBuffer, GCHandleType.Pinned);
                        var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(secondaryBuffer, 0);
                        IFilterReturnCodes returnCode;
                        try
                        {
                            returnCode = this.filter.GetText(ref readSize, ptr);
#if DEBUG
                            Trace.WriteLine("TRIED TO READ 1024 GOT" + readSize);
#endif
                        }
                        finally
                        {
                            handle.Free();
                        }

                        if (returnCode == IFilterReturnCodes.S_OK || returnCode == IFilterReturnCodes.FILTER_S_LAST_TEXT)
                        {
                            // now we have text, add it to buffer
                            for (var i = 0; i < readSize; i++)
                            {
                                this.internalBuffer.Enqueue(secondaryBuffer[i]);
                            }

                            // If we got back some text but there is no more, terminate the loop.
                            if (returnCode == IFilterReturnCodes.FILTER_S_LAST_TEXT)
                            {
                                bMoreText = false;
                                break;
                            }
                        }
                        else if (returnCode == IFilterReturnCodes.FILTER_E_NO_MORE_TEXT)
                        {
                            // Once all data is exhausted, we are done so terminate.
                            bMoreText = false;
                            break;
                        }
                        else if (returnCode == IFilterReturnCodes.FILTER_E_NO_TEXT)
                        {
                            // Check for any fatal errors.  It is a bug if you land here.
                            System.Diagnostics.Debug.Assert(false, "Should not get here");
                            throw new InvalidOperationException();
                        }
                    }
                }
                else if (rtn == IFilterReturnCodes.FILTER_E_END_OF_CHUNKS)
                {
                    // Once all chunks have been read, we are done with the file.
                    this.hasMoreChunks = false;
                    break;
                }
                else if (rtn == IFilterReturnCodes.FILTER_E_EMBEDDING_UNAVAILABLE ||
                    rtn == IFilterReturnCodes.FILTER_E_LINK_UNAVAILABLE)
                {
                    continue;
                }
                else
                {
                    throw new COMException("IFilter COM error: " + rtn.ToString());
                }
            }

            return this.internalBuffer.Count > 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (this.filter != null)
            {
                Marshal.ReleaseComObject(this.filter);
            }
        }
    }
}
