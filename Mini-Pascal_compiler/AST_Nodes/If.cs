namespace MiniPascalCompiler
{
  class If : AST
  {
    public AST expr { get; set; }
    public AST statement { get; set; }
    public AST statementElse { get; set; }

    public If(AST expr, AST statement, AST statementElse=null)
    {
      this.expr = expr;
      this.statement = statement;
      this.statementElse = statementElse;
    }
  }
}