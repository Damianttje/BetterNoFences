using System;
using System.Collections.Generic;

namespace NoFences.Model
{
    public class FenceInfo
    {
        /* 
         * DO NOT RENAME PROPERTIES. Used for XML serialization.
         */

        public Guid Id { get; set; }

        public string Name { get; set; }

        public int PosX { get; set; }

        public int PosY { get; set; }

        /// <summary>
        /// Gets or sets the DPI scaled window width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the DPI scaled window height.
        /// </summary>
        public int Height { get; set; }

        public bool Locked { get; set; }

        public bool CanMinify { get; set; }

        /// <summary>
        /// Gets or sets the logical window title height.
        /// </summary>
        public int TitleHeight { get; set; } = 35;

        /// <summary>
        /// Gets or sets the transparency level (0-100, where 100 is fully opaque)
        /// </summary>
        public int Transparency { get; set; } = 100;

        /// <summary>
        /// Gets or sets whether the fence should auto-hide when not in use
        /// </summary>
        public bool AutoHide { get; set; } = false;

        /// <summary>
        /// Gets or sets the auto-hide delay in milliseconds
        /// </summary>
        public int AutoHideDelay { get; set; } = 2000;

        public List<string> Files { get; set; } = new List<string>();

        public FenceInfo()
        {

        }

        public FenceInfo(Guid id)
        {
            Id = id;
        }
    }
}
