using Godot;
using System;

public partial class RotationComponent : Node
{
  public Basis RotateBasis(Basis Current, Vector3 Axis, float Rate)
  {
    return Current.Rotated(Axis, Mathf.DegToRad(Rate));
  }
}
