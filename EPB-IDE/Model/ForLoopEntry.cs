using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPB_IDE.Model
{
    public class ForLoopEntry
    {
        public static ForLoopEntry Make(int step, int lhs, int rhs, int increment)
        {
            ForLoopEntry entry = new ForLoopEntry();
            entry.Step = step;
            entry.LHS = lhs;
            entry.RHS = rhs;
            entry.Increment = increment;
            return entry;
        }
        public int Step { get; set; }
        public int LHS { get; set; }
        public int RHS { get; set; }
        public int Increment { get; set; }
    }
}
