

namespace DW.Tools {
	using UnityEngine;

	public static class TextRasterizer {
		public static void RasterizeStringBasic(
			Texture2D targetTexture,
			RectInt destinationRectPixelsBL,
			string text,
			ImageFielding.RenderData renderData
		) {

			if (string.IsNullOrEmpty(text) || targetTexture == null || destinationRectPixelsBL.width <= 0 || destinationRectPixelsBL.height <= 0) {
				return;
			}



			Font fallbackFont;
#if UNITY_6000_0_OR_NEWER
			fallbackFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
fallbackFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif

			var font = renderData != null && renderData.textFont != null
				? renderData.textFont
				: fallbackFont;
			var size = renderData != null ? Mathf.Max(1, renderData.textFontSizePixels) : 32;
			var color = renderData != null ? renderData.textColor : Color.black;
			int pad = renderData != null ? Mathf.Max(0, renderData.paddingPixels) : 0;

			font.RequestCharactersInTexture(text, size, FontStyle.Normal);

			var atlasTex = font.material != null ? font.material.mainTexture as Texture2D : null;
			if (atlasTex == null) {
				return;
			}

			var readableAtlas = PixelOps.EnsureTextureIsReadable(atlasTex);

			int totalAdvance = 0;
			int minY = int.MaxValue, maxY = int.MinValue;
			for (int i = 0; i < text.Length; i++) {
				if (!font.GetCharacterInfo(text[i], out CharacterInfo chr, size, FontStyle.Normal))
					continue;
				totalAdvance += chr.advance;
				minY = Mathf.Min(minY, chr.minY);
				maxY = Mathf.Max(maxY, chr.maxY);
			}
			if (minY == int.MaxValue) { return; }

			int boxW = Mathf.Max(1, destinationRectPixelsBL.width - pad * 2);
			int boxH = Mathf.Max(1, destinationRectPixelsBL.height - pad * 2);
			int glyphH = Mathf.Max(1, maxY - minY);

			int penX = destinationRectPixelsBL.x + pad + Mathf.RoundToInt((boxW - totalAdvance) * 0.5f);
			int baseline = destinationRectPixelsBL.y + pad + Mathf.RoundToInt((boxH - glyphH) * 0.5f) - minY;


			for (int i = 0; i < text.Length; i++) {
				if (!font.GetCharacterInfo(text[i], out CharacterInfo chr, size, FontStyle.Normal))
					continue;

				int dstX = penX + chr.minX;
				int dstY = baseline + chr.minY;

				int dstW = chr.glyphWidth;
				int dstH = chr.glyphHeight;
				BlitGlyphParallelogramToTextureAlpha(
					targetTexture,
					dstX, dstY,
					dstW, dstH,
					chr,
					readableAtlas,
					color
				);
				penX += chr.advance;
			}
		}

		static void BlitGlyphParallelogramToTextureAlpha(
			Texture2D dst,
			int dstLeft, int dstBottom,
			int dstW, int dstH,
			CharacterInfo ch,
			Texture2D readableAtlas,
			Color textColor
		) {
			Vector2 uvBtmLeft = ch.uvBottomLeft;
			Vector2 uvBtmRight = ch.uvBottomRight;
			Vector2 uvTopLeft = ch.uvTopLeft;

			Vector2 uRight = uvBtmRight - uvBtmLeft;
			Vector2 uUp = uvTopLeft - uvBtmLeft;


			Color[] destination = dst.GetPixels(dstLeft, dstBottom, dstW, dstH);

			// For each destination pixel, map to UV and sample the atlas
			for (int y = 0; y < dstH; y++) {
				float vNorm = (y + 0.5f) / dstH;
				Vector2 rowBaseUV = uvBtmLeft + uUp * vNorm; // move up along the glyph, from UV at the start of the current row)
				int rowOffset = y * dstW;

				for (int x = 0; x < dstW; x++) {
					float uNorm = (x + 0.5f) / dstW;
					Vector2 uv = rowBaseUV + uRight * uNorm; // move right along the glyph

					// Sample the atlas (bilinear) – works for rotated/flipped UVs.
					Color smpl = readableAtlas.GetPixelBilinear(uv.x, uv.y);

					// Pixel to Char coverage
					// Treat coverage as max of rgba to avoid black bars from colored atlases
					float coverage = Mathf.Max(smpl.a, Mathf.Max(smpl.r, Mathf.Max(smpl.g, smpl.b))); 
					if (coverage <= 0f)
						continue;

					// Alpha-over composite into the destination block
					int i = rowOffset + x;
					Color dstPxl = destination[i];
					float cvrgInvrs = 1f - coverage;
					destination[i] = new Color(
						dstPxl.r * cvrgInvrs + textColor.r * coverage,
						dstPxl.g * cvrgInvrs + textColor.g * coverage,
						dstPxl.b * cvrgInvrs + textColor.b * coverage,
						1f
					);
				}
			}

			dst.SetPixels(dstLeft, dstBottom, dstW, dstH, destination);
		}
	}
}
