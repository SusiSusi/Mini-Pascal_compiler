namespace MiniPascalCompiler
{
  class VariableSymbol : Symbol
  {
    public int scopelevel { get; set; }
    
    public VariableSymbol(string name, SymbolCategory category, Symbol type, int scopelevel) : base(name, category, type) 
    { 
      this.scopelevel = scopelevel;
    }

    public override string ToString()
    {
      return "<" + name + ":" + type + ">";
    }
  }
}