using System.Collections.Generic;

namespace MiniPascalCompiler
{
  class ProcedureDecl : AST
  {
    public string procName { get; set; } 
    public Block blockNode { get; set; }
    public List<Parameter> parameters { get; set; }

    public ProcedureDecl(string procName, Block blockNode, List<Parameter> parameters=null)
    {
      this.procName = procName;
      this.blockNode = blockNode;
      this.parameters = parameters;
    }
  }
}