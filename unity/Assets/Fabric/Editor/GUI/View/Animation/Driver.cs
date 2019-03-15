namespace Fabric.Internal.Editor.View.Animation
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System;
	
	public class Driver
	{
		private uint invocationCount = 0;
		private uint invokeAtFrame = 1;
		private uint invokedAt = (uint)EditorApplication.timeSinceStartup;
		
		private uint frameCount;
		private uint frame = 0;
		public uint Frame
		{
			get {
				Reset ();
				uint toRender = Invoke ();
				Tick ();

				return toRender;
			}
		}
		
		public Driver(uint frameCount)
		{
			if (frameCount == 0) {
				throw new Exception ("frame count cannot be 0");
			}
			this.frameCount = frameCount;
		}
		
		public void Tick()
		{
			++invocationCount;
		}
		
		private void Reset()
		{
			uint now = (uint)EditorApplication.timeSinceStartup;
			
			if (invokedAt != now) {
				invokedAt = now;
				invokeAtFrame = OneOnZero (invocationCount / frameCount);
				invocationCount = 0;
			}
		}
		
		private uint Invoke()
		{
			if (invocationCount % invokeAtFrame == 0) {
				if (frame == (frameCount - 1)) {
					frame = 0;
				} else {
					frame += 1;
				}
			}
			
			return frame;
		}
		
		private static uint OneOnZero(uint val)
		{
			return val == 0 ? 1 : val;
		}
	}
}