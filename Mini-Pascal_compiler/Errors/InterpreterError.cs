namespace MiniPascalCompiler
{
  class InterpreterError : Error
  {
    public InterpreterError(string message) :base(message) { }

    public override string ToString() 
    {
      return "\nINTERPRETER ERROR\n" + this.message;
    }
  }
}