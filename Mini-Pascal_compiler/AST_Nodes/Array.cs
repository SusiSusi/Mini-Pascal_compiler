namespace MiniPascalCompiler
{
  class Array : AST
  {
    public Type type { get; set; }
    public string name { get; set; }
    public AST expr { get; set; }


    public Array(Type type, AST expr=null)
    {
      this.type = type;
      this.name = "array";
      this.expr = expr;
    }
  }
}