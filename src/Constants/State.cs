using System.Collections.Generic;
using Godot;

namespace StateManagement
{
  public enum StateID
  {
    ERR_STATE = -1,
    DEFAULT,
    GROUNDED,
    JUMPING,
    ATTACKING,
    BLOCKING,
    HIT_BY_ATTACK,
    KNOCKED_DOWN,
    JUGGLED,
    APPROACHING,
    ORBITING,
    ENGAGING
  }

  public enum Command
  {
    ERR_CMD = -1,
    DEFAULT,
    JUMP,
    LAND,
    KNOCK_UP,
    KNOCK_DOWN,
    PUSH_BACK,
    START_ATTACK,
    FINISH_ATTACK,
    GET_HIT,
    BLOCK,
    UNBLOCK,
    APPROACH,
    ORBIT,
    ENGAGE,
    DISENGAGE
  }

  public class State
  {
    private Dictionary<Command, StateID> Transitions;
    private StateID ID; // is it necessary to store the current state ID in the state?

    public State(StateID id)
    {
      ID = id;
      Transitions = [];
    }

    public void AddTransition(Command cmd, StateID next)
    {
      Transitions.Add(cmd, next);
    }

    public bool RemoveTransition(Command cmd)
    {
      if (Transitions.TryGetValue(cmd, out StateID id))
      {
        return Transitions.Remove(cmd);
      }
      return false;
    }

    public StateID GetCurrentState()
    {
      return ID;
    }

    public StateID GetNextState(Command cmd)
    {
      if (Transitions.TryGetValue(cmd, out StateID id))
      {
        return id;
      }
      return StateID.ERR_STATE;
    }
  }
}
