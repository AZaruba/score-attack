using Godot;
using System;

public partial class HurtboxComponent : Node
{
  [Export] CollisionShape3D CollisionShape;

  [Signal] public delegate void OnHurtEventHandler();

  public override void _Ready()
  {
    base._Ready();
  }

  
}
