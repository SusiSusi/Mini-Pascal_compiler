using System;
using System.Collections.Generic;
using System.Text;

namespace MiniPascalCompiler
{
  class CodeGenerator : NodeVisitor
  {
    public ScopedSymbolTable currentScope { get; set; }
    public string output { get; set; }
    Dictionary<string, string> cTypes;
    Dictionary<string, string> cFormatSpecifiers;
    public CodeGenerator()
    {
      this.currentScope = null;
      this.output = null;
      this.cTypes = new Dictionary<string, string>();
      this.cFormatSpecifiers = new Dictionary<string, string>();
      SetTypesForC();
    }

    private void SetTypesForC()
    {
      this.cTypes.Add("integer", "int");
      this.cTypes.Add("real", "float");
      this.cTypes.Add("string", "char");
      this.cTypes.Add("Boolean", "bool");
      this.cFormatSpecifiers.Add("integer", "%d");
      this.cFormatSpecifiers.Add("real", "%e");
      this.cFormatSpecifiers.Add("string", "%s");
      this.cFormatSpecifiers.Add("Boolean", "%d");
    }

    public void VisitProgramAST(ProgramAST node)
    {
      string addBeforeProgram = "#include <stdio.h>\n"; // preprocessor command
      addBeforeProgram += "#include <assert.h>\n"; // for assert statement
      addBeforeProgram += "#include <stdbool.h>\n\n"; // boolean types
      string programName = node.name;
      string result = addBeforeProgram + "int " + programName + "0() {\n";

      ScopedSymbolTable globalScope = new ScopedSymbolTable("global", 1, this.currentScope);
      this.currentScope = globalScope;

      // visit subtree
      result += Visit(node.block); // string.Join(Visit(..), result)
      result += "\nreturn 0;\n}\n";
      this.output = result;
      this.currentScope = this.currentScope.enclosingScope;
    }

    public string VisitBlock(Block node)
    {
      List<string> results = new List<string>();
      string result = "";
      foreach (AST declaration in node.declarations)
      {
        result = (string)Visit(declaration);
        results.Add(result);
      }
      result = (string)Visit(node.compoundStatement);

      results.Add(result);
      return string.Join("\n", results);
    }

    public string VisitCompound(Compound node)
    {
      List<string> results = new List<string>();
      string result = "";
      foreach (AST child in node.children)
      {
        result = (string)Visit(child);
        if (result == null)
        {
          continue;
        }
        result += AddSemicolonIfNeeded(result);
        results.Add(result);
      }
      return string.Join("\n", results);
    }

    public void VisitNoOperation(NoOperation node) { }

    public string VisitBinaryOperation(BinaryOperation node)
    {
      object left = Visit(node.left);
      object right = Visit(node.right);
      return string.Concat(left, " ", node.operat.value, " ", right);
    }

    public string VisitProcedureDecl(ProcedureDecl node)
    {
      string procName = node.procName;
      ProcedureSymbol procSymbol = new ProcedureSymbol(procName, SymbolCategory.PROCEDURE);
      this.currentScope.Define(procSymbol);

      string result = "void " + procName + this.currentScope.scopeLevel;

      // Scope for parameters and local variables
      ScopedSymbolTable procedureScope = new ScopedSymbolTable(procName,
      this.currentScope.scopeLevel + 1, this.currentScope);

      this.currentScope = procedureScope;

      List<object> formalParameters = new List<object>();
      // Add parameters into the procedure scope
      if (node.parameters != null)
      {
        result += "(";
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
          if (parameterType.name == "string")
          {
            formalParameters.Add(this.cTypes[parameterType.name] + " " + parameterName +
            this.currentScope.scopeLevel.ToString() + "[]");
          }
          else if (parameterType.name == "array")
          {
            Array arrayType = (Array)parameter.typeNode;
            string type = arrayType.type.value;
            if (arrayType.expr != null)
            {
              string arraySize = (string)Visit(arrayType.expr);
              formalParameters.Add(this.cTypes[type] + " " + parameterName +
              this.currentScope.scopeLevel.ToString() + "[" + arraySize + "]");
            }
            else
            {
              formalParameters.Add(this.cTypes[type] + " " + parameterName +
              this.currentScope.scopeLevel.ToString() + "[]");
            }
          }
          else
          {
            formalParameters.Add(this.cTypes[parameterType.name] + " " + parameterName +
            this.currentScope.scopeLevel.ToString());
          }
        }
        result += string.Join(';', formalParameters);
        result += ") {";
      }
      result += "\n";
      result += Visit(node.blockNode);

      string finalResult = "";
      foreach (string line in result.Split('\n'))
      {
        finalResult += string.Concat('\n', line);
      }
      this.currentScope = this.currentScope.enclosingScope;
      finalResult += AddSemicolonIfNeeded(finalResult);
      return finalResult + "\n}\n";
    }

    public string VisitFunctionDecl(FunctionDecl node)
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
      FunctionSymbol funcSymbol = new FunctionSymbol(funcName, SymbolCategory.FUNCTION, funcType);
      this.currentScope.Define(funcSymbol);

      string result = this.cTypes[(string)funcType] + " " + funcName + this.currentScope.scopeLevel;

      // Scope for parameters and local variables
      ScopedSymbolTable functionScope = new ScopedSymbolTable(funcName,
      this.currentScope.scopeLevel + 1, this.currentScope);
      this.currentScope = functionScope;

      List<object> formalParameters = new List<object>();
      // Add parameters into the function scope
      if (node.parameters != null)
      {
        result += "(";
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
            paramType = arrayType.name;
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
          if (parameterType.name == "string")
          {
            formalParameters.Add(this.cTypes[parameterType.name] + " " + parameterName +
            this.currentScope.scopeLevel.ToString() + "[]");
          }
          else if (parameterType.name == "array")
          {
            Array arrayType = (Array)parameter.typeNode;
            string type = arrayType.type.value;
            if (arrayType.expr != null)
            {
              string arraySize = (string)Visit(arrayType.expr);
              formalParameters.Add(this.cTypes[type] + " " + parameterName +
              this.currentScope.scopeLevel.ToString() + "[" + arraySize + "]");
            }
            else
            {
              formalParameters.Add(this.cTypes[type] + " " + parameterName +
              this.currentScope.scopeLevel.ToString() + "[]");
            }
          }
          else
          {
            formalParameters.Add(this.cTypes[parameterType.name] + " " + parameterName +
            this.currentScope.scopeLevel.ToString());
          }
        }
        result += string.Join(';', formalParameters);
        result += ") {";
      }
      result += "\n";
      result += Visit(node.blockNode);
      string finalResult = "";
      foreach (string line in result.Split('\n'))
      {
        finalResult += string.Concat('\n', line);
      }
      this.currentScope = this.currentScope.enclosingScope;
      finalResult += AddSemicolonIfNeeded(finalResult);
      return finalResult + "} \n";
    }

    public string VisitVariableDeclaration(VariableDeclaration node)
    {
      if (node.typeNode is Array)
      {
        return VariableDeclarationArray(node);
      }
      string typeName = "";
      Type type = (Type)node.typeNode;
      typeName = type.value;
      Symbol typeSymbol = this.currentScope.Lookup(typeName);
      string typeNameForC = this.cTypes[typeName];
      List<string> variables = new List<string>();
      foreach (Variable variable in node.variableNodes)
      {
        string variableName = variable.value;
        VariableSymbol variableSymbol = new VariableSymbol(variableName, SymbolCategory.VARIABLE, typeSymbol, this.currentScope.scopeLevel);
        this.currentScope.Define(variableSymbol);

        if (typeNameForC == "char")
        {
          variables.Add(typeNameForC + " " + variableName + this.currentScope.scopeLevel.ToString() +
          "[];");
        }
        else
        {
          variables.Add(typeNameForC + " " + variableName + this.currentScope.scopeLevel.ToString() +
          ";");
        }
      }
      string result = string.Join("\n", variables);
      return result;
    }

    private string VariableDeclarationArray(VariableDeclaration node)
    {
      string typeName = "";
      Array arrayType = (Array)node.typeNode;
      typeName = arrayType.name;
      Symbol typeSymbol = this.currentScope.Lookup(typeName);
      typeSymbol.type = arrayType;

      List<string> variables = new List<string>();
      foreach (Variable variable in node.variableNodes)
      {
        string variableName = variable.value;
        VariableSymbol variableSymbol = new VariableSymbol(variableName, SymbolCategory.VARIABLE, typeSymbol, this.currentScope.scopeLevel);
        this.currentScope.Define(variableSymbol);
        string typeNameForC = this.cTypes[arrayType.type.value];
        string arraySize = (string)Visit(arrayType.expr);
        variables.Add(typeNameForC + " " + variableName + this.currentScope.scopeLevel.ToString() +
          "[" + arraySize + "];");
      }
      string result = string.Join("\n", variables);
      return result;
    }

    public string VisitAssignment(Assignment node)
    {
      object left = Visit(node.left);
      object right = Visit(node.right);
      return string.Concat(left, " = ", right);
    }

    public string VisitVariable(Variable node)
    {
      string variableName = node.value;
      VariableSymbol variableSymbol = (VariableSymbol)this.currentScope.Lookup(variableName);
      if (node.expr != null)
      {
        string arrayPointer = (string)Visit(node.expr);
        return variableName + variableSymbol.scopelevel.ToString() +
        "[" + arrayPointer + "]";
      }
      if (node.size)
      {
        return "sizeof(" + variableName + ")/sizeof(" + variableName + "[0])";
      }
      return variableName + variableSymbol.scopelevel.ToString();
    }

    public string VisitIntegerAST(IntegerAST node)
    {
      return node.tokenValue.ToString();
    }

    public string VisitStringAST(StringAST node)
    {
      return "\"" + node.tokenValue + "\"";
    }

    public string VisitRealAST(RealAST node)
    {
      return node.tokenValue.ToString();
    }

    public string VisitWhile(While node)
    {
      string result = "while" + this.currentScope.scopeLevel + " ";
      result += Visit(node.expr);
      result += "\ngoto whileStatement" + this.currentScope.scopeLevel + ";\n";
      result += "whileStatement" + this.currentScope.scopeLevel + ":\n";
      result += Visit(node.statement);
      result += AddSemicolonIfNeeded(result);
      result += "\n";
      return result;
    }

    public string VisitIf(If node)
    {
      string result = "if" + this.currentScope.scopeLevel + " ";
      result += Visit(node.expr);
      result += "\n";
      result += "goto ifStatement" + this.currentScope.scopeLevel + ";\n";
      if (node.statementElse != null)
      {
        result += "else\n";
        result += "goto ifStatementElse" + this.currentScope.scopeLevel + ";\n";
      }
      result += "ifStatement" + this.currentScope.scopeLevel + ":\n";
      result += Visit(node.statement);
      if (node.statementElse != null)
      {
        result += "\nifStatementElse" + this.currentScope.scopeLevel + ":\n";
        result += Visit(node.statementElse);
      }
      result += AddSemicolonIfNeeded(result);
      return result;
    }

    public string VisitReturn(Return node)
    {
      string result = "return ";
      result += Visit(node.expr);
      result += ";\n";
      return result;
    }

    public string VisitCall(Call node)
    {
      Symbol symbol = this.currentScope.Lookup(node.callName);
      string result = node.callName + "(";
      List<AST> actualParameters = node.actualParameters;
      List<object> parameters = new List<object>();
      foreach (AST parameterNode in node.actualParameters)
      {
        parameters.Add(Visit(parameterNode));
      }
      result += string.Join(",", parameters);
      result += ")";
      return result;
    }

    public string VisitWriteln(Writeln node)
    {
      string result = "printf(";
      List<string> exprs = new List<string>();
      foreach (AST expr in node.exprs)
      {
        exprs.Add((string)Visit(expr));
      }
      result += string.Join(", ", exprs);
      result += ");";
      return result;
    }

    public string VisitRead(Read node)
    {
      string result = "scanf(\"";
      List<string> variables = new List<string>();
      foreach (Variable variable in node.variables)
      {
        object variableName = variable.value;
        Symbol symbol = this.currentScope.Lookup(variableName);
        string typeName = "";
        if (symbol.type is BuiltinTypeSymbol)
        {
          BuiltinTypeSymbol builtIn = (BuiltinTypeSymbol)symbol.type;
          if (builtIn.type is Array)
          {
            Array arrayType = (Array)builtIn.type;
            typeName = arrayType.type.value;
          }
          else
          {
            typeName = builtIn.name;
          }
        }
        else
        {
          typeName = symbol.name;
        }
        result += this.cFormatSpecifiers[typeName];
        result += " ";
        variables.Add((string)Visit(variable));
      }
      result += "\", ";
      result += string.Join(", ", variables);
      result += ");";
      return result;
    }

    public string VisitTrue(True node)
    {
      return node.tokenValue.ToString().ToLower();
    }

    public string VisitFalse(False node)
    {
      return node.tokenValue.ToString().ToLower();
    }

    public string VisitAssert(Assert node)
    {
      string result = "assert(";
      result += Visit(node.expr);
      result += ");";
      return result;
    }

    private string AddSemicolonIfNeeded(string stringLiteral)
    {
      string lastChar = stringLiteral.Substring(stringLiteral.Length - 1);
      if (lastChar != ";" && lastChar != "\n")
      {
        return ";";
      }
      return "";
    }
  }
}