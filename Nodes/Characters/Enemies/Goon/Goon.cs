using Godot;
using Godot.Collections;
using System;

public partial class Goon : CharacterBody3D
{
  [Export] NavigationAgent3D NavigationAgent;
  [Export] VelocityComponent VelocityComponent;
  [Export] GravityComponent GravityComponent;
  [Export] CharacterStats Stats;

  private Vector3 NextTarget;
  private bool IsMovingToLink = false;
  private bool Arrived = false;

  public override void _Ready()
  {
    NavigationAgent.TargetPosition = new Vector3(0,0,0);
  }

  public override void _PhysicsProcess(double delta)
  {
    if (Arrived)
    {
      return;
    }

    GravityComponent.AddGravity(Stats.Gravity * (float)delta, IsOnFloor());
    if (IsMovingToLink)
    {
      // hmm
      if (IsOnFloor())
      {
        GravityComponent.ApplyVerticalForce(Stats.JumpForce);
        IsMovingToLink = false;
      }
    }
    else
    {
      NextTarget = NavigationAgent.GetNextPathPosition();
    }

    Vector3 FlatVelocity = GetRealVelocity();
    FlatVelocity.Y = 0;
    VelocityComponent.SetVelocity(FlatVelocity);
    Vector3 Direction = NextTarget - Position;
    Direction.Y = 0;

    VelocityComponent.AddForce(Direction.Normalized() * Stats.MoveAcceleration);
    VelocityComponent.CapVelocity(Stats.MoveSpeed);

    Velocity = VelocityComponent.GetCurrentVelocity() + GravityComponent.GetVerticalVelocity();
    MoveAndSlide();
  }

  private void OnLinkReached(Dictionary details)
  {
    IsMovingToLink = true;
    NextTarget = (Vector3)details["link_exit_position"];
  }

  private void OnTargetReached()
  {
    // Change to arrived state
    Arrived = true;
  }
}
