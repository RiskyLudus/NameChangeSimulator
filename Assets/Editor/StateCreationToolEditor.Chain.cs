#if UNITY_EDITOR
namespace NameChangeSimulator.Editor {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using DW.Tools;

	using UnityEngine;

	public partial class StateCreationToolEditor {
		private List<ImageFieldingAsset> ResolveChain(ImageFieldingAsset root) {
			var visited = new HashSet<ImageFieldingAsset>();
			var viaMethod = TryResolveChainViaMethod(root, visited);
			if (viaMethod.Count > 0)
				return viaMethod;

			var viaMembers = TryResolveChainViaMembers(root, visited);
			if (viaMembers.Count > 0)
				return viaMembers;

			return new List<ImageFieldingAsset> { root };
		}

		private List<ImageFieldingAsset> TryResolveChainViaMethod(ImageFieldingAsset root, HashSet<ImageFieldingAsset> visited) {
			var collected = new List<ImageFieldingAsset>();
			foreach (var m in GetCandidateMethods()) {
				if (TryInvokeMethodVariants(m, root, collected, visited))
					return collected;
			}
			return collected;
		}

		private static IEnumerable<MethodInfo> GetCandidateMethods() {
			var t = typeof(ImageFieldingAsset);
			var candidateNames = new[] { "GetChainedLayouts", "GetChainedLayoutChain", "GetChain" };
			return t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
					.Where(m => candidateNames.Contains(m.Name));
		}

		private bool TryInvokeMethodVariants(MethodInfo m, ImageFieldingAsset root, List<ImageFieldingAsset> collected, HashSet<ImageFieldingAsset> visited) {
			try {
				var ps = m.GetParameters();
				object target = m.IsStatic ? null : root;

				if (ps.Length == 0)
					return TryNoArg(m, target, collected, visited);
				if (ps.Length == 1 && typeof(IList).IsAssignableFrom(ps[0].ParameterType))
					return TryOutListSingle(m, target, collected, visited);
				if (ps.Length == 1 && ps[0].ParameterType == typeof(bool))
					return TryBoolSingle(m, target, collected, visited);
				if (ps.Length == 2)
					return TryTwoParamCombos(m, target, ps, collected, visited);
			}
			catch { }
			return false;
		}

		private bool TryNoArg(MethodInfo m, object target, List<ImageFieldingAsset> collected, HashSet<ImageFieldingAsset> visited) {
			var res = m.Invoke(target, null);
			return AppendFromEnumerable(res, collected, visited);
		}

		private bool TryOutListSingle(MethodInfo m, object target, List<ImageFieldingAsset> collected, HashSet<ImageFieldingAsset> visited) {
			var list = (IList)Activator.CreateInstance(typeof(List<ImageFieldingAsset>));
			m.Invoke(target, new object[] { list });
			return AppendFromEnumerable(list, collected, visited);
		}

		private bool TryBoolSingle(MethodInfo m, object target, List<ImageFieldingAsset> collected, HashSet<ImageFieldingAsset> visited) {
			foreach (var flag in new[] { true, false }) {
				var res = m.Invoke(target, new object[] { flag });
				if (AppendFromEnumerable(res, collected, visited))
					return true;
			}
			return false;
		}

		private bool TryTwoParamCombos(MethodInfo m, object target, ParameterInfo[] ps, List<ImageFieldingAsset> collected, HashSet<ImageFieldingAsset> visited) {
			bool firstIsList = typeof(IList).IsAssignableFrom(ps[0].ParameterType);
			bool secondIsList = typeof(IList).IsAssignableFrom(ps[1].ParameterType);

			if (firstIsList && ps[1].ParameterType == typeof(bool))
				return TryListThenBool(m, target, collected, visited);

			if (ps[0].ParameterType == typeof(bool) && secondIsList)
				return TryBoolThenList(m, target, collected, visited);

			return false;
		}

		private bool TryListThenBool(MethodInfo m, object target, List<ImageFieldingAsset> collected, HashSet<ImageFieldingAsset> visited) {
			var list = (IList)Activator.CreateInstance(typeof(List<ImageFieldingAsset>));
			foreach (var flag in new[] { true, false }) {
				m.Invoke(target, new object[] { list, flag });
				if (AppendFromEnumerable(list, collected, visited))
					return true;
				list.Clear();
			}
			return false;
		}

		private bool TryBoolThenList(MethodInfo m, object target, List<ImageFieldingAsset> collected, HashSet<ImageFieldingAsset> visited) {
			var list = (IList)Activator.CreateInstance(typeof(List<ImageFieldingAsset>));
			foreach (var flag in new[] { true, false }) {
				m.Invoke(target, new object[] { flag, list });
				if (AppendFromEnumerable(list, collected, visited))
					return true;
				list.Clear();
			}
			return false;
		}

		private List<ImageFieldingAsset> TryResolveChainViaMembers(ImageFieldingAsset root, HashSet<ImageFieldingAsset> visited) {
			var chain = new List<ImageFieldingAsset>();
			var current = root;

			var singleNames = new[] { "next", "nextLayout", "nextInChain", "chainNext" };
			var multiNames = new[] { "chainedLayouts", "nextLayouts", "layouts", "pages", "chain", "layoutChain" };

			int guard = 0;
			while (current && visited.Add(current) && guard++ < 1024) {
				chain.Add(current);

				var list = FindFirstListOfLayouts(current, multiNames);
				if (list != null && list.Count > 0) {
					foreach (var la in list)
						if (la && visited.Add(la))
							chain.Add(la);
					break;
				}

				var next = FindFirstSingleNext(current, singleNames);
				if (next && !visited.Contains(next)) { current = next; continue; }
				break;
			}
			return chain;
		}

		private static bool AppendFromEnumerable(object enumerable, List<ImageFieldingAsset> outList, HashSet<ImageFieldingAsset> visited) {
			if (enumerable is IEnumerable en) {
				foreach (var obj in en) {
					if (obj is ImageFieldingAsset la && la && visited.Add(la))
						outList.Add(la);
				}
				return outList.Count > 0;
			}
			return false;
		}

		private static List<ImageFieldingAsset> FindFirstListOfLayouts(ImageFieldingAsset obj, string[] preferredNames) {
			var t = obj.GetType();

			foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
				if (!typeof(IList).IsAssignableFrom(f.FieldType))
					continue;

				if (preferredNames.Any(n => string.Equals(f.Name, n, StringComparison.OrdinalIgnoreCase)) || IsIListOfLayouts(f.FieldType)) {
					var val = f.GetValue(obj) as IList;
					var list = ToLayoutList(val);

					if (list != null && list.Count > 0)
						return list;
				}
			}

			foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
				if (!p.CanRead)
					continue;
				if (!typeof(IList).IsAssignableFrom(p.PropertyType))
					continue;
				if (preferredNames.Any(n => string.Equals(p.Name, n, StringComparison.OrdinalIgnoreCase)) || IsIListOfLayouts(p.PropertyType)) {
					var val = p.GetValue(obj, null) as IList;
					var list = ToLayoutList(val);

					if (list != null && list.Count > 0)
						return list;
				}
			}

			return null;
		}

		private static ImageFieldingAsset FindFirstSingleNext(ImageFieldingAsset obj, string[] preferredNames) {
			var t = obj.GetType();

			foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
				if (typeof(ImageFieldingAsset).IsAssignableFrom(f.FieldType)) {
					if (preferredNames.Any(n => string.Equals(f.Name, n, StringComparison.OrdinalIgnoreCase)))
						return f.GetValue(obj) as ImageFieldingAsset;

					var val = f.GetValue(obj) as ImageFieldingAsset;
					if (val)
						return val;
				}
			}

			foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
				if (!p.CanRead)
					continue;

				if (typeof(ImageFieldingAsset).IsAssignableFrom(p.PropertyType)) {
					if (preferredNames.Any(n => string.Equals(p.Name, n, StringComparison.OrdinalIgnoreCase)))
						return p.GetValue(obj, null) as ImageFieldingAsset;

					var val = p.GetValue(obj, null) as ImageFieldingAsset;
					if (val)
						return val;
				}
			}

			return null;
		}

		private static bool IsIListOfLayouts(Type t) {
			if (!typeof(IList).IsAssignableFrom(t))
				return false;

			if (t.IsGenericType && t.GetGenericArguments().Length == 1) {
				return typeof(ImageFieldingAsset).IsAssignableFrom(t.GetGenericArguments()[0]);
			}

			return false;
		}

		private static List<ImageFieldingAsset> ToLayoutList(IList list) {
			if (list == null)
				return null;

			var res = new List<ImageFieldingAsset>();
			foreach (var obj in list)
				if (obj is ImageFieldingAsset la && la)
					res.Add(la);

			return res;
		}
	}
}
#endif
