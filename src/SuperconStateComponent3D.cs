using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon;

[Tool][GlobalClass][Icon($"res://addons/{nameof(Supercon)}/icons/character_body_bg.png")]
public abstract partial class SuperconStateComponent3D : SuperconStateComponent
{
	public SuperconBody3D? Character => this.GetAncestorOrDefault<SuperconBody3D>();
}
