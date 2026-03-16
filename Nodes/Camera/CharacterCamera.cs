using System.Diagnostics;
using Godot;

public partial class CharacterCamera : Camera3D
{
  [Export] private Node3D Target;
  [Export] private VelocityComponent VelocityComponent;
  [Export] private RotationComponent RotationComponent;
  [Export] private RayCast3D HeightCast;
  [Export] private CameraStats Stats;

  private Vector3 DesiredPosition;
  private Basis DesiredRotation;
  private float DesiredDistance;
  private float TargetGroundedHeight = 0;
  private float TargetGroundedAngle = 0;

  public override void _Ready()
  {
    Position = Target.Position + Target.Basis.GetRotationQuaternion() * Stats.DistanceToTarget;
    Transform = Transform.LookingAt(Target.Position + Target.Basis.GetRotationQuaternion() * Stats.TargetOffset, Vector3.Up);
    DesiredRotation = Transform.Basis;
  }

  // TODO: component-ize this
  public override void _PhysicsProcess(double delta)
  {
    Vector3 FlatTargetPosition = Target.Position;
    FlatTargetPosition.Y = TargetGroundedHeight;
    float YValue = FlatTargetPosition.Y;

    float Hypotenuse = Position.DistanceTo(FlatTargetPosition);
    DebugLog.Log((Mathf.Sin(TargetGroundedAngle) * Hypotenuse).ToString(), 0);
    DebugLog.Log((Mathf.Sin(TargetGroundedAngle) * Hypotenuse).ToString(), 1);

    if (HeightCast.IsColliding() && Mathf.Sin(TargetGroundedAngle) * Hypotenuse > HeightCast.GetCollisionPoint().Y - TargetGroundedHeight)
    {
      YValue = HeightCast.GetCollisionPoint().Y + Stats.HeightOffGround; // - (HeightCast.GetCollisionPoint().Y - TargetGroundedHeight);
    }

    DesiredPosition = FlatTargetPosition + Target.Basis.GetRotationQuaternion() * Stats.DistanceToTarget;
    DesiredPosition.Y = Mathf.Max(DesiredPosition.Y, YValue);

    Basis LAtBasis = Basis.LookingAt(FlatTargetPosition + Target.Basis.GetRotationQuaternion() * Stats.TargetOffset - Position);
    DesiredRotation = DesiredRotation.Slerp(LAtBasis.Orthonormalized(), (float)delta * Stats.TurnCorrectionRate); // how to do vertical
    Basis = DesiredRotation;

    // break out XZ and Y forces
    float Mag = DesiredPosition.Y - Position.Y;
    Vector3 VerticalForce = new Vector3(0, Mag, 0).Normalized() * Stats.MovementAcceleration;
    DesiredPosition.Y = Position.Y;
    VelocityComponent.AddForce((DesiredPosition - Position).Normalized() * Stats.MovementAcceleration / 2);
    VelocityComponent.AddForce(VerticalForce);
    VelocityComponent.CapVelocity(Position.DistanceTo(DesiredPosition)/Stats.DistanceToTarget.Length() * Stats.MovementRate, Mathf.Abs(Mag) * (Stats.MovementRate / 10 + Position.DistanceTo(DesiredPosition)/Stats.DistanceToTarget.Length()));
    Position += VelocityComponent.GetCurrentVelocity() * (float)delta;
  }

  public void SetTargetGroundedInfo(float heightIn, float angleIn)
  {
    TargetGroundedHeight = heightIn;
    TargetGroundedAngle = angleIn;
  }
}
