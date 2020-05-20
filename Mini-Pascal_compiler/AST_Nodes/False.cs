namespace MiniPascalCompiler
{
  class False : AST
  {
    public Token token { get; set; }
    public bool tokenValue { get; set; }

    public False(Token token)
    {
      this.token = token;
      this.tokenValue = false;
    }
  }
}