using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;

using Structures;

public class ColouringDropdown : MonoBehaviour
{
    public Dropdown repColourScheme;
    public int selectedColourScheme;

    // Update is called once per frame
    void Start()
    {
        PopulateDropdown();
    }

    private void PopulateDropdown()
    {
        repColourScheme.AddOptions(Enum.GetNames(typeof(ColourScheme)).ToList());
    }

    public void DropdownIndexChanged(int index)
    {
        selectedColourScheme = index;
    }
}

