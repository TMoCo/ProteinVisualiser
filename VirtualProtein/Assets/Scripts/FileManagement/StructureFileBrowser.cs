using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SimpleFileBrowser;
using Structures;

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
         string fileName = FileBrowserHelpers.GetFilename(FileBrowser.Result);

         if (FileBrowser.Success && fileName.EndsWith(".pdb"))
         {
            // If a file was chosen, load next scene
            //Debug.Log(fileName);
            //Debug.Log(fileName.Substring(fileName.Length - 3).Equals("pdb"));
            //Debug.Log(fileName.EndsWith(".pdb"));
            //FileParser parser = new FileParser();
            //List<Atom> atoms = parser.ParsePDB(FileBrowser.Result);
            SceneManager.LoadScene("viewModel");
         }
    }
}
