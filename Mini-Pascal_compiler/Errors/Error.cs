using System;
using System.Collections.Generic;

namespace MiniPascalCompiler
{
  class Error : Exception
  {
    public string message { get; set; }
    public List<Error> errors { get; set; }

    public Error()
    {
      this.errors = new List<Error>();
    }
    public Error(string message)
    {
      this.message = message;
    }

    public void AddError(Error error)
    {
      this.errors.Add(error);
    }

    public void PrintErrors()
    {
      if (this.errors != null)
      {
        foreach (Error error in this.errors)
        {
          Console.WriteLine(error.ToString());
        }
      }
    }

    public override string ToString()
    {
      return "\n" + this.message;
    }
  }
}