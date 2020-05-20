using System.Collections.Generic;

namespace MiniPascalCompiler
{
  class ProcedureSymbol : Symbol
  {
    public List<VariableSymbol> formalParameters { get; set; }
    public Block blockAst { get; set; }

    public ProcedureSymbol(string name, SymbolCategory category, List<VariableSymbol> formalParameters = null) : base(name, category)
    {
      if (formalParameters == null)
      {
        this.formalParameters = new List<VariableSymbol>();
      }
      else
      {
        this.formalParameters = formalParameters;
      }
      this.blockAst = null;
    }
  }
}