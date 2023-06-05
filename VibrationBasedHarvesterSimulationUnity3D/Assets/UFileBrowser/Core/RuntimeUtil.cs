namespace uFileBrowser {

	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;
	using System.IO;
	using System;


	public struct FileUtil {

		public static string ReadText (string path) {

			try {
				StreamReader sr = File.OpenText(path);
				string data = sr.ReadToEnd();
				sr.Close();
				return data;
			} catch {
				return "";
			}


		}


		public static void WriteText (string data, string path) {

			try {
				FileStream fs = new FileStream(path, FileMode.Create);
				StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
				sw.Write(data);
				sw.Close();
				fs.Close();
			} catch {
				return;
			}

		}

		public static bool FileOrDirectoryExists (string path) {
			return File.Exists(path) || Directory.Exists(path);
		}


		public static bool CreateFolder (string path) {
			if (Directory.Exists(path))
				return true;
			string parentPath = "";
			try {
				parentPath = new FileInfo(path).Directory.FullName;
			} catch {
				return false;
			}
			if (Directory.Exists(parentPath)) {
				try {
					Directory.CreateDirectory(path);
					return true;
				} catch {
					return false;
				}
			} else {
				bool success = CreateFolder(parentPath);
				if (!success) {
					return false;
				}
				try {
					Directory.CreateDirectory(path);
					return true;
				} catch {
					return false;
				}
			}
		}


		public static bool DeleteFile (string path) {
			try {
				if (File.Exists(path)) {
					File.Delete(path);
				} else if (Directory.Exists(path)) {
					Directory.Delete(path, true);
				}
				return true;
			} catch {
				return false;
			}
		}


		public static bool Rename (string path, string newName) {
			try {
				string ex = Path.GetExtension(path);
				if (ex != Path.GetExtension(newName)) {
					newName += ex;
				}
				File.Move(path, PathUtil.CombinePaths(PathUtil.GetFileParentPath(path), newName));
				return true;
			} catch {
				return false;
			}
		}


	}


	public struct PathUtil {



		public static string GetFullPath (string path) {
			try {
				return new FileInfo(path).FullName;
			} catch {
				return "";
			}
		}


		public static string CombinePaths (params string[] paths) {
			string path = "";
			for (int i = 0; i < paths.Length; i++) {
				path = Path.Combine(path, paths[i]);
			}
			return path;
		}


		public static string GetURL (string path) {
			return (new Uri(path)).AbsoluteUri;
		}


		public static string GetURL (params string[] paths) {
			return GetURL(CombinePaths(paths));
		}


		public static string GetFileParentPath (string path) {
			try {
				return new FileInfo(path).Directory.FullName;
			} catch {
				return "";
			}
		}


		public static string GetDirectoryParentPath (string path) {
			try {
				return new DirectoryInfo(path).Parent.FullName;
			} catch {
				return "";
			}
		}

	}


	public struct TransformUtil {


		public static Transform GetCreateName (Transform root, string name) {
			if (root) {
				if (string.IsNullOrEmpty(name)) {
					return root;
				}
				Transform tf = root.Find(name);
				if (tf) {
					return tf;
				} else {
					GameObject o = new GameObject(name);
					o.transform.SetParent(root);
					o.transform.localPosition = Vector3.zero;
					o.transform.localRotation = Quaternion.identity;
					o.transform.localScale = Vector3.one;
					return o.transform;
				}
			} else {
				return null;
			}
		}


		public static Transform GetCreate (Transform root, string path) {
			if (root) {
				if (string.IsNullOrEmpty(path)) {
					return root;
				}
				Transform tf = root.Find(path);
				if (tf) {
					return tf;
				} else {
					string[] paths = path.Split('/', '\\');
					Transform prevTF = root;
					foreach (string name in paths) {
						prevTF = GetCreateName(prevTF, name);
					}
					return prevTF;
				}
			} else {
				return null;
			}
		}


		public static void ClearChildren (Transform tf) {
			while (tf.childCount > 0) {
				GameObject.DestroyImmediate(tf.GetChild(0).gameObject, true);
			}
		}


		public static bool InCanvas (Transform tf) {
			while (tf.parent) {
				tf = tf.parent;
				if (tf.GetComponent<Canvas>()) {
					return true;
				}
			}
			return false;
		}


		public static Transform GetCreateCanvas () {
			Canvas c = Transform.FindObjectOfType<Canvas>();
			if (c) {
				return c.transform;
			} else {
				GameObject g = new GameObject("Canvas");
				c = g.AddComponent<Canvas>();
				c.renderMode = RenderMode.ScreenSpaceOverlay;
				g.AddComponent<CanvasScaler>();
				g.AddComponent<GraphicRaycaster>();
				return g.transform;
			}
		}


		public static void Clear (Transform tf) {
			int len = tf.childCount;
			for (int i = 0; i < len; i++) {
				Transform.DestroyImmediate(tf.GetChild(0).gameObject, false);
			}
		}

	}


	public struct ComponentSetter {


		public static void RectTransform (RectTransform rt, Transform parent, Vector4 anchorMinMax, Vector4 posSizeMinMax, Vector2 pivot) {
			rt.SetParent(parent);
			rt.anchorMin = new Vector2(anchorMinMax.x, anchorMinMax.y);
			rt.anchorMax = new Vector2(anchorMinMax.z, anchorMinMax.w);
			rt.pivot = pivot;
			rt.anchoredPosition3D = new Vector3(posSizeMinMax.x, posSizeMinMax.y, 0f);
			rt.sizeDelta = new Vector2(posSizeMinMax.z, posSizeMinMax.w);
			rt.rotation = Quaternion.identity;
			rt.localScale = Vector3.one;
		}


		public static void Text (Text text, string txt, int size, Color color, TextAnchor anchor, bool richText, bool hOverFlow, bool vOverFlow, bool rayCast, FontStyle style = FontStyle.Normal) {
			text.text = txt;
			text.fontSize = size;
			text.color = color;
			text.alignment = anchor;
			text.raycastTarget = rayCast;
			text.supportRichText = richText;
			text.horizontalOverflow = hOverFlow ? HorizontalWrapMode.Overflow : HorizontalWrapMode.Wrap;
			text.verticalOverflow = vOverFlow ? VerticalWrapMode.Overflow : VerticalWrapMode.Truncate;
			text.fontStyle = style;
		}


		public static void Image (Image img, Sprite sprite, Color color, Image.Type type, bool raycastTarget, bool asp = false) {
			img.sprite = sprite;
			img.color = color;
			img.type = type;
			img.raycastTarget = raycastTarget;
			img.preserveAspect = asp;
			img.fillCenter = true;
		}


		public static void OutLine (Outline otl, Color color, float dis) {
			otl.effectColor = color;
			otl.effectDistance = Vector2.one * dis;
			otl.useGraphicAlpha = false;
		}


		public static void ScrollRect (ScrollRect sr, RectTransform viewPort, RectTransform content, bool elastic, Scrollbar vBar = null, Scrollbar hBar = null, bool horizontal = false, bool vertical = true) {
			sr.horizontal = horizontal;
			sr.vertical = vertical;
			sr.movementType = elastic ? UnityEngine.UI.ScrollRect.MovementType.Elastic : UnityEngine.UI.ScrollRect.MovementType.Clamped;
			sr.viewport = viewPort;
			sr.content = content;
			sr.horizontalScrollbar = hBar;
			sr.verticalScrollbar = vBar;
			sr.elasticity = 0.1f;
			sr.inertia = true;
			sr.decelerationRate = 0.135f;
			sr.scrollSensitivity = 14f;
			sr.verticalScrollbarVisibility = UnityEngine.UI.ScrollRect.ScrollbarVisibility.AutoHide;
			sr.horizontalScrollbarVisibility = UnityEngine.UI.ScrollRect.ScrollbarVisibility.AutoHide;
		}


		public static void VerticalLayoutGroup (VerticalLayoutGroup vlg, RectOffset padding, float space = 0f, bool feW = true, bool feH = true, TextAnchor an = TextAnchor.UpperLeft) {
			vlg.padding = padding;
			vlg.spacing = space;
			vlg.childForceExpandHeight = feH;
			vlg.childForceExpandWidth = feW;
#if !UNITY_4_0 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_3 && !UNITY_5_4
			vlg.childControlWidth = true;
			vlg.childControlHeight = true;
#endif
			vlg.childAlignment = an;
		}


		public static void ContentSizeFitter (ContentSizeFitter csf, UnityEngine.UI.ContentSizeFitter.FitMode h, UnityEngine.UI.ContentSizeFitter.FitMode v) {
			csf.horizontalFit = h;
			csf.verticalFit = v;
		}


		public static void Scrollbar (Scrollbar sb, RectTransform handle, bool isHori) {
			sb.handleRect = handle;
			sb.direction = isHori ? UnityEngine.UI.Scrollbar.Direction.LeftToRight : UnityEngine.UI.Scrollbar.Direction.BottomToTop;
		}


		public static void InputField (InputField inputF, Text text, string content) {
			inputF.textComponent = text;
			inputF.text = content;
		}


		public static void Dropdown (Dropdown dd, RectTransform temp, Text label, Text itemLabel) {
			dd.template = temp;
			dd.captionText = label;
			dd.itemText = itemLabel;
		}


		public static void Toggle (Toggle tg, Image chackMark, bool hasBG = true, ToggleGroup group = null) {
			tg.graphic = chackMark;
			tg.transition = hasBG ? Selectable.Transition.ColorTint : Selectable.Transition.None;
			tg.group = group;
		}


		public static void LayoutElement (LayoutElement le, float minW, float minH, bool ignore = false) {
			le.minWidth = minW;
			le.minHeight = minH;
			le.ignoreLayout = ignore;
		}


	}


}