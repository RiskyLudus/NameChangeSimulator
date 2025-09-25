
namespace DW.Core {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text.Json;

	using UnityEngine;
	using UnityEngine.AddressableAssets;
	using UnityEngine.ResourceManagement.AsyncOperations;

	using JsonSerializer = System.Text.Json.JsonSerializer;

	public static class Collections {
		public class Blackboard {
			public readonly List<Post> Posts = new();
			public string MatchGroup;

			public Action OnPostsUpdated;
			public Action<Post> OnPostAdded;
			public Action<Post> OnPostRemoved;

			private const string TagBB = "<color=orange>[BB]</color>";
			private const string TagSynonyms = "<color=cyan>[SYNONYMS]</color>";
			private const string TagPostAdd = "<color=orange>[POST:ADD]</color>";
			private const string TagPostRemove = "<color=orange>[POST:REMOVE]</color>";
			private const string TagWarn = "<color=yellow>[WARN]</color>";
			private const string TagError = "<color=red>[ERROR]</color>";

			public Blackboard() {
				Initialize();
			}

			public void Initialize() {
				Debug.Log($"{TagBB}[Init] MatchGroup='{MatchGroup ?? "(null)"}'");
				LoadAllSynonymGroups();
			}

			private void SeedBuiltinLinks() {
				MatchLogic.Synonyms.AddLink("Name_First_Dead", "DeadFirstName");
				MatchLogic.Synonyms.AddLink("Name_Middle_Dead", "DeadMiddleName");
				MatchLogic.Synonyms.AddLink("Name_Last_Dead", "DeadLastName");

				MatchLogic.Synonyms.AddLink("Name_First_New", "NewFirstName");
				MatchLogic.Synonyms.AddLink("Name_Middle_New", "NewMiddleName");
				MatchLogic.Synonyms.AddLink("Name_Last_New", "NewLastName");

				MatchLogic.Synonyms.AddLink("FullDeadName", "DeadFirstName");
				MatchLogic.Synonyms.AddLink("FullNewName", "NewFirstName");

				Debug.Log($"{TagSynonyms}[Seeded built-ins]");
			}

			private async void LoadAllSynonymGroups() {
				try {
					var groupScriptableObjects = Resources.LoadAll<JsonGroupSO>("");

					SeedBuiltinLinks();

					int consideredCount = 0;
					int loadedCount = 0;
					int skippedCount = 0;

					foreach (var groupScriptableObject in groupScriptableObjects) {
						if (groupScriptableObject == null) {
							continue;
						}

						consideredCount++;

						bool groupMatches =
							string.Equals(groupScriptableObject.groupName, MatchGroup, StringComparison.OrdinalIgnoreCase);

						if (!groupMatches) {
							skippedCount++;
							continue;
						}

						if (groupScriptableObject.Asset == null || !groupScriptableObject.Asset.RuntimeKeyIsValid()) {
							Debug.LogWarning($"{TagSynonyms}{TagWarn} Skipping group '{groupScriptableObject.groupName}' on SO '{groupScriptableObject.name}': AssetReference missing or invalid.");
							continue;
						}

						AsyncOperationHandle<TextAsset> loadHandle;
						try {
							loadHandle = groupScriptableObject.Asset.LoadAssetAsync<TextAsset>();
							await loadHandle.Task;
						}
						catch (Exception exception) {
							Debug.LogError($"{TagSynonyms}{TagError} Exception while loading '{groupScriptableObject.groupName}' from SO '{groupScriptableObject.name}': {exception}");
							continue;
						}

						if (loadHandle.Status != AsyncOperationStatus.Succeeded || loadHandle.Result == null) {
							Debug.LogError($"{TagSynonyms}{TagError} Failed to load JSON for group '{groupScriptableObject.groupName}' (SO '{groupScriptableObject.name}').");
							Addressables.Release(loadHandle);
							continue;
						}

						try {
							string json = loadHandle.Result.text;

							if (string.IsNullOrWhiteSpace(json)) {
								Debug.LogWarning($"{TagSynonyms}{TagWarn} Empty JSON for group '{groupScriptableObject.groupName}' (SO '{groupScriptableObject.name}').");
							} else {
								int groupsBefore = MatchLogic.Synonyms.GroupCountApprox();
								MatchLogic.Synonyms.LoadFromJsonString(json);
								int groupsAfter = MatchLogic.Synonyms.GroupCountApprox();

								Debug.Log($"{TagSynonyms}[Loaded] '{groupScriptableObject.groupName}' Groups {groupsBefore} → {groupsAfter}");
								loadedCount++;
							}
						}
						catch (Exception parseException) {
							Debug.LogError($"{TagSynonyms}{TagError} JSON parse error for group '{groupScriptableObject.groupName}' (SO '{groupScriptableObject.name}'): {parseException}");
						}
						finally {
							Addressables.Release(loadHandle);
						}
					}

					Debug.Log($"{TagSynonyms}[Scan Complete] considered={consideredCount} matchedGroup='{MatchGroup}' loaded={loadedCount} skipped={skippedCount} total≈{MatchLogic.Synonyms.GroupCountApprox()}");
				}
				catch (Exception exception) {
					Debug.LogError($"{TagSynonyms}{TagError} Unexpected error in LoadAllSynonymGroups: {exception}");
				}
			}

			public void AddPost(Post post) {
				Add(post);
			}

			private void Add(Post post) {
				if (post == null) {
					Debug.LogWarning($"{TagBB}{TagPostAdd}{TagWarn} Null post ignored.");
					return;
				}

				Posts.Add(post);
				Debug.Log($"{TagBB}{TagPostAdd} Label='{Safe(post.Label)}'");

				if (OnPostAdded != null) {
					try {
						OnPostAdded.Invoke(post);
					}
					catch (Exception exception) {
						Debug.LogError($"{TagBB}{TagPostAdd}{TagError} OnPostAdded exception: {exception}");
					}
				}

				if (OnPostsUpdated != null) {
					try {
						OnPostsUpdated.Invoke();
					}
					catch (Exception exception) {
						Debug.LogError($"{TagBB}{TagPostAdd}{TagError} OnPostsUpdated exception: {exception}");
					}
				}
			}

			public bool RemovePost(Post post) {
				if (!Posts.Contains(post)) {
					return false;
				}
				return Remove(post);
			}

			private bool Remove(Post post) {
				if (post == null) {
					return false;
				}

				bool removed = Posts.Remove(post);
				if (removed) {
					Debug.Log($"{TagBB}{TagPostRemove} Label='{Safe(post.Label)}'");

					if (OnPostRemoved != null) {
						try {
							OnPostRemoved.Invoke(post);
						}
						catch (Exception exception) {
							Debug.LogError($"{TagBB}{TagPostRemove}{TagError} OnPostRemoved exception: {exception}");
						}
					}

					if (OnPostsUpdated != null) {
						try {
							OnPostsUpdated.Invoke();
						}
						catch (Exception exception) {
							Debug.LogError($"{TagBB}{TagPostRemove}{TagError} OnPostsUpdated exception: {exception}");
						}
					}
				}

				return removed;
			}

			public Post[] GetMatch(Predicate<Post> condition) {
				if (condition == null) {
					return Array.Empty<Post>();
				}
				return Posts.FindAll(condition).ToArray();
			}

			public interface Post {
				public string Label { get; set; }
				public Type types { get; set; }
				public object dataObj { get; set; }
			}

			[Serializable]
			public class Postit<T> : Post {
				[field: SerializeField, Header("SETTINGS")]
				public string Label { get; set; }

				[field: SerializeField]
				public T data;

				public Type types { get; set; }
				public object dataObj { get; set; }
			}

			public static class MatchLogic {
				public static SynonymMap Synonyms = new SynonymMap();

				public static bool Matches(Post post, string searchTerm) {
					if (post == null) {
						return false;
					}
					if (string.IsNullOrWhiteSpace(searchTerm)) {
						return false;
					}

					var synonymsToCheck = Synonyms.GetSynonyms(searchTerm);

					foreach (var synonym in synonymsToCheck) {
						int index = post.Label?.IndexOf(synonym, StringComparison.OrdinalIgnoreCase) ?? -1;
						if (index >= 0) {
							return true;
						}
					}

					return false;
				}
			}

			private static string Safe(string value) {
				if (string.IsNullOrEmpty(value)) {
					return "";
				}
				value = value.Replace("\n", " ").Replace("\r", " ");
				return value.Length > 160 ? value.Substring(0, 157) + "..." : value;
			}
		}

		public class SynonymMap {
			private readonly Dictionary<string, HashSet<string>> _map = new(StringComparer.OrdinalIgnoreCase);

			private const string TagSynonyms = "<color=cyan>[SYNONYMS]</color>";
			private const string TagWarn = "<color=yellow>[WARN]</color>";
			private const string TagError = "<color=red>[ERROR]</color>";

			public void AddLink(string word1, string word2) {
				if (string.IsNullOrWhiteSpace(word1) || string.IsNullOrWhiteSpace(word2)) {
					return;
				}

				if (!_map.TryGetValue(word1, out var set1)) {
					set1 = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { word1 };
					_map[word1] = set1;
				}

				if (!_map.TryGetValue(word2, out var set2)) {
					set2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { word2 };
					_map[word2] = set2;
				}

				if (!ReferenceEquals(set1, set2)) {
					Merge(set1, set2);
				}
			}

			private void Merge(HashSet<string> set1, HashSet<string> set2) {
				set1.UnionWith(set2);
				foreach (var word in set2) {
					_map[word] = set1;
				}
			}

			public HashSet<string> GetSynonyms(string word) {
				if (string.IsNullOrWhiteSpace(word)) {
					return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				}

				if (_map.TryGetValue(word, out var set)) {
					return set;
				}

				return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { word };
			}

			public List<List<string>> ToSynonymGroups() {
				var seen = new HashSet<HashSet<string>>();
				var groups = new List<List<string>>();

				foreach (var set in _map.Values) {
					if (seen.Add(set)) {
						groups.Add(new List<string>(set));
					}
				}

				return groups;
			}

			public int GroupCountApprox() {
				var seen = new HashSet<HashSet<string>>();
				foreach (var set in _map.Values) {
					seen.Add(set);
				}
				return seen.Count;
			}

			public void SaveToJson(string filePath) {
				try {
					var groups = ToSynonymGroups();
					var json = JsonSerializer.Serialize(groups, new JsonSerializerOptions { WriteIndented = true });
					File.WriteAllText(filePath, json);
					Debug.Log($"{TagSynonyms}[Saved] groups={groups.Count} path='{filePath}'");
				}
				catch (Exception exception) {
					Debug.LogError($"{TagSynonyms}{TagError} SaveToJson error: {exception}");
				}
			}

			public void LoadFromJson(string filePath) {
				try {
					if (!File.Exists(filePath)) {
						Debug.LogWarning($"{TagSynonyms}{TagWarn} LoadFromJson path not found: '{filePath}'");
						return;
					}

					var json = File.ReadAllText(filePath);
					LoadFromJsonString(json);
					Debug.Log($"{TagSynonyms}[Loaded File] path='{filePath}' total≈{GroupCountApprox()}");
				}
				catch (Exception exception) {
					Debug.LogError($"{TagSynonyms}{TagError} LoadFromJson error: {exception}");
				}
			}

			public void LoadFromJsonString(string json) {
				try {
					var groups = Newtonsoft.Json.JsonConvert.DeserializeObject<List<List<string>>>(json);
					if (groups == null) {
						Debug.LogWarning($"{TagSynonyms}{TagWarn} JSON contained no groups.");
						return;
					}

					int linkedGroupCount = 0;

					foreach (var group in groups) {
						if (group == null || group.Count < 2) {
							continue;
						}

						LinkPairs(group);
						linkedGroupCount++;
					}

					Debug.Log($"{TagSynonyms}[Linked] groups={linkedGroupCount} total≈{GroupCountApprox()}");
				}
				catch (Exception exception) {
					Debug.LogError($"{TagSynonyms}{TagError} LoadFromJsonString parse error: {exception}");
				}
			}

			private void LinkPairs(List<string> group) {
				for (int i = 0; i < group.Count; i++) {
					for (int j = i + 1; j < group.Count; j++) {
						AddLink(group[i], group[j]);
					}
				}
			}
		}

		public class _BlackBoard_ : MonoBehaviour { }
	}
}
