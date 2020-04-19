using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

public class VanDerWaals : Model
{
    // Use this for initialization
    public override void Start ()
    {
        //this.AddAtomsFromFile(@"C:\Program Files (x86)\University of Illinois\VMD\proteins\dna.pdb");
        this.AddAtomsFromFile(@"C:/Users/Tommy/Desktop/met.pdb");
        //this.AddAtomsFromFile(@"C:/Users/Tommy/Desktop/test.pdb");
        this.RenderVDW(backbone);
        this.ScaleTo(2);
    }

    private void RenderVDW(List<Atom> atoms)
    {

        Vector3 center = this.GetCenterPointObject(this.transform.position);

        foreach (Atom atom in atoms)
        {
            if (!atom.IsBackbone) continue;
            GameObject n_atom = Instantiate(AtomSphere, atom.Position - center, Quaternion.identity);
            n_atom.transform.parent = this.transform;
            n_atom.GetComponent<Renderer>().material.color = atom.Colour;
            n_atom.transform.localScale *= atom.VDWRadius;
        }
    }

}
