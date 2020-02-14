using System;
using System.Collections.Generic;
using UnityEngine;
using Structures;


public class BallAndStick : Model
{
    public GameObject AtomSphere;
    public GameObject AtomBond;

    public override void Start()
    {
        this.AddAtomsFromFile(@"C:\Program Files (x86)\University of Illinois\VMD\proteins\dna.pdb");
        //this.AddAtomsFromFile(@"C:/Users/Tommy/Desktop/1ubq.pdb");
        //this.AddAtomsFromFile(@"C:/Users/Tommy/Desktop/test.pdb");
        this.RenderBallAndStick(GetAtoms());
        this.ScaleTo(2);
    }

    private void RenderBallAndStick(List<Atom> atoms)
    {
        // initialise empty list of atom game objects and get center point of molecule
        // Whenever a gameobject is instantiated, substract center vector (average of all
        // atom positions) to create structure at origin
        List<GameObject> atomObjects = new List<GameObject>();
        Vector3 center = this.GetCenterPointObject(this.transform.position);
        Debug.Log(atoms.Count);

        // generate the spheres representing the atoms as children of Ball and Stick GameObject (which this script is attached to)
        foreach (Atom atom in atoms)
        {
            GameObject n_atom = Instantiate(AtomSphere, atom.position-center, Quaternion.identity);
            if (!atom.isDisplayed) n_atom.GetComponent<Renderer>().enabled = false;
            n_atom.transform.parent = this.transform;
            n_atom.GetComponent<Renderer>().material.color = atom.colour;
            atomObjects.Add(n_atom);
        }

        // generate bonds between atoms where appropriate using inter-atom distance
        // for each atom, check distance to other atoms. If distance between them is
        // between 0.4 and 1.9 Angstroms
        for (int i = 0; i < atoms.Count - 1; i++)
        {
            for (int j = (i + 1); j < atoms.Count; j++)
            {
                Vector3 bond_pos = (atoms[i].position + atoms[j].position) / (float)2.0;
                float distance = Vector3.Distance(atoms[i].position, atoms[j].position);

                // These values are the same used as the RasMol visualiser when no CONNECT fields are in a pdb file 
                // to determine whether a bond exists or not
                if ((atoms[i].element.CompareTo("H") == 0) || (atoms[j].element.CompareTo("H") == 0))
                {
                    // instantiate a bond
                    if ((0.4 <= distance && distance <= 1.2))
                    {
                        GameObject n_bond = Instantiate(AtomBond, bond_pos - center, Quaternion.identity);
                        n_bond.transform.parent = this.transform;
                        n_bond.transform.LookAt(atomObjects[j].transform, Vector3.up);
                        // need to rotate cylinder by 90 degrees around x because LookAt points the z axis of the cylinder to desired position!
                        n_bond.transform.Rotate(new Vector3(90, 0, 0), Space.Self);
                        n_bond.transform.localScale = new Vector3((float)0.2, distance / (float)2.0, (float)0.2);
                    }
                }
                else if ((0.4 <= distance && distance <= 1.9))
                {
                    // instantiate a bond
                    GameObject n_bond = Instantiate(AtomBond, bond_pos-center, Quaternion.identity);
                    n_bond.transform.parent = this.transform;
                    n_bond.transform.LookAt(atomObjects[j].transform, Vector3.up);
                    // need to rotate cylinder by 90 degrees around x because LookAt points the z axis of the cylinder to desired position!
                    n_bond.transform.Rotate(new Vector3(90, 0, 0), Space.Self);
                    n_bond.transform.localScale = new Vector3((float)0.2, distance / (float)2.0, (float)0.2);
                }
            }
        }
    }
}
