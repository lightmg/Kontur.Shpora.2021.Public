using System;
using System.Threading;

namespace ReaderWriterLock
{
    public interface IRwLock
    {
        void ReadLocked(Action action);
        void WriteLocked(Action action);
    }

    public class RwLock : IRwLock
    {
        private const long WritersRank = 0x0_0000001_00000000;
        private const long ReadersRank = 0x0_0000000_00000001;
        private const long ReadersMask = 0x0_0000000_FFFFFFFF;

        private readonly ManualResetEventSlim readingLock = new(true);
        private readonly SemaphoreSlim writingLock = new(1, 1);
        private readonly ManualResetEventSlim activeReadersLock = new(true);

        private long state;

        public void ReadLocked(Action action)
        {
            if (HasAnyWriter)
                readingLock.Wait();

            IncrementActiveReadersCount();
            activeReadersLock.Reset();

            action.Invoke();

            DecrementActiveReadersCount();
            if (IsLastActiveReader)
                activeReadersLock.Set();
        }

        public void WriteLocked(Action action)
        {
            IncrementPendingWritersCount();
            readingLock.Reset();

            if (HasActiveReaders)
                activeReadersLock.Wait();

            writingLock.Wait();

            action.Invoke();

            writingLock.Release();

            if (IsLastWriter)
                readingLock.Set();
            DecrementWritersCount();
        }

        private bool HasAnyWriter => Interlocked.Read(ref state) >= WritersRank;
        private bool IsLastWriter => Interlocked.Read(ref state) <= WritersRank;

        private bool HasActiveReaders => (Interlocked.Read(ref state) & ReadersMask) >= ReadersRank;
        private bool IsLastActiveReader => (Interlocked.Read(ref state) & ReadersMask) <= ReadersRank;

        private void IncrementPendingWritersCount() => Interlocked.Add(ref state, WritersRank);

        private void IncrementActiveReadersCount() => Interlocked.Add(ref state, ReadersRank);

        private void DecrementWritersCount() => Interlocked.Add(ref state, -WritersRank);
        private void DecrementActiveReadersCount() => Interlocked.Add(ref state, -ReadersRank);
    }
}