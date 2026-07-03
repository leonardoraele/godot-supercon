using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon;

[Tool][GlobalClass][Icon($"res://addons/{nameof(Supercon)}/icons/character_body_bg.png")]
public abstract partial class SuperconStateComponent2D : SuperconStateComponent
{
	public SuperconBody2D? Character => field ??= this.GetAncestorOrDefault<SuperconBody2D>();
}
