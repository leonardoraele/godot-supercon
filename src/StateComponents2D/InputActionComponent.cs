using Godot;

namespace Raele.Supercon2D.StateComponents2D;

[Tool][GlobalClass][Icon($"res://addons/{nameof(Supercon2D)}/icons/character_body_button.png")]
public partial class InputActionComponent : SuperconStateComponent2D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Name of the input action to be read for this ability.
	/// </summary>
	[Export(PropertyHint.InputName)] public string InputActionName = "";
	[Export] public InputModeEnum InputMode = InputModeEnum.InputIsJustDown;

	[ExportGroup("Debug", "Debug")]
	[Export] public bool DebugPrintTriggers = false;

	[ExportCategory("🔀 Connect State Transitions")]
	[ExportToolButton("On Action Triggered", Icon = "MultiplayerSpawner")] public Callable ConnectInputActionTriggeredToolButton
		=> Callable.From(this.OnConnectInputActionTriggeredToolButtonPressed);

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

	protected override void _ActivityProcess(double delta)
	{
		base._ActivityProcess(delta);
		if (this.TestInput())
		{
			if (this.DebugPrintTriggers)
			{
				GD.PrintS(Time.GetTimeStringFromSystem(), nameof(InputActionComponent), ":", "⚡", "Action triggered:", this.InputActionName);
			}
			this.EmitSignalInputActionTriggered();
		}
	}

	// public override void _ValidateProperty(Dictionary property)
	// {
	// 	base._ValidateProperty(property);
	// 	switch (property["name"].AsString())
	// 	{
	// 		case nameof():
	// 			break;
	// 	}
	// }

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private bool TestInput()
		=> !string.IsNullOrWhiteSpace(this.InputActionName) && this.InputMode switch
		{
			InputModeEnum.InputIsDown => Input.IsActionPressed(this.InputActionName),
			InputModeEnum.InputIsJustDown => this.Character?.InputController.GetInputBuffer(this.InputActionName).ConsumeInput() == true,
			InputModeEnum.InputIsReleased => !Input.IsActionPressed(this.InputActionName),
			_ => false,
		};

	private void OnConnectInputActionTriggeredToolButtonPressed() => this.ConnectStateTransition(SignalName.InputActionTriggered);
}
