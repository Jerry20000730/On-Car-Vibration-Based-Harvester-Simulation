namespace uFileBrowser {

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[AddComponentMenu("UI/FileBrowser", 10)]
[DisallowMultipleComponent]
public class FileBrowser : UIBehaviour {
	


	#region -------- SUB --------

	[System.Serializable]
	public class BookMark {

		public enum SpecialMark {
			/// <summary>
			/// 自定义地址
			/// </summary>
			Custom = 0,
			/// <summary>
			/// 包含用户程序组的目录。
			/// </summary>
			Programs = 2,
			/// <summary>
			/// 用作文档的公共储存库的目录。
			/// </summary>
			Personal = 5,
			/// <summary>
			/// “我的文档”文件夹。
			/// </summary>
			MyDocuments = 5,
			/// <summary>
			/// 用作用户收藏夹项的公共储存库的目录。
			/// </summary>
			Favorites = 6,
			/// <summary>
			/// 对应于用户的“启动”程序组的目录。
			/// </summary>
			Startup = 7,
			/// <summary>
			/// 包含用户最近使用过的文档的目录。
			/// </summary>
			Recent = 8,
			/// <summary>
			/// “My Music”文件夹。
			/// </summary>
			MyMusic = 13,
			/// <summary>
			/// 用于物理上存储桌面上的文件对象的目录。
			/// </summary>
			DesktopDirectory = 16,
			/// <summary>
			/// “我的电脑”文件夹。
			/// </summary>
			MyComputer = 17,
			/// <summary>
			/// 用作 Internet 历史记录项的公共储存库的目录。
			/// </summary>
			History = 34,
			/// <summary>
			/// “My Pictures”文件夹。
			/// </summary>
			MyPictures = 39,
		}

		public string Label;

		public SpecialMark Type;

		public string Path {
			get {
				return Type == SpecialMark.Custom ? 
					path : Type == SpecialMark.MyComputer ? "/" : 
					System.Environment.GetFolderPath((System.Environment.SpecialFolder)((int)Type));
			}
			set {
				path = value;
			}
		}

		[SerializeField]
		private string path;


		public BookMark (string l, SpecialMark type, string p = "") {
			Type = type;
			Label = l;
			if (type == SpecialMark.Custom) {
				Path = p;
			}
		}

	}


	[System.Serializable]
	public class BookMarkList {

		public List<BookMark> MarkList = new List<BookMark>();

		public void Add (BookMark b) {
			MarkList.Add(b);
		}

		public int Count {
			get {
				return MarkList.Count;
			}
		}

		public BookMark this[int i] {
			get {
				return MarkList[i];
			}
		}

	}

	#endregion



	#region -------- VAR --------


	/// <summary>
	/// Get or set the title displayed on top of the filebrowser.
	/// </summary>
	public string Title {
		get {
			return m_Title.text;
		}
		set {
			m_Title.text = value;
		}
	}


	/// <summary>
	/// Get or set the address path.
	/// </summary>
	public string AddressPath {
		get {
			return m_AddressInput.text;
		}
		set {
			m_AddressInput.text = value;
		}
	}


	/// <summary>
	/// Get or set the showing file name.
	/// </summary>
	public string FileName {
		get {
			return m_NameInput.text;
		}
		set {
			m_NameInput.text = value;
		}
	}


	/// <summary>
	/// Get or set the default showing path.
	/// </summary>
	public string DefaultPath {
		get {
			return m_DefaultPath;
		}
		set {
			m_DefaultPath = value;
		}
	}


	public Text m_Title;
	public Text m_SubmitBtnText;
	public Text m_CancelBtnText;
	public Text m_AlertWindowMSG;
	public ScrollRect m_MainContentSR;
	public Image m_Logo;
	public Sprite m_FileIcon;
	public Sprite m_DirectoryIcon;
	public InputField m_AddressInput;
	public InputField m_NameInput;
	public Button m_BackBtn;
	public Button m_ForwardBtn;
	public Button m_QuitBtn;
	public Button m_SubmitBtn;
	public Button m_CancelBtn;
	public Button m_AlertOKBtn;
	public Button m_AlertCancelBtn;
	public Dropdown m_FileTypeDropDpwn;
	public RectTransform m_FileBaseTemp;
	public RectTransform m_BookMarkTemp;
	public RectTransform m_NavPanel;
	public RectTransform m_AlertPannel;
	public BookMarkList m_BookMarkList = new BookMarkList();

	public const float DOUBLE_CLICK_TIME = 0.4f;
	public const float FILE_BASE_HEIGHT = 20f;
	

	private RectTransform MainContentRT {
		get {
			return m_MainContentSR.content;
		}
	}

	private Image AddressIMG {
		get {
			if (!addressIMG) {
				addressIMG = m_AddressInput.GetComponent<Image>();
			}
			return addressIMG;
		}
	}
	private Image addressIMG = null;

	private Image NameIMG {
		get {
			if (!nameIMG) {
				nameIMG = m_NameInput.GetComponent<Image>();
			}
			return nameIMG;
		}
	}
	private Image nameIMG = null;

	private int FileNum {
		get {
			return CurrentFiles.Count;
		}
	}

	private int DirNum {
		get {
			return CurrentDirs.Count;
		}
	}

	private int ItemNum {
		get {
			return FileNum + DirNum;
		}
	}

	private string CurrentSupportType {
		get {
			return m_FileTypeDropDpwn.options.Count > 0 ?
				m_FileTypeDropDpwn.options[m_FileTypeDropDpwn.value].text :
				"*.*";
		}
	}

	private float VerticalScrollValue {
		get {
			return Mathf.Clamp01(verticalScrollValue);
		}
	}
	private float verticalScrollValue = 0f;

	private float ContentHeight {
		get {
			return MainContentRT.rect.size.y;
		}
		set {
			MainContentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
		}
	}

	private float ViewPortHeight {
		get {
			return m_MainContentSR.viewport.rect.size.y;
		}
	}


	[SerializeField]
	private FileBrowser.FileBrowserSubmitEvent m_OnSubmit = null;
	[SerializeField]
	private FileBrowser.FileBrowserCancelEvent m_OnCancel = null;
	[SerializeField]
	private string m_DefaultPath = "";
	[SerializeField]
	private bool m_ShowLastAddressOnRestart = true;
	[SerializeField]
	private bool m_OnlySubmitExistPath = true;
	[SerializeField]
	private bool m_ShowAllSupportTypes = true;

	private Transform LastClickTF = null;
	private float LastClickTime = float.MinValue;
	private int PrevShownID = -1;
	private int SelectingID = -1;
	private string PrevShownAddress = "";
	private string AlertWindowBaseMSG = "";
	
	private List<FileInfo> CurrentFiles = new List<FileInfo>();
	private List<DirectoryInfo> CurrentDirs = new List<DirectoryInfo>();
	private List<string> BackPath = new List<string>();
	private List<string> ForwardPath = new List<string>();
	private Dictionary<int, Transform> ItemMap = new Dictionary<int, Transform>();
	private Dictionary<string, Sprite> FileIconMap = new Dictionary<string, Sprite>();

	#endregion

	

	#region -------- MSG --------

	
	protected override void Awake () {

		// Add Built-in Callback
		m_BackBtn.onClick.AddListener(this.Back);
		m_ForwardBtn.onClick.AddListener(this.Forward);
		m_AddressInput.onEndEdit.AddListener(this.Goto);
		m_MainContentSR.onValueChanged.AddListener(this.OnMainContentScroll);
		m_QuitBtn.onClick.AddListener(this.Close);
		m_SubmitBtn.onClick.AddListener(this.Submit);
		m_CancelBtn.onClick.AddListener(this.Close);
		m_FileTypeDropDpwn.onValueChanged.AddListener(this.OnTypeChange);
		m_AlertOKBtn.onClick.AddListener(() => { m_AlertPannel.gameObject.SetActive(false); });

		// Check and Fix Support Types
		// File Icon Map Init
		List<Dropdown.OptionData> supportTypes = m_FileTypeDropDpwn.options;
		for (int i = 0; i < supportTypes.Count; i++) {
			if (GetSupportTypeErrotID(supportTypes[i].text) == 0) {
				FileIconMap.Add(supportTypes[i].text, 
					supportTypes[i].image == null ?
					(supportTypes[i].text == "*." ? m_DirectoryIcon : m_FileIcon) : 
					supportTypes[i].image
				);
			} else {
				supportTypes.RemoveAt(i);
				i--;
			}
		}
		if (supportTypes.Count == 0) {
			supportTypes.Add(new Dropdown.OptionData("*.*"));
		}

		m_FileTypeDropDpwn.captionText.text = CurrentSupportType;


		// Book Mark Init

		ReloadBookMark();

		// Other

		AddressPath = DefaultPath;
		AlertWindowBaseMSG = m_AlertWindowMSG.text;

		base.Awake();
	}


	protected override void OnEnable () {
		Init();
		base.OnEnable();
	}


	protected override void OnDisable () {
		CancelInvoke("AddressInputColoring");
		CancelInvoke("NameInputColoring");
		AddressIMG.color = Color.white;
		NameIMG.color = Color.white;
		base.OnDisable();
	}


	void Update () {
		if (Input.GetKeyDown(KeyCode.Backspace)) {
			GameObject obj = EventSystem.current.currentSelectedGameObject;
			if (obj == null || obj.GetComponent<InputField>() == null) {
				if (Input.GetKey(KeyCode.LeftControl)) {
					Back();
				} else {
					GotoParent();
				}
			}
		}
	}


	#endregion



	#region -------- API --------


	/// <summary>
	/// Show the filebrowser and show last shown address.
	/// </summary>
	public void Init () {
		gameObject.SetActive(true);
		Goto(m_ShowLastAddressOnRestart ? AddressPath : DefaultPath);
	}


	/// <summary>
	/// Hide the filebroser. Equivalent to press the cancel button or close button.
	/// </summary>
	public void Close () {
		ClearItems();
		m_OnCancel.Invoke();
		gameObject.SetActive(false);
	}


	/// <summary>
	/// Show the address given.
	/// </summary>
	/// <param name="path"></param>
	public void Goto (string path) {

		// Check and Fix path
		string dirPath = path;

		if (!string.IsNullOrEmpty(dirPath)) {
			dirPath = dirPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			string c1 = Path.DirectorySeparatorChar.ToString();
			string c2 = Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString();
			while (dirPath.IndexOf(c2) != -1) {
				dirPath = dirPath.Replace(c2, c1);
			}
			if (dirPath[dirPath.Length - 1] != Path.DirectorySeparatorChar) {
				dirPath += Path.DirectorySeparatorChar;
			}
			if (File.Exists(path)) {
				dirPath = PathUtil.GetFileParentPath(path);
			}
			if (!Directory.Exists(dirPath)) {
				AddressIMG.color = new Color(0.9f, 0.3f, 0.3f, 1f);
				CancelInvoke("AddressInputColoring");
				InvokeRepeating("AddressInputColoring", 0.04f, 0.04f);
				AddressPath = PrevShownAddress;
				return;
			}
		} else {
			dirPath = Path.DirectorySeparatorChar.ToString();
		}
		
		// Undo and Redo

		if (Directory.Exists(PrevShownAddress) && dirPath != PrevShownAddress) {
			BackPath.Add(PrevShownAddress);
			ForwardPath.Clear();
		}

		// Show Them
		
		Show(dirPath);

		FreshBFBtnActive();
		
	}


	/// <summary>
	/// Goto the parent path of current address.
	/// </summary>
	public void GotoParent () {
		Goto(PathUtil.GetDirectoryParentPath(PrevShownAddress));
	}


	/// <summary>
	/// Goto last shown address. Equivalent to press the back button.
	/// </summary>
	public void Back () {
		if (BackPath.Count == 0) {
			return;
		}
		string path = BackPath[BackPath.Count - 1];
		if (Directory.Exists(path) && Directory.Exists(PrevShownAddress)) {
			BackPath.RemoveAt(BackPath.Count - 1);
			ForwardPath.Add(PrevShownAddress);
			Show(path);
		}
		FreshBFBtnActive();
	}


	/// <summary>
	/// Opposite for Back();
	/// </summary>
	public void Forward () {
		if (ForwardPath.Count == 0) {
			return;
		}
		string path = ForwardPath[ForwardPath.Count - 1];
		if (Directory.Exists(path) && Directory.Exists(PrevShownAddress)) {
			ForwardPath.RemoveAt(ForwardPath.Count - 1);
			BackPath.Add(PrevShownAddress);
			Show(path);
		}
		FreshBFBtnActive();
	}


	/// <summary>
	/// Submit current selecting path. Equivalent to press the OK button.
	/// </summary>
	public void Submit () {

		bool alert = false;

		if (m_OnlySubmitExistPath) {
			if (SelectingID >= 0 && GetItemInfo(SelectingID) != null) {
				string path = GetItemInfo(SelectingID).FullName;
				if (File.Exists(path)) {
					m_OnSubmit.Invoke(path);
					ClearItems();
					gameObject.SetActive(false);
				} else if (Directory.Exists(path)) {
					if (CurrentSupportType == "*.") {
						m_OnSubmit.Invoke(path);
						ClearItems();
						gameObject.SetActive(false);
					} else {
						Goto(path);
					}
				} else {
					AddressIMG.color = new Color(0.9f, 0.3f, 0.3f, 1f);
					CancelInvoke("AddressInputColoring");
					InvokeRepeating("AddressInputColoring", 0.04f, 0.04f);
				}
			} else {
				alert = true;
			}
		} else if (FileName.Length > 0 && FileName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1) {
			string path = PathUtil.CombinePaths(PrevShownAddress, FileName);
			string currentType = CurrentSupportType;
			if (currentType != "*.*" && currentType != "*.") {
				string ex = Path.GetExtension(currentType);
				if (Path.GetExtension(path) != ex) {
					path += ex;
				}
			}
			if (File.Exists(path)) {
				if (m_AlertPannel.gameObject.activeSelf) {
					m_OnSubmit.Invoke(path);
					ClearItems();
					gameObject.SetActive(false);
				} else {
					m_AlertWindowMSG.text = string.Format(AlertWindowBaseMSG, Path.GetFileName(path));
					m_AlertPannel.gameObject.SetActive(true);
					return;
				}
			} else {
				m_OnSubmit.Invoke(path);
				ClearItems();
				gameObject.SetActive(false);
			}
		} else {
			alert = true;
		}

		if (alert) {
			NameIMG.color = new Color(0.9f, 0.3f, 0.3f, 1f);
			CancelInvoke("NameInputColoring");
			InvokeRepeating("NameInputColoring", 0.04f, 0.04f);
		}

		m_AlertPannel.gameObject.SetActive(false);

	}


	/// <summary>
	/// Call by the uGUI system when support type is changed.
	/// </summary>
	/// <param name="id"></param>
	public void OnTypeChange (int id) {
		Goto(AddressPath);
	}


	/// <summary>
	/// Call by the uGUI system when scrolling the content window.
	/// </summary>
	/// <param name="value"></param>
	public void OnMainContentScroll (Vector2 value) {
		verticalScrollValue = 1f - value.y;
		Fresh();
	}


	/// <summary>
	/// Check the given string can correctly represent the file type or not.
	/// </summary>
	/// <param name="type"></param>
	/// <returns>0 for Correct format. -1 means too short. 1 means not start with "*.". 2 means format wrong.</returns>
	public static int GetSupportTypeErrotID (string type) {
		if (type.Length >= 2) {
			if (type[0] != '*' || type[1] != '.') {
				return 1;
			}
			string s = type.Substring(2);
			if (s != "*" && new string(s.Where(char.IsLetterOrDigit).ToArray()) != s) {
				return 2;
			}
		} else {
			return -1;
		}
		return 0;
	}


	/// <summary>
	/// Reload the book mark from this_fileBrowser.BookMarkList.
	/// </summary>
	public void ReloadBookMark () {
		TransformUtil.Clear(m_NavPanel);
		int len = m_BookMarkList.Count;
		for (int i = 0; i < len; i++) {
			GameObject g = Instantiate<GameObject>(m_BookMarkTemp.gameObject);
			g.name = m_BookMarkList[i].Label;
			g.transform.SetParent(m_NavPanel);
			g.transform.localScale = Vector3.one;
			g.transform.localPosition = Vector3.zero;
			g.SetActive(true);
			Transform tf = g.transform.Find("Text");
			if (tf) {
				Text txt = tf.GetComponent<Text>();
				if (txt) {
					txt.text = m_BookMarkList[i].Label;
				}
			}
			Button btn = g.GetComponent<Button>();
			if (btn) {
				string temp = m_BookMarkList[i].Path;
				btn.onClick.AddListener(
					() => { Goto(temp); }
				);
			}
		}
	}


	/// <summary>
	/// Add a support file format.
	/// </summary>
	/// <param name="format"></param>
	/// <param name="fileIcon"></param>
	/// <returns>0 means success, 
	/// -1 means format string too short, 
	/// 1 means format string is not start with *.
	/// 2 means format string is not a correct file format</returns>
	public int AddSupportFileFormat (string format, Sprite fileIcon = null) {
		int id = GetSupportTypeErrotID(format);
		if (id == 0) {
			Sprite fixedIcon = fileIcon ? fileIcon : format == "*." ? m_DirectoryIcon : m_FileIcon;
			m_FileTypeDropDpwn.AddOptions(
				new List<Dropdown.OptionData>() { 
					new Dropdown.OptionData(
						format, fixedIcon
			)});
			if (!FileIconMap.ContainsKey(format)) {
				FileIconMap.Add(format, fixedIcon);
			}
		}
		return id;
	}


	#endregion



	#region -------- LGC --------


	[System.Serializable]
	private class FileBrowserSubmitEvent : UnityEvent<string> { }


	[System.Serializable]
	private class FileBrowserCancelEvent : UnityEvent { }


	private FileBrowser () { }


	private void AddressInputColoring () {
		if (AddressIMG.color.b > 0.99f) {
			CancelInvoke("AddressInputColoring");
			AddressIMG.color = Color.white;
		} else {
			AddressIMG.color = Color.Lerp(AddressIMG.color, Color.white, 0.1f);
		}
	}


	private void NameInputColoring () {
		if (NameIMG.color.b > 0.99f) {
			CancelInvoke("NameInputColoring");
			NameIMG.color = Color.white;
		} else {
			NameIMG.color = Color.Lerp(NameIMG.color, Color.white, 0.1f);
		}
	}


	private void Show (string dirPath) {
		
		// Clear Prev

		ClearItems();

		// Files and Dirs List

		bool winRoot = false;

#if UNITY_STANDALONE_WIN
		if ( dirPath.Length == 1 && dirPath[0] == Path.DirectorySeparatorChar) {
			winRoot = true;
		}
#elif UNITY_STANDALONE_OSX

#else

#endif

		string sp = CurrentSupportType;
		CurrentFiles.Clear();
		CurrentDirs.Clear();

		if (winRoot) {
			string[] drs = Directory.GetLogicalDrives();
			for (int i = 0; i < drs.Length; i++) {
				if (Directory.Exists(drs[i])) {
					CurrentDirs.Add(new DirectoryInfo(drs[i]));
				}
			}
		} else {
			DirectoryInfo dir = new DirectoryInfo(dirPath);
			CurrentDirs.AddRange(
				dir.GetDirectories().Where(f => (f.Attributes & FileAttributes.Hidden) == 0).ToArray()
			);
			if (m_ShowAllSupportTypes) {
				int len = m_FileTypeDropDpwn.options.Count;
				for (int i = 0; i < len; i++) {
					string s = m_FileTypeDropDpwn.options[i].text;
					if (s != "*.") {
						CurrentFiles.AddRange(
							dir.GetFiles(
								s,
								SearchOption.TopDirectoryOnly).Where(
									f => ((f.Attributes & FileAttributes.Hidden) == 0 && !CurrentFiles.Contains(f))
						).ToArray());
					}
					if (s == "*.*") {
						break;
					}
				}
			} else if (sp != "*.") {
				CurrentFiles.AddRange(
				dir.GetFiles(
					sp,
					SearchOption.TopDirectoryOnly).Where(
						f => ((f.Attributes & FileAttributes.Hidden) == 0 && !CurrentFiles.Contains(f))
				).ToArray());
			}
		}
		
		// Fix Content Size

		ContentHeight = FILE_BASE_HEIGHT * ItemNum;

		// Other

		AddressPath = dirPath;
		PrevShownAddress = AddressPath;
		AddressIMG.color = Color.white;
		NameIMG.color = Color.white;

		CancelSelect();

		MainContentRT.anchoredPosition = Vector2.zero;
		verticalScrollValue = 0f;

		Fresh(true);
	}


	private void Fresh (bool forceFresh = false) {

		int screenNum = Mathf.FloorToInt(ViewPortHeight / FILE_BASE_HEIGHT) + 1;

		int id = (int)(VerticalScrollValue * Mathf.Max(0f, ItemNum - screenNum));

		if (id != PrevShownID || forceFresh) {

			int end = Mathf.Min(id + screenNum, ItemNum - 1);
			int prevEnd = Mathf.Min(PrevShownID + screenNum, ItemNum - 1);
			int l = id > PrevShownID ? PrevShownID : end + 1;
			int r = id > PrevShownID ? id : prevEnd + 1;

			// Remove
			for (int i = l; i < r; i++) {
				RemoveItem(i);
			}

			// Add
			for (int i = id; i <= end; i++) {
				AddItem(i, i >= DirNum, SelectingID == i);
			}

			PrevShownID = id;
		}
	}


	private void AddItem (int id, bool isFile, bool selecting) {
		if (!ItemMap.ContainsKey(id)) {

			FileSystemInfo info = GetItemInfo(id);

			if (info != null) {
				GameObject g = GameObject.Instantiate<GameObject>(m_FileBaseTemp.gameObject);
				g.SetActive(true);
				ComponentSetter.RectTransform((RectTransform)g.transform, MainContentRT,
					new Vector4(0, 1, 1, 1),
					new Vector4(0, -FILE_BASE_HEIGHT * id, 0, FILE_BASE_HEIGHT),
					new Vector2(0, 1)
				);

				Transform iconTF = g.transform.Find("Icon");
				if (iconTF) {
					Image iconIMG = iconTF.GetComponent<Image>();
					if (iconIMG) {
						Sprite icon = null;
						if (isFile) {
							string ex = "*" + Path.GetExtension(info.FullName);
							icon = FileIconMap.ContainsKey(ex) ? FileIconMap[ex] : m_FileIcon;
							icon = icon ?? m_FileIcon;
						} else {
							icon = FileIconMap.ContainsKey("*.") ? FileIconMap["*."] : m_DirectoryIcon;
							icon = icon ?? m_DirectoryIcon;
						}
						iconIMG.sprite = icon;
					}
				}

				Transform textTF = g.transform.Find("Text");
				if (textTF) {
					Text textTXT = textTF.GetComponent<Text>();
					if (textTXT) {
						string name = Path.GetFileName(info.FullName);
						textTXT.text = string.IsNullOrEmpty(name) ? info.FullName : name;
					}
				}

				Toggle tg = g.GetComponent<Toggle>();
				if (tg) {
					tg.isOn = selecting;
					tg.onValueChanged.AddListener((bool value) => {
						if (value) {
							SelectItem(id, g.transform, isFile, info.FullName);
						} else {
							CancelSelect();
						}
					});
				}

				ItemMap.Add(id, g.transform);
			}
		}
	}


	private void RemoveItem (int id) {
		if (ItemMap.ContainsKey(id)) {
			Transform tf = ItemMap[id];
			ItemMap.Remove(id);
			GameObject.DestroyImmediate(tf.gameObject, false);
		}
	}


	private void ClearItems () {
		TransformUtil.Clear(MainContentRT);
		ItemMap.Clear();
	}


	private FileSystemInfo GetItemInfo (int id) {
		if (id < 0) {
			return null;
		} else if (id < DirNum) {
			return CurrentDirs[id];
		} else if (id - DirNum < FileNum) {
			return CurrentFiles[id - DirNum];
		} else {
			return null;
		}
	}


	private void SelectItem (int id, Transform tf, bool isFile, string path) {

		if (tf == LastClickTF && Time.time - LastClickTime < DOUBLE_CLICK_TIME) {
			if (isFile) {
				Submit();
			} else {
				Goto(path);
			}
		} else {
			// Select
			FileName = Path.GetFileName(path);
			SelectingID = id;
		}
		LastClickTF = tf;
		LastClickTime = Time.time;
	}


	private void CancelSelect () {
		FileName = "";
		SelectingID = -1;
	}


	private void FreshBFBtnActive () {
		m_BackBtn.interactable = BackPath.Count > 0;
		m_ForwardBtn.interactable = ForwardPath.Count > 0;
	}


	#endregion



}
}