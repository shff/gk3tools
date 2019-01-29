using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Gk3Main.Sound
{
    class Mp3Stream : System.IO.Stream
    {
        private static bool _initialized;
        private static byte[] _workingBuffer = new byte[4096];
        private IntPtr _mpg123;
        private long _position;
        private System.IO.Stream _source;

        public Mp3Stream(System.IO.Stream source)
        {
            _source = source;
            
            if (_initialized == false)
                init();

            int err;
            _mpg123 = mpg123_new(IntPtr.Zero, out err);
            if (_mpg123 == IntPtr.Zero)
                throw new Exception("Couldn't create MPG123");

            err = mpg123_open_feed(_mpg123);
            if (err != 0)
                throw new Exception("Unable to open feed");

            // try to get the format info
            unsafe
            {
                fixed(byte* wm = _workingBuffer)
                {
                    int ret = 0, amt, total = 0;
                    do
                    {
                        amt = System.Math.Min(_workingBuffer.Length, (int)(_source.Length - _source.Position));
                        amt = _source.Read(_workingBuffer, 0, amt);

                        if (amt == 0) break;
                        total += amt;

                        uint dummy;
                        ret = mpg123_decode(_mpg123, wm, (uint)amt, null, 0, out dummy);
                    }
                    while (ret == MPG123_NEED_MORE && total < 64 * 1024);

                    int sampleRate, channels, encoding;
                    if (ret == MPG123_NEW_FORMAT && mpg123_getformat(_mpg123, out sampleRate, out channels, out encoding) == 0)
                    {
                        mpg123_format_none(_mpg123);
                        mpg123_format(_mpg123, sampleRate, channels, MPG123_ENC_SIGNED_16);
                    }
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            unsafe
            {
                fixed (byte* data = buffer)
                fixed (byte* wb = _workingBuffer)
                {
                    byte* d = &data[offset];

                    uint amt = 0;
                    while (count > 0)
                    {
                        uint got;
                        int ret = mpg123_read(_mpg123, d, (uint)count, out got);

                        count -= (int)got;
                        d += got;
                        amt += got;

                        if (ret == MPG123_NEW_FORMAT)
                        {
                            mpg123_delete(_mpg123);
                            break;
                        }
                        if (ret == MPG123_NEED_MORE)
                        {
                            int insize = System.Math.Min(_workingBuffer.Length, (int)(_source.Length - _source.Position));

                            if (insize > 0)
                            {
                                insize = _source.Read(_workingBuffer, 0, insize);
                            }
                            if (insize > 0 && mpg123_feed(_mpg123, wb, (uint)insize) == 0)
                                continue;
                        }
                        if (got == 0)
                            break;
                    }

                    return (int)amt;
                }
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        private static void init()
        {
            int err = mpg123_init();
            if (err != 0)
                throw new Exception("Couldn't init MPG123");

            _initialized = true;
        }

        const int MPG123_ENC_16 = 0x040;
        const int MPG123_ENC_SIGNED = 0x080;
        const int MPG123_ENC_SIGNED_16 = MPG123_ENC_16 | MPG123_ENC_SIGNED | 0x10;

        const int MPG123_NEED_MORE = -10;
        const int MPG123_NEW_FORMAT = -11;
        const int MPG123_DONE = -12;

        [DllImport("libmpg123-0", CallingConvention = CallingConvention.Cdecl)]
        static extern int mpg123_init();

        [DllImport("libmpg123-0", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr mpg123_new(IntPtr dummy, out int error);

        [DllImport("libmpg123-0", CallingConvention = CallingConvention.Cdecl)]
        static extern void mpg123_delete(IntPtr handle);

        [DllImport("libmpg123-0", CallingConvention = CallingConvention.Cdecl)]
        static extern int mpg123_format_none(IntPtr handle);

        [DllImport("libmpg123-0", CallingConvention = CallingConvention.Cdecl)]
        static extern int mpg123_format(IntPtr handle, int rate, int channels, int encodings);

        [DllImport("libmpg123-0", CallingConvention = CallingConvention.Cdecl)]
        static extern int mpg123_getformat(IntPtr handle, out int rate, out int channels, out int encoding);

        [DllImport("libmpg123-0", CallingConvention = CallingConvention.Cdecl)]
        static extern uint mpg123_outblock(IntPtr handle);

        [DllImport("libmpg123-0", CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe int mpg123_read(IntPtr handle, byte* outmemory, uint outmemsize, out uint done);

        [DllImport("libmpg123-0", CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe int mpg123_decode(IntPtr handle, byte* inmemory, uint inmemsize, byte* outmemory, uint outmemsize, out uint done);

        [DllImport("libmpg123-0", CallingConvention = CallingConvention.Cdecl)]
        static extern int mpg123_open_feed(IntPtr handle);

        [DllImport("libmpg123-0", CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe int mpg123_feed(IntPtr handle, byte* input, uint size);
    }
}
