using Godot;
using Raele.GodotUtils.ActivitySystem;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon2D;

[Tool][GlobalClass][Icon($"res://addons/{nameof(Supercon2D)}/icons/character_body_neutral.png")]
public partial class SuperconState : Activity, SuperconStateMachine.IState
{
	//==================================================================================================================
	// EXPORTS
	//==================================================================================================================

	//==================================================================================================================
	// FIELDS
	//==================================================================================================================

	public ISuperconStateMachineOwner? StateMachineOwner => this.GetAncestorOrDefault<ISuperconStateMachineOwner>();

	//==================================================================================================================
	// PROPERTIES
	//==================================================================================================================

	public bool IsPreviousActiveState => this.StateMachineOwner?.StateMachine.PreviousActiveState == this;
	// TOOD Do we really need this class here?
	public SuperconInputController? InputController => this.GetAncestorOrDefault<SuperconBody2D>()?.InputController
		?? this.GetAncestorOrDefault<SuperconBody3D>()?.InputController;

	//==================================================================================================================
	// SIGNALS
	//==================================================================================================================

	//==================================================================================================================
	// OVERRIDES
	//==================================================================================================================

	//==================================================================================================================
	// METHODS
	//==================================================================================================================

	// TODO Change the return of this.AsActivity().Start() to bool to indicate whether the start was successful, and[
	// cancel the transition if not. Also do the same for Finish.
	void SuperconStateMachine.IState.EnterState(SuperconStateMachine.Transition transition)
		=> this.AsActivity().Start($"{nameof(SuperconStateMachine)}.{nameof(SuperconStateMachine.Transition)}", transition);
	void SuperconStateMachine.IState.ExitState(SuperconStateMachine.Transition transition)
		=> this.AsActivity().Finish($"{nameof(SuperconStateMachine)}.{nameof(SuperconStateMachine.Transition)}", transition);

	public void QueueTransition() => this.StateMachineOwner?.StateMachine.QueueTransition(this);
	public void QueueTransition(Variant data) => this.StateMachineOwner?.StateMachine.QueueTransition(this, data);
}
