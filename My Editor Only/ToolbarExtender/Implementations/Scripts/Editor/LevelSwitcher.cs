using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityToolbarExtender.Implementation
{
	static class ToolbarStyles
	{
		public static readonly GUIStyle commandButtonStyle;

		static ToolbarStyles()
		{
			commandButtonStyle = new GUIStyle("Command") {
				fontSize = 16,
				alignment = TextAnchor.MiddleCenter,
				imagePosition = ImagePosition.ImageAbove,
				fontStyle = FontStyle.Bold
			};
		}
	}


	[InitializeOnLoad]
	public static class LevelSwitchLeftButton
	{
		private static readonly List<string> levels = new List<string> {
			"Assets/Test/RizaPlayground/PolygonComponentDevelopment/polygon-test.asset",
			"Assets/Test/OzgurLevels/MISC_Base_Level.asset",
		};

		static LevelSwitchLeftButton()
		{
			ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
		}

		static void OnToolbarGUI()
		{
			GUILayout.FlexibleSpace();
			for (int i = 0; i < levels.Count; i++) {
				var buttonContent = new GUIContent((i + 1).ToString(), levels[i]);
				if (GUILayout.Button(buttonContent, ToolbarStyles.commandButtonStyle))
					LevelHelper.SelecLevel(levels[i]);
			}
		}
	}

	internal static class LevelHelper
	{
		static Mobge.BrigRex.Level levelToOpen;
		static Mobge.Microns.MicronLevelPlayer player;
		static Pass pass;
		private enum Pass { First, Second };

		public static void SelecLevel(string levelPath)
		{
			if (player == null)
				player = Object.FindObjectOfType<Mobge.Microns.MicronLevelPlayer>();
			if (EditorApplication.isPlaying)
				EditorApplication.isPlaying = false;

			levelToOpen = (Mobge.BrigRex.Level)AssetDatabase.LoadAssetAtPath(levelPath, typeof(Mobge.BrigRex.Level));
			UnityEngine.Assertions.Assert.IsNotNull(levelToOpen, "Level file does not exist at : " + levelPath);
			pass = Pass.First;
			EditorApplication.update += OnUpdate;
		}

		private static void OnUpdate()
		{
			if (levelToOpen == null ||
				EditorApplication.isPlaying || EditorApplication.isPaused ||
				EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) {
				return;
			}

			switch (pass) {
				case Pass.First:
					player.level = levelToOpen;
					Selection.activeTransform = Object.FindObjectOfType<Mobge.TemporaryEditorObjects>().transform;
					Mobge.UnityExtensions.DestroySelf(Mobge.TemporaryEditorObjects.Shared.gameObject);
					pass = Pass.Second;
					break;
				case Pass.Second:
					EditorApplication.update -= OnUpdate;
					Selection.activeTransform = player.transform;
					levelToOpen = null;
					break;
			}
		}
	}
}
