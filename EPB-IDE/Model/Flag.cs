namespace EPB_IDE.Model
{
    public class Flag
    {
        //------------------------------------------------------------------------------------------------------------
        public static Flag Make(int index, int value)
        {
            Flag flag = new Flag();
            flag.Index = index;
            flag.Value = value;
            return flag;
        }
        public int Index { get; set; }
        public int Value { get; set; }
        private Flag() { }

        //------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return $"Index: {Index}, Value: {Value}";
        }
    }
}
