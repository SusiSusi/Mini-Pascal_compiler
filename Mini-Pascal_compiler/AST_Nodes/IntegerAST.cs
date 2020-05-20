namespace MiniPascalCompiler
{
  class IntegerAST : AST
  {
    public Token token { get; set; }
    public int tokenValue { get; set; }
    public IntegerAST(Token token)
    {
      this.token = token;
      this.tokenValue = (int)this.token.value;
    }
  }
}