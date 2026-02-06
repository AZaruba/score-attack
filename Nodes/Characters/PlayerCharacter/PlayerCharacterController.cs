using Godot;
using StateManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class PlayerCharacterController : CharacterBody3D
{
  // Exports
  [Export] private InputComponent InputComponent;
  [Export] private VelocityComponent VelocityComponent;
  [Export] private RotationComponent RotationComponent;
  [Export] private GravityComponent GravityComponent;
  [Export] private HealthComponent HealthComponent;
  [Export] private StateMachineComponent StateComponent;
  [Export] private AnimatingBodyComponent AnimatingBodyComponent;
  [Export] private CharacterStats Stats;

  // Signals
  [Signal] public delegate void OnPlayerHealthUpdateEventHandler(float newHealth);
  [Signal] public delegate void OnPlayerMaxHealthUpdateEventHandler(float newMax);

  // Local
  private Dictionary<StateID, Action<float>> StateActions;

  public override void _Ready()
  {
    InitStateMachine();
    InitSignals();
    HealthComponent.InitHealth(Stats.MaxHealth);
    EmitSignal(SignalName.OnPlayerMaxHealthUpdate, HealthComponent.GetMaxHealth());
    EmitSignal(SignalName.OnPlayerHealthUpdate, HealthComponent.GetMaxHealth());
  }

  public override void _ExitTree()
  {
    TeardownSignals();
  }

  public override void _PhysicsProcess(double delta)
  {
    RunStateMachineFunction(StateActions[StateComponent.GetCurrentState()], (float)delta);
    UpdateStateMachine();
  }

  private void OnPlayerHealthUpdated(float newHealth)
  {
    EmitSignal(SignalName.OnPlayerHealthUpdate, newHealth);
  }

  private void InitStateMachine()
  {
    StateActions = new Dictionary<StateID, Action<float>>
    {
      { StateID.GROUNDED, NormalState },
      { StateID.JUMPING, JumpingState }
    };
    StateComponent.AddState(StateID.GROUNDED);
    StateComponent.AddState(StateID.JUMPING);
    StateComponent.AddTransitionToState(StateID.GROUNDED, Command.JUMP, StateID.JUMPING);
    StateComponent.AddTransitionToState(StateID.JUMPING, Command.LAND, StateID.GROUNDED);
    StateComponent.Enter(StateID.GROUNDED);
  }

  private void InitSignals()
  {
    HealthComponent.OnHeal += OnPlayerHealthUpdated;
    HealthComponent.OnDamage += OnPlayerHealthUpdated;
  }

  private void TeardownSignals()
  {
    HealthComponent.OnHeal -= OnPlayerHealthUpdated;
    HealthComponent.OnDamage -= OnPlayerHealthUpdated;
  }

  public void RunStateMachineFunction(Action<float> StateFunction, float delta)
  {
    StateFunction(delta);
  }

  public void UpdateStateMachine()
  {
    if (IsOnFloor())
    {
      StateComponent.RunCommand(Command.LAND);
    }
    if (InputComponent.GetJump())
    {
      StateComponent.RunCommand(Command.JUMP);
    }
    DebugLog.Log($"Current Player State: {StateComponent.GetCurrentState()}");
  }

  public void NormalState(float delta)
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

    if (InputComponent.GetAttack())
    {
      if (AnimatingBodyComponent.GetCurrentAnimation().Equals("Punch"))
      {
        AnimatingBodyComponent.PlayOneShot("Punch2", false);
      } else
      {
        AnimatingBodyComponent.PlayOneShot("Punch", false);
      }
    }

    // process Rotation
    Basis = RotationComponent.RotateBasis(Basis, Vector3.Up, Stats.RotationRate * (float)delta * InputComponent.GetStrafeInput() * -1);

    // how to do animation
    if (InputComponent.GetMoveInput() > 0)
    {
      AnimatingBodyComponent.PlayAnimationLoop("MoveForward");
    }
    else if (InputComponent.GetMoveInput() < 0)
    {
      AnimatingBodyComponent.PlayAnimationLoop("MoveBackward");
    }
    else
    {
      //AnimatingBodyComponent.PlayAnimationLoop("Standing");
    }
    // Apply to Engine
    VelocityComponent.CapVelocity(Stats.MoveSpeed);
    Velocity = VelocityComponent.GetCurrentVelocity() + GravityComponent.GetVerticalVelocity();
    MoveAndSlide();
  }

  public void JumpingState(float delta)
  {
     // process Velocity
    Vector3 FlatVelocity = GetRealVelocity();
    FlatVelocity.Y = 0;
    Vector3 CameraForward = -GetViewport().GetCamera3D().Basis.Z;
    Vector3 CameraRight = GetViewport().GetCamera3D().Basis.X;
    CameraForward.Y = 0;
    CameraRight.Y = 0;
    Vector3 MotionInput = CameraForward.Normalized() * InputComponent.GetMoveInput();
    VelocityComponent.SetVelocity(FlatVelocity);
    VelocityComponent.AddForce(MotionInput.Normalized() * Stats.MoveAcceleration);

    // process Gravity
    GravityComponent.AddGravity(Stats.Gravity * (float)delta, IsOnFloor());

    // Apply to Engine
    VelocityComponent.CapVelocity(Stats.MoveSpeed);
    Velocity = VelocityComponent.GetCurrentVelocity() + GravityComponent.GetVerticalVelocity();
    MoveAndSlide();
  }
}
