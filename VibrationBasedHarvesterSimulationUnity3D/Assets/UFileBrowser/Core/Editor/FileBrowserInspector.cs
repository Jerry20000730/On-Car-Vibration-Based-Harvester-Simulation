namespace uFileBrowser {

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;

[CustomEditor(typeof(FileBrowser))]
public class FileBrowserInspector : Editor {

	

	#region -------- VAR --------


	private enum SaveOrLoad {
		SavePanel = 0,
		LoadPanel = 1,
	}


	private static bool ComponentGUIOpen = false;

	private SerializedObject Title_SOBJ;
	private SerializedObject Address_SOBJ;
	private SerializedObject Logo_SOBJ;
	private SerializedObject FileName_SOBJ;
	private SerializedObject FileType_SOBJ;
	private SerializedObject SubmitBtnTXT_SOBJ;
	private SerializedObject CancelBtnTXT_SOBJ;
	private SerializedObject AlertWindowMSG_SOBJ;

	private SerializedProperty Title_m_Text;
	private SerializedProperty Address_m_Text;
	private SerializedProperty Logo_m_Sprite;
	private SerializedProperty FileName_m_Text;
	private SerializedProperty FileType_m_Options;
	private SerializedProperty SubmitBtnTXT_m_Text;
	private SerializedProperty CancelBtnTXT_m_Text;
	private SerializedProperty m_ShowLastAddressOnRestart;
	private SerializedProperty m_OnlySubmitExistPath;
	private SerializedProperty m_ShowAllSupportTypes;
	private SerializedProperty AlertWindowMSG_m_Text;


	private static Texture2D CheatTexture {
		get{
			if(!cheatTexture){
				cheatTexture = TextureUtil.QuickTexture(1, 1, new Color(0.371f, 0.371f, 0.371f, 1f));
			}
			return cheatTexture;
		}
	}
	private static Texture2D cheatTexture = null;

	#endregion


	#region -------- SYS --------

	
	[MenuItem("GameObject/UI/FileBrowser")]
	public static void CreateNew () {
		

		#region ----- The Object -----

		GameObject root = new GameObject("FileBrowser", typeof(CanvasRenderer), typeof(FileBrowser), typeof(Image));
		RectTransform rootRT = (RectTransform)root.transform;
		Image rootIMG = root.GetComponent<Image>();
		FileBrowser fb = root.GetComponent<FileBrowser>();



		#endregion


		#region ----- Reset Parent and Create Canvas and EventSystem if don't have one -----

		root.transform.SetParent(Selection.activeTransform);
		if (!TransformUtil.InCanvas(root.transform)) {
			Transform canvasTF = TransformUtil.GetCreateCanvas();
			root.transform.SetParent(canvasTF);
		}
		if (!Transform.FindObjectOfType<UnityEngine.EventSystems.EventSystem>()) {
			GameObject eg = new GameObject("EventSystem");
			eg.AddComponent<UnityEngine.EventSystems.EventSystem>();
			eg.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
		}

		#endregion


		#region ----- Create default stuff -----


		#region --- Root ---

		ComponentSetter.RectTransform(rootRT, rootRT.parent,
			new Vector4(0.5f, 0.5f, 0.5f, 0.5f),
			new Vector4(0, 0, 480f, 333.784f),
			new Vector2(0.5f, 0.5f)
		);
		ComponentSetter.Image(rootIMG, null, 
			new Color(0.7f, 0.7f, 0.7f, 1f),
			Image.Type.Sliced, true
		);

		#endregion

		#region --- Title ---

		GameObject title = new GameObject("Title", typeof(RectTransform), typeof(Image));
		RectTransform titleRT = (RectTransform)title.transform;
		Image titleIMG = title.GetComponent<Image>();
		ComponentSetter.RectTransform(titleRT, rootRT,
			new Vector4(0f, 1f, 1f, 1f),
			new Vector4(0, 0, 0f, 48f),
			new Vector2(0.5f, 1f)
		);
		ComponentSetter.Image(titleIMG, null,
			new Color(1f, 1f, 1f, 0.75f),
			Image.Type.Sliced, true
		);

		#endregion

		#region --- Address ---

		GameObject address = new GameObject("Address", typeof(RectTransform), typeof(Image), typeof(InputField), typeof(Outline));
		RectTransform addressRT = (RectTransform)address.transform;
		Image addressIMG = address.GetComponent<Image>();
		Outline addressOUT = address.GetComponent<Outline>();
		fb.m_AddressInput = address.GetComponent<InputField>();
		ComponentSetter.RectTransform(addressRT, titleRT,
			new Vector4(0f, 0f, 1f, 0f),
			new Vector4(67f, 6f, -73f, 16f),
			new Vector2(0f, 0f)
		);
		ComponentSetter.Image(addressIMG, null,
			Color.white,
			Image.Type.Sliced, true
		);
		ComponentSetter.OutLine(addressOUT, new Color(0, 0, 0, 0.2f), 1f);
		fb.m_AddressInput.targetGraphic = addressIMG;

		#endregion

		#region --- Address Txt ---

		GameObject addressTxt = new GameObject("Text", typeof(RectTransform), typeof(Text));
		RectTransform addressTxtRT = (RectTransform)addressTxt.transform;
		Text addressTxtTXT = addressTxt.GetComponent<Text>();
		ComponentSetter.RectTransform(addressTxtRT, addressRT,
			new Vector4(0f, 0f, 1f, 1f),
			new Vector4(0f, 0f, -6f, -6f),
			new Vector2(0.5f, 0.5f)
		);
		ComponentSetter.InputField(fb.m_AddressInput, addressTxtTXT, "/");
		ComponentSetter.Text(addressTxtTXT, "/", 12,
			new Color(0.2f, 0.2f, 0.2f, 1f),
			TextAnchor.MiddleLeft,
			false, true, true, true
		);

		#endregion

		#region --- Title Text ---

		GameObject titleTxt = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
		RectTransform titleTxtRT = (RectTransform)titleTxt.transform;
		fb.m_Title = titleTxt.GetComponent<Text>();
		Outline titleTxtOTL = titleTxt.GetComponent<Outline>();
		ComponentSetter.RectTransform(titleTxtRT, titleRT,
			new Vector4(0f, 1f, 1f, 1f),
			new Vector4(24f, -3f, -78f, 20f),
			new Vector2(0f, 1f)
		);
		ComponentSetter.Text(fb.m_Title, "Title", 13, 
			new Color(0.2f, 0.2f, 0.2f, 1f),
			TextAnchor.MiddleLeft,
			false, true, true, false
		);
		ComponentSetter.OutLine(titleTxtOTL, Color.white, 1.2f);

		#endregion

		#region --- Title Logo ---

		GameObject titleIcon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
		RectTransform titleIconRT = (RectTransform)titleIcon.transform;
		fb.m_Logo = titleIcon.GetComponent<Image>();
		ComponentSetter.RectTransform(titleIconRT, titleRT,
			new Vector4(0f, 1f, 0f, 1f),
			new Vector4(6f, -6f, 14f, 14f),
			new Vector2(0f, 1f)
		);
		ComponentSetter.Image(fb.m_Logo, SpriteUtil.UnityLogoSprite,
			Color.white,
			Image.Type.Sliced, false
		);

		#endregion

		#region --- Back Btn ---

		GameObject backBtn = new GameObject("BackBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
		RectTransform backBtnRT = (RectTransform)backBtn.transform;
		Image backBtnIMG = backBtn.GetComponent<Image>();
		Outline backBtnOUT = backBtn.GetComponent<Outline>();
		fb.m_BackBtn = backBtn.GetComponent<Button>();
		ComponentSetter.RectTransform(backBtnRT, titleRT,
			new Vector4(0f, 0f, 0f, 0f),
			new Vector4(6f, 6f, 30f, 16f),
			new Vector2(0f, 0f)
		);
		ComponentSetter.Image(backBtnIMG, null,
			Color.white, Image.Type.Sliced, true
		);
		ComponentSetter.OutLine(backBtnOUT, new Color(0, 0, 0, 0.2f), 1f);

		#endregion

		#region --- Forward Btn ---

		GameObject forwardBtn = new GameObject("ForwardBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
		RectTransform forwardBtnRT = (RectTransform)forwardBtn.transform;
		Image forwardBtnIMG = forwardBtn.GetComponent<Image>();
		Outline forwardBtnOUT = forwardBtn.GetComponent<Outline>();
		fb.m_ForwardBtn = forwardBtn.GetComponent<Button>();
		ComponentSetter.RectTransform(forwardBtnRT, titleRT,
			new Vector4(0f, 0f, 0f, 0f),
			new Vector4(36f, 6f, 30f, 16f),
			new Vector2(0f, 0f)
		);
		ComponentSetter.Image(forwardBtnIMG, null,
			Color.white, Image.Type.Sliced, true
		);
		ComponentSetter.OutLine(forwardBtnOUT, new Color(0, 0, 0, 0.2f), 1f);

		#endregion

		#region --- QuitBtn ---

		GameObject quitBtn = new GameObject("QuitBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
		RectTransform quitBtnRT = (RectTransform)quitBtn.transform;
		Image quitBtnIMG = quitBtn.GetComponent<Image>();
		fb.m_QuitBtn = quitBtn.GetComponent<Button>();
		Outline quitBtnOUT = quitBtn.GetComponent<Outline>();
		ComponentSetter.RectTransform(quitBtnRT, titleRT,
			new Vector4(1f, 1f, 1f, 1f),
			new Vector4(0, 0, 48, 19),
			new Vector2(1f, 1f)
		);
		ComponentSetter.Image(quitBtnIMG, null,
			new Color(0.7f, 0.2f, 0.2f, 1f), Image.Type.Sliced, true
		);
		ComponentSetter.OutLine(quitBtnOUT, new Color(0, 0, 0, 0.2f), 1f);

		#endregion

		#region --- BackBtn Txt ---

		GameObject backBtnTxt = new GameObject("Text", typeof(RectTransform), typeof(Text));
		RectTransform backBtnTxtRT = (RectTransform)backBtnTxt.transform;
		Text backBtnTxtTXT = backBtnTxt.GetComponent<Text>();
		ComponentSetter.RectTransform(backBtnTxtRT, backBtnRT,
			new Vector4(0f, 0f, 1f, 1f),
			new Vector4(0f, 0f, 0f, 0f),
			new Vector2(0f, 0f)
		);
		ComponentSetter.Text(backBtnTxtTXT, "◀", 11,//◀◁
			new Color(0.2f, 0.2f, 0.2f, 1f),
			TextAnchor.MiddleCenter,
			false, true, true, true, 
			FontStyle.Normal
		);

		#endregion

		#region --- ForwardBtn Txt ---

		GameObject forwardBtnTxt = new GameObject("Text", typeof(RectTransform), typeof(Text));
		RectTransform forwardBtnTxtRT = (RectTransform)forwardBtnTxt.transform;
		Text forwardBtnTxtTXT = forwardBtnTxt.GetComponent<Text>();
		ComponentSetter.RectTransform(forwardBtnTxtRT, forwardBtnRT,
			new Vector4(0f, 0f, 1f, 1f),
			new Vector4(0f, 0f, 0f, 0f),
			new Vector2(0f, 0f)
		);
		ComponentSetter.Text(forwardBtnTxtTXT, "▶", 11,//▶▷
			new Color(0.2f, 0.2f, 0.2f, 1f),
			TextAnchor.MiddleCenter,
			false, true, true, true,
			FontStyle.Normal
		);

		#endregion

		#region --- QuitBtn Txt ---

		GameObject quitBtnTxt = new GameObject("Text", typeof(RectTransform), typeof(Text));
		RectTransform quitBtnTxtRT = (RectTransform)quitBtnTxt.transform;
		Text quitBtnTxtTXT = quitBtnTxt.GetComponent<Text>();
		ComponentSetter.RectTransform(quitBtnTxtRT, quitBtnRT,
			new Vector4(0f, 0f, 1f, 1f),
			new Vector4(0f, 0f, 0f, 0f),
			new Vector2(0f, 0f)
		);
		ComponentSetter.Text(quitBtnTxtTXT, "×", 20,
			new Color(1f, 1f, 1f, 1f),
			TextAnchor.MiddleCenter,
			false, true, true, true,
			FontStyle.Bold
		);
		


		#endregion

		#region --- Navigation Panel ---

		GameObject navPanel = new GameObject("NavigationPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(RectMask2D));
		fb.m_NavPanel = (RectTransform)navPanel.transform;
		Image navPanelIMG = navPanel.GetComponent<Image>();
		VerticalLayoutGroup navPanelVLG = navPanel.GetComponent<VerticalLayoutGroup>();
		ComponentSetter.RectTransform(fb.m_NavPanel, rootRT,
			new Vector4(0f, 0f, 0f, 1f),
			new Vector4(0f, 48f, 90f, -96f),
			new Vector2(0f, 0f)
		);
		ComponentSetter.Image(navPanelIMG, null,
			new Color(0.8f, 0.8f, 0.8f, 1f), Image.Type.Sliced, false
		);
		ComponentSetter.VerticalLayoutGroup(navPanelVLG, 
			new RectOffset(4, 4, 8, 8), 0, true, false
		);

		#endregion

		#region --- Main ScrollRect ---

		GameObject mainScrollRect = new GameObject("MainScrollRect", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
		RectTransform mainScrollRectRT = (RectTransform)mainScrollRect.transform;
		Image mainScrollRectIMG = mainScrollRect.GetComponent<Image>();
		fb.m_MainContentSR = mainScrollRect.GetComponent<ScrollRect>();
		ComponentSetter.RectTransform(mainScrollRectRT, rootRT,
			new Vector4(0f, 0f, 1f, 1f),
			new Vector4(90f, 48f, -90f, -96f),
			new Vector2(0f, 0f)
		);
		ComponentSetter.Image(mainScrollRectIMG, null,
			new Color(1f, 1f, 1f, 0.2f), Image.Type.Sliced, false
		);

		#endregion

		#region --- Main ViewPort ---

		GameObject mainViewPort = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
		RectTransform mainViewPortRT = (RectTransform)mainViewPort.transform;
		ComponentSetter.RectTransform(mainViewPortRT, mainScrollRectRT,
			new Vector4(0f, 0f, 1f, 1f),
			new Vector4(2f, 2f, -18f, -4f),
			new Vector2(0f, 0f)
		);

		#endregion

		#region --- Main Content ---

		GameObject mainContent = new GameObject("Content", typeof(RectTransform), typeof(Image), typeof(ToggleGroup));
		RectTransform mainContentRT = (RectTransform)mainContent.transform;
		Image mainContentIMG = mainContent.GetComponent<Image>();
		ToggleGroup mainContentTGG = mainContent.GetComponent<ToggleGroup>();
		ComponentSetter.RectTransform(mainContentRT, mainViewPortRT,
			new Vector4(0f, 1f, 1f, 1f),
			new Vector4(0f, 0f, 0f, 0f),
			new Vector2(0f, 1f)
		);
		ComponentSetter.Image(mainContentIMG, null, Color.clear, Image.Type.Simple, true);
		mainContentTGG.allowSwitchOff = false;

		#endregion

		#region --- Main Content Bar ---

		GameObject mainBar = new GameObject("ScrollBar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
		RectTransform mainBarRT = (RectTransform)mainBar.transform;
		Image mainBarIMG = mainBar.GetComponent<Image>();
		Scrollbar mainBarSB = mainBar.GetComponent<Scrollbar>();
		ComponentSetter.RectTransform(mainBarRT, mainScrollRectRT,
			new Vector4(1f, 0f, 1f, 1f),
			new Vector4(0f, 0f, 16f, 0f),
			new Vector2(1f, 0f)
		);
		ComponentSetter.Image(mainBarIMG, null,
			new Color(1f, 1f, 1f, 0.2f), Image.Type.Sliced, true
		);
		ComponentSetter.ScrollRect(fb.m_MainContentSR, mainViewPortRT, mainContentRT, false, mainBarSB);

		#endregion

		#region --- MainContentBar SlideArea ---

		GameObject mainBarSlideArea = new GameObject("SlideArea", typeof(RectTransform));
		RectTransform mainBarSlideAreaRT = (RectTransform)mainBarSlideArea.transform;
		ComponentSetter.RectTransform(mainBarSlideAreaRT, mainBarRT,
			new Vector4(0f, 0f, 1f, 1f),
			new Vector4(0f, 0f, -16f, 0f),
			new Vector2(0.5f, 0f)
		);

		#endregion

		#region --- MainContentBar Handle ---

		GameObject mainBarHandle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
		RectTransform mainBarHandleRT = (RectTransform)mainBarHandle.transform;
		Image mainBarHandleIMG = mainBarHandle.GetComponent<Image>();
		ComponentSetter.RectTransform(mainBarHandleRT, mainBarSlideAreaRT,
			new Vector4(0f, 0f, 1f, 1f),
			new Vector4(0f, 0f, 16f, 0f),
			new Vector2(0.5f, 0f)
		);
		ComponentSetter.Image(mainBarHandleIMG, null,
			new Color(1f, 1f, 1f, 0.9f), Image.Type.Sliced, true
		);
		ComponentSetter.Scrollbar(mainBarSB, mainBarHandleRT, false);

		#endregion

		#region --- Name InputField ---

		GameObject nameInput = new GameObject("Name", typeof(RectTransform), typeof(Image), typeof(InputField), typeof(Outline));
		RectTransform nameInputRT = (RectTransform)nameInput.transform;
		Image nameInputIMG = nameInput.GetComponent<Image>();
		fb.m_NameInput = nameInput.GetComponent<InputField>();
		Outline nameInputOUT = nameInput.GetComponent<Outline>();
		ComponentSetter.RectTransform(nameInputRT, rootRT,
			new Vector4(0, 0, 1, 0),
			new Vector4(100, 28, -206, 16),
			new Vector2(0, 0)
		);
		ComponentSetter.Image(nameInputIMG, null,
			new Color(1f, 1f, 1f, 1f), Image.Type.Sliced, true
		);
		ComponentSetter.OutLine(nameInputOUT, new Color(0, 0, 0, 0.2f), 1f);

		#endregion

		#region --- Name InputField Txt ---

		GameObject nameInputTxt = new GameObject("Text", typeof(RectTransform), typeof(Text));
		RectTransform nameInputTxtRT = (RectTransform)nameInputTxt.transform;
		Text nameInputTxtTXT = nameInputTxt.GetComponent<Text>();
		ComponentSetter.RectTransform(nameInputTxtRT, nameInputRT,
			new Vector4(0f, 0f, 1f, 1f),
			new Vector4(0f, 0f, -6f, -6f),
			new Vector2(0.5f, 0.5f)
		);
		ComponentSetter.InputField(fb.m_NameInput, nameInputTxtTXT, "");
		ComponentSetter.Text(nameInputTxtTXT, "", 11,
			new Color(0.2f, 0.2f, 0.2f, 1f),
			TextAnchor.MiddleLeft,
			false, true, true, true
		);

		#endregion

		#region --- File Name Label ---

		GameObject fileNameLabel = new GameObject("Label", typeof(RectTransform), typeof(Text));
		RectTransform fileNameLabelRT = (RectTransform)fileNameLabel.transform;
		Text fileNameLabelTXT = fileNameLabel.GetComponent<Text>();
		ComponentSetter.RectTransform(fileNameLabelRT, nameInputRT,
			new Vector4(0, 0, 0, 1),
			new Vector4(0, 0, 0, 0),
			new Vector2(1 ,0)
		);
		ComponentSetter.Text(fileNameLabelTXT, "File name : ", 11, 
			new Color(0, 0, 0, 1), 
			TextAnchor.MiddleRight, 
			false, true, true, false
		);

		#endregion

		#region --- Submit Btn ---

		GameObject submitBtn = new GameObject("SubmitBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
		RectTransform submitBtnRT = (RectTransform)submitBtn.transform;
		Image submitBtnIMG = submitBtn.GetComponent<Image>();
		fb.m_SubmitBtn = submitBtn.GetComponent<Button>();
		Outline submitBtnOUT = submitBtn.GetComponent<Outline>();
		ComponentSetter.RectTransform(submitBtnRT, rootRT,
			new Vector4(1f, 0f, 1f, 0f),
			new Vector4(-74f, 6f, 64f, 16f),
			new Vector2(1f, 0f)
		);
		ComponentSetter.Image(submitBtnIMG, null,
			Color.white, Image.Type.Sliced, true
		);
		ComponentSetter.OutLine(submitBtnOUT, new Color(0, 0, 0, 0.2f), 1f);

		#endregion

		#region --- Cancel Btn ---

		GameObject cancelBtn = new GameObject("CancelBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
		RectTransform cancelBtnRT = (RectTransform)cancelBtn.transform;
		Image cancelBtnIMG = cancelBtn.GetComponent<Image>();
		fb.m_CancelBtn = cancelBtn.GetComponent<Button>();
		Outline cancelBtnOUT = cancelBtn.GetComponent<Outline>();
		ComponentSetter.RectTransform(cancelBtnRT, rootRT,
			new Vector4(1f, 0f, 1f, 0f),
			new Vector4(-4f, 6f, 64f, 16f),
			new Vector2(1f, 0f)
		);
		ComponentSetter.Image(cancelBtnIMG, null,
			Color.white, Image.Type.Sliced, true
		);
		ComponentSetter.OutLine(cancelBtnOUT, new Color(0, 0, 0, 0.2f), 1f);

		#endregion

		#region --- SubmitBtn Txt ---

		GameObject submitBtnTxt = new GameObject("Text", typeof(RectTransform), typeof(Text));
		RectTransform submitBtnTxtRT = (RectTransform)submitBtnTxt.transform;
		fb.m_SubmitBtnText = submitBtnTxt.GetComponent<Text>();
		ComponentSetter.RectTransform(submitBtnTxtRT, submitBtnRT,
			new Vector4(0f, 0f, 1f, 1f),
			new Vector4(0f, 0f, 0f, 0f),
			new Vector2(0f, 0f)
		);
		ComponentSetter.Text(fb.m_SubmitBtnText, "O K", 12,
			new Color(0.2f, 0.2f, 0.2f, 1f),
			TextAnchor.MiddleCenter,
			false, true, true, true,
			FontStyle.Normal
		);

		#endregion

		#region --- CancelBtn Txt ---

		GameObject cancelBtnTxt = new GameObject("Text", typeof(RectTransform), typeof(Text));
		RectTransform cancelBtnTxtRT = (RectTransform)cancelBtnTxt.transform;
		fb.m_CancelBtnText = cancelBtnTxt.GetComponent<Text>();
		ComponentSetter.RectTransform(cancelBtnTxtRT, cancelBtnRT,
			new Vector4(0f, 0f, 1f, 1f),
			new Vector4(0f, 0f, 0f, 0f),
			new Vector2(0f, 0f)
		);
		ComponentSetter.Text(fb.m_CancelBtnText, "Cancel", 12,
			new Color(0.2f, 0.2f, 0.2f, 1f),
			TextAnchor.MiddleCenter,
			false, true, true, true,
			FontStyle.Normal
		);

		#endregion

		#region --- Type Drop Down ---

		GameObject typeDD = new GameObject("FileType", typeof(RectTransform), typeof(Image), typeof(Dropdown), typeof(Outline));
		RectTransform typeDDRT = (RectTransform)typeDD.transform;
		Image typeDDIMG = typeDD.GetComponent<Image>();
		fb.m_FileTypeDropDpwn = typeDD.GetComponent<Dropdown>();
		Outline typeDDOUT = typeDD.GetComponent<Outline>();
		ComponentSetter.RectTransform(typeDDRT, rootRT,
			new Vector4(1, 0, 1, 0),
			new Vector4(-4, 28, 100, 16),
			new Vector2(1, 0)
		);
		ComponentSetter.Image(typeDDIMG, null,
			Color.white, Image.Type.Sliced, true
		);
		ComponentSetter.OutLine(typeDDOUT, new Color(0, 0, 0, 0.2f), 1f);
		
		#endregion

		#region --- TypeDropDown Arrow ---

		GameObject typeDropDownArrow = new GameObject("Arrow", typeof(RectTransform), typeof(Image));
		RectTransform typeDropDownArrowRT = (RectTransform)typeDropDownArrow.transform;
		Image typeDropDownArrowIMG = typeDropDownArrow.GetComponent<Image>();
		ComponentSetter.RectTransform(typeDropDownArrowRT, typeDDRT,
			new Vector4(0, 0, 1, 1),
			new Vector4(-2, 2, -4, -4),
			new Vector2(1, 0)
		);
		ComponentSetter.Image(typeDropDownArrowIMG, SpriteUtil.DropdownArrowSprite,
			new Color(1, 1, 1, 1), Image.Type.Simple, false, true
		);

		#endregion

		#region --- TypeDropDown Label ---

		GameObject typeDropDownLabel = new GameObject("Label", typeof(RectTransform), typeof(Text));
		RectTransform typeDropDownLabelRT = (RectTransform)typeDropDownLabel.transform;
		Text typeDropDownLabelTXT = typeDropDownLabel.GetComponent<Text>();
		ComponentSetter.RectTransform(typeDropDownLabelRT, typeDDRT,
			new Vector4(0f, 0f, 1f, 1f),
			new Vector4(0f, 0f, -16f, 0f),
			new Vector2(0f, 0f)
		);
		ComponentSetter.Text(typeDropDownLabelTXT, "", 12,
			new Color(0.2f, 0.2f, 0.2f, 1f),
			TextAnchor.MiddleCenter,
			false, false, false, true,
			FontStyle.Normal
		);

		#endregion

		#region --- TypeDropDown Temp ---

		GameObject typeDDTemp = new GameObject("Temp", typeof(RectTransform), typeof(Image), typeof(Outline));
		RectTransform typeDDTempRT = (RectTransform)typeDDTemp.transform;
		Image typeDDTempIMG = typeDDTemp.GetComponent<Image>();
		Outline typeDDTempOUT = typeDDTemp.GetComponent<Outline>();
		ComponentSetter.RectTransform(typeDDTempRT, typeDDRT,
			new Vector4(0f, 1f, 1f, 1f),
			new Vector4(0f, 2f, 0f, 18f),
			new Vector2(0f, 0f)
		);
		ComponentSetter.Image(typeDDTempIMG, null,
			new Color(1, 1, 1, 0.9f), Image.Type.Sliced, true
		);
		ComponentSetter.OutLine(typeDDTempOUT, new Color(0, 0, 0, 0.2f), 1f);

		#endregion

		#region --- TypeDropDown Item ---

		GameObject typeDDItem = new GameObject("Item", typeof(RectTransform), typeof(Image), typeof(Toggle));
		RectTransform typeDDItemRT = (RectTransform)typeDDItem.transform;
		Image typeDDItemIMG = typeDDItem.GetComponent<Image>();
		Toggle typeDDItemTG = typeDDItem.GetComponent<Toggle>();
		ComponentSetter.RectTransform(typeDDItemRT, typeDDTempRT,
			new Vector4(0f, 0.5f, 1f, 0.5f),
			new Vector4(0f, 0f, 0f, 18f),
			new Vector2(0f, 0.5f)
		);
		ComponentSetter.Image(typeDDItemIMG, null,
			new Color(1, 1, 1, 0.9f), Image.Type.Simple, true
		);
		
		#endregion

		#region --- TypeDDItem Text ---

		GameObject typeDDItemText = new GameObject("Text", typeof(RectTransform), typeof(Text));
		RectTransform typeDDItemTextRT = (RectTransform)typeDDItemText.transform;
		Text typeDDItemTextTXT = typeDDItemText.GetComponent<Text>();
		ComponentSetter.RectTransform(typeDDItemTextRT, typeDDItemRT,
			new Vector4(0, 0, 1, 1),
			new Vector4(0, 0, -16, 0),
			new Vector2(0f, 0.5f)
		);
		ComponentSetter.Text(typeDDItemTextTXT, "", 14,
			new Color(0.2f, 0.2f, 0.2f, 1f),
			TextAnchor.MiddleCenter,
			false, true, true, true,
			FontStyle.Normal
		);
		ComponentSetter.Dropdown(fb.m_FileTypeDropDpwn, typeDDTempRT, typeDropDownLabelTXT, typeDDItemTextTXT);

		#endregion

		#region --- TypeDDItem HighLight ---

		GameObject TypeDDItemHL = new GameObject("HighLight", typeof(RectTransform), typeof(Image));
		RectTransform TypeDDItemHLRT = (RectTransform)TypeDDItemHL.transform;
		Image TypeDDItemHLIMG = TypeDDItemHL.GetComponent<Image>();
		ComponentSetter.RectTransform(TypeDDItemHLRT, typeDDItemRT,
			new Vector4(0, 0, 1, 1),
			new Vector4(0, 0, 0, 0),
			new Vector2(0.5f, 0.5f)
		);
		ComponentSetter.Image(TypeDDItemHLIMG, null,
			new Color(0.2f, 0.6f, 1f, 0.4f), Image.Type.Simple, true
		);
		ComponentSetter.Toggle(typeDDItemTG, TypeDDItemHLIMG);

		#endregion

		#region --- File Base Temp ---

		GameObject fileBaseTemp = new GameObject("FileBaseTemp", typeof(RectTransform), typeof(Toggle), typeof(RectMask2D));
		fb.m_FileBaseTemp = (RectTransform)fileBaseTemp.transform;
		Toggle fileBaseTempTG = fileBaseTemp.GetComponent<Toggle>();
		ComponentSetter.RectTransform(fb.m_FileBaseTemp, rootRT,
			new Vector4(0, 1, 1, 1),
			new Vector4(0, 0, 0, FileBrowser.FILE_BASE_HEIGHT),
			new Vector2(0, 1)
		);
		

		#endregion

		#region --- File Base Icon ---

		GameObject fileIcon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
		RectTransform fileIconRT = (RectTransform)fileIcon.transform;
		Image fileIconIMG = fileIcon.GetComponent<Image>();
		ComponentSetter.RectTransform(fileIconRT, fb.m_FileBaseTemp,
			new Vector4(0, 0, 0, 1),
			new Vector4(1, 0, FileBrowser.FILE_BASE_HEIGHT - 2, -2),
			new Vector2(0, 0.5f)
		);
		ComponentSetter.Image(fileIconIMG, null, 
			Color.white, Image.Type.Simple, 
			true, true
		);

		#endregion

		#region --- File Base Text ---

		GameObject fileBaseTxt = new GameObject("Text", typeof(RectTransform), typeof(Text));
		RectTransform fileBaseTxtRT = (RectTransform)fileBaseTxt.transform;
		Text fileBaseTxtTXT = fileBaseTxt.GetComponent<Text>();
		ComponentSetter.RectTransform(fileBaseTxtRT, fb.m_FileBaseTemp,
			new Vector4(0, 0, 1, 1),
			new Vector4(0, 0, -FileBrowser.FILE_BASE_HEIGHT, 0),
			new Vector2(1, 0)
		);
		ComponentSetter.Text(fileBaseTxtTXT, "", 10, 
			new Color(0.1f, 0.1f, 0.1f, 1f),
			TextAnchor.MiddleLeft, 
			false, true, true, true
		);

		#endregion

		#region --- File Base HighLight ---

		GameObject fileBaseHL = new GameObject("HighLight", typeof(RectTransform), typeof(Image));
		RectTransform fileBaseHLRT = (RectTransform)fileBaseHL.transform;
		Image fileBaseHLIMG = fileBaseHL.GetComponent<Image>();
		ComponentSetter.RectTransform(fileBaseHLRT, fb.m_FileBaseTemp,
			new Vector4(0,0,1,1),
			new Vector4(0,0,0,0),
			new Vector2(0,0)
		);
		ComponentSetter.Image(fileBaseHLIMG, null,
			new Color(0.8f, 0.3f, 1f, 0.4f), 
			Image.Type.Sliced, true
		);
		ComponentSetter.Toggle(fileBaseTempTG, fileBaseHLIMG, false, mainContentTGG);

		#endregion

		#region --- Bookmark Temp ---

		GameObject bookMarkBase = new GameObject("BookMarkTemp", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
		fb.m_BookMarkTemp = (RectTransform)bookMarkBase.transform;
		LayoutElement bookMarkBaseLE = bookMarkBase.GetComponent<LayoutElement>();
		Image bookMarkBaseIMG = bookMarkBase.GetComponent<Image>();
		ComponentSetter.RectTransform(fb.m_BookMarkTemp, rootRT,
			new Vector4(0, 0, 0, 0),
			new Vector4(0, 0, 0, 0),
			new Vector2(0, 0)
		);
		ComponentSetter.Image(bookMarkBaseIMG, null,
			new Color(0.8f, 0.8f, 0.8f, 1f), Image.Type.Simple, true
		);
		ComponentSetter.LayoutElement(bookMarkBaseLE, 0, 18);

		#endregion

		#region --- Bookmark Text ---

		GameObject bookmarkTxt = new GameObject("Text", typeof(RectTransform), typeof(Text));
		RectTransform bookmarkTxtRT = (RectTransform)bookmarkTxt.transform;
		Text bookmarkTxtTXT = bookmarkTxt.GetComponent<Text>();
		ComponentSetter.RectTransform(bookmarkTxtRT, fb.m_BookMarkTemp,
			new Vector4(0, 0, 1, 1),
			new Vector4(0f, 0f, -18f, 0f),
			new Vector2(1f, 0f)
		);
		ComponentSetter.Text(bookmarkTxtTXT, "", 10,
			new Color(0.2f, 0.2f, 0.2f, 1f),
			TextAnchor.MiddleLeft,
			true, true, true, true
		);

		#endregion

		#region --- Bookmark Star ---

		GameObject bookmarkStar = new GameObject("Star", typeof(RectTransform), typeof(Text), typeof(Outline));
		RectTransform bookmarkStarRT = (RectTransform)bookmarkStar.transform;
		Text bookmarkStarTXT = bookmarkStar.GetComponent<Text>();
		Outline bookmarkStarOUT = bookmarkStar.GetComponent<Outline>();
		ComponentSetter.RectTransform(bookmarkStarRT, fb.m_BookMarkTemp,
			new Vector4(0f, 0f, 0f, 1f),
			new Vector4(0f, 0f, 18, 0f),
			new Vector2(0f, 0f)
		);
		ComponentSetter.Text(bookmarkStarTXT, "●", 11,//★☆○●◆
			new Color(0.9f, 0.7f, 0.2f, 1f),
			TextAnchor.MiddleCenter,
			true, true, true, true
		);
		ComponentSetter.OutLine(bookmarkStarOUT, new Color(0.75f, 0.55f, 0.15f, 0.5f), 0.8f);

		#endregion

		#region --- Alert Pannel ---

		GameObject alertPannel = new GameObject("AlertPannel", typeof(RectTransform), typeof(Image));
		fb.m_AlertPannel = (RectTransform)alertPannel.transform;
		Image alertPannelIMG = alertPannel.GetComponent<Image>();
		ComponentSetter.RectTransform(fb.m_AlertPannel, rootRT,
			new Vector4(0, 0, 1, 1),
			new Vector4(0, 0, 0, 0),
			new Vector2(0, 0)
		);
		ComponentSetter.Image(alertPannelIMG, null,
			new Color(0, 0, 0, 0.4f), Image.Type.Simple, true
		);

		#endregion

		#region --- Alert Window ---

		GameObject alertWindow = new GameObject("Window", typeof(RectTransform), typeof(Image), typeof(Outline));
		RectTransform alertWindowRT = (RectTransform)alertWindow.transform;
		Image alertWindowIMG = alertWindow.GetComponent<Image>();
		Outline alertWindowOUT = alertWindow.GetComponent<Outline>();
		ComponentSetter.RectTransform(alertWindowRT, fb.m_AlertPannel,
			new Vector4(0.5f, 0.5f, 0.5f, 0.5f),
			new Vector4(0, 0, 240, 90),
			new Vector2(0.5f, 0.5f)
		);
		ComponentSetter.Image(alertWindowIMG, null,
			new Color(0.95f, 0.95f, 0.95f, 1), 
			Image.Type.Simple, true
		);
		ComponentSetter.OutLine(alertWindowOUT, new Color(0, 0, 0, 0.2f), 1f);

		#endregion

		#region --- Alert Title ---

		GameObject alertWindowTitle = new GameObject("Title", typeof(RectTransform), typeof(Image));
		RectTransform alertWindowTitleRT = (RectTransform)alertWindowTitle.transform;
		Image alertWindowTitleIMG = alertWindowTitle.GetComponent<Image>();
		ComponentSetter.RectTransform(alertWindowTitleRT, alertWindowRT,
			new Vector4(0, 1, 1, 1),
			new Vector4(0, -14, 0, 14),
			new Vector2(0, 0)
		);
		ComponentSetter.Image(alertWindowTitleIMG, null,
			new Color(0.8f, 0.8f, 0.8f, 1),
			Image.Type.Simple, true
		);

		#endregion

		#region --- Alert MSG ---

		GameObject alertMSG = new GameObject("Msg", typeof(RectTransform), typeof(Text));
		RectTransform alertMSGRT = (RectTransform)alertMSG.transform;
		fb.m_AlertWindowMSG = alertMSG.GetComponent<Text>();
		ComponentSetter.RectTransform(alertMSGRT, alertWindowRT,
			new Vector4(0, 0, 1, 1),
			new Vector4(12, 32, -24, -48),
			new Vector2(0, 0)
		);
		ComponentSetter.Text(fb.m_AlertWindowMSG, "{0} is already exists\ndo you want to override it ?", 12,
			new Color(0.2f, 0.2f, 0.2f, 1f),
			TextAnchor.MiddleCenter,
			true, false, false, true
		);

		#endregion

		#region --- Alert OK Btn ---

		GameObject alertBtn = new GameObject("OKBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
		RectTransform alertBtnRT = (RectTransform)alertBtn.transform;
		Image alertBtnIMG = alertBtn.GetComponent<Image>();
		Outline alertBtnOUT = alertBtn.GetComponent<Outline>();
		fb.m_AlertOKBtn = alertBtn.GetComponent<Button>();
		ComponentSetter.RectTransform(alertBtnRT, alertWindowRT,
			new Vector4(0, 0, 0, 0),
			new Vector4(20, 10, 80, 18),
			new Vector2(0, 0)
		);
		ComponentSetter.Image(alertBtnIMG, null,
			Color.white, Image.Type.Sliced, true
		);
		ComponentSetter.OutLine(alertBtnOUT, new Color(0, 0, 0, 0.2f), 1f);

		#endregion

		#region --- Alert Cancel Btn ---

		GameObject alertCancelBtn = new GameObject("CancelBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
		RectTransform alertCancelBtnRT = (RectTransform)alertCancelBtn.transform;
		Image alertCancelBtnIMG = alertCancelBtn.GetComponent<Image>();
		Outline alertCancelBtnOUT = alertCancelBtn.GetComponent<Outline>();
		fb.m_AlertCancelBtn = alertCancelBtn.GetComponent<Button>();
		ComponentSetter.RectTransform(alertCancelBtnRT, alertWindowRT,
			new Vector4(1, 0, 1, 0),
			new Vector4(-20, 10, 80, 18),
			new Vector2(1, 0)
		);
		ComponentSetter.Image(alertCancelBtnIMG, null,
			Color.white, Image.Type.Sliced, true
		);
		ComponentSetter.OutLine(alertCancelBtnOUT, new Color(0, 0, 0, 0.2f), 1f);

		#endregion

		#region --- AlertOKBtn Text ---

		GameObject alertOKBtnTxt = new GameObject("Text", typeof(RectTransform), typeof(Text));
		RectTransform alertOKBtnTxtRT = (RectTransform)alertOKBtnTxt.transform;
		Text alertOKBtnTxtTXT = alertOKBtnTxt.GetComponent<Text>();
		ComponentSetter.RectTransform(alertOKBtnTxtRT, alertBtnRT,
			new Vector4(0f, 0f, 1f, 1f),
			new Vector4(0f, 0f, 0f, 0f),
			new Vector2(0f, 0f)
		);
		ComponentSetter.Text(alertOKBtnTxtTXT, "Yes", 11,
			new Color(0.2f, 0.2f, 0.2f, 1f),
			TextAnchor.MiddleCenter,
			false, true, true, true,
			FontStyle.Normal
		);

		#endregion

		#region --- AlertCancelBtn Text ---

		GameObject alertCancelBtnTxt = new GameObject("Text", typeof(RectTransform), typeof(Text));
		RectTransform alertCancelBtnTxtRT = (RectTransform)alertCancelBtnTxt.transform;
		Text alertCancelBtnTxtTXT = alertCancelBtnTxt.GetComponent<Text>();
		ComponentSetter.RectTransform(alertCancelBtnTxtRT, alertCancelBtnRT,
			new Vector4(0f, 0f, 1f, 1f),
			new Vector4(0f, 0f, 0f, 0f),
			new Vector2(0f, 0f)
		);
		ComponentSetter.Text(alertCancelBtnTxtTXT, "No", 11,
			new Color(0.2f, 0.2f, 0.2f, 1f),
			TextAnchor.MiddleCenter,
			false, true, true, true,
			FontStyle.Normal
		);

		#endregion




		#endregion


		#region ----- Final Stuff -----

		Selection.activeTransform = rootRT;

		fb.DefaultPath = Application.persistentDataPath;
		fb.m_FileIcon = SpriteUtil.FileIconSprite;
		fb.m_DirectoryIcon = SpriteUtil.DirIconSprite;

		fileBaseTemp.SetActive(false);
		bookMarkBase.SetActive(false);
		typeDDTemp.SetActive(false);
		alertPannel.SetActive(false);
		
		Selectable[] selectables = root.GetComponentsInChildren<Selectable>(true);
		foreach (Selectable selectable in selectables) {
			Navigation n = new Navigation();
			n.mode = Navigation.Mode.None;
			selectable.navigation = n;
			ColorBlock cb = new ColorBlock();
			cb.normalColor = Color.white;
			cb.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
			cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
			cb.disabledColor = new Color(0.7f, 0.7f, 0.7f, 1f);
			cb.colorMultiplier = 1;
			cb.fadeDuration = 0;
			selectable.colors = cb;
		}

		// Support Type
		fb.AddSupportFileFormat("*.*");
		fb.m_FileTypeDropDpwn.captionText.text = "*.*";

		// Book Mark

		fb.m_BookMarkList = new FileBrowser.BookMarkList();
		
#if UNITY_STANDALONE_WIN

		fb.m_BookMarkList.Add(new FileBrowser.BookMark("Computer", FileBrowser.BookMark.SpecialMark.MyComputer));
	
#endif

		fb.m_BookMarkList.Add(new FileBrowser.BookMark("Desktop", FileBrowser.BookMark.SpecialMark.DesktopDirectory));
		fb.m_BookMarkList.Add(new FileBrowser.BookMark("Favorites", FileBrowser.BookMark.SpecialMark.Favorites));
		fb.m_BookMarkList.Add(new FileBrowser.BookMark("Personal", FileBrowser.BookMark.SpecialMark.Personal));
		fb.m_BookMarkList.Add(new FileBrowser.BookMark("MyPictures", FileBrowser.BookMark.SpecialMark.MyPictures));
		fb.m_BookMarkList.Add(new FileBrowser.BookMark("MyMusic", FileBrowser.BookMark.SpecialMark.MyMusic));
		
		fb.ReloadBookMark();

		#endregion


	}



	#endregion


	#region -------- MSG --------


	void OnEnable () {

		FileBrowser fb = target as FileBrowser;

		Title_SOBJ = new SerializedObject(serializedObject.FindProperty("m_Title").objectReferenceValue);
		Address_SOBJ = new SerializedObject(fb.m_AddressInput);
		FileName_SOBJ = new SerializedObject(fb.m_NameInput);
		Logo_SOBJ = new SerializedObject(fb.m_Logo);
		FileType_SOBJ = new SerializedObject(fb.m_FileTypeDropDpwn);
		SubmitBtnTXT_SOBJ = new SerializedObject(fb.m_SubmitBtnText);
		CancelBtnTXT_SOBJ = new SerializedObject(fb.m_CancelBtnText);
		AlertWindowMSG_SOBJ = new SerializedObject(fb.m_AlertWindowMSG);

		Title_m_Text = Title_SOBJ.FindProperty("m_Text");
		Address_m_Text = Address_SOBJ.FindProperty("m_Text");
		FileName_m_Text = FileName_SOBJ.FindProperty("m_Text");
		AlertWindowMSG_m_Text = AlertWindowMSG_SOBJ.FindProperty("m_Text");
		Logo_m_Sprite = Logo_SOBJ.FindProperty("m_Sprite");
		FileType_m_Options = FileType_SOBJ.FindProperty("m_Options");
		SubmitBtnTXT_m_Text = SubmitBtnTXT_SOBJ.FindProperty("m_Text");
		CancelBtnTXT_m_Text = CancelBtnTXT_SOBJ.FindProperty("m_Text");
		m_ShowLastAddressOnRestart = serializedObject.FindProperty("m_ShowLastAddressOnRestart");
		m_OnlySubmitExistPath = serializedObject.FindProperty("m_OnlySubmitExistPath");
		m_ShowAllSupportTypes = serializedObject.FindProperty("m_ShowAllSupportTypes");
		
		fb.ReloadBookMark();

		ComponentGUIOpen = EditorPrefs.GetBool("uFileBrowser.ComponentGUIOpen", false);

	}


	// Dont work on Unity 2017
	//void OnDestroy () {
	//	if (!target) {
	//		var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
	//		var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
	//		clearMethod.Invoke(null, null);
	//	}
	//}

	 

	public override void OnInspectorGUI () {

		serializedObject.Update();
		
		EditorGUILayout.Space();

		Title_m_Text.stringValue = EditorGUILayout.TextField("Title", Title_m_Text.stringValue);
		Address_m_Text.stringValue = EditorGUILayout.TextField("Address", Address_m_Text.stringValue);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DefaultPath"));
		FileName_m_Text.stringValue = EditorGUILayout.TextField("FileName", FileName_m_Text.stringValue);
		EditorGUILayout.PropertyField(Logo_m_Sprite, new GUIContent("Logo"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FileIcon"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DirectoryIcon"));
		SubmitBtnTXT_m_Text.stringValue = EditorGUILayout.TextField("Submit Label", SubmitBtnTXT_m_Text.stringValue);
		CancelBtnTXT_m_Text.stringValue = EditorGUILayout.TextField("Cancel Label", CancelBtnTXT_m_Text.stringValue);
		AlertWindowMSG_m_Text.stringValue = EditorGUILayout.TextField("Alert Window Message", AlertWindowMSG_m_Text.stringValue);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Panel Type", GUILayout.Width(100f));
		EditorGUILayout.Space();
		SaveOrLoad sol = m_OnlySubmitExistPath.boolValue ? SaveOrLoad.LoadPanel : SaveOrLoad.SavePanel;
		m_OnlySubmitExistPath.boolValue = (SaveOrLoad)EditorGUILayout.EnumPopup(sol) == SaveOrLoad.LoadPanel ? true : false;
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(4);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Show Last Address On Enable", GUILayout.Width(200f));
		EditorGUILayout.Space();
		m_ShowLastAddressOnRestart.boolValue = EditorGUILayout.Toggle(m_ShowLastAddressOnRestart.boolValue);
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(4);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Show All Support Formats", GUILayout.Width(200f));
		EditorGUILayout.Space();
		m_ShowAllSupportTypes.boolValue = EditorGUILayout.Toggle(m_ShowAllSupportTypes.boolValue);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		ComponentGUIOpen = EditorGUILayout.Foldout(ComponentGUIOpen, "Components");

		if (ComponentGUIOpen) {
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Title"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Logo"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MainContentSR"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AddressInput"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_NameInput"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BackBtn"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ForwardBtn"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_QuitBtn"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SubmitBtn"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CancelBtn"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AlertOKBtn"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AlertCancelBtn"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SubmitBtnText"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CancelBtnText"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FileTypeDropDpwn"), new GUIContent("File Type"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FileBaseTemp"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AlertPannel"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BookMarkTemp"));
		}
		
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnSubmit"));
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnCancel"));
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BookMarkList"));
		
		// Cheat Label
		Rect cheatRect = GUILayoutUtility.GetRect(0f, 0f);
		EditorGUILayout.PropertyField(FileType_m_Options);
		cheatRect.x += 5;
		cheatRect.y += 12;
		cheatRect.width = 80;
		cheatRect.height = 14;
		GUI.DrawTexture(cheatRect, CheatTexture);
		cheatRect.y += -1;
		GUI.Label(cheatRect, "File Format");
		EditorGUILayout.Space();
		

		serializedObject.ApplyModifiedProperties();

		if (GUI.changed) {
			Title_SOBJ.ApplyModifiedProperties();
			Address_SOBJ.ApplyModifiedProperties();
			FileName_SOBJ.ApplyModifiedProperties();
			Logo_SOBJ.ApplyModifiedProperties();
			FileType_SOBJ.ApplyModifiedProperties();
			SubmitBtnTXT_SOBJ.ApplyModifiedProperties();
			CancelBtnTXT_SOBJ.ApplyModifiedProperties();
			AlertWindowMSG_SOBJ.ApplyModifiedProperties();
			(target as FileBrowser).ReloadBookMark();
			EditorPrefs.SetBool("uFileBrowser.ComponentGUIOpen", ComponentGUIOpen);
		}

		if (Event.current.type == EventType.Used) {
			(target as FileBrowser).ReloadBookMark();
		}

		// Check Support Type
		int needHelpBoxID = 0;
		int errorID = 1;
		System.Collections.Generic.List<Dropdown.OptionData> opList = (target as FileBrowser).m_FileTypeDropDpwn.options;
		foreach (Dropdown.OptionData data in opList) {
			needHelpBoxID = FileBrowser.GetSupportTypeErrotID(data.text);
			if (needHelpBoxID != 0) {
				break;
			}
			errorID++;
		}

		if (needHelpBoxID == -1) {
			EditorGUI.HelpBox(
				GUILayoutUtility.GetRect(0f, 36f, GUILayout.ExpandWidth(true)),
				"Support-type should all longer than 3.\nError in No." + errorID + " item.",
				MessageType.Warning
			);
			EditorGUILayout.Space();
		} else if (needHelpBoxID == 1) {
			EditorGUI.HelpBox(
				GUILayoutUtility.GetRect(0f, 40f, GUILayout.ExpandWidth(true)),
				"Support-type should all start with *.\neg : *.txt  *.png  *.*\nError in No." + errorID + " item.",
				MessageType.Warning
			);
			EditorGUILayout.Space();
		} else if (needHelpBoxID == 2) {
			EditorGUI.HelpBox(
				GUILayoutUtility.GetRect(0f, 40f, GUILayout.ExpandWidth(true)),
				"The characters after *. in support-types should be Letters or Digit only.\nError in No." + errorID + " item.",
				MessageType.Warning
			);
			EditorGUILayout.Space();
		}

	}




	#endregion


}



[CustomPropertyDrawer(typeof(FileBrowser.BookMarkList), true)]
public class BookMarkDrawer : PropertyDrawer {


	private ReorderableList m_ReorderableList;


	private void Init (SerializedProperty property) {

		if (m_ReorderableList != null)
			return;

		SerializedProperty bm = property.FindPropertyRelative("MarkList");

		m_ReorderableList = new ReorderableList(property.serializedObject, bm);
		m_ReorderableList.drawElementCallback = DrawOptionData;
		m_ReorderableList.drawHeaderCallback = DrawHeader;
		m_ReorderableList.elementHeight += 16;
	}



	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		Init(property);
		m_ReorderableList.DoList(position);
	}



	private void DrawHeader (Rect rect) {
		GUI.Label(rect, "Bookmarks");
	}



	private void DrawOptionData (Rect rect, int index, bool isActive, bool isFocused) {
		SerializedProperty itemData = m_ReorderableList.serializedProperty.GetArrayElementAtIndex(index);
		SerializedProperty itemLabel = itemData.FindPropertyRelative("Label");
		SerializedProperty itemPath = itemData.FindPropertyRelative("path");
		SerializedProperty itemType = itemData.FindPropertyRelative("Type");

		RectOffset offset = new RectOffset(0, 0, -1, -3);
		rect = offset.Add(rect);
		rect.height = EditorGUIUtility.singleLineHeight;

		Rect temp;
		int labelWidth = 38;

		EditorGUILayout.BeginHorizontal();
		temp = new Rect(rect.x, rect.y, labelWidth, rect.height);
		EditorGUI.LabelField(temp, "Label");
		temp.width = rect.width - labelWidth;
		temp.x += labelWidth;
		EditorGUI.PropertyField(temp, itemLabel, GUIContent.none);
		EditorGUILayout.EndHorizontal();

		rect.y += EditorGUIUtility.singleLineHeight;

		EditorGUILayout.BeginHorizontal();
		temp = new Rect(rect.x, rect.y, labelWidth, rect.height);
		EditorGUI.LabelField(temp, " Path");
		temp.width = rect.width - labelWidth;
		temp.x += labelWidth;

		if (itemType.enumValueIndex == (int)FileBrowser.BookMark.SpecialMark.Custom) {
			EditorGUILayout.BeginHorizontal();
			EditorGUI.PropertyField(new Rect(temp.x, temp.y, 60, temp.height), itemType, GUIContent.none);
			EditorGUI.PropertyField(new Rect(temp.x + 60, temp.y, temp.width - 60, temp.height), itemPath, GUIContent.none);
			EditorGUILayout.EndHorizontal();
		} else {
			EditorGUI.PropertyField(temp, itemType, GUIContent.none);
		}
		
		EditorGUILayout.EndHorizontal();

	}


	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		Init(property);
		return m_ReorderableList.GetHeight();
	}



}



}