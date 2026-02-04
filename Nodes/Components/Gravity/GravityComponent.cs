using Godot;
using System;

public partial class GravityComponent : Node
{
  private Vector3 CurrentVerticalVelocity;

  [Export] private float Gravity;

  public override void _Ready()
  {
    
  }

  public override void _PhysicsProcess(double delta)
  {
  }
  
  public void AddGravity(float GravityForce, bool IsOnFloor)
  {
    Vector3 Result = Vector3.Zero;
    if (!IsOnFloor)
    {
      Result = CurrentVerticalVelocity + Vector3.Down * GravityForce;
    }
    CurrentVerticalVelocity = Result;
  }

  public void ApplyVerticalForce(float Value)
  {
    CurrentVerticalVelocity += Vector3.Up * Value;
  }

  public Vector3 GetVerticalVelocity() { return CurrentVerticalVelocity; }
}
