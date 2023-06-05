namespace uFileBrowser {

public class Demo : UnityEngine.MonoBehaviour {

	public UnityEngine.UI.InputField IF;


	public void OpenFile (string path) {
		IF.text = FileUtil.ReadText(path);
	}


	public void SaveFile (string path) {
		FileUtil.WriteText(IF.text, path);
	}


}
}