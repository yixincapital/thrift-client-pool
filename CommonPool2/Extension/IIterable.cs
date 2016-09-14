using System;

namespace CommonPool2.Extension
{
    // // Mimics Java's Iterable<T> interface
    public interface IIterable<T>
    {
        IIterator<T> Iterator();
    }

    // Mimics Java's Iterator interface - but
    // implements IDisposable for the sake of
    // parity with IEnumerator.
    public interface IIterator<T> : IDisposable
    {
        bool HasNext { get; }
        T Next();
        void Remove();
    }
}