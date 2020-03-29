namespace RefLocals
{
    internal class WithLargeStruct
    {
        private LargeStruct_48 ls48 = new LargeStruct_48(16);
        private LargeStruct_48_NotReadonly _ls48NonReadOnly = new LargeStruct_48_NotReadonly(16);
        private LargeStruct_40 ls40 = new LargeStruct_40(16);
        private LargeStruct_32 ls32 = new LargeStruct_32(16);
        private LargeStruct_16 ls16 = new LargeStruct_16(16);
        private Struct_4 s4 = new Struct_4(16);

        public ref readonly LargeStruct_48_NotReadonly L48_NonReadOnly => ref _ls48NonReadOnly;
        public LargeStruct_48_NotReadonly L48NonReadOnnlyVal => _ls48NonReadOnly;

        public ref readonly LargeStruct_48 L48 => ref ls48;
        public ref readonly LargeStruct_40 L40 => ref ls40;
        public ref readonly LargeStruct_32 L32 => ref ls32;
        public ref readonly LargeStruct_16 L16 => ref ls16;
        public ref readonly Struct_4 L4 => ref s4;

        public LargeStruct_48 L48V => ls48;
        public LargeStruct_40 L40V => ls40;
        public LargeStruct_32 L32V => ls32;
        public LargeStruct_16 L16V => ls16;
        public Struct_4 L4V => s4;
    }

    internal class WithLargeStruct2
    {
        private LargeStruct_48 ls48 = new LargeStruct_48(48);

        public ref readonly LargeStruct_48 L48 => ref ls48;
    }
}