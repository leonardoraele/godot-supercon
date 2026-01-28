#if TOOLS
using Godot;
using Raele.Supercon2D.StateComponents;

namespace Raele.Supercon2D;

[Tool]
public partial class SuperconPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		Texture2D multiAxisControlIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_multi_axis_control.png");
		Texture2D forceIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_force_2.png");
		Texture2D slopeIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_slope.png");
		Texture2D impulseIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_impulse.png");
		Texture2D presetIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_preset.png");
		Texture2D facingIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_facing.png");
		Texture2D animationPlayIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_animation_play.png");
		Texture2D animationParamIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_animation_param.png");
		Texture2D gateIdon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_t_gate.png");
		Texture2D velocityResetIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_velocity_reset.png");

		// Core Types
		this.AddCustomType(nameof(SuperconBody2D), nameof(CharacterBody2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconBody2D)}.cs"), null);

		// State Components
		this.AddCustomType(nameof(PlayAnimationComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(PlayAnimationComponent)}.cs"), animationPlayIcon);
		this.AddCustomType(nameof(AnimationParamComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(AnimationParamComponent)}.cs"), animationParamIcon);
		this.AddCustomType(nameof(ForceComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(ForceComponent)}.cs"), forceIcon);
		this.AddCustomType(nameof(ImpulseComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(ImpulseComponent)}.cs"), impulseIcon);
		this.AddCustomType(nameof(MultiAxisControlComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(MultiAxisControlComponent)}.cs"), multiAxisControlIcon);
		this.AddCustomType(nameof(PresetMovementComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(PresetMovementComponent)}.cs"), presetIcon);
		this.AddCustomType(nameof(SlopeComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(SlopeComponent)}.cs"), slopeIcon);
		this.AddCustomType(nameof(TransitionGateComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(TransitionGateComponent)}.cs"), gateIdon);
		this.AddCustomType(nameof(VelocityResetComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(VelocityResetComponent)}.cs"), velocityResetIcon);
	}

	public override void _ExitTree()
	{}
}
#endif
