using Godot;
using Raele.GodotUtils;

namespace Raele.Supercon2D.StateComponents;

[Tool]
public partial class SuperconStateController : Node
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public bool Enabled = true;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public SuperconState State => field ??= this.RequireAncestor<SuperconState>();
	public SuperconBody2D Character => this.State.Character;
	public SuperconInputMapping InputMapping => this.Character.InputMapping;
	public SuperconStateMachine StateMachine => this.Character.StateMachine;

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterTree()
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		base._EnterTree();
		this.State.StateEntered += this.OnEnterState;
		this.State.StateExited += this.OnExitState;
	}

	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		base._Process(delta);
		if (this.State.IsActive && this.Enabled)
		{
			this._ProcessActive(delta);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		base._PhysicsProcess(delta);
		if (this.State.IsActive && this.Enabled)
		{
			this._PhysicsProcessActive(delta);
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// VIRTUAL METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void OnEnterState()
	{
		if (this.Enabled)
		{
			this._EnterState();
		}
	}

	private void OnExitState()
	{
		if (this.Enabled)
		{
			this._ExitState();
		}
	}

	public virtual void _EnterState() { }
	public virtual void _ExitState() { }
	public virtual void _ProcessActive(double delta) { }
	public virtual void _PhysicsProcessActive(double delta) {}

}
