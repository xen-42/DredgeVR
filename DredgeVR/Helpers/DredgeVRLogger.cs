using Winch.Core;

namespace DredgeVR.Helpers;

public static class DredgeVRLogger
{
	public static void Info(object msg) => DredgeVRLogger.Info(msg);
	public static void Info(params object[] objs) => DredgeVRLogger.Info(string.Join(", ", objs));
	public static void Debug(object msg) => DredgeVRLogger.Debug(msg);
	public static void Debug(params object[] objs) => DredgeVRLogger.Debug(string.Join(", ", objs));
	public static void Warn(object msg) => DredgeVRLogger.Warn(msg);
	public static void Warn(params object[] objs) => DredgeVRLogger.Warn(string.Join(", ", objs));
	public static void Error(object msg) => DredgeVRLogger.Error(msg);
	public static void Error(params object[] objs) => DredgeVRLogger.Error(string.Join(", ", objs));
}
