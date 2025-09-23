#if UNITY_EDITOR
namespace NameChangeSimulator.Editor {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;

	using UnityEditor;

	using UnityEngine;

	using XNode;

	public partial class StateCreationToolEditor {
		private void CreateDialogueFromRows() {
			if (orderedKeys.Count == 0) {
				EditorUtility.DisplayDialog("No Rows", "No rows defined in Step 3.", "OK");
				return;
			}

			var graph = ScriptableObject.CreateInstance<DialogueGraph>();
			string defaultFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(selectedLayout)).Replace("\\", "/");

			if (!AssetDatabase.IsValidFolder(defaultFolder)) {
				var parts = defaultFolder.Split('/');
				string acc = parts[0];
				for (int i = 1; i < parts.Length; i++) {
					string next = acc + "/" + parts[i];
					if (!AssetDatabase.IsValidFolder(next))
						AssetDatabase.CreateFolder(acc, parts[i]);
					acc = next;
				}
			}

			string savePath = EditorUtility.SaveFilePanelInProject(
				"Save Dialogue",
				$"{(string.IsNullOrEmpty(stateName) ? "State" : stateName)}Dialogue",
				"asset",
				"Save the dialogue as a ScriptableObject asset.",
				defaultFolder
			);
			if (string.IsNullOrEmpty(savePath))
				return;

			AssetDatabase.CreateAsset(graph, savePath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			var startNode = ScriptableObject.CreateInstance<StartNode>();
			startNode.name = "StartNode";
			startNode.graph = graph;
			AssetDatabase.AddObjectToAsset(startNode, graph);
			graph.nodes.Add(startNode);

			var endNode = ScriptableObject.CreateInstance<EndNode>();
			endNode.name = "EndNode";
			endNode.graph = graph;
			AssetDatabase.AddObjectToAsset(endNode, graph);
			graph.nodes.Add(endNode);

			var spriteEnumType = ResolveTypeBySimpleName("CharacterSpriteType");
			var voiceEnumType = ResolveTypeBySimpleName("VoiceLineType");
			var poseChoices = spriteEnumType?.IsEnum == true ? Enum.GetNames(spriteEnumType) : Array.Empty<string>();
			var voiceChoices = voiceEnumType?.IsEnum == true ? Enum.GetNames(voiceEnumType) : Array.Empty<string>();

			var created = new List<Node>();
			var byName = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);

			for (int i = 0; i < orderedKeys.Count; i++) {
				var key = orderedKeys[i];
				if (!fieldConfigs.TryGetValue(key, out var cfg))
					continue;

				var typeName = NodeTypeOptions[Mathf.Clamp(cfg.TypeIndex, 0, NodeTypeOptions.Length - 1)];
				var nodeType = ResolveNodeType(typeName) ?? typeof(InputNode);

				var node = (Node)ScriptableObject.CreateInstance(nodeType);
				node.name = string.IsNullOrEmpty(cfg.NodeName) ? key : cfg.NodeName;

				SetStringMember(node, "DialogueText", cfg.Dialog);
				SetStringMember(node, "Keyword", cfg.Keyword);

				if (string.Equals(typeName, "DialogueNode", StringComparison.OrdinalIgnoreCase)) {
					SetEnumMember(node, "SpriteType", spriteEnumType, cfg.PoseIndex, poseChoices);
					SetEnumMember(node, "VoiceLine", voiceEnumType, cfg.VoiceIndex, voiceChoices);
				}

				if (IsOptionsType(cfg.TypeIndex)) {
					var opts = new List<string>();
					if (!string.IsNullOrWhiteSpace(cfg.Opt1Label))
						opts.Add(cfg.Opt1Label);
					if (!string.IsNullOrWhiteSpace(cfg.Opt2Label))
						opts.Add(cfg.Opt2Label);
					SetOptionsIfPresent(node, opts.ToArray());
					var updatePorts = typeof(XNode.Node).GetMethod("UpdatePorts", BindingFlags.Instance | BindingFlags.NonPublic);
					updatePorts?.Invoke(node, null);
				}

				node.graph = graph;
				AssetDatabase.AddObjectToAsset(node, graph);
				graph.nodes.Add(node);

				if (IsOptionsType(cfg.TypeIndex)) {
					var opt1 = string.IsNullOrWhiteSpace(cfg.Opt1Label) ? "Option 1" : cfg.Opt1Label;
					var opt2 = string.IsNullOrWhiteSpace(cfg.Opt2Label) ? "Option 2" : cfg.Opt2Label;
					SetOptionsIfPresent(node, new[] { opt1, opt2 });
					var updatePorts = typeof(XNode.Node).GetMethod("UpdatePorts", BindingFlags.Instance | BindingFlags.NonPublic);
					updatePorts?.Invoke(node, null);
					EditorUtility.SetDirty(node);
				}

				created.Add(node);
				if (!byName.ContainsKey(node.name))
					byName[node.name] = node;
			}

			Vector2 pos = new Vector2(-1600, 600);
			startNode.position = pos;

			for (int i = 0; i < created.Count; i++) {
				pos = new Vector2(pos.x + 500, pos.y);
				if (i % 4 == 0 && i != 0)
					pos = new Vector2(pos.x - 2000, pos.y + 600);
				(created[i] as XNode.Node).position = pos;
			}
			endNode.position = new Vector2(pos.x + 500, pos.y);

			try {
				if (created.Count > 0) {
					var first = created[0] as XNode.Node;
					startNode.GetOutputPort("Output").Connect(first.GetInputPort("Input"));
				} else {
					startNode.GetOutputPort("Output").Connect(endNode.GetInputPort("Input"));
				}
			}
			catch (Exception e) {
				Debug.LogWarning($"[StateCreationTool] Port connect error (Start->First): {e.Message}");
			}

			for (int i = 0; i < created.Count - 1; i++) {
				var key = orderedKeys[i];
				if (!fieldConfigs.TryGetValue(key, out var cfg))
					continue;
				if (IsOptionsType(cfg.TypeIndex))
					continue;

				var a = created[i] as XNode.Node;
				var b = created[i + 1] as XNode.Node;

				var outPort = a.GetOutputPort("Output");
				var inPort = (cfg.NextRowPort == PortSel.OverrideInput)
					? (b.GetInputPort("OverrideInput") ?? b.GetInputPort("InputOverride") ?? b.GetInputPort("Input"))
					: b.GetInputPort("Input");

				if (outPort != null && inPort != null) {
					try { outPort.Connect(inPort); }
					catch (Exception e) {
						Debug.LogWarning($"[StateCreationTool] Port connect error ({a.name} -> {b.name}): {e.Message}");
					}
				}
			}

			for (int i = 0; i < created.Count; i++) {
				var key = orderedKeys[i];
				if (!fieldConfigs.TryGetValue(key, out var cfg))
					continue;
				if (!IsOptionsType(cfg.TypeIndex))
					continue;

				var node = created[i] as XNode.Node;

				var updatePorts = typeof(XNode.Node).GetMethod("UpdatePorts", BindingFlags.Instance | BindingFlags.NonPublic);
				updatePorts?.Invoke(node, null);

				Node nextNode = (i + 1 < created.Count) ? created[i + 1] : endNode;
				var fallbackIn = (nextNode as XNode.Node).GetInputPort("Input");

				WireOption(node, byName, cfg.Opt1TargetName, cfg.Opt1TargetPort, 0, fallbackIn);
				WireOption(node, byName, cfg.Opt2TargetName, cfg.Opt2TargetPort, 1, fallbackIn);
			}

			for (int i = 0; i < created.Count; i++) {
				var key = orderedKeys[i];
				if (!fieldConfigs.TryGetValue(key, out var cfg))
					continue;
				if (string.IsNullOrWhiteSpace(cfg.InputOverride))
					continue;

				if (!byName.TryGetValue(cfg.InputOverride, out var source))
					continue;

				var current = created[i] as XNode.Node;
				var srcOut = source.GetOutputPort("Output");
				var dstIn = current.GetInputPort("OverrideInput") ?? current.GetInputPort("InputOverride");

				if (srcOut != null && dstIn != null) {
					try {
						if (!dstIn.IsConnected || !dstIn.GetConnections().Any(c => c.node == source))
							srcOut.Connect(dstIn);
					}
					catch (Exception e) {
						Debug.LogWarning($"[StateCreationTool] Port connect error (override {source.name} -> {current.name}): {e.Message}");
					}
				}
			}

			try {
				if (created.Count > 0) {
					var last = created.Last() as XNode.Node;
					var lastOut = last.GetOutputPort("Output");
					if (lastOut != null && !lastOut.IsConnected) {
						lastOut.Connect(endNode.GetInputPort("Input"));
					}
				}
			}
			catch (Exception e) {
				Debug.LogWarning($"[StateCreationTool] Port connect error (Last->End): {e.Message}");
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.DisplayDialog("Done", $"Dialogue created with {created.Count} nodes.", "OK");
		}

		private void WireOption(
			XNode.Node node,
			Dictionary<string, Node> byName,
			string targetName,
			PortSel targetPortSel,
			int optionIndex,
			XNode.NodePort fallbackIn) {

			var updatePorts = typeof(XNode.Node).GetMethod("UpdatePorts", BindingFlags.Instance | BindingFlags.NonPublic);
			updatePorts?.Invoke(node, null);

			string portName = $"Options {optionIndex}";
			var outPort = node.GetOutputPort(portName);
			if (outPort == null) {
				Debug.LogWarning($"[StateCreationTool] '{node.name}' missing output port '{portName}'. Ensure Options are set to length >= {optionIndex + 1}.");
				return;
			}

			XNode.Node dest = null;
			if (!string.IsNullOrWhiteSpace(targetName)) {
				byName.TryGetValue(targetName, out dest);
			}

			var destIn = (dest != null)
				? (targetPortSel == PortSel.OverrideInput
					? (dest.GetInputPort("OverrideInput") ?? dest.GetInputPort("InputOverride") ?? dest.GetInputPort("Input"))
					: dest.GetInputPort("Input"))
				: fallbackIn;

			if (destIn == null) {
				Debug.LogWarning($"[StateCreationTool] No valid destination input for option {optionIndex} on '{node.name}'.");
				return;
			}

			try { outPort.Connect(destIn); }
			catch (Exception e) {
				Debug.LogWarning($"[StateCreationTool] Port connect error (option {optionIndex} from {node.name}): {e.Message}");
			}
		}

		private static void SetOptionsIfPresent(object node, string[] options) {
			if (node == null || options == null)
				return;
			var t = node.GetType();

			var p = t.GetProperty("Options", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (p != null && p.CanWrite) {
				if (p.PropertyType == typeof(string[])) { p.SetValue(node, options, null); return; }
				if (typeof(System.Collections.Generic.IList<string>).IsAssignableFrom(p.PropertyType)) {
					var list = (System.Collections.Generic.IList<string>)Activator.CreateInstance(p.PropertyType);
					foreach (var s in options)
						list.Add(s);
					p.SetValue(node, list, null);
					return;
				}
			}

			var f = t.GetField("Options", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (f != null) {
				if (f.FieldType == typeof(string[])) { f.SetValue(node, options); return; }
				if (typeof(System.Collections.Generic.IList<string>).IsAssignableFrom(f.FieldType)) {
					var list = (System.Collections.Generic.IList<string>)Activator.CreateInstance(f.FieldType);
					foreach (var s in options)
						list.Add(s);
					f.SetValue(node, list);
				}
			}
		}

		private static Type ResolveNodeType(string simpleName) {
			if (string.IsNullOrEmpty(simpleName))
				return null;
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
				try {
					var t = asm.GetTypes().FirstOrDefault(tt => tt.Name == simpleName && typeof(Node).IsAssignableFrom(tt));
					if (t != null)
						return t;
				}
				catch { }
			}
			return null;
		}

		private static Type ResolveTypeBySimpleName(string simpleName) {
			if (string.IsNullOrEmpty(simpleName))
				return null;
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
				try {
					var t = asm.GetTypes().FirstOrDefault(tt => tt.Name == simpleName);
					if (t != null)
						return t;
				}
				catch { }
			}
			return null;
		}

		private static void SetStringMember(object target, string memberName, string value) {
			if (target == null)
				return;
			var t = target.GetType();

			var p = t.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (p != null && p.CanWrite && p.PropertyType == typeof(string)) { p.SetValue(target, value, null); return; }

			var f = t.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (f != null && f.FieldType == typeof(string)) { f.SetValue(target, value); }
		}

		private static void SetEnumMember(object target, string memberName, Type enumType, int index, string[] names) {
			if (target == null || enumType == null || names == null || names.Length == 0)
				return;
			index = Mathf.Clamp(index, 0, names.Length - 1);
			var name = names[index];
			try {
				var value = Enum.Parse(enumType, name);
				var t = target.GetType();

				var p = t.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (p != null && p.CanWrite && p.PropertyType.IsEnum && p.PropertyType == enumType) { p.SetValue(target, value, null); return; }

				var f = t.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (f != null && f.FieldType.IsEnum && f.FieldType == enumType) { f.SetValue(target, value); }
			}
			catch { }
		}
	}
}
#endif
