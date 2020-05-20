namespace MiniPascalCompiler
{
  class ParserError : Error
  {
    public ParserError(string message) :base(message) { }

    public override string ToString() 
    {
      return "\nPARSER ERROR\n" + this.message;
    }
  }
}