using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace InterpolatedStrings.Examples
{
    public class BasicExamples
    {
        [Fact]
        public void SimpleExpression()
        {
            int n = 0;

            // s is System.String
            var s = $"n == {n}";

            // s2 is of type 'DefaultInterploatedStringHandler'
            DefaultInterpolatedStringHandler s2 = $"n == {n}";
        }

        [Fact]
        public void ContractCall()
        {
            int n = 0;

            // Contract is not violated! No messages will be constructed!
            Contract.Requires(true, $"No side effects! n == {++n}");

            Console.WriteLine($"n == {n}"); // n == 0

            Assert.Equal(0, n);

            try { Contract.Requires(false, $"Side effect! n == {++n}"); } catch { }

            Console.WriteLine($"n == {n}"); // n == 1
            Assert.Equal(1, n);
        }

        [Fact]
        public void SpecialCasingACustomStruct()
        {
            var customStruct = new MyStruct();
            // This will call an overload for 'AppendFormatted' with a custom struct.
            try { Contract.Requires(false, $"Side effect! n == {customStruct}"); } catch { }
        }

        [Fact]
        public async Task AsyncCase()
        {
            // This won't compile because you can't use 'await' with an interpolated string handler that is a ref struct!

            string s = $"n == {await GetValueAsync()}";
            //// This will call an overload for 'AppendFormatted' with a custom struct.
            //string message = null;
            //try { Contract.Requires(false, $"n == {await GetValueAsync()}"); } catch (Exception e) { message = e.Message; }

            //Assert.Equal("n == 42", message);

            Task<int> GetValueAsync() => Task.FromResult(42);
        }

        [Fact]
        public void StringInterpolationWithSpans()
        {
            string str = "foo bar";
            var final = $"Foo: {str.AsSpan().Trim()}";
        }

        public async Task FooAsync()
        {
            string s = $"x = {await Task.Run(() => 42)}";
        }
    }
}
