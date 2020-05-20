namespace MiniPascalCompiler
{
  class LexerError : Error
  {
    public LexerError(string message) :base(message) { }

    public override string ToString() 
    {
      return "\nLEXER ERROR\n" + this.message;
    }
  }
}