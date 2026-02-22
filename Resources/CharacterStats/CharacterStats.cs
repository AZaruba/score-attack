using Godot;

public partial class CharacterStats : Resource
{
  [Export] public float MoveSpeed;
  [Export] public float MoveAcceleration;
  [Export] public float RotationRate;
  [Export] public float RotationAcceleration;
  [Export] public float JumpForce;
  [Export] public float Gravity;
  [Export] public float GroundFriction;
  [Export] public float MaxHealth;
}
