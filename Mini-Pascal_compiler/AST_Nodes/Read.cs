using System.Collections.Generic;

namespace MiniPascalCompiler
{
  class Read : AST
  {
    public List<Variable> variables { get; set; }
    public string type { get; set; }

    public Read(List<Variable> variables, string type=null)
    {
      this.variables = variables;
      this.type = type;
    }
  }
}