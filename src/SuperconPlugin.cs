#if TOOLS
using Godot;
using Raele.Supercon.StateComponents2D;

namespace Raele.Supercon;

[Tool]
public partial class SuperconPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		Texture2D stateIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon)}/icons/character_body_neutral.png.png");

		this.AddCustomType($"{nameof(SuperconState)}2D", nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(SuperconState)}.cs"), stateIcon);
		this.AddCustomType($"{nameof(SuperconStateLayer)}2D", nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(SuperconStateLayer)}.cs"), stateIcon);

		this.AddCustomType($"{nameof(SuperconState)}3D", nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(SuperconState)}.cs"), stateIcon);
		this.AddCustomType($"{nameof(SuperconStateLayer)}3D", nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(SuperconStateLayer)}.cs"), stateIcon);

		this.AddCustomType(nameof(SuperconStateComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(SuperconStateComponent)}.cs"), null);
		this.AddCustomType(nameof(PlayAnimationComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(PlayAnimationComponent)}.cs"), null);
		this.AddCustomType(nameof(AnimationParamComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(AnimationParamComponent)}.cs"), null);
		this.AddCustomType(nameof(CustomTriggerComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(CustomTriggerComponent)}.cs"), null);
		this.AddCustomType(nameof(ForceComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(ForceComponent)}.cs"), null);
		this.AddCustomType(nameof(GravityComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(GravityComponent)}.cs"), null);
		this.AddCustomType(nameof(ImpulseComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(ImpulseComponent)}.cs"), null);
		this.AddCustomType(nameof(InputActionComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(InputActionComponent)}.cs"), null);
		this.AddCustomType(nameof(MultiAxisControlComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(MultiAxisControlComponent)}.cs"), null);
		this.AddCustomType(nameof(PresetMovementComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(PresetMovementComponent)}.cs"), null);
		this.AddCustomType(nameof(SingleAxisControlComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(SingleAxisControlComponent)}.cs"), null);
		this.AddCustomType(nameof(SlopeComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(SlopeComponent)}.cs"), null);
		this.AddCustomType(nameof(SpriteAnimationComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(SpriteAnimationComponent)}.cs"), null);
		this.AddCustomType(nameof(TransitionGateComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(TransitionGateComponent)}.cs"), null);
		this.AddCustomType(nameof(VelocityResetComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(VelocityResetComponent)}.cs"), null);
	}

	public override void _ExitTree()
	{
		this.RemoveCustomType($"{nameof(SuperconState)}2D");
		this.RemoveCustomType($"{nameof(SuperconStateLayer)}2D");

		this.RemoveCustomType($"{nameof(SuperconState)}3D");
		this.RemoveCustomType($"{nameof(SuperconStateLayer)}3D");

		this.RemoveCustomType(nameof(PlayAnimationComponent));
		this.RemoveCustomType(nameof(AnimationParamComponent));
		this.RemoveCustomType(nameof(CustomTriggerComponent));
		this.RemoveCustomType(nameof(ForceComponent));
		this.RemoveCustomType(nameof(GravityComponent));
		this.RemoveCustomType(nameof(ImpulseComponent));
		this.RemoveCustomType(nameof(InputActionComponent));
		this.RemoveCustomType(nameof(MultiAxisControlComponent));
		this.RemoveCustomType(nameof(PresetMovementComponent));
		this.RemoveCustomType(nameof(SingleAxisControlComponent));
		this.RemoveCustomType(nameof(SlopeComponent));
		this.RemoveCustomType(nameof(SpriteAnimationComponent));
		this.RemoveCustomType(nameof(TransitionGateComponent));
		this.RemoveCustomType(nameof(VelocityResetComponent));
	}
}
#endif
