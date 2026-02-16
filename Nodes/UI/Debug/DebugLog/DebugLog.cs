using Godot;
using Godot.Collections;

public partial class DebugLog : Control
{
  
  [Export] private Array<RichTextLabel> LogLines;
  [Export] private RichTextLabel TimerLabel;
  [Export] private Timer Timer;

  public static DebugLog Instance;

  // TODO add "Timer message"

  public override void _Ready()
  {
    Instance = this;
  }

  public static void Log(string Input, int line = 0)
  {
    Instance.LogLines[line].Text = Input;
  }

  public static void LogTemp(string Input, float length)
  {
    Instance.Timer.Start(length);
    Instance.TimerLabel.Text = Input;
  }

  public void OnTimeout()
  {
    Instance.TimerLabel.Text = "";
  }
}
