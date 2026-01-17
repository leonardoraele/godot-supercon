using Godot;
using Raele.GodotUtils.ActivitySystem;

namespace Raele.Supercon2D;

[Tool][GlobalClass]
public partial class SuperconState : Activity, SuperconStateMachine.IState
{
	//==================================================================================================================
	// EXPORTS
	//==================================================================================================================

	//==================================================================================================================
	// FIELDS
	//==================================================================================================================

	public ISuperconStateMachineOwner? StateMachineOwner => ISuperconStateMachineOwner.GetStateMachineOwnerOf(this);

	//==================================================================================================================
	// PROPERTIES
	//==================================================================================================================

	public bool IsPreviousActiveState => this.StateMachineOwner?.StateMachine.PreviousActiveState == this;
	// TOOD Do we really need this class here?
	public SuperconInputMapping? InputMapping => this.StateMachineOwner?.Character?.InputMapping;

	//==================================================================================================================
	// SIGNALS
	//==================================================================================================================

	//==================================================================================================================
	// OVERRIDES
	//==================================================================================================================

	//==================================================================================================================
	// METHODS
	//==================================================================================================================

	void SuperconStateMachine.IState.EnterState(SuperconStateMachine.Transition transition)
		=> this.AsActivity().Start($"{nameof(SuperconStateMachine)}.{nameof(SuperconStateMachine.Transition)}", transition);
	void SuperconStateMachine.IState.ExitState(SuperconStateMachine.Transition transition)
		=> this.AsActivity().Finish($"{nameof(SuperconStateMachine)}.{nameof(SuperconStateMachine.Transition)}", transition);

	public void QueueTransition() => this.StateMachineOwner?.StateMachine.QueueTransition(this);
	public void QueueTransition(Variant data) => this.StateMachineOwner?.StateMachine.QueueTransition(this, data);
}
