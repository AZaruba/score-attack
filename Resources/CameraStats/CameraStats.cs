using Godot;
using System;

public partial class CameraStats : Resource
{
  [Export] public Vector3 TargetOffset;
  [Export] public Vector3 DistanceToTarget;
  [Export] public float TurnCorrectionRate;
  [Export] public float MovementRate;
}
