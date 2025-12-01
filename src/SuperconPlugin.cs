#if TOOLS
using Godot;
using Raele.Supercon2D.StateComponents;
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
		this.AddCustomType(nameof(AnimationComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(AnimationComponent)}.cs"), null);
		this.AddCustomType(nameof(ForceComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(ForceComponent)}.cs"), null);
		this.AddCustomType(nameof(ImpulseComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(ImpulseComponent)}.cs"), null);
		this.AddCustomType(nameof(AxisPresetMovementComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(AxisPresetMovementComponent)}.cs"), null);
		this.AddCustomType(nameof(AxisControlComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(AxisControlComponent)}.cs"), null);
		this.AddCustomType(nameof(FullControlComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(FullControlComponent)}.cs"), null);
		this.AddCustomType(nameof(SmoothSlopeComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(SmoothSlopeComponent)}.cs"), null);

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

		this.RemoveCustomType(nameof(AnimationComponent));
		this.RemoveCustomType(nameof(ForceComponent));
		this.RemoveCustomType(nameof(ImpulseComponent));
		this.RemoveCustomType(nameof(AxisPresetMovementComponent));
		this.RemoveCustomType(nameof(AxisControlComponent));
		this.RemoveCustomType(nameof(FullControlComponent));
		this.RemoveCustomType(nameof(SmoothSlopeComponent));

		this.RemoveCustomType(nameof(InputActionTransition));
		this.RemoveCustomType(nameof(ConditionalStateTransition));
	}
}
#endif
