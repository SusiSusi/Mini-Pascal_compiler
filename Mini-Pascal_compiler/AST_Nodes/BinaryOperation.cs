namespace MiniPascalCompiler
{
  class BinaryOperation : AST
  {
    public AST left { get; set; } // left node
    public Token operat { get; set; } // operator (+, -, *, /)
    public AST right { get; set; } // right node 

    // operates two operands
    public BinaryOperation(AST left, Token operat, AST right)
    {
      this.left = left;
      this.operat = operat;
      this.right = right;
    }
  }
}