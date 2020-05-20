namespace MiniPascalCompiler
{
  class Assert : AST
  {
    public AST expr { get; set; }
    public Assert(AST expr)
    {
      this.expr = expr;
    }
  }
}