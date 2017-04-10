using System;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder
{
    public class EncodeBuffer
    {
        private const int BufferIncrement = 256;

        private static readonly uint[] Mask =
        {
            0x00000000, 0x00000001, 0x00000003, 0x00000007, 0x0000000f,
            0x0000001f, 0x0000003f, 0x0000007f, 0x000000ff, 0x000001ff,
            0x000003ff, 0x000007ff, 0x00000fff, 0x00001fff, 0x00003fff,
            0x00007fff, 0x0000ffff, 0x0001ffff, 0x0003ffff, 0x0007ffff,
            0x000fffff, 0x001fffff, 0x003fffff, 0x007fffff, 0x00ffffff,
            0x01ffffff, 0x03ffffff, 0x07ffffff, 0x0fffffff, 0x1fffffff,
            0x3fffffff, 0x7fffffff, 0xffffffff
        };

        private byte[] _buffer;
        private int _endBit;
        private int _endByte;

        public EncodeBuffer()
            : this(BufferIncrement)
        {
        }

        public EncodeBuffer(int initialBufferSize)
        {
            _buffer = new byte[initialBufferSize];
        }

        private int Bytes { get { return _endByte + (_endBit + 7) / 8; } }

        public void WriteBook(CodeBook book, int a)
        {
            if ((a < 0) || (a >= book.Entries))
                return;

            Write(book.CodeList[a], book.StaticBook.LengthList[a]);
        }

        public void WriteString(string str)
        {
            foreach (var c in str)
                Write(c, 8);
        }

        public void Write(uint value, int bits)
        {
            if ((bits < 0) || (bits > 32))
                throw new ArgumentException("bits must be between 0 and 32");

            if (_endByte >= _buffer.Length - 4)
            {
                if (_buffer.Length > int.MaxValue - BufferIncrement)
                    throw new InvalidOperationException("Maximum buffer size exceeded");

                Array.Resize(ref _buffer, _buffer.Length + BufferIncrement);
            }

            value &= Mask[bits];
            bits += _endBit;

            _buffer[_endByte] = (byte) (_buffer[_endByte] | (value << _endBit));

            if (bits >= 8)
            {
                _buffer[_endByte + 1] = (byte) (value >> (8 - _endBit));
                if (bits >= 16)
                {
                    _buffer[_endByte + 2] = (byte) (value >> (16 - _endBit));
                    if (bits >= 24)
                    {
                        _buffer[_endByte + 3] = (byte) (value >> (24 - _endBit));
                        if (bits >= 32)
                            if (_endBit != 0)
                                _buffer[_endByte + 4] = (byte) (value >> (32 - _endBit));
                            else
                                _buffer[_endByte + 4] = 0;
                    }
                }
            }

            _endByte += bits/8;
            _endBit = bits & 7;
        }

        public byte[] GetBytes()
        {
            Array.Resize(ref _buffer, Bytes);
            return _buffer;
        }
    }
}