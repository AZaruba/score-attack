using Godot;
using Godot.Collections;

public partial class DebugLog : Control
{
  
  [Export] private Array<RichTextLabel> LogLines;

  public static DebugLog Instance;

  public override void _Ready()
  {
    Instance = this;
  }

  public static void Log(string Input, int line = 0)
  {
    Instance.LogLines[line].Text = Input;
  }
}
