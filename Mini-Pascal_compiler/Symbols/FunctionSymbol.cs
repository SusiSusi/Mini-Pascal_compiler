using System.Collections.Generic;

namespace MiniPascalCompiler
{
  class FunctionSymbol : Symbol
  {
    public List<VariableSymbol> formalParameters { get; set; }
    public Block blockAst { get; set; }

    public FunctionSymbol(string name, SymbolCategory category, object type, List<VariableSymbol> formalParameters = null) : base(name, category, type)
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