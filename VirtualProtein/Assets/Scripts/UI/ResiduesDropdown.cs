﻿using UnityEngine;
using UnityEngine.UI;

using Structures;

public class ResiduesDropdown : MonoBehaviour
{
    public Dropdown residueDropdown;
    public Dropdown chainsDropdown;
    public GameObject modelObject;
    
    public static bool hasSelectedChain = false;
    public static bool hasNewModel = false;

    public static int selectedChainIndex;
    public static int selectedResidueIndex;

    private void Start()
    {
        residueDropdown.options.Add(new Dropdown.OptionData { text = "Select residue" });
    }

    // Update is called once per frame
    private void Update()
    {
        if (hasSelectedChain)
        {
            InitOptions();
            PopulateDropdown();
            hasSelectedChain = false;
        }

        if (hasNewModel)
        {
            InitDropdown();
            hasNewModel = false;
        }
    }

    public void SelectedResIndexChanged(int index)
    {
        selectedResidueIndex = index;
    }

    public void SelectedChainIndexChanged(int index)
    {
        selectedChainIndex = index - 1;
        hasSelectedChain = true;
    }

    private void PopulateDropdown()
    {
        InitOptions();
        
        if (selectedChainIndex >= 0)
        {
            foreach(Residue residue in Model.chains[selectedChainIndex].chainResidues)
            {
                residueDropdown.options.Add(new Dropdown.OptionData { text = residue.ResidueToString()} );
            }
        } 
    }

    public void InitDropdown()
    {
        hasNewModel = true;
        InitOptions();
        selectedChainIndex = 0;
        selectedResidueIndex = 0;
    }

    private void InitOptions()
    {
        // remove all residues from previous chain and model
        if (residueDropdown.options.Count > 1)
        {
            residueDropdown.options.RemoveRange(1, residueDropdown.options.Count - 1);
        }
    }


}
