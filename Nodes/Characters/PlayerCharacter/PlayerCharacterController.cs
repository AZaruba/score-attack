using Godot;
using System;

public partial class PlayerCharacterController : CharacterBody3D
{
  [Export] private InputComponent InputComponent;
  [Export] private VelocityComponent VelocityComponent;
  [Export] private RotationComponent RotationComponent;
  [Export] private GravityComponent GravityComponent;
  [Export] private CharacterStats Stats;

  public override void _Ready()
  {
    
  }

  public override void _PhysicsProcess(double delta)
  {
    // process Velocity
    Vector3 FlatVelocity = GetRealVelocity();
    FlatVelocity.Y = 0;
    VelocityComponent.SetVelocity(FlatVelocity);

    Vector3 CameraForward = -GetViewport().GetCamera3D().Basis.Z;
    Vector3 CameraRight = GetViewport().GetCamera3D().Basis.X;
    CameraForward.Y = 0;
    CameraRight.Y = 0;

    Vector3 MotionInput = CameraForward.Normalized() * InputComponent.GetMoveInput();
    Vector3 FrictionInput = VelocityComponent.GetCurrentVelocity() * -1 * (float)delta * Stats.GroundFriction;
    VelocityComponent.AddForce(MotionInput.Normalized() * Stats.MoveAcceleration);
    VelocityComponent.AddFrictionForce(FrictionInput, IsOnFloor());

    // process Gravity
    GravityComponent.AddGravity(Stats.Gravity * (float)delta, IsOnFloor());
    if (InputComponent.GetJump())
    {
      GravityComponent.ApplyVerticalForce(Stats.JumpForce);
    }

    // process Rotation
    Basis = RotationComponent.RotateBasis(Basis, Vector3.Up, Stats.RotationRate * (float)delta * InputComponent.GetStrafeInput() * -1);

    // Apply to Engine
    VelocityComponent.CapVelocity(Stats.MoveSpeed);
    Velocity = VelocityComponent.GetCurrentVelocity() + GravityComponent.GetVerticalVelocity();
    MoveAndSlide();
  }
}
