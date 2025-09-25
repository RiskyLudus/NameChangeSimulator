

// TODO : Uhm this has PDF logic so what did you expect.
namespace DW.Tools {
	using System;
	using System.IO;
	using System.IO.Compression;
	using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif

	using UnityEngine;

	public partial class ImageFielding {
		public static class AssetRenderer {
			public enum FileFormat { Png, Jpg, Pdf }

			public static void RenderToFile(
				ImageFieldingAsset layoutAsset,
				RenderData renderData,
				int outputWidthPixels,
				int outputHeightPixels,
				string filePath,
				FileFormat fileFormat) {
				ValidateRenderArgs(layoutAsset, outputWidthPixels, outputHeightPixels);

				var composedTexture = RenderToTexture(layoutAsset, renderData, outputWidthPixels, outputHeightPixels);
				try {
					switch (fileFormat) {
					case FileFormat.Png: {
						var bytes = composedTexture.EncodeToPNG();
						EnsureDirectory(filePath);
						File.WriteAllBytes(filePath, bytes);

						break;
					}
					case FileFormat.Pdf: {
						EnsureDirectory(filePath);
						SaveTextureAsPdfSinglePage_Pure(composedTexture, filePath, 8.5 * 72.0, 11.0 * 72.0, 0.0);

						break;
					}
					}
				}
				finally {
#if UNITY_EDITOR
					UnityEngine.Object.DestroyImmediate(composedTexture);
#else
					UnityEngine.Object.Destroy(composedTexture);
#endif
#if UNITY_EDITOR
					TryRefreshIfInsideAssets(filePath);
#endif
				}
			}

			public static void RenderLayoutsToPdf(
				IList<ImageFieldingAsset> layouts,
				IList<RenderData> renderDatas,
				int outputWidthPixels,
				int outputHeightPixels,
				string pdfPath,
				float pdfDpi = 300f,
				double pageWidthInches = 8.5,
				double pageHeightInches = 11.0,
				double marginInches = 0.25) {
				if (layouts == null || layouts.Count == 0)
					throw new ArgumentException("No layouts provided.");

				ValidateRenderArgs(layouts[0], outputWidthPixels, outputHeightPixels);
				EnsureDirectory(pdfPath);

				var textures = new List<Texture2D>(layouts.Count);
				try {
					for (int i = 0; i < layouts.Count; i++) {
						var rd = (renderDatas != null && renderDatas.Count > 0)
							? renderDatas[Mathf.Clamp(i, 0, renderDatas.Count - 1)]
							: new RenderData();
						var tex = RenderToTexture(layouts[i], rd, outputWidthPixels, outputHeightPixels);
						textures.Add(tex);
					}

					SaveTexturesAsPdfMulti_Pure(
						textures,
						pdfPath,
						pageWidthInches * 72.0,
						pageHeightInches * 72.0,
						marginInches * 72.0
					);
				}
				finally {
					foreach (var t in textures) {
						if (t)
#if UNITY_EDITOR
							UnityEngine.Object.DestroyImmediate(t);
#else
							UnityEngine.Object.Destroy(t);
#endif
					}
#if UNITY_EDITOR
					TryRefreshIfInsideAssets(pdfPath);
#endif
				}
			}

			static void SaveTextureAsPdfSinglePage_Pure(
				Texture2D tex,
				string pdfPath,
				double pageWidthPt,
				double pageHeightPt,
				double marginPt) {
				SaveTexturesAsPdfMulti_Pure(
					new List<Texture2D> { tex },
					pdfPath,
					pageWidthPt,
					pageHeightPt,
					marginPt);
			}

			static void SaveTexturesAsPdfMulti_Pure(
				IList<Texture2D> textures,
				string pdfPath,
				double pageWidthPt,
				double pageHeightPt,
				double marginPt) {
				if (textures == null || textures.Count == 0)
					throw new ArgumentException("No textures");

				var pages = new List<PdfPageData>(textures.Count);

				foreach (var txtr in textures) {
					int txtrWdth = txtr.width;
					int txtrHght = txtr.height;

					double imgWdthPxl = Math.Max(1.0, pageWidthPt - 2.0 * marginPt);
					double imgHghtPxl = Math.Max(1.0, pageHeightPt - 2.0 * marginPt);

					double srcAspect = (double)txtrWdth / (double)txtrHght;
					double boxAspect = imgWdthPxl / imgHghtPxl;
					double drwWdthPnt = (srcAspect >= boxAspect) ? imgWdthPxl : imgHghtPxl * srcAspect;
					double drWdthPnt = (srcAspect >= boxAspect) ? imgWdthPxl / srcAspect : imgHghtPxl;
					double drwOrgnXPt = marginPt + (imgWdthPxl - drwWdthPnt) * 0.5;
					double drwOrgnYPt = marginPt + (imgHghtPxl - drWdthPnt) * 0.5;

					byte[] rgb = ExtractRgb(txtr);
					byte[] flateCmprsd = Flate(rgb);

					pages.Add(new PdfPageData {
						ImgWidth = txtrWdth,
						ImgHeight = txtrHght,
						ImgFlate = flateCmprsd,
						PageW = pageWidthPt,
						PageH = pageHeightPt,
						DrawX = drwOrgnXPt,
						DrawY = drwOrgnYPt,
						DrawW = drwWdthPnt,
						DrawH = drWdthPnt
					});
				}

				byte[] pdf = BuildPdf(pages);
				File.WriteAllBytes(pdfPath, pdf);
			}

			public static Texture2D RenderToTexture(ImageFieldingAsset layout, RenderData rndrData, int outW, int outH) {
				ValidateRenderArgs(layout, outW, outH);

				Texture2D bgReadable = PixelOps.EnsureTextureIsReadable(layout.backgroundImage);
				Color32[] scaled = PixelOps.ScaleTextureNearest(bgReadable, outW, outH);
				var canvas = new Texture2D(outW, outH, TextureFormat.RGBA32, false, false);
				canvas.SetPixels32(scaled);

				RenderAllFields(canvas, layout, rndrData, outW, outH);
				canvas.Apply(false, false);
				return canvas;
			}

			static void ValidateRenderArgs(ImageFieldingAsset layout, int width, int height) {
				if (layout == null || layout.backgroundImage == null)
					throw new ArgumentException("Layout asset or background image is missing.");
				if (width < 8 || height < 8)
					throw new ArgumentException("Output dimensions are too small.");
			}

			static void RenderAllFields(Texture2D canvas, ImageFieldingAsset layout, RenderData rndrData, int outW, int outH) {
				for (int i = 0; i < layout.fields.Count; i++) {
					var fld = layout.fields[i];
					RectInt dstBtmLeft = PixelOps.NormalizedToPixelRectTopLeft(fld.normalizedRect, outW, outH);
					dstBtmLeft = PixelOps.ClampRectangleToTexture(dstBtmLeft, outW, outH);

					if (fld.fieldType == ImageFieldingTypes.Image)
						RenderImageField(canvas, dstBtmLeft, fld, rndrData);
					else
						RenderStringField(canvas, dstBtmLeft, fld, rndrData);
				}
			}

			static void RenderImageField(
				Texture2D canvas,
				RectInt dstBL,
				Field field,
				RenderData rd) {
				Texture2D src = null;

				if (rd?.fieldImageValues != null && rd.fieldImageValues.TryGetValue(field.ID, out var overrideImg))
					src = overrideImg;

				if (!src) {
					var defaultImg = field.image;
					if (defaultImg)
						src = defaultImg;
				}

				if (!src) {
					if (rd != null && rd.drawPlaceholderBoxesForMissingValues)
						PixelOps.DrawFilledRectangleWithOutline(canvas, dstBL, rd.placeholderFillColor, rd.placeholderOutlineColor, rd.placeholderOutlineThicknessPixels);
					return;
				}

				PixelOps.BlitScaledIntoRegionFit(canvas, src, dstBL);
			}


			static void RenderStringField(
				Texture2D canvas,
				RectInt dstBL,
				Field field,
				RenderData rndrData) {
				string value = null;
				if (rndrData?.fieldStringValues != null)
					rndrData.fieldStringValues.TryGetValue(field.ID, out value);

				if (string.IsNullOrEmpty(value)) {
					value = field.text;
				}

				if (!string.IsNullOrEmpty(value)) {
					TextRasterizer.RasterizeStringBasic(canvas, dstBL, value, rndrData);
				} else if (rndrData != null && rndrData.drawPlaceholderBoxesForMissingValues) {
					PixelOps.DrawFilledRectangleWithOutline(canvas, dstBL, rndrData.placeholderFillColor, rndrData.placeholderOutlineColor, rndrData.placeholderOutlineThicknessPixels);
				}
			}

			static Texture2D TryGetFieldImage(RenderData rd, string name) {
				if (rd == null || rd.fieldImageValues == null || string.IsNullOrEmpty(name))
					return null;
				rd.fieldImageValues.TryGetValue(name, out var tex);
				return tex;
			}

			static string TryGetFieldText(RenderData rd, string name) {
				if (rd == null || rd.fieldStringValues == null || string.IsNullOrEmpty(name))
					return null;
				return rd.fieldStringValues.TryGetValue(name, out var s) ? s : null;
			}

			// In memory only (WebGL safe)
			public static byte[] RenderToPngBytes(ImageFieldingAsset layout, RenderData rd, int w, int h) {
				ValidateRenderArgs(layout, w, h);
				var tex = RenderToTexture(layout, rd, w, h);
				try { return tex.EncodeToPNG(); }
				finally { UnityEngine.Object.Destroy(tex); }
			}

			// In memory only (WebGL safe)
			public static byte[] RenderLayoutsToPdfBytes(
				IList<ImageFieldingAsset> layouts,
				IList<RenderData> rds,
				int w, int h,
				double pageWidthInches = 8.5,
				double pageHeightInches = 11.0,
				double marginInches = 0.25
			) {
				if (layouts == null || layouts.Count == 0)
					throw new ArgumentException("No layouts provided.");
				ValidateRenderArgs(layouts[0], w, h);

				var textures = new List<Texture2D>(layouts.Count);
				try {
					for (int i = 0; i < layouts.Count; i++) {
						var rd = (rds != null && rds.Count > 0) ? rds[Mathf.Clamp(i, 0, rds.Count - 1)] : new RenderData();
						textures.Add(RenderToTexture(layouts[i], rd, w, h));
					}

					var pages = new List<PdfPageData>(textures.Count);
					foreach (var tx in textures) {
						double pageW = pageWidthInches * 72.0;
						double pageH = pageHeightInches * 72.0;
						double margin = marginInches * 72.0;

						double boxW = Math.Max(1.0, pageW - 2.0 * margin);
						double boxH = Math.Max(1.0, pageH - 2.0 * margin);

						double srcAspect = (double)tx.width / (double)tx.height;
						double boxAspect = boxW / boxH;
						double drawW = (srcAspect >= boxAspect) ? boxW : boxH * srcAspect;
						double drawH = (srcAspect >= boxAspect) ? boxW / srcAspect : boxH;
						double drawX = margin + (boxW - drawW) * 0.5;
						double drawY = margin + (boxH - drawH) * 0.5;

						byte[] rgb = ExtractRgb(tx);
						byte[] flate = Flate(rgb);

						pages.Add(new PdfPageData {
							ImgWidth = tx.width,
							ImgHeight = tx.height,
							ImgFlate = flate,
							PageW = pageW,
							PageH = pageH,
							DrawX = drawX,
							DrawY = drawY,
							DrawW = drawW,
							DrawH = drawH
						});
					}
					return BuildPdf(pages);
				}
				finally {
					for (int i = 0; i < textures.Count; i++)
						if (textures[i])
							UnityEngine.Object.Destroy(textures[i]);
				}
			}

			static void EnsureDirectory(string path) {
				string dir = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
					Directory.CreateDirectory(dir);
			}

#if UNITY_EDITOR
			static void TryRefreshIfInsideAssets(string filePath) {
				if (string.IsNullOrEmpty(filePath))
					return;
				string normalized = filePath.Replace('\\', '/');
				string assets = Application.dataPath.Replace('\\', '/');
				if (normalized.StartsWith(assets))
					UnityEditor.AssetDatabase.Refresh();
			}
#endif

			class PdfPageData {
				public int ImgWidth;
				public int ImgHeight;
				public byte[] ImgFlate;
				public double PageW, PageH;
				public double DrawX, DrawY, DrawW, DrawH;
			}

			static byte[] ExtractRgb(Texture2D tex) {
				var px = tex.GetPixels32();
				var rgb = new byte[px.Length * 3];
				int o = 0;
				for (int i = 0; i < px.Length; i++) {
					var c = px[i];
					rgb[o++] = c.r;
					rgb[o++] = c.g;
					rgb[o++] = c.b;
				}
				return rgb;
			}

			static byte[] Flate(byte[] data) {
				using (var ms = new MemoryStream()) {
					ms.WriteByte(0x78);
					ms.WriteByte(0x9C);
					using (var ds = new DeflateStream(ms, System.IO.Compression.CompressionLevel.Optimal, true)) {
						ds.Write(data, 0, data.Length);
					}
					uint adler = Adler32(data);
					ms.WriteByte((byte)((adler >> 24) & 0xFF));
					ms.WriteByte((byte)((adler >> 16) & 0xFF));
					ms.WriteByte((byte)((adler >> 8) & 0xFF));
					ms.WriteByte((byte)(adler & 0xFF));
					return ms.ToArray();
				}
			}

			private static uint Adler32(byte[] data) {
				const uint MOD = 65521;
				uint a = 1, b = 0;
				for (int i = 0; i < data.Length; i++) {
					a = (a + data[i]) % MOD;
					b = (b + a) % MOD;
				}
				return (b << 16) | a;
			}

			private static byte[] BuildPdf(List<PdfPageData> pages) {
				int pagesId = 1;
				var ids = AllocateObjectIds(pages.Count, firstAfterPages: pagesId + 1);

				var objects = new List<byte[]> { null };

				objects.Add(BuildPagesNode(pagesId, ids.PageIds));
				for (int i = 0; i < pages.Count; i++) {
					var p = pages[i];
					objects.Add(BuildImageObject(p, ids.ImageIds[i]));
					objects.Add(BuildResourcesDict(ids.ImageIds[i]));
					objects.Add(BuildContentStream(p));
					objects.Add(BuildPageObject(p, pagesId, ids.ResourceIds[i], ids.ContentIds[i]));
				}
				objects.Add(BuildCatalog(ids.CatalogId, pagesId));

				return WritePdf(objects, rootId: ids.CatalogId);
			}

			private sealed class IdPlan {
				public List<int> PageIds = new();
				public List<int> ImageIds = new();
				public List<int> ResourceIds = new();
				public List<int> ContentIds = new();
				public int CatalogId;
			}

			private static IdPlan AllocateObjectIds(int pageCount, int firstAfterPages) {
				var plan = new IdPlan();
				int nextId = firstAfterPages;

				for (int i = 0; i < pageCount; i++) {
					int img = nextId++;
					int res = nextId++;
					int cnt = nextId++;
					int pg = nextId++;

					plan.ImageIds.Add(img);
					plan.ResourceIds.Add(res);
					plan.ContentIds.Add(cnt);
					plan.PageIds.Add(pg);
				}
				plan.CatalogId = nextId++;
				return plan;
			}

			private static byte[] BuildPagesNode(int pagesId, List<int> pageIds) {
				var sb = new System.Text.StringBuilder(64 + pageIds.Count * 8);
				sb.Append("<< /Type /Pages /Count ").Append(pageIds.Count).Append(" /Kids [ ");
				for (int i = 0; i < pageIds.Count; i++)
					sb.Append(pageIds[i]).Append(" 0 R ");
				sb.Append("] >>");
				return Enc(sb.ToString());
			}

			private static byte[] BuildImageObject(PdfPageData p, int imageId) {
				string dict = "<< /Type /XObject /Subtype /Image"
					+ " /Width " + p.ImgWidth
					+ " /Height " + p.ImgHeight
					+ " /ColorSpace /DeviceRGB /BitsPerComponent 8"
					+ " /Filter /FlateDecode /Length " + p.ImgFlate.Length
					+ " >>\n";

				using var ms = new MemoryStream();
				WriteAscii(ms, dict);
				WriteAscii(ms, "stream\n");
				ms.Write(p.ImgFlate, 0, p.ImgFlate.Length);
				WriteAscii(ms, "\nendstream");
				return ms.ToArray();
			}

			private static byte[] BuildResourcesDict(int imageObjectId) {
				string res = "<< /XObject << /Im0 " + imageObjectId + " 0 R >> >>";
				return Enc(res);
			}

			private static byte[] BuildContentStream(PdfPageData p) {
				string content =
					"q\n" +
					F(p.DrawW) + " 0 0 " + F(-p.DrawH) + " " + F(p.DrawX) + " " + F(p.DrawY + p.DrawH) + " cm\n" +
					"/Im0 Do\nQ\n";

				byte[] body = System.Text.Encoding.ASCII.GetBytes(content);
				string dict = "<< /Length " + body.Length + " >>\n";

				using var ms = new MemoryStream();
				WriteAscii(ms, dict);
				WriteAscii(ms, "stream\n");
				ms.Write(body, 0, body.Length);
				WriteAscii(ms, "\nendstream");
				return ms.ToArray();
			}

			private static byte[] BuildPageObject(PdfPageData p, int pagesId, int resourcesId, int contentsId) {
				string page =
					"<< /Type /Page /Parent " + pagesId + " 0 R"
					+ " /MediaBox [0 0 " + F(p.PageW) + " " + F(p.PageH) + "]"
					+ " /Resources " + resourcesId + " 0 R"
					+ " /Contents " + contentsId + " 0 R >>";
				return Enc(page);
			}

			private static byte[] BuildCatalog(int catalogId, int pagesId) {
				string root = "<< /Type /Catalog /Pages " + pagesId + " 0 R >>";
				return Enc(root);
			}

			private static byte[] WritePdf(List<byte[]> objects, int rootId) {
				using var ms = new MemoryStream();

				WriteHeader(ms);
				var offsets = WriteObjects(ms, objects);
				long xrefPos = WriteXref(ms, offsets);
				WriteTrailer(ms, objects.Count, rootId, xrefPos);

				return ms.ToArray();
			}

			private static void WriteHeader(Stream s) {
				WriteAscii(s, "%PDF-1.4\n%‚„œ”\n");
			}

			private static List<long> WriteObjects(Stream s, List<byte[]> objects) {
				var xref = new List<long>(objects.Count) { 0L };
				for (int i = 1; i < objects.Count; i++) {
					long pos = s.Position;
					xref.Add(pos);
					WriteAscii(s, i.ToString());
					WriteAscii(s, " 0 obj\n");
					s.Write(objects[i], 0, objects[i].Length);
					WriteAscii(s, "\nendobj\n");
				}
				return xref;
			}

			private static long WriteXref(Stream s, List<long> offsets) {
				long start = s.Position;
				WriteAscii(s, "xref\n");
				WriteAscii(s, "0 " + (offsets.Count) + "\n");
				WriteAscii(s, "0000000000 65535 f \n");
				for (int i = 1; i < offsets.Count; i++) {
					string off = ((int)offsets[i]).ToString().PadLeft(10, '0');
					WriteAscii(s, off + " 00000 n \n");
				}
				return start;
			}

			private static void WriteTrailer(Stream s, int objectCount, int rootId, long xrefPos) {
				WriteAscii(s, "trailer\n");
				WriteAscii(s, "<< /Size " + objectCount + " /Root " + rootId + " 0 R >>\n");
				WriteAscii(s, "startxref\n");
				WriteAscii(s, xrefPos.ToString() + "\n");
				WriteAscii(s, "%%EOF");
			}

			static void WriteAscii(Stream s, string text) {
				var b = System.Text.Encoding.ASCII.GetBytes(text);
				s.Write(b, 0, b.Length);
			}
			static byte[] Enc(string s) => System.Text.Encoding.ASCII.GetBytes(s);
			static string F(double v) {
				return v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
			}

			public static System.Collections.IEnumerator RenderLayoutsToPdf_Co(
	System.Collections.Generic.IList<ImageFieldingAsset> layouts,
	System.Collections.Generic.IList<RenderData> renderDatas,
	int outputWidthPixels,
	int outputHeightPixels,
	string pdfPath,
	float pdfDpi,
	double pageWidthInches,
	double pageHeightInches,
	double marginInches,
	System.Action<int, int, string> onProgress // (current, total, phase)
) {
				if (layouts == null || layouts.Count == 0)
					yield break;

				var textures = new System.Collections.Generic.List<Texture2D>(layouts.Count);
				try {
					int total = layouts.Count;
					for (int i = 0; i < layouts.Count; i++) {
						onProgress?.Invoke(i, total, "render");
						var rd = (renderDatas != null && renderDatas.Count > 0)
							? renderDatas[Mathf.Clamp(i, 0, renderDatas.Count - 1)]
							: new RenderData();
						var tex = RenderToTexture(layouts[i], rd, outputWidthPixels, outputHeightPixels);
						textures.Add(tex);
						yield return null; // let UI breathe
					}

					onProgress?.Invoke(total, total, "compress");
					double pageW = pageWidthInches * 72.0;
					double pageH = pageHeightInches * 72.0;
					double margin = marginInches * 72.0;

					var pages = new System.Collections.Generic.List<PdfPageData>(textures.Count);
					for (int i = 0; i < textures.Count; i++) {
						var tx = textures[i];
						double boxW = System.Math.Max(1.0, pageW - 2.0 * margin);
						double boxH = System.Math.Max(1.0, pageH - 2.0 * margin);

						double srcAspect = (double)tx.width / (double)tx.height;
						double boxAspect = boxW / boxH;
						double drawW = (srcAspect >= boxAspect) ? boxW : boxH * srcAspect;
						double drawH = (srcAspect >= boxAspect) ? boxW / srcAspect : boxH;
						double drawX = margin + (boxW - drawW) * 0.5;
						double drawY = margin + (boxH - drawH) * 0.5;

						byte[] rgb = ExtractRgb(tx);
						byte[] flate = Flate(rgb);

						pages.Add(new PdfPageData {
							ImgWidth = tx.width,
							ImgHeight = tx.height,
							ImgFlate = flate,
							PageW = pageW,
							PageH = pageH,
							DrawX = drawX,
							DrawY = drawY,
							DrawW = drawW,
							DrawH = drawH
						});
						if ((i & 1) == 0)
							yield return null; // yield every couple pages of compression
					}

					onProgress?.Invoke(total, total, "write");
					var pdfBytes = BuildPdf(pages);
					EnsureDirectory(pdfPath);
					System.IO.File.WriteAllBytes(pdfPath, pdfBytes);
				}
				finally {
					for (int i = 0; i < textures.Count; i++) {
						if (textures[i]) {
#if UNITY_EDITOR
							UnityEngine.Object.DestroyImmediate(textures[i]);
#else
				UnityEngine.Object.Destroy(textures[i]);
#endif
						}
					}
#if UNITY_EDITOR
					TryRefreshIfInsideAssets(pdfPath);
#endif
				}
			}
		}
	}
}
