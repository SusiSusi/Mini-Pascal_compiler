using System;
using System.IO;

namespace MiniPascalCompiler
{
  class Program
  {
    static void Main(string[] args)
    {
      string codeExamples = File.ReadAllText("CodeExamples.txt");
      Error errorList = new Error();
      Lexer lexer = new Lexer(codeExamples, errorList);
      Parser parser = new Parser(lexer, errorList);
      AST tree = parser.Parse();
      SemanticAnalyzer semanticAnalyzer = new SemanticAnalyzer(errorList);
      semanticAnalyzer.Analyze(tree);

      if (errorList.errors.Count > 0)
      {
        errorList.PrintErrors();
      }
      else
      {
        // if you want to log CallStack, give true value to Interpreter
        Interpreter interpreter = new Interpreter(false);
        interpreter.Interpret(tree);
        CodeGenerator generator = new CodeGenerator();
        generator.Visit(tree);
        File.WriteAllText("GeneratorOutput.txt", generator.output);
      }
    }
  }
}
