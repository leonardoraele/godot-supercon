using Godot;

namespace Raele.Supercon2D.StateComponents;

[Tool]
public abstract partial class SuperconStateComponent : Node2D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public bool Enabled = true;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public SuperconState State => field ??= this.GetParent<SuperconState>();
	public SuperconBody2D Character => this.State.Character;
	public SuperconInputMapping InputMapping => this.Character.InputMapping;
	public SuperconStateMachine StateMachine => this.Character.StateMachine;

	// -----------------------------------------------------------------------------------------------------------------
	// VIRTUALS & OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	private void _supercon_enter(SuperconStateMachine.Transition transition)
	{
		if (this.Enabled)
		{
			this._SuperconEnter();
		}
	}

	private void _supercon_exit(SuperconStateMachine.Transition transition)
	{
		if (this.Enabled)
		{
			this._SuperconExit();
		}
	}

	private void _supercon_process(double delta)
	{
		if (this.Enabled)
		{
			this._SuperconProcess(delta);
		}
	}

	private void _supercon_physics_process(double delta)
	{
		if (this.Enabled)
		{
			this._SuperconPhysicsProcess(delta);
		}
	}

	public virtual void _SuperconEnter() { }
	public virtual void _SuperconExit() { }
	public virtual void _SuperconProcess(double delta) { }
	public virtual void _SuperconPhysicsProcess(double delta) {}
}
