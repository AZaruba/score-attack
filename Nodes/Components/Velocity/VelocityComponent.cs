using Godot;
using System;

public partial class VelocityComponent : Node
{
  Vector3 CurrentVelocity;

  public override void _Ready()
  {
    CurrentVelocity = Vector3.Zero;
  }

  public override void _Process(double delta)
  {
    // stub
  }

  public void AddForce(Vector3 Input)
  {
    CurrentVelocity += Input;
  }

  public void SetVelocity(Vector3 Input)
  {
    CurrentVelocity = Input;
  }

  public Vector3 GetCurrentVelocity() { return CurrentVelocity; }
}
