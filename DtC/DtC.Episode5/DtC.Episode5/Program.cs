using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DtC.Episode5
{
#nullable enable
    public sealed class FileCopier : IDisposable
    {
        private readonly Stream _source;
        private readonly Stream _destination;

        public FileCopier(string sourcePath, string destinationPath)
        {
            _source = new FileStream(sourcePath, FileMode.Open);
            _destination = new FileStream(destinationPath, FileMode.Create);
        }

        public async Task CopyAsync()
            => await _source.CopyToAsync(_destination);

        public void Dispose() => Dispose(disposing: true);

        ~FileCopier()
        {
            Console.WriteLine("Running ~FileCopier");
            Dispose(disposing: false);
        }

        private void Dispose(bool disposing)
        {
            _source?.Dispose();
            _destination?.Dispose();
        }
    }

    internal class Program
    {


        static void Allocate()
        {
            Inner();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Console.WriteLine();

            void Inner()
            {
                new B();

            }
        }
        private static async Task Copy(string source, string destination)
        {
            using (var copier = new FileCopier(source, destination))
            {
                await copier.CopyAsync();
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        static void Main(string[] args)
        {
            new RocksDbWrapper().UseRocksDb();
            Console.WriteLine("Done using RocksDb!");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public class CrazyConstructor
        {
            private readonly int _field;

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            public CrazyConstructor()
            {
                Console.WriteLine(".ctor start");
                _field = 42;
                Console.WriteLine(_field);
                // We’re not touching ‘this’ pointer after this point.
                #region 

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                #endregion
                Console.WriteLine(".ctor end");
            }
            ~CrazyConstructor()
            {
                Console.WriteLine(".dtor end");
            }
        }
    }

    public static class RocksDbNative
    {
        private static readonly HashSet<IntPtr> ValidHandles = new();
        public static IntPtr CreateDb()
        {
            // Allocating native resource used by the DB.
            IntPtr handle = 42;
            Trace("Creating Db", handle);
            ValidHandles.Add(handle);
            return handle;
        }

        public static void DestroyDb(RocksDbSafeHandle handle)
        {
            Trace("Destroying Db", handle.Handle);
            ValidHandles.Remove(handle.Handle);
            // Cleaning up the resources associated with the handle.
        }

        public static void UseDb(RocksDbSafeHandle handle)
        {
            Trace("Starting using Db", handle.Handle);
            // Just mimic some extra work a method might do.
            PerformLongRunningPrerequisite();

            // Using the handle
            Trace("Using Db", handle.Handle);
            PInvokeIntoDb(handle);
        }

        private static void PInvokeIntoDb(RocksDbSafeHandle handle) { }

        private static void Trace(string message, IntPtr handle)
        {
            Console.WriteLine(
                $"{message}. Id: {handle}, IsValid: {IsValid(handle)}.");
        }

        public static bool IsValid(IntPtr handle) => ValidHandles.Contains(handle);

        private static void PerformLongRunningPrerequisite()
        {
            Thread.Sleep(100);
            // Code runs long enough to cause the GC to run twice.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    public class RocksDbWrapper : IDisposable
    {
        private RocksDbSafeHandle _handle = RocksDbSafeHandle.Create();

        public void Dispose() => _handle.Dispose();

        public void UseRocksDb() => RocksDbNative.UseDb(_handle);
    }

    public class RocksDbSafeHandle : SafeHandle
    {
        private int _released = 0;

        /// <inheritdoc />
        private RocksDbSafeHandle(IntPtr handle) : base(handle, ownsHandle: true) { }

        public static RocksDbSafeHandle Create()
            => new RocksDbSafeHandle(RocksDbNative.CreateDb());

        /// <inheritdoc />
        protected override bool ReleaseHandle()
        {
            if (Interlocked.CompareExchange(ref _released, 1, 0) == 0)
            {
                RocksDbNative.DestroyDb(this);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override bool IsInvalid => _released != 0;

        /// <inheritdoc />
        public override string ToString() => handle.ToString();

        internal IntPtr Handle => this.handle;
    }


    public class A
    {
        public A() { Console.WriteLine("A.ctor"); }

        ~A() { Console.WriteLine("A.dtor"); }
    }

    public class B
    {
        private readonly A _a = new();
        public B() { Console.WriteLine("B.ctor"); }

        ~B() { Console.WriteLine("B.dtor"); }
    }
}
