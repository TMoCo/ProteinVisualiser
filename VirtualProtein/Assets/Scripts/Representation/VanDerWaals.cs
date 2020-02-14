using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

public class VanDerWaals : Model
{

    public GameObject AtomSphere;
    // Use this for initialization
    public override void Start ()
    {
        this.AddAtomsFromFile(@"C:/Users/Tommy/Desktop/1ubq.pdb");
        //this.AddAtomsFromFile(@"C:/Users/Tommy/Desktop/test.pdb");
        this.RenderVDW(GetAtoms());
        this.ScaleTo(2);
    }

    private void RenderVDW(List<Atom> atoms)
    {

        Vector3 center = this.GetCenterPointObject(this.transform.position);

        foreach (Atom atom in atoms)
        {
            if (!atom.isDisplayed) continue;
            GameObject n_atom = Instantiate(AtomSphere, atom.position - center, Quaternion.identity);
            n_atom.transform.parent = this.transform;
            n_atom.GetComponent<Renderer>().material.color = atom.colour;
            n_atom.transform.localScale = new Vector3(atom.VDWRadius, atom.VDWRadius, atom.VDWRadius);
        }
    }

}
