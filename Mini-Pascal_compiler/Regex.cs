using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniPascalCompiler
{
  class Regex
  {
    public Dictionary<string, Token> reservedKeywords { get; set; }
    public Dictionary<char, char> characterEscapes { get; set; }
    public Dictionary<char, TokenType> tokenTypes { get; set; }
    public char[] letters { get; set; }
    public char[] digits { get; set; }

    public Regex()
    {
      this.reservedKeywords = new Dictionary<string, Token>();
      this.characterEscapes = new Dictionary<char, char>();
      this.tokenTypes = new Dictionary<char, TokenType>();
      this.letters = SetLetters();
      this.digits = SetDigits();
      SetReserverdKeywords();
      SetCharacterEscapes();
      SetTokenTypes();
    }

    private void SetReserverdKeywords()
    {
      this.reservedKeywords.Add("var", new Token(TokenType.VAR, "var"));
      this.reservedKeywords.Add("while", new Token(TokenType.WHILE, "while"));
      this.reservedKeywords.Add("end", new Token(TokenType.END, "end"));
      this.reservedKeywords.Add("do", new Token(TokenType.DO, "do"));
      this.reservedKeywords.Add("read", new Token(TokenType.READ, "read"));
      this.reservedKeywords.Add("writeln", new Token(TokenType.WRITELN, "writeln"));
      this.reservedKeywords.Add("integer", new Token(TokenType.INTEGER, "integer"));
      this.reservedKeywords.Add("string", new Token(TokenType.STRING, "string"));
      this.reservedKeywords.Add("boolean", new Token(TokenType.BOOLEAN, "Boolean"));
      this.reservedKeywords.Add("true", new Token(TokenType.TRUE, "true"));
      this.reservedKeywords.Add("false", new Token(TokenType.FALSE, "false"));
      this.reservedKeywords.Add("assert", new Token(TokenType.ASSERT, "assert"));
      this.reservedKeywords.Add("and", new Token(TokenType.AND, "and"));
      this.reservedKeywords.Add("not", new Token(TokenType.NOT, "not"));
      this.reservedKeywords.Add("or", new Token(TokenType.OR, "or"));
      this.reservedKeywords.Add("begin", new Token(TokenType.BEGIN, "begin"));
      this.reservedKeywords.Add("return", new Token(TokenType.RETURN, "return"));
      this.reservedKeywords.Add("program", new Token(TokenType.PROGRAM, "program"));
      this.reservedKeywords.Add("real", new Token(TokenType.REAL, "real"));
      this.reservedKeywords.Add("procedure", new Token(TokenType.PROCEDURE, "procedure"));
      this.reservedKeywords.Add("function", new Token(TokenType.FUNCTION, "function"));
      this.reservedKeywords.Add("if", new Token(TokenType.IF, "if"));
      this.reservedKeywords.Add("then", new Token(TokenType.THEN, "then"));
      this.reservedKeywords.Add("else", new Token(TokenType.ELSE, "else"));
      this.reservedKeywords.Add("array", new Token(TokenType.ARRAY, "array"));
      this.reservedKeywords.Add("of", new Token(TokenType.OF, "of"));
      this.reservedKeywords.Add("size", new Token(TokenType.SIZE, "size"));
    }

    private void SetCharacterEscapes()
    {
      this.characterEscapes.Add('\\', '\\'); // backslash
      this.characterEscapes.Add('a', '\a'); // alert
      this.characterEscapes.Add('b', '\b'); // backspace
      this.characterEscapes.Add('f', '\f'); // form feed
      this.characterEscapes.Add('n', '\n'); // new line
      this.characterEscapes.Add('r', '\r'); // carriage return
      this.characterEscapes.Add('t', '\t'); // horizontal tab
      this.characterEscapes.Add('v', '\v'); // vertical quote
    }

    private void SetTokenTypes()
    {
      this.tokenTypes.Add('-', TokenType.MINUS);
      this.tokenTypes.Add('+', TokenType.PLUS);
      this.tokenTypes.Add('*', TokenType.MUL);
      this.tokenTypes.Add('/', TokenType.DIV);
      this.tokenTypes.Add('(', TokenType.LEFT_PARENTHESIS);
      this.tokenTypes.Add(')', TokenType.RIGHT_PARENTHESIS);
      this.tokenTypes.Add('[', TokenType.LEFT_BRACKET);
      this.tokenTypes.Add(']', TokenType.RIGHT_BRACKET);
      this.tokenTypes.Add(';', TokenType.SEMI);
      this.tokenTypes.Add(':', TokenType.COLON);
      this.tokenTypes.Add('.', TokenType.DOT);
      this.tokenTypes.Add(',', TokenType.COMMA);
      this.tokenTypes.Add('<', TokenType.LESS);
      this.tokenTypes.Add('>', TokenType.GREATER);
      this.tokenTypes.Add('=', TokenType.EQUAL);
      this.tokenTypes.Add('%', TokenType.MODULO);
    }

    private char[] SetLetters()
    {
      char[] alpha = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
      return alpha;
    }

    private char[] SetDigits()
    {
      char[] digits = "0123456789".ToCharArray();
      return digits;
    }
  }
}