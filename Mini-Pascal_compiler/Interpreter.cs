using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniPascalCompiler
{
  class Interpreter : NodeVisitor
  {
    public AST tree { get; set; }
    public CallStack callStack { get; set; }
    public bool logStack { get; set; }

    public Interpreter(bool logStack)
    {
      this.callStack = new CallStack();
      this.logStack = logStack;
    }

    public void Log(string message)
    {
      if (logStack)
      {
        Console.WriteLine(message);
      }
    }

    public object VisitBinaryOperation(BinaryOperation node)
    {
      TokenType opType = node.operat.type;
      object nodeLeft = Visit(node.left);
      object nodeRight = Visit(node.right);

      if (nodeLeft is int && nodeRight is int)
      {
        return IntBinaryOperations(opType, (int)nodeLeft, (int)nodeRight);
      }
      else if (nodeLeft is float && nodeRight is float)
      {
        return FloatBinaryOperations(opType, (float)nodeLeft, (float)nodeRight);
      }
      else if (nodeLeft is string && nodeRight is string)
      {
        return StringBinaryOperations(opType, (string)nodeLeft, (string)nodeRight);
      }
      else if (nodeLeft is bool && nodeRight is bool)
      {
        return BoolBinaryOperations(opType, (bool)nodeLeft, (bool)nodeRight);
      }
      throw new InterpreterError("VisitBinaryOperation error");
    }

    public int VisitIntegerAST(IntegerAST node)
    {
      return node.tokenValue;
    }

    public float VisitRealAST(RealAST node)
    {
      return node.tokenValue;
    }

    public string VisitStringAST(StringAST node)
    {
      return node.tokenValue;
    }

    public bool VisitBooleanAST(BooleanAST node)
    {
      return node.tokenValue;
    }

    public bool VisitTrue(True node)
    {
      return node.tokenValue;
    }

    public bool VisitFalse(False node)
    {
      return node.tokenValue;
    }

    public object VisitUnaryOperation(UnaryOperation node)
    {
      TokenType opType = node.operat.type;
      if (opType == TokenType.PLUS)
      {
        return +(int)Visit(node.expr);
      }
      else if (opType == TokenType.MINUS)
      {
        return -(int)Visit(node.expr);
      }
      throw new InterpreterError("VisitUnaryOperation error");
    }

    public void VisitProgramAST(ProgramAST node)
    {
      string programName = (string)node.name;
      Log("ENTER: PROGRAM " + programName);
      ActivationRecord ar = new ActivationRecord(programName, ARType.PROGRAM, 1);
      this.callStack.Push(ar);
      Log(this.callStack.ToString());
      Visit(node.block);
      Log("\nLEAVE: PROGRAM " + programName);
      Log(this.callStack.ToString());
      this.callStack.Pop();
    }

    public void VisitProcedureDecl(ProcedureDecl node) { }

    public void VisitFunctionDecl(FunctionDecl node) { }

    public object VisitCall(Call node)
    {
      if (node.symbol != null && node.symbol.category == SymbolCategory.PROCEDURE)
      {
        ProcedureCall(node);
        return null;
      }
      else if (node.symbol != null && node.symbol.category == SymbolCategory.FUNCTION)
      {
        FunctionCall(node);
        return this.callStack.Peek().Get("rv");
      }
      else
      {
        throw new InterpreterError("Symbol category is incorrect.");
      }
    }

    public void VisitWhile(While node)
    {
      Log("ENTER: WHILE ");
      while ((bool)Visit(node.expr))
      {
        Visit(node.statement);
      }
      Log("\nLEAVE: WHILE ");
    }

    public void VisitIf(If node)
    {
      Log("ENTER: IF ");
      if ((bool)Visit(node.expr))
      {
        Visit(node.statement);
      }
      else
      {
        Visit(node.statementElse);
      }
      Log("\nLEAVE: IF ");
    }

    public void VisitAssert(Assert node)
    {
      object response = Visit(node.expr);
      if (!(bool)response)
      {
        Console.WriteLine("Assertion fails: " + response.ToString().ToUpper());
      }
      else
      {
        Console.WriteLine(response.ToString().ToUpper());
      }
    }

    public void VisitStatementList(StatementList node)
    {
      foreach (AST child in node.children)
      {
        Visit(child);
      }
    }

    public void VisitBlock(Block node)
    {
      foreach (AST declaration in node.declarations)
      {
        Visit(declaration);
      }
      Visit(node.compoundStatement);
    }

    public void VisitCompound(Compound node)
    {
      foreach (AST child in node.children)
      {
        Visit(child);
      }
    }

    public void VisitNoOperation(NoOperation node) { }

    public void VisitAssignment(Assignment node)
    {
      object variableName = node.left.value;
      if (node.right is Call)
      {
        object variableValue = Visit(node.right);
        ActivationRecord ar = this.callStack.Peek();
        object returnValue = ar.Get("rv");
        this.callStack.Pop();
        ar = this.callStack.Peek();
        ar.SetItem(variableName, returnValue);
      }
      else if (node.left.expr != null)
      {
        // node left is array
        int arrayPointer = (int)Visit(node.left.expr);
        object variableValue = Visit(node.right);
        ActivationRecord ar = this.callStack.Peek();
        string arraysType = ar.GetArraysType(variableName);
        if (arraysType == "Int32[]")
        {
          int[] copyOfArray = (int[])ar.Get(variableName);
          copyOfArray[arrayPointer] = (int)variableValue;
          ar.SetItem(variableName, copyOfArray);
        }
        else if (arraysType == "String[]")
        {
          string[] copyOfArray = (string[])ar.Get(variableName);
          copyOfArray[arrayPointer] = (string)variableValue;
          ar.SetItem(variableName, copyOfArray);
        }
        else if (arraysType == "Boolean[]")
        {
          bool[] copyOfArray = (bool[])ar.Get(variableName);
          copyOfArray[arrayPointer] = (bool)variableValue;
          ar.SetItem(variableName, copyOfArray);
        }
      }
      else
      {
        object variableValue = Visit(node.right);
        ActivationRecord ar = this.callStack.Peek();
        ar.SetItem(variableName, variableValue);
      }
      Log(this.callStack.ToString());
    }

    public object VisitVariable(Variable node)
    {
      string variableName = node.value;
      ActivationRecord ar = this.callStack.Peek();
      // handling array-types
      if (node.expr != null || node.size)
      {
        string arraysType = ar.GetArraysType(variableName);
        if (arraysType == "Int32[]")
        {
          int[] copyOfArray = (int[])ar.Get(variableName);
          if (node.expr != null)
          {
            int arrayPointer = (int)Visit(node.expr);
            return copyOfArray[arrayPointer];
          }
          return copyOfArray.Length;
        }
        else if (arraysType == "String[]")
        {
          string[] copyOfArray = (string[])ar.Get(variableName);
          if (node.expr != null)
          {
            int arrayPointer = (int)Visit(node.expr);
            return copyOfArray[arrayPointer];
          }
          return copyOfArray.Length;
        }
        else if (arraysType == "Boolean[]")
        {
          bool[] copyOfArray = (bool[])ar.Get(variableName);
          if (node.expr != null)
          {
            int arrayPointer = (int)Visit(node.expr);
            return copyOfArray[arrayPointer];
          }
          return copyOfArray.Length;
        }
      }
      object variableValue = ar.Get(variableName);
      if (variableValue == null)
      {
        variableValue = ar.Get("rv");
      }
      return variableValue;
    }

    public void VisitVariableDeclaration(VariableDeclaration node)
    {
      object typeName = "";
      int size = 256; // random value, arrays max size
      if (node.typeNode is Type)
      {
        Type type = (Type)node.typeNode;
        typeName = type.value;
      }
      else if (node.typeNode is Array)
      {
        Array arrayType = (Array)node.typeNode;
        size = (int)Visit(arrayType.expr);
        typeName = arrayType.type.value;
      }

      foreach (Variable variable in node.variableNodes)
      {
        object variableName = variable.value;
        if ((string)typeName == TokenType.INTEGER.ToString().ToLower() ||
        (string)typeName == TokenType.REAL.ToString().ToLower())
        {
          ActivationRecord ar = this.callStack.Peek();
          if (node.typeNode is Array)
          {
            ar.SetItem(variableName, new int[size]);
          }
          else
          {
            ar.SetItem(variableName, 0);
          }
        }
        else if ((string)typeName == TokenType.STRING.ToString().ToLower())
        {
          ActivationRecord ar = this.callStack.Peek();
          if (node.typeNode is Array)
          {
            ar.SetItem(variableName, new string[size]);
          }
          else
          {
            ar.SetItem(variableName, "");
          }
        }
        else
        {
          ActivationRecord ar = this.callStack.Peek();
          if (node.typeNode is Array)
          {
            ar.SetItem(variableName, new bool[size]);
          }
          else
          {
            ar.SetItem(variableName, false);
          }
        }
      }
    }

    public void VisitType(Type node) { }

    public void VisitReturn(Return node)
    {
      ActivationRecord ar = this.callStack.Peek();
      object returnValueName = "rv";
      if (node.expr != null)
      {
        ar.SetItem(returnValueName, Visit(node.expr));
      }
    }

    public void VisitWriteln(Writeln node)
    {
      foreach (AST expr in node.exprs)
      {
        Console.WriteLine(Visit(expr));
      }
    }

    public void VisitRead(Read node)
    {
      foreach (Variable variable in node.variables)
      {
        string variableType = node.type;
        string input = Console.ReadLine();
        if (variable.expr != null)
        {
          string variableName = variable.value;
          int arrayPointer = (int)Visit(variable.expr);
          ActivationRecord ar = this.callStack.Peek();
          string arraysType = ar.GetArraysType(variableName);

          if (arraysType == "Int32[]")
          {
            try
            {
              int number = int.Parse(input);
              int[] copyOfArray = (int[])ar.Get(variableName);
              copyOfArray[arrayPointer] = number;
              ar.SetItem(variableName, copyOfArray);

            }
            catch (System.Exception)
            {
              throw new InterpreterError("Invalid input to array. Expected input value is a number.");
            }
          }
          else if (arraysType == "String[]")
          {
            string[] copyOfArray = (string[])ar.Get(variableName);
            copyOfArray[arrayPointer] = input;
            ar.SetItem(variableName, copyOfArray);
          }
          else if (arraysType == "Boolean[]")
          {
            bool[] copyOfArray = (bool[])ar.Get(variableName);
            if (input == "true")
            {
              copyOfArray[arrayPointer] = true;
            }
            else
            {
              copyOfArray[arrayPointer] = false;
            }
            ar.SetItem(variableName, copyOfArray);
          }
        }
        else if (variableType == TokenType.INTEGER.ToString().ToLower())
        {
          try
          {
            int number = int.Parse(input);
            ActivationRecord ar = this.callStack.Peek();
            ar.SetItem(variable.value, number);
          }
          catch (System.Exception)
          {
            throw new InterpreterError("Invalid input. Expected input value is a number.");
          }
        }
        else
        {
          ActivationRecord ar = this.callStack.Peek();
          ar.SetItem(variable.value, input);
        }
      }
    }

    public void Interpret(AST tree)
    {
      if (tree == null)
      {
        throw new InterpreterError("Can not interpret because the tree is null");
      }
      Visit(tree);
    }

    private object IntBinaryOperations(TokenType type, int nodeLeft, int nodeRight)
    {
      if (type == TokenType.PLUS)
      {
        return nodeLeft + nodeRight;
      }
      else if (type == TokenType.MINUS)
      {
        return nodeLeft - nodeRight;
      }
      else if (type == TokenType.MUL)
      {
        return nodeLeft * nodeRight;
      }
      else if (type == TokenType.DIV)
      {
        return nodeLeft / nodeRight;
      }
      else if (type == TokenType.EQUAL)
      {
        return nodeLeft == nodeRight;
      }
      else if (type == TokenType.LESS)
      {
        return nodeLeft < nodeRight;
      }
      else if (type == TokenType.INEQUALITY)
      {
        return nodeLeft != nodeRight;
      }
      else if (type == TokenType.LESS_OR_EQUAL)
      {
        return nodeLeft <= nodeRight;
      }
      else if (type == TokenType.GREATER_OR_EQUAL)
      {
        return nodeLeft >= nodeRight;
      }
      else if (type == TokenType.GREATER)
      {
        return nodeLeft > nodeRight;
      }
      else if (type == TokenType.MODULO)
      {
        return nodeLeft % nodeRight;
      }
      throw new InterpreterError("IntBinaryOperations error");
    }

    private object FloatBinaryOperations(TokenType type, float nodeLeft, float nodeRight)
    {
      if (type == TokenType.PLUS)
      {
        return nodeLeft + nodeRight;
      }
      else if (type == TokenType.MINUS)
      {
        return nodeLeft - nodeRight;
      }
      else if (type == TokenType.MUL)
      {
        return nodeLeft * nodeRight;
      }
      else if (type == TokenType.DIV)
      {
        return nodeLeft / nodeRight;
      }
      else if (type == TokenType.EQUAL)
      {
        return nodeLeft == nodeRight;
      }
      else if (type == TokenType.LESS)
      {
        return nodeLeft < nodeRight;
      }
      else if (type == TokenType.INEQUALITY)
      {
        return nodeLeft != nodeRight;
      }
      else if (type == TokenType.LESS_OR_EQUAL)
      {
        return nodeLeft <= nodeRight;
      }
      else if (type == TokenType.GREATER_OR_EQUAL)
      {
        return nodeLeft >= nodeRight;
      }
      else if (type == TokenType.GREATER)
      {
        return nodeLeft > nodeRight;
      }
      throw new InterpreterError("FloatBinaryOperations error");
    }

    private object StringBinaryOperations(TokenType type, string nodeLeft, string nodeRight)
    {
      if (type == TokenType.PLUS)
      {
        return nodeLeft + nodeRight;
      }
      else if (type == TokenType.EQUAL)
      {
        return nodeLeft.Equals(nodeRight);
      }
      else if (type == TokenType.LESS)
      {
        if (nodeLeft.CompareTo(nodeRight) == -1)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      else if (type == TokenType.INEQUALITY)
      {
        return nodeLeft != nodeRight;
      }
      else if (type == TokenType.LESS_OR_EQUAL)
      {
        if (nodeLeft.CompareTo(nodeRight) == -1 ||
        nodeLeft.CompareTo(nodeRight) == 0)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      else if (type == TokenType.GREATER_OR_EQUAL)
      {
        if (nodeLeft.CompareTo(nodeRight) == 1 ||
        nodeLeft.CompareTo(nodeRight) == 0)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      else if (type == TokenType.GREATER)
      {
        if (nodeLeft.CompareTo(nodeRight) == 1)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      throw new InterpreterError("StringBinaryOperations error");
    }

    private object BoolBinaryOperations(TokenType type, bool nodeLeft, bool nodeRight)
    {
      if (type == TokenType.AND)
      {
        return nodeLeft & nodeRight;
      }
      else if (type == TokenType.NOT)
      {
        return nodeLeft != nodeRight;
      }
      else if (type == TokenType.OR)
      {
        if (nodeLeft || nodeRight)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      else if (type == TokenType.EQUAL)
      {
        return nodeLeft == nodeRight;
      }
      else if (type == TokenType.LESS)
      {
        if (nodeLeft.CompareTo(nodeRight) == -1)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      else if (type == TokenType.INEQUALITY)
      {
        return nodeLeft != nodeRight;
      }
      else if (type == TokenType.LESS_OR_EQUAL)
      {
        if (nodeLeft.CompareTo(nodeRight) == -1 ||
        nodeLeft.CompareTo(nodeRight) == 0)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      else if (type == TokenType.GREATER_OR_EQUAL)
      {
        if (nodeLeft.CompareTo(nodeRight) == 1 ||
        nodeLeft.CompareTo(nodeRight) == 0)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      else if (type == TokenType.GREATER)
      {
        if (nodeLeft.CompareTo(nodeRight) == 1)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      throw new InterpreterError("BoolBinaryOperations error");
    }

    private void ProcedureCall(Call node)
    {
      ProcedureSymbol procSymbol = (ProcedureSymbol)node.symbol;
      ActivationRecord ar = new ActivationRecord(node.callName, ARType.PROCEDURE, procSymbol.scopeLevel + 1);

      List<VariableSymbol> formalParameters = procSymbol.formalParameters;
      List<AST> actualParameters = node.actualParameters;

      // mapping formal and actual parameters into a single list
      Dictionary<VariableSymbol, AST> result = formalParameters.Zip(actualParameters, (k, v)
      => new { k, v }).ToDictionary(x => x.k, x => x.v);

      foreach (KeyValuePair<VariableSymbol, AST> kvp in result)
      {
        ar.SetItem(kvp.Key.name, Visit(kvp.Value));
      }
      this.callStack.Push(ar);

      Log("\nENTER: PROCEDURE " + node.callName);
      Log(this.callStack.ToString());

      Visit(procSymbol.blockAst);

      Log("\nLEAVE: PROCEDURE " + node.callName);
      Log(this.callStack.ToString());

      this.callStack.Pop();
    }

    private void FunctionCall(Call node)
    {
      FunctionSymbol funcSymbol = (FunctionSymbol)node.symbol;
      ActivationRecord ar = this.callStack.Peek();
      bool newAr = false;
      if (ar.name != node.callName)
      {
        ar = new ActivationRecord(node.callName, ARType.FUNCTION, funcSymbol.scopeLevel + 1);
        newAr = true;
      }

      List<VariableSymbol> formalParameters = funcSymbol.formalParameters;
      List<AST> actualParameters = node.actualParameters;

      // mapping formal and actual parameters into a single list
      Dictionary<VariableSymbol, AST> result = formalParameters.Zip(actualParameters, (k, v)
      => new { k, v }).ToDictionary(x => x.k, x => x.v);

      foreach (KeyValuePair<VariableSymbol, AST> kvp in result)
      {
        ar.SetItem(kvp.Key.name, Visit(kvp.Value));
      }
      // if new AR, push to callStack
      if (newAr)
      {
        this.callStack.Push(ar);
      }

      Log("\nENTER: FUNCTION " + node.callName);
      Log(this.callStack.ToString());

      Visit(funcSymbol.blockAst);

      Log("\nLEAVE: FUNCTION " + node.callName);
      Log(this.callStack.ToString());
    }
  }
}