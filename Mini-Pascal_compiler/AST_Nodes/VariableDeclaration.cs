using System.Collections.Generic;

namespace MiniPascalCompiler
{
  class VariableDeclaration : AST
  {
    public List<Variable> variableNodes { get; set; }
    public AST typeNode { get; set; }

    public VariableDeclaration(List<Variable> variableNodes, AST typeNode) 
    {
      this.variableNodes = variableNodes;
      this.typeNode = typeNode;
    } 
  } 
}