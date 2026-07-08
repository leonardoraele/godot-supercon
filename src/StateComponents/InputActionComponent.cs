using System.Linq;
using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon.StateComponents2D;

[Tool][GlobalClass][Icon($"res://addons/{nameof(Supercon)}/icons/character_body_button.png")]
public partial class InputActionComponent : SuperconStateComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Name of the input action to be read for this ability.
	/// </summary>
	[Export(PropertyHint.InputName)] public string InputActionName = "";
	[Export] public InputModeEnum InputMode = InputModeEnum.InputIsJustDown;

	[ExportGroup("Conditional Expression", "Condition")]
	[Export] public Node? ConditionContext
		{ get => field ??= this.Owner; set; }
	[Export] public Godot.Collections.Dictionary ConditionVariables = [];
	[Export(PropertyHint.Expression)] public string ConditionExpression
		{ get; set { field = value.Trim(); this.Interpreter = null!; } }
		= "";

	[ExportGroup("Debug", "Debug")]
	[Export] public bool DebugPrintTriggers = false;

	[ExportCategory("🔀 Connect State Transitions")]
	[ExportToolButton("On Action Triggered", Icon = "MultiplayerSpawner")] public Callable ConnectInputActionTriggeredToolButton
		=> Callable.From(this.OnConnectInputActionTriggeredToolButtonPressed);

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private Expression Interpreter
	{
		get
		{
			if (field == null)
			{
				field = new();
				field.Parse(this.ConditionExpression, this.ConditionVariables.Keys.Select(key => key.AsString()).ToArray());
			}
			return field;
		}
		set;
	}

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void InputActionTriggeredEventHandler();

	// -----------------------------------------------------------------------------------------------------------------
	// LOCAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum InputModeEnum
	{
		InputIsDown,
		InputIsJustDown,
		InputIsReleased,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.ConditionVariables):
				property["type"] = (long) Variant.Type.Dictionary;
				property["hint"] = (long) PropertyHint.DictionaryType;
				property["hint_string"] = "String;Nil";
				break;
		}
	}

	protected override void _ActivityProcessActive(double delta)
	{
		base._ActivityProcessActive(delta);
		if (!this.TestInput() || !this.TestCondition())
			return;
		if (this.DebugPrintTriggers)
			this.DebugLog("⚡ Action triggered.", new { this.InputActionName });
		this.EmitSignalInputActionTriggered();
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private bool TestInput()
		=> !string.IsNullOrWhiteSpace(this.InputActionName) && this.InputMode switch
		{
			InputModeEnum.InputIsDown => Input.IsActionPressed(this.InputActionName),
			InputModeEnum.InputIsJustDown => this.Character?.InputController?.GetInputBuffer(this.InputActionName).ConsumeInput() == true,
			InputModeEnum.InputIsReleased => !Input.IsActionPressed(this.InputActionName),
			_ => false,
		};

	private bool TestCondition()
	{
		if (this.ConditionExpression == "")
			return true;
		Variant result = this.Interpreter.Execute(this.ConditionVariables.Values.ToGodotArray(), this.ConditionContext);
		if (this.Interpreter.HasExecuteFailed())
		{
			this.DebugLog("⚠️ Condition expression failed to execute.", this.ConditionVariables, new { Error = this.Interpreter.GetErrorText() });
			return false;
		}
		return result.IsTruthy();
	}

	private void OnConnectInputActionTriggeredToolButtonPressed()
		=> this.ConnectStateTransition(SignalName.InputActionTriggered);
}
