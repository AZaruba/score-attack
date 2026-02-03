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

  public void SetGravity(Vector3 Input)
  {
    CurrentVerticalVelocity = Input;
  }

  public void ApplyVerticalForce(float Value)
  {
    CurrentVerticalVelocity += Vector3.Up * Value;
  }

  public Vector3 GetVerticalVelocity() { return CurrentVerticalVelocity; }
}
