

namespace DW.Tools {
	using System;

	using UnityEngine;

	public static class PixelOps {
		public static Action<string> Logger;
		private static void Log(string msg) { Logger?.Invoke(msg); }

		public static Texture2D EnsureTextureIsReadable(Texture2D source) {
			if (source == null)
				return null;
			if (source.isReadable)
				return source;

			Log($"EnsureTextureIsReadable: creating CPU copy of '{source.name}' {source.width}x{source.height}");

			var rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
			Graphics.Blit(source, rt);

			var prev = RenderTexture.active;
			RenderTexture.active = rt;

			var tex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, false);
			tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
			tex.Apply(false, false);

			// restore and release safely
			RenderTexture.active = prev == rt ? null : prev;
			RenderTexture.ReleaseTemporary(rt);

			return tex;
		}


		public static Color32[] ScaleTextureNearest(Texture2D src, int dstWdth, int dstHght) {
			var readable = EnsureTextureIsReadable(src);
			Log($"ScaleTextureNearest: src={readable.width}x{readable.height} -> dst={dstWdth}x{dstHght}");
			Color32[] srcPxls = readable.GetPixels32();
			Color32[] dstPxls = new Color32[dstWdth * dstHght];

			int srcWdth = readable.width;
			int srcHght = readable.height;

			for (int y = 0; y < dstHght; y++) {
				int sampleY = Mathf.Clamp((int)((y / (float)dstHght) * srcHght), 0, srcHght - 1);
				for (int x = 0; x < dstWdth; x++) {
					int sampleX = Mathf.Clamp((int)((x / (float)dstWdth) * srcWdth), 0, srcWdth - 1);
					dstPxls[y * dstWdth + x] = srcPxls[sampleY * srcWdth + sampleX];
				}
			}

			if (readable != src)
				UnityEngine.Object.Destroy(readable);
			return dstPxls;
		}

		public static RectInt ClampRectangleToTexture(RectInt rect, int texW, int texH) {
			int x = Mathf.Clamp(rect.x, 0, Mathf.Max(0, texW));
			int y = Mathf.Clamp(rect.y, 0, Mathf.Max(0, texH));
			int w = Mathf.Clamp(rect.width, 0, Mathf.Max(0, texW - x));
			int h = Mathf.Clamp(rect.height, 0, Mathf.Max(0, texH - y));
			return new RectInt(x, y, w, h);
		}

		// Convert normalized-rect using TOP-LEFT origin to a pixel rect using BOTTOM-LEFT origin
		public static RectInt NormalizedToPixelRectTopLeft(Rect normTopLeft, int outW, int outH) {
			int pxlX = Mathf.RoundToInt(normTopLeft.x * outW);
			int pxlYTop = Mathf.RoundToInt(normTopLeft.y * outH);
			int pxlWdth = Mathf.RoundToInt(normTopLeft.width * outW);
			int pxlHght = Mathf.RoundToInt(normTopLeft.height * outH);
			int pxlYBtmLeft = outH - pxlYTop - pxlHght;
			return new RectInt(pxlX, pxlYBtmLeft, pxlWdth, pxlHght);
		}

		/// <summary>
		/// bit block transfer (copying a block of pixels from one buffer to another)
		/// </summary>
		/// <param name="dst"></param>
		/// <param name="src"></param>
		/// <param name="dstRegionBL"></param>
		public static void BlitScaledIntoRegionFit(Texture2D dst, Texture2D src, RectInt dstRegionBL) {
			if (dst == null || src == null || dstRegionBL.width <= 0 || dstRegionBL.height <= 0)
				return;

			var readable = EnsureTextureIsReadable(src);

			float srcAspect = readable.width / (float)readable.height;
			float dstAspect = dstRegionBL.width / (float)dstRegionBL.height;
			int trgWdth;
			int trgHght;

			if (srcAspect > dstAspect) {
				trgWdth = dstRegionBL.width;
				trgHght = Mathf.Max(1, Mathf.RoundToInt(trgWdth / srcAspect));
			} else {
				trgHght = dstRegionBL.height;
				trgWdth = Mathf.Max(1, Mathf.RoundToInt(trgHght * srcAspect));
			}

			int offsetX = dstRegionBL.x + (dstRegionBL.width - trgWdth) / 2;
			int offsetY = dstRegionBL.y + (dstRegionBL.height - trgHght) / 2;

			Color[] dstBlock = dst.GetPixels(offsetX, offsetY, trgWdth, trgHght);

			// UV Space
			for (int y = 0; y < trgHght; y++) {
				float vNorm = (y + 0.5f) / trgHght;
				int row = y * trgWdth;
				for (int x = 0; x < trgWdth; x++) {
					float uNorm = (x + 0.5f) / trgWdth;

					Color smpl = readable.GetPixelBilinear(uNorm, vNorm);

					if (smpl.a <= 0f) continue; // fully transparent → skip

					int i = row + x;
					Color destBck = dstBlock[i];                       

					// standard "source-over" alpha blend; flatten to opaque (a=1)
					float invrsAlpha = 1f - smpl.a;
					dstBlock[i] = new Color(
						destBck.r * invrsAlpha + smpl.r * smpl.a,
						destBck.g * invrsAlpha + smpl.g * smpl.a,
						destBck.b * invrsAlpha + smpl.b * smpl.a,
						1f
					);
				}
			}

			dst.SetPixels(offsetX, offsetY, trgWdth, trgHght, dstBlock);
			if (readable != src)
				UnityEngine.Object.Destroy(readable);
		}


		public static void DrawFilledRectangleWithOutline(Texture2D tex, RectInt rect, Color fill, Color outline, int outlineThickness) {
			if (tex == null || rect.width <= 0 || rect.height <= 0)
				return;

			Color32[] fillBlock = new Color32[rect.width * rect.height];
			for (int i = 0; i < fillBlock.Length; i++)
				fillBlock[i] = fill;
			tex.SetPixels32(rect.x, rect.y, rect.width, rect.height, fillBlock);

			// Top
			Color32[] line = new Color32[rect.width];
			for (int i = 0; i < line.Length; i++)
				line[i] = outline;

			Color32[] vline = new Color32[rect.height];
			for (int i = 0; i < vline.Length; i++) {
				vline[i] = outline;
			}

			int tthick = Mathf.Max(1, outlineThickness);
			for (int i = 0; i < tthick; i++) {
				// Top
				tex.SetPixels32(rect.x, rect.y + rect.height - 1 - i, rect.width, 1, line);
				// Left/Right
				tex.SetPixels32(rect.x + i, rect.y, 1, rect.height, vline);
				tex.SetPixels32(rect.x + rect.width - 1 - i, rect.y, 1, rect.height, vline);
				// Bottom
				tex.SetPixels32(rect.x, rect.y + i, rect.width, 1, line);
			}
		}
	}
}
