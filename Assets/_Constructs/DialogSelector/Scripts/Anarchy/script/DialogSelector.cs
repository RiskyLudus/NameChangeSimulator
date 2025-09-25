using DW.Core;

using UnityEngine;

namespace NCS {
	public static class Dialog {
		public struct Entry {
			public string Keyword;
			public string Text;
		}

		public static class Selector {
			private static Entry[] _entries;

			static Selector() {
				var objs = Resources.LoadAll("Dialogs");
				var list = new System.Collections.Generic.List<Entry>(objs.Length);

				for (int i = 0; i < objs.Length; i++) {
					var o = objs[i];
					// Type 1: global DialogSO
					{
						var d1 = o as global::DialogSO;
						if (d1 != null)
							list.Add(new Entry { Keyword = d1.Keyword, Text = d1.Dialog });
					}
					// Type 2: NCS.Dialog.DialogSO (if any old assets exist)
					{
						var d2 = o as NCS.Dialog.DialogSO;
						if (d2 != null)
							list.Add(new Entry { Keyword = d2.Keyword, Text = d2.Dialog });
					}
				}
				_entries = list.ToArray();
				Debug.Log($"[Dialog.Selector] Loaded {_entries.Length} dialogs (both types).");
			}

			public static string MatchText(string searchKeyword) {
				if (string.IsNullOrEmpty(searchKeyword) || _entries == null || _entries.Length == 0)
					return null;

				var synonyms = Collections.Blackboard.MatchLogic.Synonyms.GetSynonyms(searchKeyword);

				for (int i = 0; i < _entries.Length; i++) {
					var kw = _entries[i].Keyword ?? "";

					foreach (var syn in synonyms) {
						if (!string.IsNullOrEmpty(syn) && kw.IndexOf(syn, System.StringComparison.OrdinalIgnoreCase) >= 0) {

							return _entries[i].Text;
						}
					}
				}

				return null;
			}
		}

		[CreateAssetMenu(fileName = "DialogSO_New", menuName = "Anarchy/DialogSO")]
		public class DialogSO : ScriptableObject {
			public string Keyword;
			[TextArea] public string Dialog;
		}
	}
}
