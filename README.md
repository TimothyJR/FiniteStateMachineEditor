# FiniteStateMachineEditor

A visual editor for finite state machines. The state machine uses a scriptable object implementation. The main purpose of this tool is to set up state machines visually so that the set up can be seen easily.

###### The visual aspect:
1. States can be created and linked with transitions using the editor. 
2. A state called "any state" can be created. This state is used for transitions that can happen at any time (Like going to a death state when health reaches 0).

###### The programming aspect:
1. States are made up of actions.
   - Each action supports both update and fixed update.
   - States can hold more than one action.
   - Actions are completely unrelated to transitions.
2. Transitions are made up of decisions.
   - Transitions can hold more than one decision.
   - A transition only occurs if all decisions return true.

Documentation can be found here:
https://docs.google.com/document/d/10p1V7mEKYm4Hf4PyEWriroeG0DAjdj9iywN2x6sBojI/edit?usp=sharing
