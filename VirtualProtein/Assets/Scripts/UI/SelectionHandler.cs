using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using Structures;

public class SelectionHandler : MonoBehaviour
{
    public Text selectionText;
    public GameObject UI;
    public GameObject modelObject;

    List<List<int>> selectedResidues;

    public static bool hasNewModel = false;
    public static bool hasAdded = false;
    public static bool modelLoaded = false;

    // Update is called once per frame
    void Update()
    {
        if (hasNewModel)
        {
            InitList();
            PopulateText();
            modelLoaded = true;
            hasNewModel = false;
        }

        if (hasAdded)
        {
            PopulateText();
            hasAdded = false;
        }

    }

    void PopulateText()
    {
        selectionText.text = string.Empty;

        Model modelScript = modelObject.GetComponent<Model>();

        foreach(List<int> chainList in selectedResidues)
        {
            if (chainList.Any())
            {
                selectionText.text += "CHAIN " + modelScript.modelChains[selectedResidues.IndexOf(chainList)].ChainId + '\n';
                foreach(int index in chainList)
                {
                    selectionText.text += '\t' + modelScript.modelChains[selectedResidues.IndexOf(chainList)].chainResidues[index].ResidueToString() + '\n';
                }
            }
        }
    }

    public void SelectAllChains()
    {
        if (modelLoaded)
        {
            selectedResidues.Clear();
            foreach(Chain chain in modelObject.GetComponent<Model>().modelChains)
            {
                List<int> residuesInChain = new List<int>();
                int index = 0;
                foreach(Residue residue in chain.chainResidues)
                {
                    residuesInChain.Add(index);
                    index += 1;
                }
                selectedResidues.Add(residuesInChain);
            }
            hasAdded = true;
        }
    }

    public void AddToSelection()
    {

        if(ResiduesDropdown.selectedResidueIndex > 0)
        {
            if (!selectedResidues[ResiduesDropdown.selectedResidueIndex].Contains(ResiduesDropdown.selectedResidueIndex - 1))
            {
                selectedResidues[ResiduesDropdown.selectedChainIndex].Add(ResiduesDropdown.selectedResidueIndex - 1);
                hasAdded = true;
            }
        }

    }

    private void InitList()
    {
        selectedResidues = new List<List<int>>();

        foreach(Chain chain in modelObject.GetComponent<Model>().modelChains)
        {
            selectedResidues.Add(new List<int>());
        }
    }
}
