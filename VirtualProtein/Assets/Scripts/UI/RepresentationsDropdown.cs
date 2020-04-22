using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;

using Structures;

public class RepresentationsDropdown : MonoBehaviour
{
    // to populate dropdown and send selected rep to the model object
    public Dropdown repDropdown;
    public int SelectedRep { get; set; }

    // bool to signal that a new rep has been added and should be added to dropdown list
    public static bool hasNewRep = false;
    public static bool hasNewModel = false;

    // where we get info from the model
    public GameObject modelObject;

    // to check if there is a new representation for the model
    void Update()
    {
        if (hasNewRep)
        {
            PopulateDropdown();
            hasNewRep = false;
        }

        if (hasNewModel)
        {
            repDropdown.options.Clear();
            hasNewModel = false;
        }
    }

    private void PopulateDropdown()
    {
        Model modelScript = modelObject.GetComponent<Model>();
        
        // intitalise dropdown
        repDropdown.options.Clear();

        // populate with updated list of representation
        repDropdown.AddOptions(modelScript.RepsToString());
        
    }

    public void SelectedRepIndexChanged(int index)
    {
        SelectedRep = index;
    }

    public void ConfirmSelect()
    {
        Model modelScript = modelObject.GetComponent<Model>();
        modelScript.hasSelected = true;
    }
}