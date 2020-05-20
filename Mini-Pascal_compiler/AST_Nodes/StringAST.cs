namespace MiniPascalCompiler
{
  class StringAST : AST
  {
    public Token token { get; set; }
    public string tokenValue { get; set; }

    public StringAST(Token token)
    {
      this.token = token;
      this.tokenValue = (string)token.value;
    }
  }
}