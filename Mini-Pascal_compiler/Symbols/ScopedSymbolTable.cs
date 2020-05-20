using System.Collections.Generic;
using System;

namespace MiniPascalCompiler
{
  class ScopedSymbolTable
  {
    public Dictionary<object, Symbol> symbols { get; set; }
    public object scopeName { get; set; }
    public int scopeLevel { get; set; }
    public ScopedSymbolTable enclosingScope { get; set; }

    public ScopedSymbolTable(object scopeName, int scopeLevel, ScopedSymbolTable enclosingScope = null)
    {
      this.symbols = new Dictionary<object, Symbol>();
      this.scopeName = scopeName;
      this.scopeLevel = scopeLevel;
      this.enclosingScope = enclosingScope;
      InitBuiltins();
    }

    private void InitBuiltins()
    {
      Define(new BuiltinTypeSymbol("Boolean", SymbolCategory.BUILTIN));
      Define(new BuiltinTypeSymbol("false", SymbolCategory.BUILTIN));
      Define(new BuiltinTypeSymbol("integer", SymbolCategory.BUILTIN));
      Define(new BuiltinTypeSymbol("read", SymbolCategory.BUILTIN));
      Define(new BuiltinTypeSymbol("real", SymbolCategory.BUILTIN));
      Define(new BuiltinTypeSymbol("size", SymbolCategory.BUILTIN));
      Define(new BuiltinTypeSymbol("string", SymbolCategory.BUILTIN));
      Define(new BuiltinTypeSymbol("true", SymbolCategory.BUILTIN));
      Define(new BuiltinTypeSymbol("writeln", SymbolCategory.BUILTIN));
      Define(new BuiltinTypeSymbol("array", SymbolCategory.BUILTIN));
    }

    public void Define(Symbol symbol)
    {
      symbol.scopeLevel = this.scopeLevel;
      this.symbols[symbol.name] = symbol;
    }

    // responsible for searching symbol by name
    public Symbol Lookup(object name, bool currentScopeOnly = false)
    {
      if (this.symbols.ContainsKey(name))
      {
        return this.symbols[name];
      }
      if (currentScopeOnly)
      {
        return null;
      }
      if (this.enclosingScope != null)
      {
        return this.enclosingScope.Lookup(name);
      }
      return null;
    }

    public override string ToString()
    {
      return "SCOPE: name - level : " + this.scopeName + " - " + this.scopeLevel +
      "\n enclosing: " + this.enclosingScope;
    }
  }
}