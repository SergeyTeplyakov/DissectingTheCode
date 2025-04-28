using Xunit;

namespace DtC.Episode0.Tests
{
    public struct Blittable(long L, int N, short S1, short S2)
    {
        public long L = L;
        public int N = N;
        public short S1 = S1;
        public short S2 = S2;
    }

    public struct NonBlittable(long L, int N, short S1)
    {
        public long L = L;
        public int N = N;
        public long S1 = S1;
    }

    public class DefaultHashCodeTests
    {
        [Fact]
        public void Blittable_HashCode_Are_The_Same()
        {
            var b1 = new Blittable(1, 2, 3, 4);
            var b2 = new Blittable(1, 2, 3, 5);

            Assert.NotEqual(b1.GetHashCode(), b2.GetHashCode());
            Assert.NotEqual(b1, b2);
        }

        [Fact]
        public void NonBlittable_HashCode_Are_Different()
        {
            var nb1 = new NonBlittable(1, 2, 3);
            var nb2 = new NonBlittable(1, 2, 4);

            Assert.Equal(nb1.GetHashCode(), nb2.GetHashCode());
            Assert.NotEqual(nb1, nb2);
        }
    }
}
