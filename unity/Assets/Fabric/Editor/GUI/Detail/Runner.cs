namespace Fabric.Internal.Editor.Detail
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	
	// Allows using corotines in the context of the editor window.
	public class Runner
	{
		private readonly IEnumerator routine;

		public static Runner StartCoroutine(IEnumerator routine)
		{
			return new Runner (routine).start ();
		}

		public Runner(IEnumerator routine)
		{
			this.routine = routine;
		}
		
		private Runner start()
		{
			EditorApplication.update += update;
			return this;
		}
		
		private void stop()
		{
			EditorApplication.update -= update;
		}
		
		private void update()
		{
			if (!routine.MoveNext ()) {
				stop ();
			}
		}
	}
}