using System;
//// You can't do this: compilation error
//using Point = (int x, int y);

//// But you *can* do this
//using SetOfPoints = System.Collections.Generic.HashSet<(int x, int y)>;
namespace Tuples
{

public static class VersionDeconstrucion
{
    public static void Deconstruct(this Version v, out int major, out int minor, out int build, out int revision)
    {
        major = v.Major;
        minor = v.Minor;
        build = v.Build;
        revision = v.Revision;
    }
}

    public class Deconstruction
    {
        public void VersionDeconstruction()
        {
var version = Version.Parse("1.2.3.4");
var (major, minor, build, _) = version;

// Prints: 1.2.3
Console.WriteLine($"{major}.{minor}.{build}");
        }
        public void Sample()
        {
var (a, b) = Foo();
            var ff = Foo();

            //switch (ff)
            //{
            //    case var (x, y): break;

            //}
(int x, int y) Foo() => (1, 2);
        }
    }
}