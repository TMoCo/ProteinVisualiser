using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

public class WireFrame : Model
{ 
    // Use this for initialization
    public override void Start ()
    {
        //this.AddAtomsFromFile(@"C:\Program Files (x86)\University of Illinois\VMD\proteins\dna.pdb");
        this.AddAtomsFromFile(@"C:/Users/Tommy/Desktop/1ubq.pdb");
        //this.AddAtomsFromFile(@"C:/Users/Tommy/Desktop/test.pdb");
        this.RenderWireFrame(atoms);
        this.ScaleTo(2);
    }
	
	void RenderWireFrame(List<Atom> atoms)
    {
        List<GameObject> atomObjects = new List<GameObject>();
        Vector3 center = this.GetCenterPointObject(this.transform.position);

        foreach (Atom atom in atoms)
        {
            InstantiateAtom(atom, center, atomObjects);
        }

        for (int i = 0; i < atoms.Count - 1; i++)
        {
            for (int j = (i + 1); j < atoms.Count; j++)
            {
                Vector3 bond_center = (atoms[i].Position + atoms[j].Position) / (float)2.0;
                float distance = Vector3.Distance(atoms[i].Position, atoms[j].Position);

                if ((atoms[i].Element.CompareTo("H") == 0) || (atoms[j].Element.CompareTo("H") == 0))
                {
                    if ((0.4 <= distance && distance <= 1.2))
                    {
                        
                        InstantiateBond(center, ((bond_center+atoms[j].Position) / (float)2.0), atomObjects[j], distance);
                        InstantiateBond(center, ((bond_center + atoms[i].Position) / (float)2.0), atomObjects[i], distance);
                    }
                }
                else if ((0.4 <= distance && distance <= 1.9))
                {
                    InstantiateBond(center, ((bond_center + atoms[j].Position) / (float)2.0), atomObjects[j], distance);
                    InstantiateBond(center, ((bond_center + atoms[i].Position) / (float)2.0), atomObjects[i], distance);
                }
            }
        }
    }

    private void InstantiateAtom(Atom atom, Vector3 center, List<GameObject> atomObjects)
    {
        // instantiate an atom sphere
        GameObject n_atom = Instantiate(AtomSphere, atom.Position - center, Quaternion.identity);
        if (!atom.IsDisplayed) n_atom.GetComponent<Renderer>().enabled = false;
        n_atom.transform.parent = this.transform;
        n_atom.transform.localScale = new Vector3(10f, 10f, 10f);
        n_atom.GetComponent<Renderer>().material.color = atom.Colour;
        atomObjects.Add(n_atom);
    }

    private void InstantiateBond(Vector3 center, Vector3 bond_pos, GameObject atomObject, float distance)
    {
        // instantiate a bond
        GameObject n_bond = Instantiate(AtomBond, bond_pos - center, Quaternion.identity);
        n_bond.transform.parent = this.transform;
        n_bond.transform.LookAt(atomObject.transform, Vector3.up);
        n_bond.GetComponent<Renderer>().material.color = atomObject.GetComponent<Renderer>().material.color;
        n_bond.transform.localScale = new Vector3(10, 10, distance*25);
    }
}
