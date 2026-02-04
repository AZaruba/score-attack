using Godot;

public partial class CharacterCamera : Camera3D
{
  [Export] private Node3D Target;
  [Export] private VelocityComponent VelocityComponent;
  [Export] private CameraStats Stats;

  public override void _Ready()
  {
    Position = Target.Position + Target.Basis.GetRotationQuaternion() * Stats.DistanceToTarget;
    Transform = Transform.LookingAt(Target.Position + Target.Basis.GetRotationQuaternion() * Stats.TargetOffset, Vector3.Up);
    
  }

  public override void _PhysicsProcess(double delta)
  {
    // Rotate offset Vector, 
    Vector3 CurrentOffset = Position - Target.Position;
    CurrentOffset.Y = Stats.DistanceToTarget.Y;
    Vector3 TargetOffset = Target.Basis.GetRotationQuaternion() * Stats.DistanceToTarget;
    TargetOffset.Y = Stats.DistanceToTarget.Y;
    float Rate = Mathf.Clamp(CurrentOffset.SignedAngleTo(TargetOffset, Vector3.Up), Stats.TurnCorrectionRate * -1 * (float)delta, Stats.TurnCorrectionRate * (float)delta);
    Position = Target.Position + CurrentOffset.Rotated(Vector3.Up, Rate);

    // move toward
    Position = Position.MoveToward(Target.Position + TargetOffset, (float)delta * Stats.MovementRate);
    Transform = Transform.LookingAt(Target.Position + Target.Basis.GetRotationQuaternion() * Stats.TargetOffset, Vector3.Up);
  }
}
