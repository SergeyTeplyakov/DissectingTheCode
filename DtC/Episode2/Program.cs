using System.Runtime.InteropServices;

namespace Episode2
{
    internal class Program
    {
        static void ShowMemoryLeak()
        {
            int bytesToAllocate = 1024;
            IntPtr ptr = Marshal.AllocHGlobal(cb: 1024); // Allocating 1Kb

            if (ptr != IntPtr.Zero)
            {
                Console.WriteLine($"Successfully allocated {bytesToAllocate} bytes");

                Marshal.FreeHGlobal(ptr);
            }

        }

        static void PrintContent(string path)
        {
            var file1 = File.Open(path, FileMode.Open);
            var reader = new StreamReader(file1);
            var content = reader.ReadToEnd();
            Console.WriteLine($"Content: {content}");
        }

        static void Main(string[] args)
        {
            ShowMemoryLeak();

            // Writing to the output directory
            string path = "foobar.txt";
            File.WriteAllText(path, "The Content");

            PrintContent(path);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            PrintContent(path);
        }
    }
}
