using UnityEngine;

namespace NCS {
	using Levels.Core;
	public static class Dialog {
		[CreateAssetMenu(fileName = "DialogSO_New", menuName = "Anarchy/DialogSO")]
		public class DialogSO : ScriptableObject {
			public string Keyword;
			public string Dialog;
		}
		public static class Selector {
			public static Collections.Blackboard Blackboard = new() { MatchGroup = "Dialogs" };

			private static DialogSO[] _dialogs;

			static Selector() {
				// Load all dialogs at startup once
				_dialogs = Resources.LoadAll<DialogSO>("Dialogs");
				Debug.Log($"[Dialog.Selector] Loaded {_dialogs.Length} DialogSO assets");
			}

			public static DialogSO Match(string searchKeyword) {
				Debug.Log($"[Dialog.Selector] Searching for: {searchKeyword}");

				foreach (var dialog in _dialogs) {
					if (MatchLogic.Matches(dialog.Keyword, searchKeyword)) {
						Debug.Log($"[Dialog.Selector] Found match: {dialog.Keyword} => {dialog.Dialog}");
						return dialog;
					}
				}

				Debug.LogWarning($"[Dialog.Selector] No match found for: {searchKeyword}");
				return null;
			}

			/// <summary>
			/// Use SynonymMap to match the dialog keyword to the input keyword.
			/// </summary>
			private static class MatchLogic {
				public static bool Matches(string dialogKeyword, string searchKeyword) {
					var synonyms = Collections.Blackboard.MatchLogic.Synonyms.GetSynonyms(searchKeyword);
					foreach (var synonym in synonyms) {
						if (dialogKeyword.IndexOf(synonym, System.StringComparison.OrdinalIgnoreCase) >= 0) {
							return true;
						}
					}
					return false;
				}
			}
		}
	}
}