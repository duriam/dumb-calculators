using UnityEditor;
using UnityEngine;

public static class CustomShortcuts
{
	public static class SelectLevelPlayer
	{
		private const string ITEM_NAME = "Edit/Plus/Select LevelPlayer %e";

		[MenuItem(ITEM_NAME)]
		private static void SelectPlayerEditor()
		{
			var player = Object.FindObjectOfType<Mobge.Microns.MicronLevelPlayer>();
			Selection.activeTransform = player.transform;
		}

		private const string ITEM_NAME_2 = "Edit/Plus/Select LevelPlayer Legacy #%e";

		[MenuItem(ITEM_NAME_2)]
		private static void SelectPlayerEditorLegacy()
		{
			var player = Object.FindObjectOfType<Mobge.Microns.MicronLevelPlayer>();
			var tempobj = Object.FindObjectOfType<Mobge.TemporaryEditorObjects>();
			Mobge.UnityExtensions.DestroyObj(tempobj);
			Selection.activeTransform = player.transform;
		}
	}
	public static class LockInspector
	{
		private const string ITEM_NAME = "Edit/Plus/Lock Inspector %l";

		[MenuItem(ITEM_NAME)]
		private static void Lock()
		{
			var tracker = ActiveEditorTracker.sharedTracker;
			tracker.isLocked = !tracker.isLocked;
			tracker.ForceRebuild();
		}

		[MenuItem(ITEM_NAME, true)]
		private static bool CanLock()
		{
			return ActiveEditorTracker.sharedTracker != null;
		}
	}
	public static class ClearConsole
	{
		private const string ITEM_NAME = "Edit/Plus/Clear Console &#c";

		[MenuItem(ITEM_NAME)]
		public static void Invert()
		{
			System.Type consoleWindow = typeof(Editor).Assembly.GetType("UnityEditor.ConsoleWindow");
			EditorWindow.GetWindow(consoleWindow);

			var type = System.Reflection.Assembly
				.GetAssembly(typeof(SceneView))
				.GetType("UnityEditor.LogEntries");

			var attr = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public;
			var method = type.GetMethod("Clear", attr);
			method.Invoke(null, null);
		}
	}
	public static class ToggleDebugMode
	{
		private const string ITEM_NAME = "Edit/Plus/Toggle Inspector Debug %k";

		[MenuItem(ITEM_NAME)]
		private static void Toggle()
		{
			var window = Resources.FindObjectsOfTypeAll<EditorWindow>();
			var inspectorWindow = ArrayUtility.Find(window, c => c.GetType().Name == "InspectorWindow");

			if (inspectorWindow == null) return;

			var inspectorType = inspectorWindow.GetType();
			var tracker = ActiveEditorTracker.sharedTracker;
			var isNormal = tracker.inspectorMode == InspectorMode.Normal;
			var methodName = isNormal ? "SetDebug" : "SetNormal";

			var attr = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
			var methodInfo = inspectorType.GetMethod(methodName, attr);
			methodInfo.Invoke(inspectorWindow, null);
			inspectorWindow.Repaint();
			tracker.ForceRebuild();
		}

		[MenuItem(ITEM_NAME, true)]
		private static bool CanToggle()
		{
			var window = Resources.FindObjectsOfTypeAll<EditorWindow>();
			var inspectorWindow = ArrayUtility.Find(window, c => c.GetType().Name == "InspectorWindow");
			return inspectorWindow != null;
		}
	}
}
