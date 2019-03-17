using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
   public class AnyState : State
   {
      public override void UpdateState(StateMachine stateMachine)
      {
         CheckTransitions(stateMachine);
      }
   }
}