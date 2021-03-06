﻿using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;

using Structures;

public class CreateRepresentation : MonoBehaviour
{
    public GameObject modelObject;

    public Dropdown repTypeDropdown;
    public Dropdown repColourScheme;

    public static int SelectedRepType { get; set; }
    public static int SelectedColourScheme { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        PopulateDropdown();
    }

    private void PopulateDropdown()
    {
        repTypeDropdown.AddOptions(Enum.GetNames(typeof(RepresentationType)).ToList());
        repColourScheme.AddOptions(Enum.GetNames(typeof(ColourScheme)).ToList());
    }

    public void TypeDropdownIndexChanged(int index)
    {
        SelectedRepType = index;
    }

    public void ColourDropdownIndexChanged(int index)
    {
        SelectedColourScheme = index;
    }

    // signal to model that a new rep has been selected and can be added
    public void ConfirmCreate()
    {
        Model.newRep = true;
    }
}
