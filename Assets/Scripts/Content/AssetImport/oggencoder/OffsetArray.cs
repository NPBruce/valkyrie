using System;
using System.Collections;
using System.Collections.Generic;

namespace OggVorbisEncoder
{
    public class OffsetArray<T> : IList<T>
    {
        private readonly IList<T> _array;

        public OffsetArray(IList<T> array, int offset)
        {
            _array = array;
            Offset = offset;
        }

        public int Offset { get; }

        public T this[int index]
        {
            get { return _array[Offset + index]; }
            set { _array[Offset + index] = value; }
        }

        public int Count => _array.Count - Offset;

        public bool IsReadOnly { get; } = false;

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }
    }
}