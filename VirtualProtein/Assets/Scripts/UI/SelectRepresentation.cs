using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using System.Linq;

using Structures;

public class SelectRepresentation : MonoBehaviour
{
    // to populate dropdown and send selected rep to the model object
    public Dropdown repDropdown;
    public Text selectionText;

    public static int selectedRepresentationIndex;
    public static bool selectedRepresentationStatus;

    public static bool createdNewRep;
    public static bool hasNewModel;
    public static bool selectedRepresentation;

    public void Start()
    {
        repDropdown.options.Add(new Dropdown.OptionData { text = "Select Representation" });
    }

    public void Update()
    {
        if(hasNewModel)
        {
            InitDropdown();
            hasNewModel = false;
        }

        if (createdNewRep)
        {
            PopulateDropdown();
            createdNewRep = false;
        }

        if(selectedRepresentation)
        {
            PopulateText(selectedRepresentationIndex);
            selectedRepresentation = false;
        }
    }

    // called when dropdown option is clicked on
    public void GetSelectedRepresentationIndex()
    {
        selectedRepresentationIndex = repDropdown.value - 1;
        selectedRepresentation = true;
    }

    // initialise the dropdown
    public void InitDropdown()
    {
        if (repDropdown.options.Count > 1)
        {
            repDropdown.options.RemoveRange(1, repDropdown.options.Count - 1);
        }
    }

    // populate dropdown with model's representations
    private void PopulateDropdown()
    {
        InitDropdown();
        foreach(Representation representation in Model.representations)
        {
            repDropdown.options.Add(new Dropdown.OptionData { text = Model.representations.IndexOf(representation).ToString() + " - " + representation.residueIndices.Count + " chains / " + representation.GetResidueCount().ToString() + " residues / display : " + representation.IsDisplayed.ToString() + '\n' });
        }
    }

    // show selected representation's residues in a scrollable text box
    public void PopulateText(int representationIndex)
    {
        selectionText.text = string.Empty;

        if (selectedRepresentationIndex >= 0)
        {
            int chainIndex = 0;
            foreach (List<int> chainResiduesIndices in Model.representations[representationIndex].residueIndices)
            {
                if (chainResiduesIndices.Any())
                {
                    selectionText.text += "CHAIN " + Model.chains[chainIndex].ChainId + '\n';
                    foreach (int residueIndex in chainResiduesIndices)
                    {
                        selectionText.text += '\t' + Model.chains[chainIndex].chainResidues[residueIndex].ResidueToString() + '\n';
                    }
                }
                chainIndex += 1;
            }
        }
    }

    public void HideSelectedRepresentation()
    {
        selectedRepresentationStatus = false;
        if (selectedRepresentationIndex >= 0)
        {
            Model.hasSelected = true;
        }
    }

    public void ShowSelectedRepresentation()
    {
        selectedRepresentationStatus = true;
        if (selectedRepresentationIndex >= 0)
        {
            Model.hasSelected = true;
        }
    }
}