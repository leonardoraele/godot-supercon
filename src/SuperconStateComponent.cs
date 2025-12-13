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

	public override void _EnterTree()
	{
		base._ExitTree();
		this.GetParentOrNull<SuperconState>()?.Connect(SuperconState.SignalName.StateEntered, new Callable(this, MethodName._SuperconEnter));
		this.GetParentOrNull<SuperconState>()?.Connect(SuperconState.SignalName.StateExited, new Callable(this, MethodName._SuperconExit));
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		this.GetParentOrNull<SuperconState>()?.Disconnect(SuperconState.SignalName.StateEntered, new Callable(this, MethodName._SuperconEnter));
		this.GetParentOrNull<SuperconState>()?.Disconnect(SuperconState.SignalName.StateExited, new Callable(this, MethodName._SuperconExit));
	}

	public override void _Process(double delta)
	{
		if (this.Enabled)
		{
			this._SuperconProcess(delta);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (this.Enabled)
		{
			this._SuperconPhysicsProcess(delta);
		}
	}

	public virtual void _SuperconEnter(SuperconStateMachine.Transition transition) { }
	public virtual void _SuperconExit(SuperconStateMachine.Transition transition) { }
	public virtual void _SuperconProcess(double delta) { }
	public virtual void _SuperconPhysicsProcess(double delta) {}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void OnStateEnter(SuperconStateMachine.Transition transition)
	{
		if (this.Enabled && this.CanProcess())
		{
			this._SuperconEnter(transition);
		}
	}

	private void OnStateExit(SuperconStateMachine.Transition transition)
	{
		if (this.Enabled && this.CanProcess())
		{
			this._SuperconExit(transition);
		}
	}
}
