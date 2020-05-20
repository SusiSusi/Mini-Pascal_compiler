namespace MiniPascalCompiler
{
  class SemanticError : Error
  {
    public SemanticError(string message) :base(message) { }

    public override string ToString() 
    {
      return "\nSEMANTIC ERROR\n" + this.message;
    }
  }
}