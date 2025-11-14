#if TOOLS
using Godot;
using Raele.Supercon2D.StateControllers;
using Raele.Supercon2D.StateTransitions;

namespace Raele.Supercon2D;

[Tool]
public partial class SuperconPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		this.AddDebuggerPlugin(new EditorDebuggerPlugin());

		// Core Types
		this.AddCustomType(nameof(SuperconBody2D), nameof(CharacterBody2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconBody2D)}.cs"), null);
		this.AddCustomType(nameof(SuperconInputManager), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconInputManager)}.cs"), null);
		this.AddCustomType(nameof(SuperconStateMachine), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconStateMachine)}.cs"), null);
		this.AddCustomType(nameof(SuperconState), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconState)}.cs"), null);

		// State Controllers
		this.AddCustomType(nameof(HorizontalComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateControllers)}/{nameof(HorizontalComponent)}.cs"), null);
		this.AddCustomType(nameof(GravityComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateControllers)}/{nameof(GravityComponent)}.cs"), null);
		this.AddCustomType(nameof(JumpComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateControllers)}/{nameof(JumpComponent)}.cs"), null);
		this.AddCustomType(nameof(WallClimbComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateControllers)}/{nameof(WallClimbComponent)}.cs"), null);
		this.AddCustomType(nameof(WallSlideComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateControllers)}/{nameof(WallSlideComponent)}.cs"), null);

		// State Transitions
		this.AddCustomType(nameof(InputActionTransition), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateTransitions)}/{nameof(InputActionTransition)}.cs"), null);
		this.AddCustomType(nameof(ConditionalStateTransition), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateTransitions)}/{nameof(ConditionalStateTransition)}.cs"), null);
	}

	public override void _ExitTree()
	{
		this.RemoveCustomType(nameof(SuperconBody2D));
		this.RemoveCustomType(nameof(SuperconInputManager));
		this.RemoveCustomType(nameof(SuperconStateMachine));
		this.RemoveCustomType(nameof(SuperconState));

		this.RemoveCustomType(nameof(HorizontalComponent));
		this.RemoveCustomType(nameof(GravityComponent));
		this.RemoveCustomType(nameof(JumpComponent));
		this.RemoveCustomType(nameof(WallClimbComponent));
		this.RemoveCustomType(nameof(WallSlideComponent));

		this.RemoveCustomType(nameof(InputActionTransition));
		this.RemoveCustomType(nameof(ConditionalStateTransition));
	}
}
#endif
