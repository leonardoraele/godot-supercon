using Godot;
using Raele.Supercon2D.StateControllers;

namespace Raele.Supercon2D.StateTransitions;

public partial class ConditionalStateTransition : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public Node? Subject;
	[Export(PropertyHint.EnumSuggestion, "is_on_ceiling,is_on_floor,is_on_wall")] public string FieldOrMethodName = "";
	[Export] public Variant ExpectedValue = true;
	[Export] public SuperconState? TargetState;

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _ProcessActive(double delta)
	{
		if (
			this.TargetState != null
			&& this.ExpectedValue.Equals(
				this.Subject?.HasMethod(this.FieldOrMethodName) == true
					? this.Subject.Call(this.FieldOrMethodName)
					: this.Subject?.Get(this.FieldOrMethodName)
			)
		)
		{
			this.StateMachine.QueueTransition(this.TargetState);
		}
	}
}
