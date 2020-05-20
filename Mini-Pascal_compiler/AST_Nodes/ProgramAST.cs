namespace MiniPascalCompiler
{
  class ProgramAST : AST
  {
    public string name { get; set; } 
    public Block block { get; set; }

    public ProgramAST(string name, Block block)
    {
      this.name = name;
      this.block = block;
    }
  }
}