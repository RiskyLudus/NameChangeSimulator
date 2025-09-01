

namespace DW.Tools {
	using System;
	using System.Collections.Generic;

	using UnityEngine;

	public partial class ImageFielding {
		[Serializable]
		public class RenderData {
			public Dictionary<string, string> fieldStringValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			public Dictionary<string, Texture2D> fieldImageValues = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

			public Font textFont;
			public int textFontSizePixels = 32;
			public Color textColor = Color.black;
			public TextAnchor textAlignment = TextAnchor.MiddleCenter;
			public int paddingPixels = 8;

			public bool drawPlaceholderBoxesForMissingValues = true;
			public Color placeholderFillColor = new Color(0, 0, 0, 0.10f);
			public Color placeholderOutlineColor = new Color(0, 0, 0, 0.85f);
			public int placeholderOutlineThicknessPixels = 2;

			public Color stringBackgroundFillColor = new Color(1, 1, 1, 0);
		}
	}
}
