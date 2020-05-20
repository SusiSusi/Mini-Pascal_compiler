using System;
using System.Collections.Generic;
using System.Text;

namespace MiniPascalCompiler
{
  class Lexer
  {
    string text;
    int pos;
    public char currentChar { get; set; }
    int lineNo;
    int columnNo;
    Dictionary<string, Token> reservedKeywords;
    Dictionary<char, char> characterEscapes;
    Dictionary<char, TokenType> tokenTypes;
    char[] letters;
    char[] digits;
    Error errorList;

    public Lexer(string text, Error errorList)
    {
      this.text = text; // string input
      this.pos = 0; // index into this.text
      this.currentChar = this.text[this.pos];
      // token line number and column number
      this.lineNo = 1;
      this.columnNo = 1;

      this.errorList = errorList;
      Regex regex = new Regex();
      this.reservedKeywords = regex.reservedKeywords;
      this.characterEscapes = regex.characterEscapes;
      this.tokenTypes = regex.tokenTypes;
      this.letters = regex.letters;
      this.digits = regex.digits;

      // Ensures that floating numbers are separated by a dot
      System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
      customCulture.NumberFormat.NumberDecimalSeparator = ".";
      System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
    }

    private void Error()
    {
      string message = "Invalid character: " + this.currentChar + ", position=" + this.lineNo
      + ":" + this.columnNo;
      this.errorList.AddError(new LexerError(message));
    }

    // Takes care of identifiers and reserved keywords
    private Token SetId()
    {
      string result = "";
      int line = 0;
      int column = 0;
      while (this.currentChar != '\0' && Char.IsLetterOrDigit(this.currentChar))
      {
        bool validChar = false;
        foreach (var letter in this.letters)
        {
          if (this.currentChar == letter)
          {
            validChar = true;
          }
        }
        foreach (var digit in this.digits)
        {
          if (this.currentChar == digit)
          {
            validChar = true;
          }
        }
        line = this.lineNo;
        column = this.columnNo;
        if (validChar)
        {
          result += this.currentChar;
        }
        else
        {
          Error();
          while (this.currentChar != '\0' && Char.IsLetterOrDigit(this.currentChar))
          {
            result += this.currentChar;
            ReadNextChar();
          }
          line = this.lineNo;
          column = this.columnNo;
          return new Token(TokenType.ERROR, result, line, column);
        }
        ReadNextChar();
      }
      if (this.currentChar == '_')
      {
        result += this.currentChar;
        ReadNextChar();
      }
      if (this.reservedKeywords.ContainsKey(result.ToLower()))
      {
        Token token = this.reservedKeywords[result.ToLower()];
        token.lineNo = line;
        token.columnNo = column;
        return token;
      }
      return new Token(TokenType.ID, result, line, column);
    }

    // Moves the this.pos pointer and sets the this.currentChar
    private void ReadNextChar()
    {
      if (this.currentChar == '\n')
      {
        this.lineNo += 1;
        this.columnNo = 0;
      }

      this.pos += 1;
      if (this.pos > this.text.Length - 1)
      {
        this.currentChar = '\0'; // end of input
      }
      else
      {
        this.currentChar = this.text[this.pos];
        this.columnNo += 1;
      }
    }

    // Returns the next character from the this.text without adding this.pos value
    private char Peek()
    {
      int peekPos = this.pos + 1;
      if (peekPos > this.text.Length - 1)
      {
        return '\0';
      }
      else
      {
        return this.text[peekPos];
      }
    }

    private void SkipWhitespace()
    {
      while (this.currentChar != '\0' && Char.IsWhiteSpace(this.currentChar))
      {
        ReadNextChar();
      }
    }

    private void SkipCommentOneLine()
    {
      while (this.currentChar != '\0' && this.currentChar != '}')
      {
        ReadNextChar();
      }
      ReadNextChar(); // closing }
    }

    private void SkipCommentMultipleLines()
    {
      int nested = 1;
      while (this.currentChar != '\0' && nested > 0)
      {
        if (this.currentChar == '*' && Peek() == '}')
        {
          ReadNextChar();
          ReadNextChar();
          nested -= 1;
        }
        if (this.currentChar == '{' && Peek() == '*')
        {
          ReadNextChar();
          ReadNextChar();
          nested += 1;
        }
        ReadNextChar();
      }

      if (nested > 0)
      {
        this.errorList.AddError(new LexerError("Missing comment closure: [" + this.lineNo + ":" + this.columnNo + "]"));
      }
    }

    // Returns a possible multi-digit integer or float from the input
    private Token Number()
    {
      string result = "";
      Token token;
      while (this.currentChar != '\0' && Char.IsDigit(this.currentChar))
      {
        result += this.currentChar;
        ReadNextChar();
      }

      if (this.currentChar == '.')
      {
        result += this.currentChar;
        ReadNextChar();

        while (this.currentChar != '\0' && Char.IsDigit(this.currentChar))
        {
          result += this.currentChar;
          ReadNextChar();
        }
        token = new Token(TokenType.REAL_CONST, float.Parse(result), this.lineNo, this.columnNo);
      }
      else
      {
        token = new Token(TokenType.INTEGER_CONST, int.Parse(result), this.lineNo, this.columnNo);
      }
      return token;
    }

    // Returns a string literals from the input
    private Token StringLiteralToken()
    {
      StringBuilder builder = new StringBuilder();
      while (this.currentChar != '"')
      {
        if (this.currentChar == '\n')
        {
          this.errorList.AddError(new LexerError("End of input while scanning a string literal"));
          return new Token(TokenType.ERROR, builder.ToString(), this.lineNo, this.columnNo);
        }
        if (this.currentChar == '\\')
        {
          if (this.characterEscapes.ContainsKey(Peek()))
          {
            builder.Append(this.characterEscapes[Peek()]);
            ReadNextChar();
            ReadNextChar();
          }
          else
          {
            builder.Append(this.currentChar); // it is not a escape character
            ReadNextChar();
          }
          continue;
        }
        builder.Append(this.currentChar);
        ReadNextChar();
      }
      ReadNextChar();
      return new Token(TokenType.STRING, builder.ToString(), this.lineNo, this.columnNo);
    }

    // Responsible for breaking a sentence into tokens, one token at a time
    public Token GetNextToken()
    {
      while (this.currentChar != '\0')
      {
        if (Char.IsWhiteSpace(this.currentChar))
        {
          SkipWhitespace();
          continue;
        }
        if (this.currentChar == '{' && Peek() == '*')
        {
          ReadNextChar();
          ReadNextChar();
          SkipCommentMultipleLines();
          continue;
        }
        if (this.currentChar == '{')
        {
          ReadNextChar();
          SkipCommentOneLine();
          continue;
        }
        if (this.currentChar == '"')
        {
          ReadNextChar();
          return StringLiteralToken();
        }
        if (this.currentChar == ':' && Peek() == '=')
        {
          Token token = new Token(TokenType.ASSIGN, ":=", this.lineNo, this.columnNo);
          ReadNextChar();
          ReadNextChar();
          return token;
        }
        if (this.currentChar == '<' && Peek() == '>')
        {
          Token token = new Token(TokenType.INEQUALITY, "<>", this.lineNo, this.columnNo);
          ReadNextChar();
          ReadNextChar();
          return token;
        }
        if (this.currentChar == '<' && Peek() == '=')
        {
          Token token = new Token(TokenType.LESS_OR_EQUAL, "<=", this.lineNo, this.columnNo);
          ReadNextChar();
          ReadNextChar();
          return token;
        }
        if (this.currentChar == '>' && Peek() == '=')
        {
          Token token = new Token(TokenType.GREATER_OR_EQUAL, ">=", this.lineNo, this.columnNo);
          ReadNextChar();
          ReadNextChar();
          return token;
        }
        if (Char.IsLetter(this.currentChar))
        {
          return SetId();
        }
        if (Char.IsDigit(this.currentChar))
        {
          return Number();
        }
        // single-character token
        object tokenType = null;
        object tokenTypeValue = null;

        foreach (KeyValuePair<char, TokenType> kvp in this.tokenTypes)
        {
          if (kvp.Key == this.currentChar)
          {
            tokenType = kvp.Value;
            tokenTypeValue = kvp.Key;
            break;
          }
        }

        if (tokenType != null && tokenTypeValue != null)
        {
          Token token = new Token((TokenType)tokenType, tokenTypeValue, this.lineNo, this.columnNo);
          ReadNextChar();
          return token;
        }
        else
        {
          Error();
          ReadNextChar();
        }
      }
      // no more input left, the last token is EOF
      return new Token(TokenType.EOF, '\0', this.lineNo, this.columnNo);
    }
  }
}