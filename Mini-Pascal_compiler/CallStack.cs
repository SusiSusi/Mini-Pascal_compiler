using System.Collections.Generic;

namespace MiniPascalCompiler
{
  class CallStack
  {
    public Stack<ActivationRecord> records { get; set; }

    public CallStack()
    {
      this.records = new Stack<ActivationRecord>();
    }

    public void Push(ActivationRecord ar)
    {
      this.records.Push(ar);
    }

    public ActivationRecord Pop()
    {
      return this.records.Pop();
    }

    public ActivationRecord Peek()
    {
      return this.records.Peek();
    }

    public override string ToString()
    {
      string result = string.Join('\n', this.records);
      result = "CALL STACK\n" + result;
      return result;
    }
  }
}