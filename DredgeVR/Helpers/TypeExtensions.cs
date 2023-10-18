using System;
using System.Linq;
using System.Reflection;

namespace DredgeVR.Helpers;

/// <summary>
/// While the publicizer for the Dredge game libs makes this mostly unnecessary, there are some other resources that I'm too lazy to publicize right now
/// Taken from Rai
/// </summary>
public static class TypeExtensions
{
	private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public |
									   BindingFlags.Static;

	public static MethodInfo GetAnyMethod(this Type type, string name)
	{
		return type.GetMethod(name, Flags) ??
			   type.BaseType?.GetMethod(name, Flags) ??
			   type.BaseType?.BaseType?.GetMethod(name, Flags);
	}

	private static MemberInfo GetAnyMember(this Type type, string name)
	{
		return type.GetMember(name, Flags).FirstOrDefault() ??
			   type.BaseType?.GetMember(name, Flags).FirstOrDefault() ??
			   type.BaseType?.BaseType?.GetMember(name, Flags).FirstOrDefault();
	}

	public static void SetValue(this object obj, string name, object value)
	{
		switch (obj.GetType().GetAnyMember(name))
		{
			case FieldInfo field:
				field.SetValue(obj, value);
				break;
			case PropertyInfo property:
				property.SetValue(obj, value, null);
				break;
		}
	}

	public static T GetValue<T>(this object obj, string name)
	{
		return obj.GetType().GetAnyMember(name) switch
		{
			FieldInfo field => (T)field.GetValue(obj),
			PropertyInfo property => (T)property.GetValue(obj),
			_ => throw new Exception($"Type {obj.GetType()} has no field or property named {name}"),
		};
	}
}
