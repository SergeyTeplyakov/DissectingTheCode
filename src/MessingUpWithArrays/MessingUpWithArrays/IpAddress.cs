using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MessingUpWithArrays
{
[StructLayout(LayoutKind.Explicit)]
public struct IpAddress
{
    [FieldOffset(0)]
    public readonly int Value;

    [FieldOffset(3)]
    public readonly byte Byte0;

    [FieldOffset(2)]
    public readonly byte Byte1;

    [FieldOffset(1)]
    public readonly byte Byte2;

    [FieldOffset(0)]
    public readonly byte Byte3;

    public IpAddress(byte byte0, byte byte1, byte byte2, byte byte3)
        : this()
    {
        Byte0 = byte0;
        Byte1 = byte1;
        Byte2 = byte2;
        Byte3 = byte3;
    }

    public IpAddress(int value)
        : this()
    {
        Value = value;
    }

    public override string ToString()
    {
        return $"{Byte0}.{Byte1}.{Byte2}.{Byte3}";
    }
}
}
