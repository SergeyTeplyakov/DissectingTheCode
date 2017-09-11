using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MessingUpWithArrays
{
    [StructLayout(LayoutKind.Explicit)]
    public class ArrayLayout
    {
        [FieldOffset(0)]
        public int Length;
        [FieldOffset(4)]
        public RuntimeTypeHandle Type;
        [FieldOffset(8)]
        public object Eement0;
        [FieldOffset(16)]
        public object Element1;
    }

    [StructLayout(LayoutKind.Explicit)]
    class RefArrayLayout
    {
        [FieldOffset(0)]
        public int Length;

        // TypeHandle stored here
        [FieldOffset(4)]
        public IntPtr Type;

        [FieldOffset(8)]
        public object Element0;

        [FieldOffset(12)]
        public object Element1;

        public void ChangeType(Type type)
        {
            Type = type.TypeHandle.Value;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    class ArrayExplorer
    {
        [FieldOffset(0)]
        public object[] Array;

        [FieldOffset(0)]
        public RefArrayLayout Layout;
    }

    [StructLayout(LayoutKind.Explicit)]
    public class ArrayHacker
    {
        [FieldOffset(0)]
        public readonly ArrayLayout ArrayInternals;
        [FieldOffset(0)]
        public readonly object[] Array;

        public ArrayHacker(object[] array)
            //: this()
        {
            Array = array;
        }
    }
}
