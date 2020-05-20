using System;
using System.Collections.Generic;

namespace MiniPascalCompiler
{
  class SemanticAnalyzer : NodeVisitor
  {
    public ScopedSymbolTable currentScope { get; set; }
    string currentSymbolType;
    bool skipTypeCheck;
    Error errorList;
    List<Call> onHoldNodes;

    public SemanticAnalyzer(Error errorList)
    {
      this.currentScope = null;
      this.currentSymbolType = null;
      this.skipTypeCheck = false;
      this.errorList = errorList;
      this.onHoldNodes = new List<Call>();
    }

    private void Error(string errorMessage, Token token)
    {
      string message = errorMessage + " -> " + token.ToString();
      this.errorList.AddError(new SemanticError(message));
    }

    public void VisitBinaryOperation(BinaryOperation node)
    {
      Visit(node.left);
      Visit(node.right);
    }

    public object VisitIntegerAST(IntegerAST node)
    {
      if (!this.skipTypeCheck && !this.currentSymbolType.Equals("integer"))
      {
        Error("Wrong type! Should be " + this.currentSymbolType + ", not integer", node.token);
        return null;
      }
      return node.tokenValue;
    }

    public void VisitRealAST(RealAST node)
    {
      if (!this.skipTypeCheck && !this.currentSymbolType.Equals("real"))
      {
        Error("Wrong type! Should be " + this.currentSymbolType + ", not real", node.token);
      }
    }

    public void VisitUnaryOperation(UnaryOperation node)
    {
      Visit(node.expr);
    }

    public void VisitStatementList(StatementList node)
    {
      foreach (AST child in node.children)
      {
        Visit(child);
      }
    }

    public void VisitProgramAST(ProgramAST node)
    {
      ScopedSymbolTable globalScope = new ScopedSymbolTable("global", 1, this.currentScope);
      this.currentScope = globalScope;
      Visit(node.block);
      this.currentScope = this.currentScope.enclosingScope;
    }

    public void VisitProcedureDecl(ProcedureDecl node)
    {
      string procName = node.procName;
      ProcedureSymbol procSymbol = new ProcedureSymbol(procName, SymbolCategory.PROCEDURE);
      this.currentScope.Define(procSymbol);

      // Scope for parameters and local variables
      ScopedSymbolTable procedureScope = new ScopedSymbolTable(procName,
      this.currentScope.scopeLevel + 1, this.currentScope);
      this.currentScope = procedureScope;

      // Add parameters into the procedure scope
      if (node.parameters != null)
      {
        foreach (Parameter parameter in node.parameters)
        {
          object paramType = null;
          if (parameter.typeNode is Type)
          {
            Type type = (Type)parameter.typeNode;
            paramType = type.value;
          }
          else if (parameter.typeNode is Array)
          {
            Array arrayType = (Array)parameter.typeNode;
            paramType = arrayType.type.value;
          }
          else
          {
            Error("Parameter must have a type", parameter.token);
            return;
          }
          Symbol parameterType = this.currentScope.Lookup(paramType);
          if (parameter.typeNode is Array)
          {
            Array arrayType = (Array)parameter.typeNode;
            parameterType.type = arrayType;
          }
          string parameterName = (string)parameter.variableNode.value;
          VariableSymbol varSymbol = new VariableSymbol(parameterName, SymbolCategory.VARIABLE, parameterType, this.currentScope.scopeLevel);
          this.currentScope.Define(varSymbol);
          procSymbol.formalParameters.Add(varSymbol);
        }
      }
      Visit(node.blockNode);
      this.currentScope = this.currentScope.enclosingScope;
      procSymbol.blockAst = node.blockNode;
    }

    public void VisitFunctionDecl(FunctionDecl node)
    {
      string funcName = node.funcName;
      object funcType = "";
      if (node.typeNode is Type)
      {
        Type type = (Type)node.typeNode;
        funcType = type.value;
      }
      else if (node.typeNode is Array)
      {
        Array arrayType = (Array)node.typeNode;
        funcType = arrayType.type.value;
      }
      else
      {
        this.errorList.AddError(new SemanticError("Functio must have a type"));
        return;
      }
      FunctionSymbol funcSymbol = new FunctionSymbol(funcName, SymbolCategory.FUNCTION, funcType);
      this.currentScope.Define(funcSymbol);

      // Scope for parameters and local variables
      ScopedSymbolTable functionScope = new ScopedSymbolTable(funcName,
      this.currentScope.scopeLevel + 1, this.currentScope);
      this.currentScope = functionScope;

      // Add parameters into the function scope
      if (node.parameters != null)
      {
        foreach (Parameter parameter in node.parameters)
        {
          object paramType = null;
          if (parameter.typeNode is Type)
          {
            Type type = (Type)parameter.typeNode;
            paramType = type.value;
          }
          else if (parameter.typeNode is Array)
          {
            Array arrayType = (Array)parameter.typeNode;
            paramType = arrayType.type.value;
          }
          else
          {
            Error("Parameter must have a type", parameter.token);
            return;
          }
          Symbol parameterType = this.currentScope.Lookup(paramType);
          if (parameter.typeNode is Array)
          {
            Array arrayType = (Array)parameter.typeNode;
            parameterType.type = arrayType;
          }
          string parameterName = (string)parameter.variableNode.value;
          VariableSymbol varSymbol = new VariableSymbol(parameterName, SymbolCategory.VARIABLE, parameterType, this.currentScope.scopeLevel);
          this.currentScope.Define(varSymbol);
          funcSymbol.formalParameters.Add(varSymbol);
        }
      }
      Visit(node.blockNode);
      if (this.onHoldNodes.Count > 0)
      {
        foreach (Call onHoldNode in this.onHoldNodes)
        {
          if (funcName == onHoldNode.callName)
          {
            this.onHoldNodes.Remove(onHoldNode);
            Visit(onHoldNode);
            break;
          }
        }
      }
      this.currentScope = this.currentScope.enclosingScope;
      funcSymbol.blockAst = node.blockNode;
    }

    public void VisitCall(Call node)
    {
      Symbol symbol = this.currentScope.Lookup(node.callName);
      // Function call has not been defined
      if (symbol == null)
      {
        this.onHoldNodes.Add(node);
        return;
      }
      List<VariableSymbol> formalParameters = new List<VariableSymbol>();

      if (symbol.category == SymbolCategory.PROCEDURE)
      {
        ProcedureSymbol procedureSymbol = (ProcedureSymbol)symbol;
        node.symbol = procedureSymbol;
        formalParameters = procedureSymbol.formalParameters;
        this.skipTypeCheck = true;
      }
      else if (symbol.category == SymbolCategory.FUNCTION)
      {
        FunctionSymbol functionSymbol = (FunctionSymbol)symbol;
        node.symbol = functionSymbol;
        formalParameters = functionSymbol.formalParameters;
        this.currentSymbolType = (string)node.symbol.type;
      }
      else
      {
        Error("Call is accepted only in Procedures and Functions", node.token);
      }

      List<AST> actualParameters = node.actualParameters;
      if (actualParameters.Count != formalParameters.Count)
      {
        Error("Wrong number of arguments", node.token);
      }
      foreach (AST parameterNode in node.actualParameters)
      {
        Visit(parameterNode);
      }
      this.currentSymbolType = null;
      this.skipTypeCheck = false;
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

    public void VisitVariableDeclaration(VariableDeclaration node)
    {
      object typeName = "";
      if (node.typeNode is Type)
      {
        Type type = (Type)node.typeNode;
        if (type.token.type == TokenType.ERROR)
        {
          Error("Variable must have a type", type.token);
          return;
        }
        typeName = type.value;
      }
      else if (node.typeNode is Array)
      {
        Array arrayType = (Array)node.typeNode;
        typeName = arrayType.name;
      }
      else
      {
        this.errorList.AddError(new SemanticError("Variable must have a type"));
        return;
      }
      Symbol typeSymbol = this.currentScope.Lookup(typeName);
      if (node.typeNode is Array)
      {
        Array arrayType = (Array)node.typeNode;
        typeSymbol.type = arrayType;
      }
      foreach (Variable variable in node.variableNodes)
      {
        string variableName = variable.value;
        VariableSymbol variableSymbol = new VariableSymbol(variableName, SymbolCategory.VARIABLE, typeSymbol, this.currentScope.scopeLevel);
        if (this.currentScope.Lookup(variableName, true) != null)
        {
          Error("Duplicate identifier found", variable.token);
        }
        this.currentScope.Define(variableSymbol);
      }
    }

    public void VisitAssignment(Assignment node)
    {
      object variableName = node.left.value;
      Symbol variableSymbol = this.currentScope.Lookup(variableName);
      if (variableSymbol == null)
      {
        Error("Variable name not found", node.left.token);
      }
      else
      {
        BuiltinTypeSymbol type = (BuiltinTypeSymbol)variableSymbol.type;
        if (type.name == "array")
        {
          Array array = (Array)type.type;
          if (node.left.expr == null)
          {
            Error("Array pointer not found", node.left.token);
            return;
          }
          this.currentSymbolType = "integer";
          object pointer = Visit(node.left.expr);
          object arraySize = Visit(array.expr);
          if (pointer == null)
          {
            Error("Array pointer must be a number", node.left.token);
            return;
          }
          if ((int)pointer > (int)arraySize)
          {
            Error("Pointer value is incorrect. Array's size is " + arraySize.ToString(), node.left.token);
            return;
          }
          this.currentSymbolType = array.type.value;
        }
        else
        {
          this.currentSymbolType = type.name;
        }
        Visit(node.right);
        this.currentSymbolType = null;
      }
    }

    public void VisitVariable(Variable node)
    {
      string variableName = node.value;
      Symbol variableSymbol = this.currentScope.Lookup(variableName);
      if (variableSymbol == null)
      {
        Error("Identifier not found", node.token);
      }
      if (node.size)
      {
        BuiltinTypeSymbol type = (BuiltinTypeSymbol)variableSymbol.type;
        if (type.name != "array")
        {
          Error("Operation \"size\" only applies to values of array type", node.token);
        }
      }
    }

    public void VisitStringAST(StringAST node)
    {
      if (!this.skipTypeCheck && !this.currentSymbolType.Equals("string"))
      {
        Error("Wrong type! Should be " + this.currentSymbolType + ", not a string", node.token);
      }
    }

    public void VisitBooleanAST(BooleanAST node)
    {
      if (!this.skipTypeCheck && !this.currentSymbolType.Equals("Boolean"))
      {
        Error("Wrong type! Should be " + this.currentSymbolType + ", not Boolean", node.token);
      }
    }

    public void VisitTrue(True node) { }

    public void VisitFalse(False node) { }

    public void VisitWhile(While node)
    {
      this.skipTypeCheck = true;
      Visit(node.expr);
      Visit(node.statement);
      this.skipTypeCheck = false;
    }

    public void VisitIf(If node)
    {
      this.skipTypeCheck = true;
      Visit(node.expr);
      Visit(node.statement);
      if (node.statementElse != null)
      {
        Visit(node.statementElse);
      }
      this.skipTypeCheck = false;
    }

    public void VisitAssert(Assert node)
    {
      this.skipTypeCheck = true;
      if (node.expr is BinaryOperation)
      {
        Visit(node.expr);
      }
      else
      {
        this.errorList.AddError(new SemanticError("Assert failure - Should be Boolean expression"));
      }
      this.skipTypeCheck = false;
    }

    public void VisitReturn(Return node)
    {
      string scopeName = (string)this.currentScope.scopeName;
      Symbol typeSymbol = this.currentScope.Lookup(scopeName);
      if (node.expr != null)
      {
        Visit(node.expr);
      }
    }

    public void VisitWriteln(Writeln node)
    {
      this.skipTypeCheck = true;
      foreach (AST expr in node.exprs)
      {
        Visit(expr);
      }
      this.skipTypeCheck = false;
    }

    public void VisitRead(Read node)
    {
      foreach (Variable variable in node.variables)
      {
        object variableName = variable.value;
        Visit(variable);
        Symbol symbol = this.currentScope.Lookup(variableName);
        string variableType = symbol.type.ToString();
        node.type = variableType;
      }
    }

    public void Analyze(AST tree)
    {
      if (tree == null)
      {
        this.errorList.AddError(new SemanticError("Tree is null!"));
      }
      Visit(tree);
    }
  }
}