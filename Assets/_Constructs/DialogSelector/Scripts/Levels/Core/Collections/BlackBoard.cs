using UnityEngine;


namespace DW.Core {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text.Json;


	using UnityEngine;
	using UnityEngine.ResourceManagement.AsyncOperations;

	using JsonSerializer = System.Text.Json.JsonSerializer;

	public static class Collections {
		public class Blackboard {
			public readonly List<Post> Posts = new();
			public string MatchGroup;

			public Action OnPostsUpdated;
			public Action<Post> OnPostAdded;
			public Action<Post> OnPostRemoved;

			public Blackboard() {
				Initialize();
			}

			public void Initialize() {
				LoadAllSynonymGroups();
			}

			private async void LoadAllSynonymGroups() {
				JsonGroupSO[] groups = Resources.LoadAll<JsonGroupSO>("");

				foreach (var groupSO in groups) {
					if (!string.Equals(groupSO.groupName, MatchGroup, StringComparison.OrdinalIgnoreCase))
						return;

					Debug.Log($"[BlackBoard] Loading group: {groupSO.groupName}");

					if (groupSO.Asset == null) {
						Debug.LogWarning($"No AssetReference for group: {groupSO.groupName}");
						continue;
					}

					AsyncOperationHandle<TextAsset> handle = groupSO.Asset.LoadAssetAsync<TextAsset>();
					await handle.Task;

					if (handle.Status == AsyncOperationStatus.Succeeded) {
						TextAsset jsonAsset = handle.Result;
						Debug.Log($"Loaded JSON for group: {groupSO.groupName}\n{jsonAsset.text}");

						MatchLogic.Synonyms.LoadFromJsonString(jsonAsset.text);
					} else {
						Debug.LogError($"Failed to load JSON for group: {groupSO.groupName}");
					}
				}
			}

			public void AddPost(Post post) {
				Add(post);
			}

			private void Add(Post post) {
				Posts.Add(post);
				OnPostAdded(post);
			}

			public bool RemovePost(Post post) {
				return !Posts.Contains(post) ? false : Remove(post);
			}

			private bool Remove(Post post) {
				Posts.Remove(post);

				OnPostRemoved(post);

				return true;
			}

			public Post[] GetMatch(Predicate<Post> condition) {
				return Posts.FindAll(condition).ToArray();
			}

			public interface Post {
				public string Label { get; set; }

				public Type types { get; set; }

				public object dataObj { get; set; }
			}

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
					var synonyms = Synonyms.GetSynonyms(searchTerm);

					foreach (var synonym in synonyms) {
						if (post.Label.IndexOf(synonym, StringComparison.OrdinalIgnoreCase) >= 0)
							return true;
					}

					return false;
				}
			}
		}

		public class SynonymMap {
			private readonly Dictionary<string, HashSet<string>> _map = new(StringComparer.OrdinalIgnoreCase);

			public void AddLink(string word1, string word2) {
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
				return _map.TryGetValue(word, out var set)
					? set
					: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { word };
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

			public void SaveToJson(string filePath) {
				var groups = ToSynonymGroups();
				var json = JsonSerializer.Serialize(groups, new JsonSerializerOptions { WriteIndented = true });
				File.WriteAllText(filePath, json);
			}

			public void LoadFromJson(string filePath) {
				if (!File.Exists(filePath))
					return;

				var json = File.ReadAllText(filePath);
				var groups = JsonSerializer.Deserialize<List<List<string>>>(json);

				if (groups == null)
					return;

				foreach (var group in groups) {
					if (group.Count < 2)
						continue;

					LinkPairs(group);
				}
			}

			public void LoadFromJsonString(string json) {
				var groups = Newtonsoft.Json.JsonConvert.DeserializeObject<List<List<string>>>(json);
				if (groups == null)
					return;

				foreach (var group in groups) {
					if (group.Count < 2)
						continue;
					LinkPairs(group);
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

		public class _BlackBoard_ : MonoBehaviour {

		}
	}
}