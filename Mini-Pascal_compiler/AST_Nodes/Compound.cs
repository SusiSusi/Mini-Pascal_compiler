using System.Collections.Generic;

namespace MiniPascalCompiler
{
  class Compound : AST
  {
    // 'BEGIN ... END' block
    public List<AST> children { get; set; }
    public Compound()
    {
      this.children = new List<AST>();
    }
  }
}