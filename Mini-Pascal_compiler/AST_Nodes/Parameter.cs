namespace MiniPascalCompiler
{
  class Parameter : AST
  {
    public Variable variableNode { get; set; } 
    public AST typeNode { get; set; }
    public Token token { get; set; }

    public Parameter(Variable variableNode, AST typeNode, Token token)
    {
      this.variableNode = variableNode;
      this.typeNode = typeNode;
      this.token = token;
    }
  }
}