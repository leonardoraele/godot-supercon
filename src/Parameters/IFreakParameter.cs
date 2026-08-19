using System;
using Godot;

namespace Raele.Supercon;

/// <summary>
/// A parameter definition is an object that defines a parameter in a parameter profile, including its name,
/// type, and any associated getter or setter logic.
/// </summary>
public interface IFreakParameter
{
	public string Name { get; }
	public Variant.Type Type { get; }
	public PropertyHint Hint { get; }
	public string HintString { get; }
	public bool IsReadOnly { get; }

	/// <summary>
	/// A mapping function that should be called whenever the attribute is read from a container.
	/// </summary>
	public Variant RunGetter(IReadOnlyFreakParameterContainer container, Variant rawValue)
		=> rawValue;

	/// <summary>
	/// A mapping function that should be called whenever the attribute is stored in a container.
	/// </summary>
	public Variant RunSetter(IReadOnlyFreakParameterContainer container, Variant input)
		=> throw new NotSupportedException($"Parameter \"{this.Name}\" is read-only and cannot be set.");

	public Godot.Collections.Dictionary ToDictionary()
		=> new()
		{
			["name"] = this.Name,
			["type"] = (long) this.Type,
			["hint"] = (long) this.Hint,
			["hint_string"] = this.HintString,
			["usage"] = (long) PropertyUsageFlags.Default
		};
}

public static class IFreakParameterExtensions
{
	extension (IFreakParameter self)
	{
		public IFreakParameter AsParameter() => self;
	}
}
