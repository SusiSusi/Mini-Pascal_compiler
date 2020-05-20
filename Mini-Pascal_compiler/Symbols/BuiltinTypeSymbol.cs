namespace MiniPascalCompiler
{
  class BuiltinTypeSymbol : Symbol
  {
    public BuiltinTypeSymbol(string name, SymbolCategory category) : base(name, category) { }

    public override string ToString()
    {
      return name;
    }
  }
}