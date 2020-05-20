using System.Collections.Generic;

namespace MiniPascalCompiler
{
  class Writeln : AST
  {
    public List<AST> exprs { get; set; }  

    public Writeln(List<AST> exprs=null)
    {
      this.exprs = exprs;
    }
  }
}