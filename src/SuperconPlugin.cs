#if TOOLS
using Godot;
using Raele.Supercon2D.StateComponents;

namespace Raele.Supercon2D;

[Tool]
public partial class SuperconPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		Texture2D stateIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_neutral.png");
		// Texture2D genericStateIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_bg.png");
		Texture2D gravityIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_gravity.png");
		Texture2D singleAxisControlIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_single_axis_control.png");
		Texture2D animatedSprite = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_animated_sprite_3.png");
		Texture2D binaryIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_binary.png");
		Texture2D keyboardIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_keyboard.png");
		Texture2D multiAxisControlIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_multi_axis_control.png");
		Texture2D forceIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_force_2.png");
		Texture2D slopeIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_slope.png");
		Texture2D impulseIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_impulse.png");
		Texture2D presetIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_preset.png");
		Texture2D facingIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_facing.png");

		// Core Types
		this.AddCustomType(nameof(SuperconBody2D), nameof(CharacterBody2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconBody2D)}.cs"), null);
		this.AddCustomType(nameof(SuperconState), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconState)}.cs"), stateIcon);

		// State Components
		this.AddCustomType(nameof(ExpressionComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(ExpressionComponent)}.cs"), binaryIcon);
		this.AddCustomType(nameof(FacingDirectionComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(FacingDirectionComponent)}.cs"), facingIcon);
		this.AddCustomType(nameof(ForceComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(ForceComponent)}.cs"), forceIcon);
		this.AddCustomType(nameof(GravityComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(GravityComponent)}.cs"), gravityIcon);
		this.AddCustomType(nameof(ImpulseComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(ImpulseComponent)}.cs"), impulseIcon);
		this.AddCustomType(nameof(InputActionComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(InputActionComponent)}.cs"), keyboardIcon);
		this.AddCustomType(nameof(MultiAxisControlComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(MultiAxisControlComponent)}.cs"), multiAxisControlIcon);
		this.AddCustomType(nameof(PresetMovementComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(PresetMovementComponent)}.cs"), presetIcon);
		this.AddCustomType(nameof(SingleAxisControlComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(SingleAxisControlComponent)}.cs"), singleAxisControlIcon);
		this.AddCustomType(nameof(SlopeComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(SlopeComponent)}.cs"), slopeIcon);
		this.AddCustomType(nameof(SpriteAnimationComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(SpriteAnimationComponent)}.cs"), animatedSprite);
	}

	public override void _ExitTree()
	{
		this.RemoveCustomType(nameof(SuperconBody2D));
		this.RemoveCustomType(nameof(SuperconState));

		this.RemoveCustomType(nameof(ExpressionComponent));
		this.RemoveCustomType(nameof(FacingDirectionComponent));
		this.RemoveCustomType(nameof(ForceComponent));
		this.RemoveCustomType(nameof(GravityComponent));
		this.RemoveCustomType(nameof(ImpulseComponent));
		this.RemoveCustomType(nameof(InputActionComponent));
		this.RemoveCustomType(nameof(MultiAxisControlComponent));
		this.RemoveCustomType(nameof(PresetMovementComponent));
		this.RemoveCustomType(nameof(SingleAxisControlComponent));
		this.RemoveCustomType(nameof(SlopeComponent));
		this.RemoveCustomType(nameof(SpriteAnimationComponent));
	}
}
#endif
