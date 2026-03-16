using Godot;
using System;

public partial class VelocityComponent : Node
{
  Vector3 CurrentVelocity;

  public override void _Ready()
  {
    CurrentVelocity = Vector3.Zero;
  }

  public override void _PhysicsProcess(double delta)
  {
    // stub
  }

  public void AddForce(Vector3 Input)
  {
    CurrentVelocity += Input;
  }

  public void AddFrictionForce(Vector3 Input, bool IsOnFloor)
  {
    if (IsOnFloor)
    {
      CurrentVelocity += Input;
    }
  }

  public void SetVelocity(Vector3 Input)
  {
    CurrentVelocity = Input;
  }

  public void CapVelocity(float Cap)
  {
    CurrentVelocity = CurrentVelocity.LimitLength(Cap);
  }

  public void CapVelocity(float XZCap, float YCap)
  {
    Vector3 VertVelocity = (Vector3.Up * CurrentVelocity.Y).LimitLength(YCap);
    Vector3 HorizontalVelocity = new Vector3(CurrentVelocity.X, 0, CurrentVelocity.Z).LimitLength(XZCap);
    CurrentVelocity = VertVelocity + HorizontalVelocity;
  }

  public Vector3 GetCurrentVelocity() { return CurrentVelocity; }
}
