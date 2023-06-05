namespace uFileBrowser {

using UnityEngine;
using UnityEditor;

public struct SpriteUtil {

	
	private const string STANDARD_SPRITE_PATH = "UI/Skin/UISprite.psd";
	private const string BACKGROUND_SPRITE_PATH = "UI/Skin/Background.psd";
	private const string INPUTFIELD_BACKGROUND_PATH = "UI/Skin/InputFieldBackground.psd";
	private const string KNOB_PATH = "UI/Skin/Knob.psd";
	private const string CHECKMARK_PATH = "UI/Skin/Checkmark.psd";
	private const string DROPDOWN_ARROW_PATH = "UI/Skin/DropdownArrow.psd";
	private const string DUI_MASK_PATH = "UI/Skin/UIMask.psd";

	private static string RootPath {
		get {
			if (rootPath == "") {
				string[] paths = AssetDatabase.GetAllAssetPaths();
				foreach (string p in paths) {
					if (System.IO.Path.GetFileName(p) == "UFileBrowser_Icons") {
						rootPath = p;
						break;
					}
				}
			}
			return rootPath;
		}
	}
	private static string rootPath = "";

	public static Sprite UISprite {
		get {
			if (!uiSprite) {
				uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(STANDARD_SPRITE_PATH);
			}
			return uiSprite;
		}
	}
	private static Sprite uiSprite = null;

	public static Sprite BackGroundSprite {
		get {
			if (!backGroundSprite) {
				backGroundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(BACKGROUND_SPRITE_PATH);
			}
			return backGroundSprite;
		}
	}
	private static Sprite backGroundSprite = null;

	public static Sprite InputFieldBackGroundSprite {
		get {
			if (!inputFieldBackGroundSprite) {
				inputFieldBackGroundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(INPUTFIELD_BACKGROUND_PATH);
			}
			return inputFieldBackGroundSprite;
		}
	}
	private static Sprite inputFieldBackGroundSprite = null;

	public static Sprite KnobSprite {
		get {
			if (!knobSprite) {
				knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(KNOB_PATH);
			}
			return knobSprite;
		}
	}
	private static Sprite knobSprite = null;

	public static Sprite CheckMarkSprite {
		get {
			if (!checkMarkSprite) {
				checkMarkSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(CHECKMARK_PATH);
			}
			return checkMarkSprite;
		}
	}
	private static Sprite checkMarkSprite = null;

	public static Sprite DropdownArrowSprite {
		get {
			if (!dropdownArrow) {
				dropdownArrow = AssetDatabase.GetBuiltinExtraResource<Sprite>(DROPDOWN_ARROW_PATH);
			}
			return dropdownArrow;
		}
	}
	private static Sprite dropdownArrow = null;

	public static Sprite UIMaskSprite {
		get {
			if (!uiMaskSprite) {
				uiMaskSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(DUI_MASK_PATH);
			}
			return uiMaskSprite;
		}
	}
	private static Sprite uiMaskSprite = null;

	public static Sprite UnityLogoSprite {
		get {
			if (!unityLogoSprite) {
				unityLogoSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PathUtil.CombinePaths(RootPath, "UnityLogo.png"));
			}
			return unityLogoSprite;
		}
	}
	private static Sprite unityLogoSprite = null;

	public static Sprite FileIconSprite {
		get {
			if (!fileIconSprite) {
				fileIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PathUtil.CombinePaths(RootPath, "FileIcon.png"));
			}
			return fileIconSprite;
		}
	}
	private static Sprite fileIconSprite = null;

	public static Sprite DirIconSprite {
		get {
			if (!dirIconSprite) {
				dirIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PathUtil.CombinePaths(RootPath, "DirIcon.png"));
			}
			return dirIconSprite;
		}
	}
	private static Sprite dirIconSprite = null;


	

}




public struct TextureUtil {

	public static Texture2D QuickTexture (int w, int h, int wB, int hB, Color c, Color cB) {
		Texture2D txt = new Texture2D(w, h);
		Color[] colors = new Color[w * h];
		for (int i = 0; i < w; i++) {
			for (int j = 0; j < h; j++) {
				colors[j * w + i] = i < wB || j < hB || i > w - wB - 1 || j > h - hB - 1 ? cB : c;
			}
		}
		txt.SetPixels(colors);
		txt.Apply();
		return txt;
	}

	public static Texture2D QuickTexture (int w, int h, Color c) {
		return QuickTexture(w, h, 0, 0, c, Color.clear);
	}

}




}