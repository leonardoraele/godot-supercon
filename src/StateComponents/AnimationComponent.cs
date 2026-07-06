using System.Linq;
using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon.StateComponents;

[GlobalClass]
public partial class AnimationComponent : SuperconStateComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public AnimationMixer? PlayerOrTree
		{ get; set { field = value; this.NotifyPropertyListChanged(); } }

	[Export] public UpdateStrategyEnum UpdateStrategy = UpdateStrategyEnum.OnActivityStarted;

	[Export] public string[] PlayAnimations = [];

	[Export] public Godot.Collections.Dictionary<string, string> Parameters = [];
	[Export] public bool UnsetConditionsOnExit = true;

	[ExportGroup("Additional Options")]
	[Export] public bool RunInEditor = false;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------



	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public AnimationPlayer? AnimationPlayer => this.PlayerOrTree as AnimationPlayer;
	public AnimationTree? AnimationTree => this.PlayerOrTree as AnimationTree;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	// TODO Relay animations from the animation mixer while this activity is active
	// [Signal] public delegate void EventHandler()

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum UpdateStrategyEnum {
		OnProcess,
		OnPhysicsProcess,
		OnActivityStarted,
		OnActivityFinished,
		Never,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.Parameters):
				if (this.AnimationTree == null)
				{
					property["usage"] = (ulong) PropertyUsageFlags.None;
					break;
				}
				string conditions = this.AnimationTree.GetPropertyList()
					.Where(prop => prop["name"].AsString().StartsWith("parameters/"))
					.Select(prop => prop["name"].AsString().Replace("parameters/", ""))
					.JoinIntoString(",");
				property["type"] = Variant.Type.Dictionary.As<long>();
				property["hint"] = PropertyHint.DictionaryType.As<long>();
				property["hint_string"] = $"String/{PropertyHint.Enum:D}:{conditions};String/{PropertyHint.Expression:D}";
				break;
			case nameof(this.PlayAnimations):
				if (this.AnimationPlayer == null)
				{
					property["usage"] = (ulong) PropertyUsageFlags.None;
					break;
				}
				string animations = this.AnimationPlayer.GetAnimationList().JoinIntoString(",");
				property["type"] = Variant.Type.Array.As<long>();
				property["hint"] = PropertyHint.ArrayType.As<long>();
				property["hint_string"] = $"String/{PropertyHint.EnumSuggestion:D}:{animations}";
				break;
		}
	}

	// public override string[] _GetConfigurationWarnings()
	// 	=> base._PhysicsProcess(delta);

	// public override void _EnterTree()
	// {
	// 	base._EnterTree();
	// }

	// public override void _ExitTree()
	// {
	// 	base._ExitTree();
	// }

	// public override void _Ready()
	// {
	// 	base._Ready();
	// }

	// public override void _Process(double delta)
	// {
	// 	base._Process(delta);
	// }

	// public override void _PhysicsProcess(double delta)
	// {
	// 	base._PhysicsProcess(delta);
	// }

	protected override void _ActivityProcess(double delta)
	{
		base._ActivityProcess(delta);
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.UpdateStrategy == UpdateStrategyEnum.OnProcess)
			this.UpdateAnimation();
	}

	protected override void _ActivityPhysicsProcess(double delta)
	{
		base._ActivityPhysicsProcess(delta);
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.UpdateStrategy == UpdateStrategyEnum.OnPhysicsProcess)
			this.UpdateAnimation();
	}

	protected override void _ActivityStarted(string mode, Variant argument)
	{
		base._ActivityStarted(mode, argument);
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.UpdateStrategy == UpdateStrategyEnum.OnActivityStarted)
			this.UpdateAnimation();
	}

	protected override void _ActivityFinished(string reason, Variant details)
	{
		base._ActivityFinished(reason, details);
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.UpdateStrategy == UpdateStrategyEnum.OnActivityFinished)
			this.UpdateAnimation();
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void UpdateAnimation()
	{
		if (this.AnimationPlayer != null)
			this.UpdateAnimationPlayer(this.AnimationPlayer);
		else if (this.AnimationTree != null)
			this.UpdateAnimationTree(this.AnimationTree);
	}

	private void UpdateAnimationPlayer(AnimationPlayer player)
	{
		if (this.PlayAnimations == null || this.PlayAnimations.Length == 0)
			return;
		player.Stop();
		player.Play("REST");
		player.Advance(0);
		player.Play(this.PlayAnimations[0]);
		foreach (string animation in this.PlayAnimations.Skip(1))
			player.Queue(animation);
	}

	private void UpdateAnimationTree(AnimationTree tree)
	{
		foreach ((string? parameter, string? expression) in this.Parameters)
			if (!string.IsNullOrWhiteSpace(parameter) && !string.IsNullOrWhiteSpace(expression))
				this.UpdateCondition(tree, parameter, expression);
	}

	private void UpdateCondition(AnimationTree tree, string parameter, string expression)
	{

	}
}
