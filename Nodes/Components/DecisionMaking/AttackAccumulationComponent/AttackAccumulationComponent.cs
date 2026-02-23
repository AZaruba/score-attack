using Godot;
using StateManagement;
using System;

public partial class AttackAccumulationComponent : Node
{
  [Export] public float DamageThreshold; // do we want multiple?
  [Export] float DecayRate;

  [Export] Command ThresholdCommand;

  [Signal] public delegate void OnThresholdExceededEventHandler(int cmd);

  private float AccumulatedDamage;

  public override void _Ready()
  {
    AccumulatedDamage = 0;
  }
  public override void _PhysicsProcess(double delta)
  {
    //DebugLog.Log(AccumulatedDamage.ToString(), 1);
    if (AccumulatedDamage > DamageThreshold)
    {
      AccumulatedDamage = 0;
      EmitSignal(SignalName.OnThresholdExceeded, (int)ThresholdCommand);
    }
    AccumulatedDamage = Mathf.Clamp(AccumulatedDamage - (float)delta*DecayRate, 0, DamageThreshold);
  }

  public void AddDamage(float dmg)
  {
    AccumulatedDamage += dmg;
  }
}
