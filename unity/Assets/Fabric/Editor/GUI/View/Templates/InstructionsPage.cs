namespace Fabric.Internal.Editor.View.Templates
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Collections.Generic;
	using Fabric.Internal.Editor.View;

	public class InstructionsPage : Page
	{
		private KeyValuePair<string, Action> apply;
		private KeyValuePair<string, Action> back;
		private List<string> instructions;

		public InstructionsPage(Action onApplyClicked, Action onBackClicked, List<string> instructions)
		{
			this.apply = new KeyValuePair<string, Action>("Apply", onApplyClicked);
			this.back = new KeyValuePair<string, Action>("Back", onBackClicked);
			this.instructions = instructions;
		}

		#region Components
		private static class Components
		{
			private static readonly GUIStyle InstructionsStyle;

			static Components()
			{
				InstructionsStyle = new GUIStyle ();

				InstructionsStyle.normal.textColor = Color.white;
				InstructionsStyle.fontSize = 14;
				InstructionsStyle.margin = new RectOffset (30, 30, 20, 20);
				InstructionsStyle.padding = new RectOffset (0, 0, 0, 0);
				InstructionsStyle.wordWrap = true;
			}

			public static void RenderInstructions(List<string> instructions)
			{
				GUILayout.BeginVertical ();
				foreach (string instruction in instructions) {
					GUILayout.Label (instruction, InstructionsStyle);
				}
				GUILayout.EndVertical ();
			}
		}
		#endregion

		public override void RenderImpl(Rect position)
		{
			RenderHeader ("Fabric will make the following changes:");
			RenderFooter (back, apply);
			Components.RenderInstructions (instructions);
		}
	}
}