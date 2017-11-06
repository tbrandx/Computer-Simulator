
namespace EPB_IDE.Model
{
    public class CompilerFactory : ICompilerFactory
    {
        public ICompiler Make()
        {
            return (ICompiler)(new Compiler());
        }
    }
}
