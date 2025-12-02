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
		this.AddCustomType(nameof(SuperconInputMapping), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconInputMapping)}.cs"), null);
		this.AddCustomType(nameof(SuperconStateMachine), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconStateMachine)}.cs"), null);
		this.AddCustomType(nameof(SuperconState), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconState)}.cs"), null);

		// State Controllers
		this.AddCustomType(nameof(AnimationComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(AnimationComponent)}.cs"), null);
		this.AddCustomType(nameof(BothAxisControlComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(BothAxisControlComponent)}.cs"), null);
		this.AddCustomType(nameof(DirectionalPresetMovementComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(DirectionalPresetMovementComponent)}.cs"), null);
		this.AddCustomType(nameof(ForceComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(ForceComponent)}.cs"), null);
		this.AddCustomType(nameof(GravityComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(GravityComponent)}.cs"), null);
		this.AddCustomType(nameof(ImpulseComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(ImpulseComponent)}.cs"), null);
		this.AddCustomType(nameof(SingleAxisControlComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(SingleAxisControlComponent)}.cs"), null);
		this.AddCustomType(nameof(SingleAxisPresetMovementComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(SingleAxisPresetMovementComponent)}.cs"), null);
		this.AddCustomType(nameof(SlopeComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(SlopeComponent)}.cs"), null);

		// State Transitions
		this.AddCustomType(nameof(InputActionTransition), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateTransitions)}/{nameof(InputActionTransition)}.cs"), null);
		this.AddCustomType(nameof(ConditionalStateTransition), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateTransitions)}/{nameof(ConditionalStateTransition)}.cs"), null);
	}

	public override void _ExitTree()
	{
		this.RemoveCustomType(nameof(SuperconBody2D));
		this.RemoveCustomType(nameof(SuperconInputMapping));
		this.RemoveCustomType(nameof(SuperconStateMachine));
		this.RemoveCustomType(nameof(SuperconState));

		this.RemoveCustomType(nameof(AnimationComponent));
		this.RemoveCustomType(nameof(BothAxisControlComponent));
		this.RemoveCustomType(nameof(DirectionalPresetMovementComponent));
		this.RemoveCustomType(nameof(ForceComponent));
		this.RemoveCustomType(nameof(GravityComponent));
		this.RemoveCustomType(nameof(ImpulseComponent));
		this.RemoveCustomType(nameof(SingleAxisControlComponent));
		this.RemoveCustomType(nameof(SingleAxisPresetMovementComponent));
		this.RemoveCustomType(nameof(SlopeComponent));

		this.RemoveCustomType(nameof(InputActionTransition));
		this.RemoveCustomType(nameof(ConditionalStateTransition));
	}
}
#endif
