namespace MiniPascalCompiler
{
  class While : AST
  {
    public AST expr { get; set; }
    public AST statement { get; set; }

    public While(AST expr, AST statement)
    {
      this.expr = expr;
      this.statement = statement;
    }
  }
}