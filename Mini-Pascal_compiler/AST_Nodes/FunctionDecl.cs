using System.Collections.Generic;

namespace MiniPascalCompiler
{
  class FunctionDecl : AST
  {
    public string funcName { get; set; } 
    public Block blockNode { get; set; }
    public AST typeNode { get; set; }
    public List<Parameter> parameters { get; set; }

    public FunctionDecl(string funcName, Block blockNode, AST typeNode, List<Parameter> parameters=null)
    {
      this.funcName = funcName;
      this.blockNode = blockNode;
      this.typeNode = typeNode;
      this.parameters = parameters;
    }
  }
}