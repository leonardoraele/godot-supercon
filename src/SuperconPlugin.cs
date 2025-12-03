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

		Texture2D stateIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_neutral.png");
		// Texture2D genericStateIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_bg.png");
		Texture2D gravityIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_gravity.png");
		Texture2D singleAxisControlIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_single_axis_control.png");
		Texture2D animatedSprite = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_animated_sprite_3.png");
		Texture2D binaryIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_binary.png");
		Texture2D keyboardIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_keyboard.png");
		Texture2D dualAxisControlIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_dual_axis_control.png");
		Texture2D forceIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_force_2.png");
		Texture2D slopeIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_slope.png");
		Texture2D impulseIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_impulse.png");
		Texture2D presetIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_preset.png");

		// Core Types
		this.AddCustomType(nameof(SuperconBody2D), nameof(CharacterBody2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconBody2D)}.cs"), null);
		this.AddCustomType(nameof(SuperconState), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconState)}.cs"), stateIcon);

		// State Controllers
		this.AddCustomType(nameof(AnimationComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(AnimationComponent)}.cs"), animatedSprite);
		this.AddCustomType(nameof(BothAxisControlComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(BothAxisControlComponent)}.cs"), dualAxisControlIcon);
		this.AddCustomType(nameof(PresetMovementComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(PresetMovementComponent)}.cs"), presetIcon);
		this.AddCustomType(nameof(ForceComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(ForceComponent)}.cs"), forceIcon);
		this.AddCustomType(nameof(GravityComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(GravityComponent)}.cs"), gravityIcon);
		this.AddCustomType(nameof(ImpulseComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(ImpulseComponent)}.cs"), impulseIcon);
		this.AddCustomType(nameof(SingleAxisControlComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(SingleAxisControlComponent)}.cs"), singleAxisControlIcon);
		this.AddCustomType(nameof(SlopeComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(SlopeComponent)}.cs"), slopeIcon);

		// State Transitions
		this.AddCustomType(nameof(InputActionTransition), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateTransitions)}/{nameof(InputActionTransition)}.cs"), keyboardIcon);
		this.AddCustomType(nameof(ConditionalStateTransition), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateTransitions)}/{nameof(ConditionalStateTransition)}.cs"), binaryIcon);
	}

	public override void _ExitTree()
	{
		this.RemoveCustomType(nameof(SuperconBody2D));
		this.RemoveCustomType(nameof(SuperconState));

		this.RemoveCustomType(nameof(AnimationComponent));
		this.RemoveCustomType(nameof(BothAxisControlComponent));
		this.RemoveCustomType(nameof(PresetMovementComponent));
		this.RemoveCustomType(nameof(ForceComponent));
		this.RemoveCustomType(nameof(GravityComponent));
		this.RemoveCustomType(nameof(ImpulseComponent));
		this.RemoveCustomType(nameof(SingleAxisControlComponent));
		this.RemoveCustomType(nameof(SlopeComponent));

		this.RemoveCustomType(nameof(InputActionTransition));
		this.RemoveCustomType(nameof(ConditionalStateTransition));
	}
}
#endif
