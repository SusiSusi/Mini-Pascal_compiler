namespace MiniPascalCompiler
{
  class BooleanAST : AST
  {
    public Token token { get; set; }
    public bool tokenValue { get; set; }

    public BooleanAST(Token token)
    {
      this.token = token;
      this.tokenValue = (bool)token.value;
    }
  }
}