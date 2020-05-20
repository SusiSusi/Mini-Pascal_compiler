using System.Reflection;

namespace MiniPascalCompiler
{
  abstract class NodeVisitor
  {
    public object Visit(AST node)
    {
      string methodName = "Visit" + node.GetType().Name;
      MethodInfo method = this.GetType().GetMethod(methodName);
      
      try 
      { 
        return method.Invoke(this, new object[] { node }); 
      }
      catch (TargetInvocationException e) 
      { 
        throw e.InnerException; 
      }

    }
  }
}