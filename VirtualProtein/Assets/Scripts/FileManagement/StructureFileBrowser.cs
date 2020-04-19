using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using SimpleFileBrowser;

public class StructureFileBrowser : MonoBehaviour

{
    public GameObject modelObject;
    public Text fileName;

    // Use this for initialization
    void Update()
    {
        if (enabled)
        {
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Structure Files", ".pdb"));
            FileBrowser.SetDefaultFilter(".jpg");
            FileBrowser.AddQuickLink("User", "C:\\Users", null);

         StartCoroutine(ShowLoadDialogCoroutine());
            this.enabled = false;
        }
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(false, null, "Load File", "Load");

        // Dialog is closed
        // Print whether a file is chosen (FileBrowser.Success)
        // and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)
        fileName.text = FileBrowserHelpers.GetFilename(FileBrowser.Result);

        if (FileBrowser.Success && fileName.text.EndsWith(".pdb"))
        {
            Model modelScript = modelObject.GetComponent<Model>();
            modelScript.PdbPath = FileBrowser.Result;
            modelScript.newModel = true;
            modelScript.showModel = true;
        }
    }
}