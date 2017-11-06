using System.Collections.Generic;

namespace EPB_IDE.Model
{
    public class Flags
    {
        //------------------------------------------------------------------------------------------------------------
        public static Flags Make()
        {
            return new Flags();
        }
        private List<Flag> _flags = new List<Flag>();
        public List<Flag> All { get { return _flags; } }

        //------------------------------------------------------------------------------------------------------------
        public Flags Add(int index, int value)
        {
            _flags.Add(Flag.Make(index, value));
            return this;
        }
        //------------------------------------------------------------------------------------------------------------
        private Flags() { }

        //------------------------------------------------------------------------------------------------------------
        public Flag Find(int index)
        {
            foreach (var flag in _flags)
            {
                if (flag.Index == index) { return flag; }
            }
            return null;
        }

        //------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            string response = "Flags:\n";
            foreach (var flag in _flags)
            {
                response += flag.ToString() + '\n';
            }
            return response;

        }
    }
}
