using System.Collections.Generic;

namespace MiniPascalCompiler
{
  enum ARType
  {
    PROGRAM,
    PROCEDURE,
    FUNCTION
  }
  class ActivationRecord
  {
    public string name { get; set; }
    public ARType type { get; set; }
    public int nestingLevel { get; set; }
    public Dictionary<object, object> members { get; set; }

    public ActivationRecord(string name, ARType type, int nestingLevel)
    {
      this.name = name;
      this.type = type;
      this.nestingLevel = nestingLevel;
      this.members = new Dictionary<object, object>();
    }

    public void SetItem(object key, object value)
    {
      this.members[key] = value;
    }

    public string GetArraysType(object key)
    {
      if (this.members.ContainsKey(key))
      {
        return this.members[key].GetType().Name;
      }
      return null;
    }

    public object Get(object key)
    {
      if (this.members.ContainsKey(key))
      {
        return this.members[key];
      }
      return null;
    }

    public override string ToString()
    {
      List<string> lines = new List<string>();
      lines.Add(this.nestingLevel + ": " + this.type + " " + this.name);

      foreach (KeyValuePair<object, object> kvp in this.members)
      {
        lines.Add("  " + kvp.Key + ":            " + kvp.Value);
      }
      return string.Join('\n', lines);
    }
  }
}