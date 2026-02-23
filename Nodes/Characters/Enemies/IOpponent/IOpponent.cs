using Godot;
using System;

public partial class IOpponent : CharacterBody3D
{
  [Export] public HealthComponent HealthComponent;
  [Signal] public delegate void OnHealthZeroEventHandler(IOpponent self);
  public virtual void UpdateTargetPosition(Vector3 position)
  {
  }
  public void WhenHealthZero()
  {
    // TODO: return to object pool
    EmitSignal(SignalName.OnHealthZero, this);
  }
}
