namespace RefLocalsAndRefReturns
{
    public class MyList<T>
    {
        private readonly T[] _data = new T[0];
        public ref T this[int idx]
        {
            get { return ref _data[idx]; }
        }
    }
}