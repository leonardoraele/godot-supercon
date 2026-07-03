#if TOOLS
using Godot;
using Raele.Supercon.StateComponents2D;
using Raele.Supercon.StateComponents3D;

namespace Raele.Supercon;

[Tool]
public partial class SuperconPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		this.AddCustomType($"{nameof(SuperconState)}2D", nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(SuperconState)}.cs"), GD.Load<Texture2D>($"res://addons/{nameof(Supercon)}/icons/character_body_neutral.png"));
		this.AddCustomType($"{nameof(SuperconStateLayer)}2D", nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(SuperconStateLayer)}.cs"), GD.Load<Texture2D>($"res://addons/{nameof(Supercon)}/icons/character_body_neutral.png"));

		this.AddCustomType($"{nameof(SuperconState)}3D", nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(SuperconState)}.cs"), GD.Load<Texture2D>($"res://addons/{nameof(Supercon)}/icons/character_body_neutral-3d.png"));
		this.AddCustomType($"{nameof(SuperconStateLayer)}3D", nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(SuperconStateLayer)}.cs"), GD.Load<Texture2D>($"res://addons/{nameof(Supercon)}/icons/character_body_neutral-3d.png"));

		// Base classes
		this.AddCustomType(nameof(SuperconStateComponent), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(SuperconStateComponent)}.cs"), null);
		this.AddCustomType(nameof(SuperconStateComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(SuperconStateComponent2D)}.cs"), null);
		this.AddCustomType(nameof(SuperconStateComponent3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(SuperconStateComponent3D)}.cs"), null);

		// 2D Components
		this.AddCustomType(nameof(PlayAnimationComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(PlayAnimationComponent2D)}.cs"), null);
		this.AddCustomType(nameof(AnimationParamComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(AnimationParamComponent2D)}.cs"), null);
		this.AddCustomType(nameof(CustomTriggerComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(CustomTriggerComponent2D)}.cs"), null);
		this.AddCustomType(nameof(ForceComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(ForceComponent2D)}.cs"), null);
		this.AddCustomType(nameof(GravityComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(GravityComponent2D)}.cs"), null);
		this.AddCustomType(nameof(ImpulseComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(ImpulseComponent2D)}.cs"), null);
		this.AddCustomType(nameof(InputActionComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(InputActionComponent2D)}.cs"), null);
		this.AddCustomType(nameof(MultiAxisControlComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(MultiAxisControlComponent2D)}.cs"), null);
		this.AddCustomType(nameof(PresetMovementComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(PresetMovementComponent2D)}.cs"), null);
		this.AddCustomType(nameof(SingleAxisControlComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(SingleAxisControlComponent2D)}.cs"), null);
		this.AddCustomType(nameof(SlopeComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(SlopeComponent2D)}.cs"), null);
		this.AddCustomType(nameof(SpriteAnimationComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(SpriteAnimationComponent2D)}.cs"), null);
		this.AddCustomType(nameof(TransitionGateComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(TransitionGateComponent2D)}.cs"), null);
		this.AddCustomType(nameof(VelocityResetComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents2D)}/{nameof(VelocityResetComponent2D)}.cs"), null);

		// 3D Components
		this.AddCustomType(nameof(ForceComponent3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents3D)}/{nameof(ForceComponent3D)}.cs"), null);
		this.AddCustomType(nameof(GravityComponent3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents3D)}/{nameof(GravityComponent3D)}.cs"), null);
		this.AddCustomType(nameof(ImpulseComponent3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents3D)}/{nameof(ImpulseComponent3D)}.cs"), null);
		this.AddCustomType(nameof(SurfaceControlComponent3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Supercon)}/src/{nameof(StateComponents3D)}/{nameof(SurfaceControlComponent3D)}.cs"), null);
	}

	public override void _ExitTree()
	{
		this.RemoveCustomType($"{nameof(SuperconState)}2D");
		this.RemoveCustomType($"{nameof(SuperconStateLayer)}2D");

		this.RemoveCustomType($"{nameof(SuperconState)}3D");
		this.RemoveCustomType($"{nameof(SuperconStateLayer)}3D");

		this.RemoveCustomType(nameof(SuperconStateComponent));
		this.RemoveCustomType(nameof(SuperconStateComponent2D));
		this.RemoveCustomType(nameof(SuperconStateComponent3D));

		this.RemoveCustomType(nameof(PlayAnimationComponent2D));
		this.RemoveCustomType(nameof(AnimationParamComponent2D));
		this.RemoveCustomType(nameof(CustomTriggerComponent2D));
		this.RemoveCustomType(nameof(ForceComponent2D));
		this.RemoveCustomType(nameof(GravityComponent2D));
		this.RemoveCustomType(nameof(ImpulseComponent2D));
		this.RemoveCustomType(nameof(InputActionComponent2D));
		this.RemoveCustomType(nameof(MultiAxisControlComponent2D));
		this.RemoveCustomType(nameof(PresetMovementComponent2D));
		this.RemoveCustomType(nameof(SingleAxisControlComponent2D));
		this.RemoveCustomType(nameof(SlopeComponent2D));
		this.RemoveCustomType(nameof(SpriteAnimationComponent2D));
		this.RemoveCustomType(nameof(TransitionGateComponent2D));
		this.RemoveCustomType(nameof(VelocityResetComponent2D));

		this.RemoveCustomType(nameof(ForceComponent3D));
		this.RemoveCustomType(nameof(GravityComponent3D));
		this.RemoveCustomType(nameof(ImpulseComponent3D));
		this.RemoveCustomType(nameof(SurfaceControlComponent3D));
	}
}
#endif
