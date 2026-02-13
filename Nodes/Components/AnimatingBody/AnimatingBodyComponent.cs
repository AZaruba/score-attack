using System.Diagnostics;
using Godot;
using StateManagement;

public partial class AnimatingBodyComponent : Node
{
  [Export] AnimationTree AnimationTreeComponent;
  [Export] bool AnimationTransitionReady;

  [Signal] public delegate void OnCurrentAnimationFinishedEventHandler();
  public bool AnimationQueued;

  private bool IsPlayingLockedAnimation;
  private StateID CurrentState;

  public override void _Ready()
  {
    AnimationTreeComponent.AdvanceExpressionBaseNode = GetPath();
  }

  public override void _PhysicsProcess(double delta)
  {
  }

  public void PlayAnimationLoop(string AnimationName)
  {
    
  }

  public void SetAnimationParameter(string Path, float Value)
  {
    AnimationTreeComponent.Set(Path, Value);
  }

  // somewhat hacky solution to cleanly pass this information to the AnimationTree
  public void SetCurrentState(StateID state)
  {
    CurrentState = state;
  }

  // should allow for cancelable animation override?
  public void QueueAnimation()
  {
    if (AnimationTransitionReady)
    {
      AnimationQueued = true;
    }
  }

  private void OnAnimationFinished(StringName AnimationName)
  {
    AnimationQueued = false;
    EmitSignal(SignalName.OnCurrentAnimationFinished);
  }
}
