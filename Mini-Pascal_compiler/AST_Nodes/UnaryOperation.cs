namespace MiniPascalCompiler
{
  class UnaryOperation : AST
  {
    public Token operat { get; set; } // unary operator (+, -)
    public AST expr { get; set; }

    public UnaryOperation(Token operat, AST expr)
    {
      this.operat = operat;
      this.expr = expr;
    }
  }
}