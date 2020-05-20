namespace MiniPascalCompiler
{
  class RealAST : AST
  {
    public Token token { get; set; }
    public float tokenValue { get; set; }
    public RealAST(Token token)
    {
      this.token = token;
      this.tokenValue = (float)this.token.value;
    }
  }
}