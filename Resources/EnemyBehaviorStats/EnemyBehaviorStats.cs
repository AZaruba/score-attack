using Godot;
using System;

public partial class EnemyBehaviorStats : Resource
{
  [Export] public float OrbitDistance;
  [Export] public float EngageDistance;
  [Export] public float GuardCounterThreshold;
}
