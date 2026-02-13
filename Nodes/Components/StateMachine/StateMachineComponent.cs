using Godot;
using StateManagement;
using System.Collections.Generic;
using System.Linq;


/*
State Machine that manages IDs and transitions
- This component contains no orchestration and purely acts as a data structure for the state graph
- When initializing a state machine, add all states before adding any transitions
*/
public partial class StateMachineComponent : Node
{
  private StateID CurrentState;
  private Dictionary<StateID, State> StateLookup;

  public override void _Ready()
  {
    StateLookup = [];
  }

  public StateID GetCurrentState()
  {
    return CurrentState;
  }

  public void Enter(StateID defaultState)
  {
    if (!StateLookup.ContainsKey(defaultState))
    {
      throw new System.Exception($"State Machine does not contain default state {defaultState}");
    }
    CurrentState = defaultState;
  }

  public bool AddState(StateID id)
  {
    if (StateLookup.ContainsKey(id))
    {
      return false;
    }
    StateLookup.Add(id, new State(id));
    return true;
  }

  public bool RemoveState(StateID id)
  {
    if (StateLookup.ContainsKey(id))
    {
      return StateLookup.Remove(id);
    }
    return false;
  }

  public bool AddTransitionToState(StateID id, Command cmd, StateID next)
  {
    if (StateLookup.TryGetValue(id, out State value) && StateLookup.ContainsKey(next))
    {
      value.AddTransition(cmd, next);
      return true;
    }
    throw new System.Exception($"State Add Exception, State Machine {GetInstanceId()} does not contain both states: {id} and {next}");
  }

  public bool AddTransitionToAllStates(Command cmd, StateID next, StateID[] exceptions = null)
  {
    foreach(StateID id in StateLookup.Keys)
    {
      if (exceptions != null && exceptions.Contains(id))
      {
        continue;
      }
      else if (StateLookup.TryGetValue(id, out State value) && StateLookup.ContainsKey(next))
      {
        value.AddTransition(cmd, next);
      }
      else
      {
        throw new System.Exception($"State Add Exception, State Machine {GetInstanceId()} does not contain both states: {id} and {next}");
      }
    }
    return true;
  }

  /*
  Sends the command to the current state and updates the state machine accordingly
  Parameters:
    - cmd: a Command to be executed on the current state
  Return Value:
    - if the command leads to a new state, return the new state
    - if the current state does not have a transition using the command, return the current state
  */
  public StateID RunCommand(Command cmd)
  {
    StateID next = StateLookup[CurrentState].GetNextState(cmd);
    if (next != StateID.ERR_STATE && StateLookup.ContainsKey(next))
    {
      CurrentState = next;
    }
    return CurrentState;
  }
}
