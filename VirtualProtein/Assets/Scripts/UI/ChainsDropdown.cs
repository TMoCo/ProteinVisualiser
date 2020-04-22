using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Structures;

public class ChainsDropdown : MonoBehaviour
{
    public Dropdown chainsDropdown;
    public GameObject modelObject;

    public bool hasNewModel = false;

    public int selectedChainIndex;

    private void Start()
    {
        chainsDropdown.options.Add(new Dropdown.OptionData { text = "Select chain" });
    }

    // Update is called once per frame
    private void Update()
    {
        if (hasNewModel)
        {
            PopulateDropdown();
            hasNewModel = false;
        }
    }

    public void SelectedRepIndexChanged(int index)
    {
        selectedChainIndex = index;
    }

    private void PopulateDropdown()
    {
        Model modelScript = modelObject.GetComponent<Model>();

        foreach (Chain chain in modelScript.modelChains)
        {
            chainsDropdown.options.Add(new Dropdown.OptionData { text = chain.ChainId });
        }
    }

    public void InitDropdown()
    {
        hasNewModel = true;
        selectedChainIndex = 0;
        InitOptions();
    }

    private void InitOptions()
    {
        // remove all previous chains from previous model
        if (chainsDropdown.options.Count > 1)
        {
            chainsDropdown.options.RemoveRange(1, chainsDropdown.options.Count - 1);
        }
    }


}
