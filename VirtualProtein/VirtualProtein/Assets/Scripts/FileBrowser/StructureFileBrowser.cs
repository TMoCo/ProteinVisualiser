using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SimpleFileBrowser;

public class StructureFileBrowser : MonoBehaviour {

	// Use this for initialization
	void Start () {

        FileBrowser.SetFilters(true, new FileBrowser.Filter("Structure Files", ".pdb"));
        FileBrowser.SetDefaultFilter(".jpg");
        FileBrowser.AddQuickLink("User", "C:\\Users", null);

        StartCoroutine(ShowLoadDialogCoroutine() );
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
    // Show a load file dialog and wait for a response from user
    // Load file/folder: file, Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(false, null, "Load File", "Load");

    // Dialog is closed
    // Print whether a file is chosen (FileBrowser.Success)
    // and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)
         Debug.Log(FileBrowser.Success + " " + FileBrowser.Result);


         if (FileBrowser.Success)
         {
            // If a file was chosen, load next scene
            string fileName = FileBrowserHelpers.GetFilename(FileBrowser.Result);
            Debug.Log(fileName);
            //Debug.Log(fileName.Substring(fileName.Length - 3).Equals("pdb"));
            Debug.Log(fileName.EndsWith(".pdb"));
            SceneManager.LoadScene("Test");
         }
    }
}
