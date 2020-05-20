namespace MiniPascalCompiler
{
  class True : AST
  {
    public Token token { get; set; }
    public bool tokenValue { get; set; }

    public True(Token token)
    {
      this.token = token;
      this.tokenValue = true;
    }
  }
}