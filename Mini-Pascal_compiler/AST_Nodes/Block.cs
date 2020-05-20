using System.Collections.Generic;

namespace MiniPascalCompiler
{
  class Block : AST
  {
    public List<AST> declarations { get; set; }
    public AST compoundStatement { get; set; }

    public Block(List<AST> declarations, AST compoundStatement)
    {
      this.declarations = declarations;
      this.compoundStatement = compoundStatement;
    }
  }
}