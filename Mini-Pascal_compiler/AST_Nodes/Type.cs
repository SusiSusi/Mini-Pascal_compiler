namespace MiniPascalCompiler
{
  class Type : AST
  {
    public Token token { get; set; }
    public string value { get; set; }

    public Type(Token token) 
    {
      this.token = token;
      this.value = (string)token.value;
    }
  }
}