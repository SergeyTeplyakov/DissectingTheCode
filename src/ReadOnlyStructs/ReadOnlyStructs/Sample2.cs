using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;

namespace ReadOnlyStructs
{
    public struct CustomStruct
    {
        public readonly int N;
    }

    public class WeirdOverload
    {
// Use non-readonly field to avoid redundant defensive copy on each field access
private /*readonly*/FairlyLargeStruct _fairlyLargeStruct;
        private CustomStruct customStruct;
        public ref readonly CustomStruct ByRef() => ref customStruct;

        //public int Foo(string s) => s.Length;
public int Foo(in string s) => s.Length;
public int Foo(string s) => s.Length;

public int ByIn(in string s) => s.Length;

public void Sample2()
{
string s = string.Empty;
ByIn(in s); // Works fine
ByIn(s); // Works fine as well!
// Fail?!?! An expression cannot be used in this context because it may not be passed or returned by reference
//ByIn(in "some string");
ByIn("some string"); // Works fine!
}

        public void Sample1()
        {
string s = string.Empty;
Foo(in s);
// The call is ambiguous between the following methods or properties: 
// 'WeirdOverload.Foo(in string)' and 'WeirdOverload.Foo(string)'
//Foo(s);
        }

//// Async methods cannot have ref or out parameters
//async Task ByInAsync(in string s) => await Task.Yield();

struct Disposable : IDisposable
{
    public void Dispose() { }
}

public void DisposableSample()
{
using (var d = new Disposable())
{
    // Ok
    ByIn(d);
    // Cannot use 'd' as a ref or out value because it is a 'using variable'
    //ByRef(ref d);
}

void ByRef(ref Disposable disposable) { }
void ByIn(in Disposable disposable) { }
}

        //public void Sample2()
        //{
        //    string s = string.Empty;
        //    //Foo(s); // Ok
        //    //Foo(in s); // Ok
        //    //Foo("string literal"); // Ok as well
        //}
    }
}