using System.Diagnostics;
using Godot;
using StateManagement;

public partial class AnimatingBodyComponent : Node
{
  [Export] AnimationTree AnimationTreeComponent;
  [Export] AnimationPlayer AnimationPlayerComponent;
  [Export] bool AnimationTransitionReady;

  [Signal] public delegate void OnCurrentAnimationFinishedEventHandler();
  public bool AnimationQueued;

  private bool IsPlayingLockedAnimation;
  private StateID CurrentState;

  public void DEBUGLogState(int idx)
  {
    DebugLog.Log(CurrentState.ToString(), idx);
  }

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

  public void DirectAnimationPlay(string AnimationName)
  {
    AnimationPlayerComponent.Play(AnimationName);
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

  public void ResetAnimationQueue()
  {
    AnimationQueued = false;
  }

  public void TravelTo(string stateName)
  {
    AnimationNodeStateMachinePlayback animSM = (AnimationNodeStateMachinePlayback)(GodotObject)AnimationTreeComponent.Get("parameters/playback");
    animSM.Travel(stateName);
  }

  private void OnAnimationFinished(StringName AnimationName)
  {
    EmitSignal(SignalName.OnCurrentAnimationFinished);
  }
}
