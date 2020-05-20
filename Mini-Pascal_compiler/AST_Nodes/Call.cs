using System.Collections.Generic;

namespace MiniPascalCompiler
{
  class Call : AST
  {
    public string callName { get; set; } 
    public List<AST> actualParameters { get; set; }
    public Token token { get; set; }
    public Symbol symbol { get; set; }
    

    public Call(string callName, List<AST> actualParameters, Token token)
    {
      this.callName = callName;
      this.actualParameters = actualParameters;
      this.token = token;
      this.symbol = null;
    }
  }
}