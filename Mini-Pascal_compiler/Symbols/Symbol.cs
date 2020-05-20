namespace MiniPascalCompiler
{
  enum SymbolCategory
  {
    PROCEDURE,
    FUNCTION,
    VARIABLE,
    BUILTIN
  }
  abstract class Symbol
  {
    public string name { get; set; }
    public object type { get; set; }
    public SymbolCategory category { get; set; }
    public int scopeLevel { get; set; }

    public Symbol(string name, SymbolCategory category, object type=null)
    {
      this.name = name;
      this.category = category;
      this.type = type;
      this.scopeLevel = 0;
    }
  }
}