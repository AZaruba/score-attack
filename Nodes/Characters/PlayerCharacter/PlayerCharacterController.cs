using Godot;
using StateManagement;
using System;
using System.Collections.Generic;

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

  private void OnCurrentAnimationFinished()
  {
    StateComponent.RunCommand(Command.FINISH_ATTACK);
  }

  private void InitStateMachine()
  {
    StateActions = new Dictionary<StateID, Action<float>>
    {
      { StateID.GROUNDED, NormalState },
      { StateID.JUMPING, JumpingState },
      { StateID.ATTACKING, AttackingState }
    };
    StateComponent.AddState(StateID.GROUNDED);
    StateComponent.AddState(StateID.JUMPING);
    StateComponent.AddState(StateID.ATTACKING);
    StateComponent.AddTransitionToState(StateID.GROUNDED, Command.JUMP, StateID.JUMPING);
    StateComponent.AddTransitionToState(StateID.GROUNDED, Command.START_ATTACK, StateID.ATTACKING);
    StateComponent.AddTransitionToState(StateID.ATTACKING, Command.FINISH_ATTACK, StateID.GROUNDED);
    StateComponent.AddTransitionToState(StateID.JUMPING, Command.LAND, StateID.GROUNDED);
    StateComponent.Enter(StateID.GROUNDED);
  }

  private void InitSignals()
  {
    HealthComponent.OnHeal += OnPlayerHealthUpdated;
    HealthComponent.OnDamage += OnPlayerHealthUpdated;
    AnimatingBodyComponent.OnCurrentAnimationFinished += OnCurrentAnimationFinished;
  }

  private void TeardownSignals()
  {
    HealthComponent.OnHeal -= OnPlayerHealthUpdated;
    HealthComponent.OnDamage -= OnPlayerHealthUpdated;
    AnimatingBodyComponent.OnCurrentAnimationFinished -= OnCurrentAnimationFinished;
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
    if (InputComponent.GetAttack())
    {
      StateComponent.RunCommand(Command.START_ATTACK);
    }
    AnimatingBodyComponent.SetCurrentState(StateComponent.GetCurrentState());
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

    // process Rotation
    Basis = RotationComponent.RotateBasis(Basis, Vector3.Up, Stats.RotationRate * (float)delta * InputComponent.GetStrafeInput() * -1);

    //Animate 
    AnimatingBodyComponent.SetAnimationParameter(PCAnimationNames.MoveBlendPath, InputComponent.GetMoveInput());
    
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

  public void AttackingState(float delta)
  {
    Velocity = Vector3.Zero;
    if (InputComponent.GetAttack())
    {
      AnimatingBodyComponent.QueueAnimation();
    }
    MoveAndSlide();
  }
}
