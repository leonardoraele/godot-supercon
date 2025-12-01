using Godot;
using Raele.GodotUtils;

namespace Raele.Supercon2D;

[Tool]
public partial class SuperconState : GodotUtils.StateMachine.BaseState
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[ExportGroup("Options")]
	[Export(PropertyHint.Flags, "X:1,Y:2")] public byte ResetVelocityOnEnter = 0;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public SuperconBody2D Character => field != null ? field : field = this.RequireAncestor<SuperconBody2D>();

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public SuperconInputManager InputManager => this.Character.InputManager;

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterState()
	{
		base._EnterState();
		if ((this.ResetVelocityOnEnter & 1) != 0)
		{
			this.Character.VelocityX = 0;
		}
		if ((this.ResetVelocityOnEnter & 2) != 0)
		{
			this.Character.VelocityY = 0;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		base._PhysicsProcess(delta);
		Callable.From(this.Character.MoveAndSlide).CallDeferred();
	}
}
