using System.Runtime.InteropServices;

namespace DtC.Episode4
{
    internal class Program
    {
        public static void UseNativeMemory()
        {
            int size = 42;

            var memoryWrapper = new UnmanagedMemoryHandle(size);
            UseMemory(memoryWrapper.DangerousGetHandle());
        }

        private static void UseMemory(IntPtr handle)
        {
            // Simulate using the unmanaged memory
            // For example, you could write to it or read from it
            // This is just a placeholder for demonstration purposes
        }

        static void Main(string[] args)
        {
            UseNativeMemory();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Console.WriteLine("Done");
        }
    }
    public class UnmanagedMemoryHandle : SafeHandle
    {
        public UnmanagedMemoryHandle(int size)
            : base(Marshal.AllocHGlobal(size), ownsHandle: true)
        {
            Console.WriteLine($"Allocating {size} bytes");
        }
        protected override bool ReleaseHandle()
        {
            Console.WriteLine("Releasing memory");
            Marshal.FreeHGlobal(this.handle);
            return true;
        }
        public override bool IsInvalid => false;
    }

    public class MemoryWrapper : IDisposable
    {
        private readonly IntPtr _handle;
        private bool _disposed;

        public MemoryWrapper(int size)
        {
            Console.WriteLine($"Allocating {size} bytes.");
            _handle = Marshal.AllocHGlobal(size);
        }

        public IntPtr Handle => _handle;

        ~MemoryWrapper()
        {
            // Intentionally not following the Dispose pattern here.
            Dispose();

        }
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            Console.WriteLine("Releasing memory!");
            Marshal.FreeHGlobal(_handle);
        }
    }
}
