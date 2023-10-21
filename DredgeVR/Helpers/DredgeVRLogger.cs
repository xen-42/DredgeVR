using Winch.Core;

namespace DredgeVR.Helpers;

public static class DredgeVRLogger
{
	public static void Info(object msg) => WinchCore.Log.Info(msg);
	public static void Info(params object[] objs) => WinchCore.Log.Info(string.Join(", ", objs));
	public static void Debug(object msg) => WinchCore.Log.Debug(msg);
	public static void Debug(params object[] objs) => WinchCore.Log.Debug(string.Join(", ", objs));
	public static void Warn(object msg) => WinchCore.Log.Warn(msg);
	public static void Warn(params object[] objs) => WinchCore.Log.Warn(string.Join(", ", objs));
	public static void Error(object msg) => WinchCore.Log.Error(msg);
	public static void Error(params object[] objs) => WinchCore.Log.Error(string.Join(", ", objs));
}
