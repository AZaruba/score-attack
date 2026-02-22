using Godot;
using System;

public partial class RotationComponent : Node
{
  private float CurrentRotationRate;
  private Vector3 Axis;

  public override void _Ready()
  {
    CurrentRotationRate = 0;
  }

  public override void _PhysicsProcess(double delta)
  {
    // TODO set decay rate
    CurrentRotationRate = Mathf.MoveToward(CurrentRotationRate, 0, (float)delta * 10);
  }
  public Basis RotateBasis(Basis Current, Vector3 Axis, float Rate)
  {
    return Current.Rotated(Axis, Mathf.DegToRad(Rate));
  }

  // alternate implementation using Radians
  public Basis RotateBasisR(Basis Current, Vector3 Axis, float Rate)
  {
    return Current.Rotated(Axis, Rate);
  }

  public Basis RotateBasis(Basis Current, float delta)
  {
    return Current.Rotated(this.Axis, CurrentRotationRate * delta);
  }

  public void SetAxis(Vector3 axisIn)
  {
    Axis = axisIn;
  }

  public void AddRotationForce(float magnitude)
  {
    CurrentRotationRate += magnitude;
  }

  public void SetRotationForce(float magnitude)
  {
    CurrentRotationRate = magnitude;
  }

  public float GetRotationForce()
  {
    return CurrentRotationRate;
  }

  public void CapRotationForce(float cap)
  {
    CurrentRotationRate = Mathf.Min(Mathf.Abs(CurrentRotationRate), cap) * Mathf.Sign(CurrentRotationRate);
  }
}
