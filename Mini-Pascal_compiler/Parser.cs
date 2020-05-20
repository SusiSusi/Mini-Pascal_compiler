using System;
using System.Collections.Generic;

namespace MiniPascalCompiler
{
  class Parser
  {
    public Lexer lexer { get; set; }
    public Token currentToken { get; set; }
    Error errorList;
    bool skipped; // if error was found and skipped to the next statement
    bool continueList; // if there is if - then statement without else

    public Parser(Lexer lexer, Error errorList)
    {
      this.lexer = lexer;
      this.currentToken = this.lexer.GetNextToken();
      this.errorList = errorList;
      this.skipped = false;
      this.continueList = false;
    }

    private void Error(string errorMessage, Token token)
    {
      string message = errorMessage + " -> " + token.ToString();
      this.errorList.AddError(new ParserError(message));
    }

    private AST Program()
    {
      Eat(TokenType.PROGRAM);
      Variable variableNode = Variable();
      string programName = variableNode.value;
      Eat(TokenType.SEMI);
      Block blockNode = Block();
      ProgramAST programNode = new ProgramAST(programName, blockNode);
      Eat(TokenType.DOT);
      return programNode;
    }

    private AST CompoundStatement()
    {
      Eat(TokenType.BEGIN);
      List<AST> nodes = StatementList();
      Eat(TokenType.END);

      Compound root = new Compound();
      foreach (AST node in nodes)
      {
        root.children.Add(node);
      }
      return root;
    }

    private Block Block()
    {
      List<AST> declarations = Declarations();
      AST compoundStatement = CompoundStatement();
      return new Block(declarations, compoundStatement);
    }

    private List<AST> Declarations()
    {
      List<AST> declarations = new List<AST>();

      while (true)
      {
        if (this.currentToken.type == TokenType.VAR)
        {
          declarations.Add(VariableDeclaration());
          Eat(TokenType.SEMI);
        }
        else if (this.currentToken.type == TokenType.PROCEDURE)
        {
          ProcedureDecl procDecl = ProcedureDeclaration();
          declarations.Add(procDecl);
        }
        else if (this.currentToken.type == TokenType.FUNCTION)
        {
          FunctionDecl funcDecl = FunctionDeclaration();
          declarations.Add(funcDecl);
        }
        else
        {
          break;
        }
      }
      return declarations;
    }

    private ProcedureDecl ProcedureDeclaration()
    {
      Eat(TokenType.PROCEDURE);
      string procName = (string)this.currentToken.value;
      Eat(TokenType.ID);

      List<Parameter> parameters = new List<Parameter>();
      if (this.currentToken.type == TokenType.LEFT_PARENTHESIS)
      {
        Eat(TokenType.LEFT_PARENTHESIS);
        parameters = FormalParameterList();
        Eat(TokenType.RIGHT_PARENTHESIS);
      }

      Eat(TokenType.SEMI);
      Block blockNode = Block();
      ProcedureDecl procDecl = new ProcedureDecl(procName, blockNode, parameters);
      Eat(TokenType.SEMI);
      return procDecl;
    }

    private FunctionDecl FunctionDeclaration()
    {
      Eat(TokenType.FUNCTION);
      string funcName = (string)this.currentToken.value;
      Eat(TokenType.ID);

      List<Parameter> parameters = new List<Parameter>();

      if (this.currentToken.type == TokenType.LEFT_PARENTHESIS)
      {
        Eat(TokenType.LEFT_PARENTHESIS);
        parameters = FormalParameterList();
        Eat(TokenType.RIGHT_PARENTHESIS);
      }
      Eat(TokenType.COLON);
      AST typeNode = null;
      if (this.currentToken.type == TokenType.ARRAY)
      {
        typeNode = Array();
      }
      else
      {
        typeNode = TypeSpec();
      }
      Eat(TokenType.SEMI);
      Block blockNode = Block();
      FunctionDecl funcDecl = new FunctionDecl(funcName, blockNode, typeNode, parameters);
      Eat(TokenType.SEMI);
      return funcDecl;
    }

    private Call CallStatement()
    {
      Token token = this.currentToken;
      string procName = (string)this.currentToken.value;
      Eat(TokenType.ID);
      Eat(TokenType.LEFT_PARENTHESIS);
      List<AST> actualParameters = new List<AST>();
      if (this.currentToken.type != TokenType.RIGHT_PARENTHESIS)
      {
        AST exprNode = Expr();
        actualParameters.Add(exprNode);
      }
      while (this.currentToken.type == TokenType.COMMA)
      {
        Eat(TokenType.COMMA);
        AST exprNode = Expr();
        actualParameters.Add(exprNode);
      }
      Eat(TokenType.RIGHT_PARENTHESIS);

      Call node = new Call(procName, actualParameters, token);
      return node;
    }

    private List<Parameter> FormalParameterList()
    {
      if (this.currentToken.type != TokenType.ID)
      {
        return new List<Parameter>();
      }

      List<Parameter> parameterNodes = FormalParameters();

      while (this.currentToken.type == TokenType.SEMI)
      {
        Eat(TokenType.SEMI);
        parameterNodes.AddRange(FormalParameters());
      }
      return parameterNodes;
    }

    private List<Parameter> FormalParameters()
    {
      List<Parameter> parameterNodes = new List<Parameter>();
      List<Token> parameterTokens = new List<Token>();
      parameterTokens.Add(this.currentToken);
      Eat(TokenType.ID);

      while (this.currentToken.type == TokenType.COMMA)
      {
        Eat(TokenType.COMMA);
        parameterTokens.Add(this.currentToken);
        Eat(TokenType.ID);
      }
      Eat(TokenType.COLON);
      AST typeNode = null;
      if (this.currentToken.type == TokenType.ARRAY)
      {
        typeNode = Array();
      }
      else
      {
        typeNode = TypeSpec();
      }

      foreach (Token parameterToken in parameterTokens)
      {
        Parameter parameterNode = new Parameter(new Variable(parameterToken), typeNode, parameterToken);
        parameterNodes.Add(parameterNode);
      }
      return parameterNodes;
    }

    private List<AST> StatementList()
    {
      List<AST> statements = new List<AST>();
      AST node = Statement();
      statements.Add(node);

      while (this.currentToken.type == TokenType.SEMI || this.continueList)
      {
        // if there is if - then statement without else
        if (this.continueList)
        {
          node = Statement();
          if (node != null)
          {
            statements.Add(node);
          }
          this.continueList = false;
        }
        else
        {
          Eat(TokenType.SEMI);
          node = Statement();
          if (node != null)
          {
            statements.Add(node);
          }
        }
      }

      if (this.currentToken.type == TokenType.ID)
      {
        Error("Token type can not be ID", this.currentToken);
      }
      return statements;
    }

    private AST Statement()
    {
      if (this.currentToken.type == TokenType.BEGIN)
      {
        return CompoundStatement();
      }
      else if (this.currentToken.type == TokenType.VAR)
      {
        return VariableDeclaration();
      }
      else if (this.currentToken.type == TokenType.ID &&
      this.lexer.currentChar == '(')
      {
        return CallStatement();
      }
      else if (this.currentToken.type == TokenType.ID)
      {
        return AssignmentStatement();
      }
      else if (this.currentToken.type == TokenType.WHILE)
      {
        return While();
      }
      else if (this.currentToken.type == TokenType.IF)
      {
        return If();
      }
      else if (this.currentToken.type == TokenType.READ)
      {
        return Read();
      }
      else if (this.currentToken.type == TokenType.RETURN)
      {
        return Return();
      }
      else if (this.currentToken.type == TokenType.WRITELN)
      {
        return Writeln();
      }
      else if (this.currentToken.type == TokenType.ASSERT)
      {
        return Assert();
      }
      else if (this.currentToken.type == TokenType.ERROR)
      {
        Eat(TokenType.ERROR);
        SkipToNextStatement();
        return Empty();
      }
      return Empty();
    }

    private Assignment AssignmentStatement()
    {
      Variable left = Variable();
      // if array type
      if (this.currentToken.type == TokenType.LEFT_BRACKET)
      {
        Eat(TokenType.LEFT_BRACKET);
        if (this.currentToken.type == TokenType.INTEGER_CONST)
        {
          left.expr = Expr();
        }
        Eat(TokenType.RIGHT_BRACKET);
      }
      Token token = this.currentToken;
      Eat(TokenType.ASSIGN);
      if (!skipped)
      {
        AST right = null;
        if (this.currentToken.type == TokenType.ID &&
         this.lexer.currentChar == '(')
        {
          right = CallStatement();
        }
        else
        {
          right = Expr();
        }
        Assignment node = new Assignment(left, token, right);
        return node;
      }
      this.skipped = false;
      return null;
    }

    private Variable Variable()
    {
      Variable node = new Variable(this.currentToken);
      Eat(TokenType.ID);
      return node;
    }

    private NoOperation Empty()
    {
      return new NoOperation();
    }

    private VariableDeclaration VariableDeclaration()
    {
      Eat(TokenType.VAR);
      List<Variable> variableNodes = new List<Variable>();
      variableNodes.Add(new Variable(this.currentToken)); // First ID
      Eat(TokenType.ID);
      AST typeNode = null;

      while (this.currentToken.type == TokenType.COMMA)
      {
        Eat(TokenType.COMMA);
        variableNodes.Add(new Variable(this.currentToken));
        Eat(TokenType.ID);
      }
      if (variableNodes[0].token.type != TokenType.ERROR)
      {
        Eat(TokenType.COLON);
      }
      if (!skipped)
      {
        if (this.currentToken.type == TokenType.ARRAY)
        {
          typeNode = Array();
        }
        else
        {
          typeNode = TypeSpec();
        }
      }
      if (skipped)
      {
        skipped = false;
        Token token = variableNodes[0].token;
        if (token.type != TokenType.ERROR)
        {
          token.type = TokenType.ERROR;
        }
        return new VariableDeclaration(variableNodes, new Type(token));
      }
      skipped = false;
      return new VariableDeclaration(variableNodes, typeNode);
    }

    private Type TypeSpec()
    {
      Token token = this.currentToken;
      if (this.currentToken.type == TokenType.INTEGER)
      {
        Eat(TokenType.INTEGER);
      }
      else if (this.currentToken.type == TokenType.REAL)
      {
        Eat(TokenType.REAL);
      }
      else if (this.currentToken.type == TokenType.STRING)
      {
        Eat(TokenType.STRING);
      }
      else if (this.currentToken.type == TokenType.BOOLEAN)
      {
        Eat(TokenType.BOOLEAN);
      }
      else
      {
        Error("Invalid token type", this.currentToken);
        return null;
      }
      return new Type(token);
    }

    private While While()
    {
      Eat(TokenType.WHILE);
      AST expr = Expr();
      Eat(TokenType.DO);
      AST statement = Statement();
      return new While(expr, statement);
    }

    private If If()
    {
      Eat(TokenType.IF);
      AST expr = Expr();
      Eat(TokenType.THEN);
      AST statement = Statement();
      Eat(TokenType.SEMI);
      if (this.currentToken.type == TokenType.ELSE)
      {
        Eat(TokenType.ELSE);
        AST statementElse = Statement();
        return new If(expr, statement, statementElse);
      }
      this.continueList = true;
      return new If(expr, statement);
    }

    private Assert Assert()
    {
      Token token = this.currentToken;
      Eat(TokenType.ASSERT);
      Eat(TokenType.LEFT_PARENTHESIS);
      AST expr = Expr();
      Eat(TokenType.RIGHT_PARENTHESIS);
      return new Assert(expr);
    }

    private Return Return()
    {
      Eat(TokenType.RETURN);
      if (this.currentToken.type == TokenType.SEMI)
      {
        return new Return();
      }
      else
      {
        return new Return(Expr());
      }
    }

    private Array Array()
    {
      Eat(TokenType.ARRAY);
      Eat(TokenType.LEFT_BRACKET);
      AST expr = null;
      if (this.currentToken.type != TokenType.RIGHT_BRACKET)
      {
        expr = Expr();
      }
      Eat(TokenType.RIGHT_BRACKET);
      Eat(TokenType.OF);
      Type type = TypeSpec();
      return new Array(type, expr);
    }

    private Writeln Writeln()
    {
      Eat(TokenType.WRITELN);
      Eat(TokenType.LEFT_PARENTHESIS);
      List<AST> exprs = new List<AST>();
      AST expr = Expr();
      exprs.Add(expr); // Firts expr
      while (this.currentToken.type == TokenType.COMMA)
      {
        Eat(TokenType.COMMA);
        expr = Expr();
        exprs.Add(expr);
      }
      if (skipped)
      {
        skipped = false;
        Eat(TokenType.SEMI);
        return new Writeln(exprs);
      }
      Eat(TokenType.RIGHT_PARENTHESIS);
      return new Writeln(exprs);
    }

    private Read Read()
    {
      Token token = this.currentToken;
      Eat(TokenType.READ);
      Eat(TokenType.LEFT_PARENTHESIS);

      List<Variable> variableNodes = new List<Variable>();
      Variable variable = (Variable)Factor();
      variableNodes.Add(variable); // First ID
      while (this.currentToken.type == TokenType.COMMA)
      {
        Eat(TokenType.COMMA);
        variable = (Variable)Factor();
        variableNodes.Add(variable);
      }
      Eat(TokenType.RIGHT_PARENTHESIS);
      return new Read(variableNodes);
    }

    // Compares the current token type and the given token type. If they match,
    // "eat" the current token and place the next token in the this.currentToken
    private void Eat(TokenType tokenType)
    {
      if (this.currentToken.type == tokenType)
      {
        this.currentToken = this.lexer.GetNextToken();
      }
      else
      {
        Error("Unexpected token", this.currentToken);
        SkipToNextStatement();
      }
    }

    private void SkipToNextStatement()
    {
      this.skipped = true;
      // skip to the next statement
      while (this.currentToken.type != TokenType.SEMI && this.currentToken.type != TokenType.EOF)
      {
        this.currentToken = this.lexer.GetNextToken();
      }
    }

    private AST Factor()
    {
      Token token = this.currentToken;
      if (token.type == TokenType.PLUS)
      {
        Eat(TokenType.PLUS);
        return new UnaryOperation(token, Factor());
      }
      else if (token.type == TokenType.MINUS)
      {
        Eat(TokenType.MINUS);
        return new UnaryOperation(token, Factor());
      }
      else if (token.type == TokenType.INTEGER_CONST)
      {
        Eat(TokenType.INTEGER_CONST);
        return new IntegerAST(token);
      }
      else if (token.type == TokenType.REAL_CONST)
      {
        Eat(TokenType.REAL_CONST);
        return new RealAST(token);
      }
      else if (token.type == TokenType.STRING)
      {
        Eat(TokenType.STRING);
        return new StringAST(token);
      }
      else if (token.type == TokenType.BOOLEAN)
      {
        Eat(TokenType.BOOLEAN);
        return new BooleanAST(token);
      }
      else if (token.type == TokenType.TRUE)
      {
        Eat(TokenType.TRUE);
        return new True(token);
      }
      else if (token.type == TokenType.FALSE)
      {
        Eat(TokenType.FALSE);
        return new False(token);
      }
      else if (token.type == TokenType.ERROR)
      {
        Eat(TokenType.ERROR);
        SkipToNextStatement();
        return Empty();
      }
      else if (token.type == TokenType.LEFT_PARENTHESIS)
      {
        Eat(TokenType.LEFT_PARENTHESIS);
        AST node = Expr();
        Eat(TokenType.RIGHT_PARENTHESIS);
        return node;
      }
      else if (token.type == TokenType.NOT)
      {
        Eat(TokenType.NOT);
        return Factor();
      }
      else if (this.currentToken.type == TokenType.ID &&
      this.lexer.currentChar == '.')
      {
        Variable node = Variable();
        Eat(TokenType.DOT);
        Eat(TokenType.SIZE);
        node.size = true;
        return node;
      }
      else if (this.currentToken.type == TokenType.ID &&
      this.lexer.currentChar == '(')
      {
        return CallStatement();
      }
      else
      {
        Variable node = Variable();
        // if array
        if (this.currentToken.type == TokenType.LEFT_BRACKET)
        {
          Eat(TokenType.LEFT_BRACKET);
          if (this.currentToken.type == TokenType.INTEGER_CONST)
          {
            node.expr = Expr();
          }
          Eat(TokenType.RIGHT_BRACKET);
        }
        return node;
      }
    }

    private AST Term()
    {
      AST node = Factor();
      while (this.currentToken.type == TokenType.MUL ||
            this.currentToken.type == TokenType.DIV ||
            this.currentToken.type == TokenType.MODULO ||
            this.currentToken.type == TokenType.AND)
      {
        Token token = this.currentToken;
        if (token.type == TokenType.MUL)
        {
          Eat(TokenType.MUL);
        }
        else if (token.type == TokenType.DIV)
        {
          Eat(TokenType.DIV);
        }
        else if (token.type == TokenType.MODULO)
        {
          Eat(TokenType.MODULO);
        }
        else if (token.type == TokenType.AND)
        {
          Eat(TokenType.AND);
        }
        node = new BinaryOperation(node, token, Factor());
      }
      return node;
    }

    private AST Expr()
    {
      AST node = Term();
      while (this.currentToken.type == TokenType.PLUS ||
            this.currentToken.type == TokenType.MINUS ||
            this.currentToken.type == TokenType.EQUAL ||
            this.currentToken.type == TokenType.LESS ||
            this.currentToken.type == TokenType.INEQUALITY ||
            this.currentToken.type == TokenType.LESS_OR_EQUAL ||
            this.currentToken.type == TokenType.GREATER_OR_EQUAL ||
            this.currentToken.type == TokenType.GREATER ||
            this.currentToken.type == TokenType.OR)
      {
        Token token = this.currentToken;
        if (token.type == TokenType.PLUS)
        {
          Eat(TokenType.PLUS);
        }
        else if (token.type == TokenType.MINUS)
        {
          Eat(TokenType.MINUS);
        }
        else if (token.type == TokenType.EQUAL)
        {
          Eat(TokenType.EQUAL);
        }
        else if (token.type == TokenType.LESS)
        {
          Eat(TokenType.LESS);
        }
        else if (token.type == TokenType.INEQUALITY)
        {
          Eat(TokenType.INEQUALITY);
        }
        else if (token.type == TokenType.LESS_OR_EQUAL)
        {
          Eat(TokenType.LESS_OR_EQUAL);
        }
        else if (token.type == TokenType.GREATER_OR_EQUAL)
        {
          Eat(TokenType.GREATER_OR_EQUAL);
        }
        else if (token.type == TokenType.GREATER)
        {
          Eat(TokenType.GREATER);
        }
        else if (token.type == TokenType.OR)
        {
          Eat(TokenType.OR);
        }
        node = new BinaryOperation(node, token, Term());
      }
      return node;
    }

    public AST Parse()
    {
      AST node = Program();
      if (this.currentToken.type != TokenType.EOF)
      {
        Error("Expected EOF token type but token type is", this.currentToken);
      }
      return node;
    }
  }
}