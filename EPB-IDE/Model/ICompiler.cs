using Computer_Simulator;

namespace EPB_IDE.Model
{
    public interface ICompiler
    {
        Flags Flags { get; }
        SymbolTable Symbols { get; }

        Compiler Compile();
        EPML CompiledCode();
        Compiler LoadProgram(string[] lines);
        Compiler Optimize();
    }
}