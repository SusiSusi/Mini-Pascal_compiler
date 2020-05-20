namespace MiniPascalCompiler
{
  class Variable : AST
  {
    public Token token { get; set; }
    public string value { get; set; } // variable's name
    public AST expr { get; set; } // array expression
    public bool size { get; set; } // array size flag

    public Variable(Token token, AST expr=null, bool size=false)
    {
      this.token = token;
      this.value = (string)token.value;
      this.expr = expr;
      this.size = size;
    }
  }
}