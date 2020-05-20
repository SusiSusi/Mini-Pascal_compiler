namespace MiniPascalCompiler
{
  enum TokenType
  {
    // Relational operators
    EQUAL,
    INEQUALITY,
    LESS,
    LESS_OR_EQUAL,
    GREATER_OR_EQUAL,
    GREATER,

    // Negation
    NOT,

    // Adding operators
    PLUS,
    MINUS,
    OR,

    // Multiplying operators
    MUL,
    DIV,
    MODULO,
    AND,

    // Special symbols and keywords
    LEFT_PARENTHESIS,
    RIGHT_PARENTHESIS,
    LEFT_BRACKET,
    RIGHT_BRACKET,
    ASSIGN,
    DOT,
    COMMA,
    SEMI,
    COLON,
    IF,
    THEN,
    ELSE,
    OF,
    WHILE,
    DO,
    BEGIN,
    END,
    VAR,
    ARRAY,
    PROCEDURE,
    FUNCTION,
    PROGRAM,
    ASSERT,
    RETURN,

    // Predefined id
    BOOLEAN,
    FALSE,
    INTEGER,
    READ,
    REAL,
    SIZE,
    STRING,
    TRUE,
    WRITELN,

    // Other
    INTEGER_CONST,
    REAL_CONST,
    EOF,
    ID,
    ERROR
  }

  class Token
  {
    public TokenType type { get; set; }
    public object value { get; set; }
    public int? lineNo { get; set; }
    public int? columnNo { get; set; }

    public Token(TokenType type, object value, int? lineNo = null, int? columnNo = null)
    {
      this.type = type;
      this.value = value;
      this.lineNo = lineNo;
      this.columnNo = columnNo;
    }

    public override string ToString()
    {
      return "Token(" + this.type + ", " + this.value + ", position=" + this.lineNo
      + ":" + this.columnNo + ")";
    }
  }
}