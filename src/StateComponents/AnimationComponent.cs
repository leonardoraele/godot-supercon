using System.Collections.Generic;
using System.Linq;
using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon.StateComponents;

[Tool][GlobalClass]
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

	[ExportGroup("Set Parameters", "Expression")]
	[Export] public Node? ExpressionContext
		{ get => field ??= this.Owner; set; }
	[Export] public Godot.Collections.Dictionary ExpressionParameters = [];
	[ExportSubgroup("Expression Options")]
	[Export(PropertyHint.DictionaryType, "String;Variant")] public Godot.Collections.Dictionary ExpressionVariables = [];
	[Export] public bool ExpressionUnsetConditionsOnExit = false;

	[ExportGroup("Debug")]
	[Export] public bool RunInEditor = false;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private Dictionary<string, Expression> Interpreters = new();

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public AnimationPlayer? AnimationPlayer => this.PlayerOrTree as AnimationPlayer;
	public AnimationTree? AnimationTree => this.PlayerOrTree as AnimationTree;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void AnimationFinishedEventHandler(string animationName);
	[Signal] public delegate void AnimationStartedEventHandler(string animationName);
	[Signal] public delegate void AnimationChangedEventHandler(string oldAnimation, string newAnimation);
	[Signal] public delegate void CurrentAnimationChangedEventHandler(string animationName);

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
			case nameof(this.ExpressionParameters):
				if (this.AnimationTree == null)
				{
					property["usage"] = (ulong) PropertyUsageFlags.None;
					break;
				}
				string parameters = this.AnimationTree.GetPropertyList()
					.Select(prop => prop["name"].AsString())
					.Where(propName => propName.StartsWith("parameters/"))
					.JoinIntoString(",");
				property["type"] = Variant.Type.Dictionary.As<long>();
				property["hint"] = PropertyHint.DictionaryType.As<long>();
				property["hint_string"] = $"{Variant.Type.String:D}/{PropertyHint.EnumSuggestion:D}:{parameters};String";
				break;
		}
	}

	// public override string[] _GetConfigurationWarnings()
	// 	=> base._PhysicsProcess(delta);

	public override void _EnterTree()
	{
		base._EnterTree();
		if (this.PlayerOrTree == null)
			return;
		this.PlayerOrTree.AnimationFinished += this.OnAnimationFinished;
		this.PlayerOrTree.AnimationStarted += this.OnAnimationStarted;
		if (this.AnimationPlayer != null)
		{
			this.AnimationPlayer.AnimationChanged += this.OnAnimationChanged;
			this.AnimationPlayer.CurrentAnimationChanged += this.OnCurrentAnimationChanged;
		}
	}

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

	protected override void _ActivityProcessActive(double delta)
	{
		base._ActivityProcessActive(delta);
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		if (this.UpdateStrategy == UpdateStrategyEnum.OnProcess)
			this.UpdateAnimation();
	}

	protected override void _ActivityPhysicsProcessActive(double delta)
	{
		base._ActivityPhysicsProcessActive(delta);
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
		if (this.ExpressionUnsetConditionsOnExit)
			this.UnsetConditions();
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void OnAnimationFinished(StringName animationName)
		=> this.EmitSignalAnimationFinished(animationName);
	private void OnAnimationStarted(StringName animationName)
		=> this.EmitSignalAnimationStarted(animationName);
	private void OnAnimationChanged(StringName oldAnimation, StringName newAnimation)
		=> this.EmitSignalAnimationChanged(oldAnimation, newAnimation);
	private void OnCurrentAnimationChanged(StringName animationName)
		=> this.EmitSignalCurrentAnimationChanged(animationName);

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
		foreach ((Variant parameter, Variant expression) in this.ExpressionParameters)
			this.UpdateCondition(tree, parameter.AsString(), expression.AsString());
	}

	private void UpdateCondition(AnimationTree tree, string parameter, string expression)
	{
		if (Engine.IsEditorHint() || !this.Interpreters.TryGetValue(parameter, out Expression? interpreter))
		{
			this.Interpreters[parameter] = interpreter = new();
			interpreter.Parse(expression, this.ExpressionVariables.Keys.Select(key => key.AsString()).ToArray());
		}
		interpreter.Execute(this.ExpressionVariables.Values.ToGodotArray(), this.ExpressionContext);
	}

	private void UnsetConditions()
		=> this.ExpressionParameters.Keys.Select(key => key.AsString())
			.Where(parameter => parameter.Contains("/conditions"))
			.ForEach(condition => this.AnimationTree?.Set(condition, false));
}
