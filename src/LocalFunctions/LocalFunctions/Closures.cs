using System;

namespace LocalFunctions
{
    public class Closures
    {
private Func<int> func;
public void ImplicitCapture(int arg)
{
    var o = new VeryExpensiveObject();
    Func<int> a = () => o.GetHashCode();
    Console.WriteLine(a());

    Func<int> b = () => arg;
    func = b;
}
    }

    public class VeryExpensiveObject
    {
        public int i1;
        public int i2;
    }
}