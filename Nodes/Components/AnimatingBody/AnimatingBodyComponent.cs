using Godot;
using System;

public partial class AnimatingBodyComponent : Node
{
  [Export] AnimationPlayer AnimPlayer;

  private bool IsPlayingLockedAnimation;

  public override void _Ready()
  {
    
  }

  public override void _PhysicsProcess(double delta)
  {
    DebugLog.Log($"Animation Locked: {IsPlayingLockedAnimation}", 2);
  }

  public void PlayAnimationLoop(string AnimationName)
  {
    if (!IsPlayingLockedAnimation)
    {
      AnimPlayer.Play(AnimationName);
    }
  }

  public void PlayOneShot(string AnimationName, bool Cancelable = false)
  {
    AnimPlayer.Play(AnimationName);
  }

  public void OnAnimFinished(string AnimationName)
  {
    IsPlayingLockedAnimation = false;
  }

  public string GetCurrentAnimation()
  {
    return AnimPlayer.CurrentAnimation;
  }
}
