using Godot;

public partial class CharacterCamera : Camera3D
{
  [Export] private Node3D Target;
  [Export] private VelocityComponent VelocityComponent;
  [Export] private RotationComponent RotationComponent;
  [Export] private CameraStats Stats;

  private Vector3 DesiredPosition;
  private Basis DesiredRotation;
  private float DesiredDistance;

  public override void _Ready()
  {
    Position = Target.Position + Target.Basis.GetRotationQuaternion() * Stats.DistanceToTarget;
    Transform = Transform.LookingAt(Target.Position + Target.Basis.GetRotationQuaternion() * Stats.TargetOffset, Vector3.Up);
  }

  // TODO: component-ize this
  public override void _PhysicsProcess(double delta)
  {
    Vector3 FlatTargetPosition = Target.Position;
    DesiredPosition = FlatTargetPosition + Target.Basis.GetRotationQuaternion() * Stats.DistanceToTarget;
    DesiredRotation = Basis.LookingAt(FlatTargetPosition + Target.Basis.GetRotationQuaternion() * Stats.TargetOffset - Position);
    Basis = DesiredRotation;

    VelocityComponent.AddForce((DesiredPosition - Position).Normalized() * Stats.MovementAcceleration);
    VelocityComponent.CapVelocity(Position.DistanceTo(DesiredPosition)/Stats.DistanceToTarget.Length() * Stats.MovementRate);
    
    Position += VelocityComponent.GetCurrentVelocity() * (float)delta;

  }
}
