using Godot;
using StateManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Goon : IOpponent
{
  [Export] NavigationAgent3D NavigationAgent;
  [Export] AnimatingBodyComponent AnimatingBodyComponent;
  [Export] VelocityComponent VelocityComponent;
  [Export] RotationComponent RotationComponent;
  [Export] GravityComponent GravityComponent;
  [Export] DecisionTreeComponent DecisionTreeComponent;
  [Export] AttackAccumulationComponent AttackAccumulationComponent;
  [Export] WorldResourceBar HealthBar;
  [Export] CharacterStats Stats;
  [Export] EnemyBehaviorStats BehaviorStats;

  private StateID LastDecisionState;

  // is this necessary if the game manager already knows the player state? (Stealth/last known position?)
  //[Signal] public delegate void RequestPlayerPositionEventHandler()
  private Dictionary<StateID, Action<float>> StateActions;

  // will each state have much more complicated reactions to getting hit?
  //private Dictionary<StateID, Action<float>> StateOnHitByAttackActions;

  private Vector3 NextTarget;
  private bool IsMovingToLink = false;
  private bool Arrived = false;

  private bool Launched = false;
  private bool KnockedBack = false;

  public override void _Ready()
  {;
    HealthComponent.InitHealth(Stats.MaxHealth);
    HealthBar.SetResourceMax(HealthComponent.GetCurrentHealth());
    HealthBar.SetResourceValue(HealthComponent.GetCurrentHealth());
    InitStateMachine();
    InitSignals();
    NavigationAgent.TargetDesiredDistance = BehaviorStats.OrbitDistance;
    AttackAccumulationComponent.DamageThreshold = BehaviorStats.GuardCounterThreshold;
  }

  public override void _ExitTree()
  {
    TeardownSignals();
  }

  private void InitStateMachine()
  {
    StateActions = new Dictionary<StateID, Action<float>>
    {
      { StateID.APPROACHING, ApproachingState },
      { StateID.ORBITING, OrbitingState },
      { StateID.ENGAGING, EngagingState },
      { StateID.ATTACKING, AttackingState },
      { StateID.HIT_BY_ATTACK, HitRecoveryState },
      { StateID.BLOCKING, BlockingState },
      { StateID.KNOCKED_BACK, KnockbackState },
      { StateID.LAUNCHED, LaunchedState }
    };

    // Decision Tree version
    DecisionTreeComponent.AddBranch(DecisionID.TARGET_IS_NOTICED);
    DecisionTreeComponent.AddBranch(DecisionID.KNOCKED_BACK);
    DecisionTreeComponent.AddBranch(DecisionID.LAUNCHED);
    DecisionTreeComponent.AddBranch(DecisionID.IDLE); // currently will never get here
    DecisionTreeComponent.AddBranch(DecisionID.TARGET_IN_RANGE, DecisionID.TARGET_IS_NOTICED);
    DecisionTreeComponent.AddBranch(DecisionID.TARGET_NOT_IN_RANGE, DecisionID.TARGET_IS_NOTICED);
    DecisionTreeComponent.AddBranch(DecisionID.WANT_TO_ENGAGE, DecisionID.TARGET_IN_RANGE);
    DecisionTreeComponent.AddBranch(DecisionID.WANT_TO_ORBIT, DecisionID.TARGET_IN_RANGE);
    DecisionTreeComponent.AddBranch(DecisionID.WANT_TO_ATTACK, DecisionID.WANT_TO_ENGAGE);
    DecisionTreeComponent.AddBranch(DecisionID.WANT_TO_CLOSE_IN, DecisionID.WANT_TO_ENGAGE);

    DecisionTreeComponent.AddStateLeaf(StateID.GROUNDED, DecisionID.IDLE);
    DecisionTreeComponent.AddStateLeaf(StateID.APPROACHING, DecisionID.TARGET_NOT_IN_RANGE);
    DecisionTreeComponent.AddStateLeaf(StateID.ORBITING, DecisionID.WANT_TO_ORBIT);
    DecisionTreeComponent.AddStateLeaf(StateID.ATTACKING, DecisionID.WANT_TO_ATTACK);
    DecisionTreeComponent.AddStateLeaf(StateID.ENGAGING, DecisionID.WANT_TO_CLOSE_IN);
    DecisionTreeComponent.AddStateLeaf(StateID.KNOCKED_BACK, DecisionID.KNOCKED_BACK);
    DecisionTreeComponent.AddStateLeaf(StateID.LAUNCHED, DecisionID.LAUNCHED);

    //DecisionTreeComponent.SetLeafTimer(2f, DecisionID.WANT_TO_ATTACK);

    DecisionTreeComponent.SetBehavior(NoticeIfNotStunned, DecisionID.ROOT);
    DecisionTreeComponent.SetBehavior(IsTargetInRange, DecisionID.TARGET_IS_NOTICED);
    DecisionTreeComponent.SetBehavior(IsReadyToEngage, DecisionID.TARGET_IN_RANGE);
    DecisionTreeComponent.SetBehavior(SelectEngageAction, DecisionID.WANT_TO_ENGAGE);


  }

  public DecisionID NoticeIfNotStunned()
  {
    if (Launched)
    {
      if (IsOnFloor() && GravityComponent.GetVerticalVelocity().Y <= 0)
      {
        AnimatingBodyComponent.TravelTo("Idle");
        Launched = false;
      }
      return DecisionID.LAUNCHED;
    }
    else if (KnockedBack)
    {
      if (IsOnWall())
      {
        AnimatingBodyComponent.TravelTo("Idle");
        KnockedBack = false;
      }
      return DecisionID.KNOCKED_BACK;
    }
    return DecisionID.TARGET_IS_NOTICED;
  }

  public DecisionID IsTargetInRange()
  {
    if (Arrived)
    {
      return DecisionID.TARGET_IN_RANGE;
    }
    NavigationAgent.TargetDesiredDistance = BehaviorStats.OrbitDistance;
    return DecisionID.TARGET_NOT_IN_RANGE;
  }

  public DecisionID IsReadyToEngage()
  {
    // some circumstance will make us want to orbit instead of approach
    NavigationAgent.TargetDesiredDistance = BehaviorStats.EngageDistance;
    return DecisionID.WANT_TO_ENGAGE;
  }

  public DecisionID SelectEngageAction()
  {
    if (Position.DistanceTo(NavigationAgent.TargetPosition) <= BehaviorStats.EngageDistance)
    {
      AnimatingBodyComponent.DirectAnimationPlay("Punch");
      AnimatingBodyComponent.TravelTo("Punch");
      return DecisionID.WANT_TO_ATTACK;
    }
    return DecisionID.WANT_TO_CLOSE_IN;
  }

  public DecisionID IsReadyToAttack()
  {
    return DecisionID.WANT_TO_ATTACK;
  }

  private void InitSignals()
  {
    HealthComponent.OnHealthZero += WhenHealthZero;
    AnimatingBodyComponent.OnCurrentAnimationFinished += OnCurrentAnimationFinished;
    AttackAccumulationComponent.OnThresholdExceeded += OnAttackDamageThresholdHit;
  }

  private void TeardownSignals()
  {
    HealthComponent.OnHealthZero -= WhenHealthZero;
    AnimatingBodyComponent.OnCurrentAnimationFinished -= OnCurrentAnimationFinished;
    AttackAccumulationComponent.OnThresholdExceeded -= OnAttackDamageThresholdHit;
  }

  public override void _PhysicsProcess(double delta)
  {
    LastDecisionState = DecisionTreeComponent.MakeDecision();
    RunStateMachineFunction(StateActions[LastDecisionState], (float)delta);
    // UpdateStateMachine();
    DebugLog.Log(LastDecisionState.ToString(), 1);
  }

  private void OnLinkReached(Godot.Collections.Dictionary details)
  {
    IsMovingToLink = true;
    NextTarget = (Vector3)details["link_exit_position"];
  }

  private void OnTargetReached()
  {
    // Change to arrived state
    Arrived = true;
  }

  private void OnHitByAttack(AttackPropertiesContainer AttackNode)
  {
    // TODO: Different enemies have different launch/knockback resistance
    if (AttackNode.IsLauncher())
    {
      AnimatingBodyComponent.TravelTo("Launched");
      Launched = true;
    }
    if (AttackNode.GetKnockbackForce() > 0.5f)
    {
      AnimatingBodyComponent.TravelTo("KnockedBack");
      VelocityComponent.SetVelocity(Basis.Z * -1 * AttackNode.GetKnockbackForce());
      KnockedBack = true;
      Launched = false;
    }
    else if (!Launched)
    {
      AnimatingBodyComponent.TravelTo("HitStun");
    }
    //DebugLog.LogTemp("Got Hit", 2);
    // TODO: investigate simply setting a flag for "eligible for hit"
    if (LastDecisionState != StateID.BLOCKING)
    {
      HealthComponent.Damage(AttackNode.GetAttackPotency());
      HealthBar.RevealHealthBar();
      HealthBar.SetResourceValue(HealthComponent.GetCurrentHealth());
    }
    if (Launched)
    {
      GravityComponent.ApplyVerticalForce(AttackNode.GetLauncherForce());
    }
    AttackAccumulationComponent.AddDamage(AttackNode.GetAttackPotency());
  }

  private void OnCurrentAnimationFinished()
  {
    // TODO: make this smarter
    // StateComponent.RunCommand(Command.RECOVER_FROM_HIT);
    // StateComponent.RunCommand(Command.FINISH_ATTACK);
  }

  private void OnAttackDamageThresholdHit(int cmd)
  {
    if (LastDecisionState == StateID.BLOCKING)
    {
      //StateComponent.RunCommand(Command.START_ATTACK);
      AnimatingBodyComponent.TravelTo("Punch");
    }
    else
    {
      //StateComponent.RunCommand((Command)cmd);
      AnimatingBodyComponent.TravelTo("Block");
    }
  }

  public void RunStateMachineFunction(Action<float> StateFunction, float delta)
  {
    StateFunction(delta);
  }

  public override void UpdateTargetPosition(Vector3 position)
  {
    NavigationAgent.TargetPosition = position;
  }

  private void ApproachingState(float delta)
  {
    GravityComponent.AddGravity(Stats.Gravity * delta, IsOnFloor());
    if (IsMovingToLink)
    {
      // hmm
      if (IsOnFloor())
      {
        GravityComponent.ApplyVerticalForce(Stats.JumpForce);
        IsMovingToLink = false;
      }
    }
    else
    {
      NextTarget = NavigationAgent.GetNextPathPosition();
    }

    Vector3 FlatVelocity = GetRealVelocity();
    FlatVelocity.Y = 0;
    VelocityComponent.SetVelocity(FlatVelocity);
    Vector3 Direction = NextTarget - Position;
    Direction.Y = 0;

    VelocityComponent.AddForce(Direction.Normalized() * Stats.MoveAcceleration);

    float ToAngle = Basis.Z.SignedAngleTo(VelocityComponent.GetCurrentVelocity(), Vector3.Up);
    Basis = RotationComponent.RotateBasisR(Basis, Vector3.Up, ToAngle);

    //Animate 
    AnimatingBodyComponent.SetAnimationParameter(PCAnimationNames.MoveBlendPath, VelocityComponent.GetCurrentVelocity().Length() / Stats.MoveSpeed);
    
    VelocityComponent.CapVelocity(Stats.MoveSpeed);
    Velocity = GravityComponent.GetVerticalVelocity();
    if (IsOnFloor()) Velocity += VelocityComponent.GetCurrentVelocity();
    MoveAndSlide();
  }

  private void OrbitingState(float delta)
  {
    GravityComponent.AddGravity(Stats.Gravity * delta, IsOnFloor());
    Vector3 FlatVelocity = GetRealVelocity();
    FlatVelocity.Y = 0;
    VelocityComponent.SetVelocity(Vector3.Zero);

    Vector3 CurrentOffset = Position - NavigationAgent.TargetPosition;

    // the orbit target should be an open position within the field of view
    Vector3 OrbitTarget = NavigationAgent.TargetPosition + -GetViewport().GetCamera3D().Basis.Z * NavigationAgent.TargetDesiredDistance;
    // Rotate Orbit Target by random amount on enter?
    //OrbitTarget = OrbitTarget.Rotated(Vector3.Up, (float)GD.RandRange(-0.1f, 0.1f));

    VelocityComponent.AddForce((OrbitTarget - Position).Normalized() * Stats.MoveAcceleration);
    
    Vector3 FlatOffset = CurrentOffset;
    FlatOffset.Y = 0;
    float ToAngle = Basis.Z.SignedAngleTo(FlatOffset * -1, Vector3.Up);
    Basis = RotationComponent.RotateBasisR(Basis, Vector3.Up, ToAngle);

    // close distance

    if ((CurrentOffset.Length() - NavigationAgent.TargetDesiredDistance) > 1)
    {
      Arrived = false;
    }
    else if ((CurrentOffset.Length() - NavigationAgent.TargetDesiredDistance) > 0)
    {
      VelocityComponent.AddForce((CurrentOffset.Length() - NavigationAgent.TargetDesiredDistance) * -1 * Stats.MoveSpeed * CurrentOffset);
    }

    VelocityComponent.CapVelocity(Mathf.Min(Stats.MoveSpeed, Position.DistanceTo(OrbitTarget)));
    Velocity = VelocityComponent.GetCurrentVelocity() + GravityComponent.GetVerticalVelocity();
    MoveAndSlide();

  }

  private void EngagingState(float delta)
  {
    GravityComponent.AddGravity(Stats.Gravity * delta, IsOnFloor());
    Vector3 FlatVelocity = GetRealVelocity();
    FlatVelocity.Y = 0;
    VelocityComponent.SetVelocity(Vector3.Zero);

    Vector3 CurrentOffset = Position - NavigationAgent.TargetPosition;

    // Rotate Orbit Target by random amount on enter?
    
    CurrentOffset.Y = 0;
    Vector3 FlatOffset = CurrentOffset;
    FlatOffset.Y = 0;
    float ToAngle = Basis.Z.SignedAngleTo(FlatOffset * -1, Vector3.Up);
    Basis = RotationComponent.RotateBasisR(Basis, Vector3.Up, ToAngle);

    // close distance

    if ((CurrentOffset.Length() - NavigationAgent.TargetDesiredDistance) > 0)
    {
      VelocityComponent.AddForce((CurrentOffset.Length() - NavigationAgent.TargetDesiredDistance + 0.1f) * -1 * Stats.MoveSpeed * CurrentOffset);
    }

    VelocityComponent.CapVelocity(Stats.MoveSpeed);
    Velocity = VelocityComponent.GetCurrentVelocity() + GravityComponent.GetVerticalVelocity();
    MoveAndSlide();
  }

  private void AttackingState(float delta)
  {
    
  }

  private void HitRecoveryState(float delta)
  {
    
  }

  private void BlockingState(float delta)
  {
    
  }

  // TODO: Waiting state
  private void WaitingState(float delta)
  {
    
  }

  private void KnockbackState(float delta)
  {
    Velocity = VelocityComponent.GetCurrentVelocity();
    DebugLog.Log(VelocityComponent.GetCurrentVelocity().ToString(), 2);
    MoveAndSlide();
  }

  private void LaunchedState(float delta)
  {
    GravityComponent.AddGravity(Stats.Gravity * delta, false);
    DebugLog.Log(GravityComponent.GetVerticalVelocity().ToString(), 2);
    Velocity = GravityComponent.GetVerticalVelocity();
    MoveAndSlide();
  }
}
