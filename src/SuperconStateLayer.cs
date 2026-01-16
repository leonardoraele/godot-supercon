using Godot;

namespace Raele.Supercon2D;

[Tool][GlobalClass]
public partial class SuperconStateLayer : SuperconState, ISuperconStateMachineOwner
{
	//------------------------------------------------------------------------------------------------------------------
	// EXPORTS
	//------------------------------------------------------------------------------------------------------------------

	[Export] public SuperconState? RestState { get; set; }

	[ExportGroup("Debug", "Debug")]
	[Export] public bool DebugPrintStateChanges
	{
		get => this.StateMachine.DebugPrintContext != null;
		set => this.StateMachine.DebugPrintContext = value ? this : null;
	}

	//------------------------------------------------------------------------------------------------------------------
	// FIELDS
	//------------------------------------------------------------------------------------------------------------------

	public SuperconStateMachine StateMachine { get; } = new();

	//------------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	//------------------------------------------------------------------------------------------------------------------

	//------------------------------------------------------------------------------------------------------------------
	// SIGNALS
	//------------------------------------------------------------------------------------------------------------------

	//------------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	//------------------------------------------------------------------------------------------------------------------

	public override void _EnterTree()
	{
		base._EnterTree();
		if (Engine.IsEditorHint())
		{
			return;
		}
		this.Started += this.OnStateEntered;
		this.Finished += this.OnStateExited;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (Engine.IsEditorHint())
		{
			return;
		}
		this.Started -= this.OnStateEntered;
		this.Finished -= this.OnStateExited;
	}

	//------------------------------------------------------------------------------------------------------------------
	// METHODS
	//------------------------------------------------------------------------------------------------------------------

	Node ISuperconStateMachineOwner.AsNode() => this;
	public ISuperconStateMachineOwner AsStateMachineOwner() => this;

	private void OnStateEntered(string mode, Variant payload)
		=> this.AsStateMachineOwner().QueueTransition(this.RestState);
	private void OnStateExited(string reason, Variant details)
		=> this.AsStateMachineOwner().Stop();
}
