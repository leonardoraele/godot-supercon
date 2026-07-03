using System;
using System.Collections.Generic;
using Godot;
using Raele.GodotUtils.Debug;

namespace Raele.Supercon;

[Tool][GlobalClass]
public partial class SuperconInputController : Resource
{
	// -----------------------------------------------------------------------------------------------------------------
	// LOCAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public class InputBuffer
	{
		public ulong LastInputTime { get; private set; } = 0;
		public bool IsInputBuffered => this.LastInputTime >= Time.GetTicksMsec() - this.InputBufferDurationMs().TotalMilliseconds;
		// The respon why we get these variables as functions instead of plain values is so that we can always get the
		// most updated value, since they might change at runtime if the user tweaks with the inspector variables.
		// TODO A possible optimization would be to save the buffer duration and input action name in a field if game
		// was build for production so we don't need to call an anonymous function every frame; but I don't know how to
		// use (of if it's suppoerted) build-time variables.
		public Func<TimeSpan> InputBufferDurationMs { get; init; } = () => TimeSpan.Zero;
		public Func<string> InputActionName { get; init; } = () => "";
		public bool ConsumeInput()
		{
			bool isBuffered = this.IsInputBuffered;
			this.LastInputTime = 0;
			return isBuffered;
		}
		public void Update()
		{
			if (Input.IsActionJustPressed(this.InputActionName()))
			{
				this.ProduceInput();
			}
		}
		public void ProduceInput() => this.LastInputTime = Time.GetTicksMsec();
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public bool Enabled { get; private set; } = true;

	[Export(PropertyHint.InputName)] public string MoveLeftAction = "character_move_left";
	[Export(PropertyHint.InputName)] public string MoveRightAction = "character_move_right";
	[Export(PropertyHint.InputName)] public string MoveUpAction = "character_move_up";
	[Export(PropertyHint.InputName)] public string MoveDownAction = "character_move_down";

	[ExportGroup("Buffer Inputs")]
	[Export(PropertyHint.GroupEnable)] public bool BufferInputsEnabled = true;
	[Export] public int InputBufferDurationMs = 150;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// +---------------------------+
	/// | Directional input vector: |
	/// |                           |
	/// |            -Y             |
	/// |             ┃             |
	/// |      -X ━━━━╋━━━━ +X      |
	/// |             ┃             |
	/// |            +Y             |
	/// |                           |
	/// +---------------------------+
	/// </summary>
	public Vector2 RawDirectionalInput
		{ get; private set { field = value; NormalizedDirectionalInput = value.Normalized(); } }
	public Vector2 NormalizedDirectionalInput { get; private set; }
	private Dictionary<string, InputBuffer> InputBuffers = new();

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public TimeSpan InputBufferDuration => this.BufferInputsEnabled
		? TimeSpan.FromMilliseconds(this.InputBufferDurationMs)
		: TimeSpan.Zero;

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	// public override void _ValidateProperty(Godot.Collections.Dictionary property)
	// {
	// 	base._ValidateProperty(property);
	// 	switch (property["name"].AsString())
	// 	{
	// 		case nameof():
	// 	}
	// }

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------


	public void Update()
	{
		if (!this.Enabled)
			return;
		this.RawDirectionalInput = Input.GetVector(
			this.MoveLeftAction,
			this.MoveRightAction,
			this.MoveUpAction,
			this.MoveDownAction
		);
		foreach (InputBuffer buffer in this.InputBuffers.Values)
			buffer.Update();
	}
	public InputBuffer GetInputBuffer(string name)
	{
		if (!this.InputBuffers.ContainsKey(name))
		{
			this.InputBuffers[name] = new InputBuffer()
			{
				InputBufferDurationMs = () => InputBufferDuration,
				InputActionName = () => name,
			};
		}
		return this.InputBuffers[name];
	}

	public bool SetEnabled(bool enabled)
	{
		this.Enabled = enabled;
		if (!enabled)
		{
			this.RawDirectionalInput = Vector2.Zero;
			foreach (InputBuffer buffer in this.InputBuffers.Values)
			{
				buffer.ConsumeInput();
			}
		}
		return this.Enabled;
	}
}
