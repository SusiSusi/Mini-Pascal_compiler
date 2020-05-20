namespace MiniPascalCompiler
{
  class Return : AST
  {
    public AST expr { get; set; }

    public Return(AST expr=null)
    {
      this.expr = expr;
    }
  }
}