

namespace DW.Tools {
	using UnityEditor;
	using DW.Tools;

	public partial class  ImageFielding {
			public static class AssetContextMenu {
			[MenuItem("Assets/DW/Open in Image Fielding Editor", true)]
			private static bool ValidateOpenWith()
				=> Selection.activeObject is ImageFieldingAsset;

			[MenuItem("Assets/DW/Open in Image Fielding Editor")]
			private static void OpenWith() {
				var asset = Selection.activeObject as ImageFieldingAsset;
				Editor.Window.OpenWith(asset);
			}
		}
	}
}
