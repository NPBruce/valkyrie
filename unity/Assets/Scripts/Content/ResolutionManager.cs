using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Content
{
    internal static class ResolutionManager
    {
        // Return a list of unique resolutions (width x height). Sorted ascending.
        public static List<Resolution> GetAvailableResolutions()
        {
            // Screen.resolutions may contain duplicates with different refresh rates.
            // Group by width/height to present sane options to the user.
            var res = Screen.resolutions;
            var uniq = res
                .GroupBy(r => new { r.width, r.height })
                .Select(g =>
                {
                    // pick the highest refresh rate available for that resolution
                    var best = g.OrderByDescending(x => x.refreshRate).First();
                    return new Resolution { width = best.width, height = best.height, refreshRate = best.refreshRate };
                })
                .OrderBy(r => r.width)
                .ThenBy(r => r.height)
                .ToList();
            return uniq;
        }

        // Current screen resolution (width/height). Refresh rate taken from currentResolution if available.
        public static Resolution GetCurrentResolution()
        {
            return new Resolution
            {
                width = Screen.width,
                height = Screen.height,
                refreshRate = Screen.currentResolution.refreshRate
            };
        }

        // Set resolution using Unity API. fullscreen controls full screen vs windowed.
        public static void SetResolution(int width, int height, bool fullscreen)
        {
            Screen.SetResolution(width, height, fullscreen);
        }

        // Query fullscreen state
        public static bool IsFullscreen()
        {
            return Screen.fullScreen;
        }

        // Set fullscreen state
        public static void SetFullscreen(bool fullscreen)
        {
            // prefer assigning Screen.fullScreen for a simple toggle
            Screen.fullScreen = fullscreen;
        }
    }
}
