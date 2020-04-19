using System;
using System.Collections.Generic;
using UnityEngine;
using Structures;


public class BallAndStick : Model
{
    public override void Start()
    {
        //this.AddAtomsFromFile(@"C:\Program Files (x86)\University of Illinois\VMD\proteins\dna.pdb");
        this.AddAtomsFromFile(@"C:/Users/Tommy/Desktop/1ubq.pdb");
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

        // generate the spheres representing the atoms as children of Ball and Stick GameObject (which this script is attached to)
        foreach (Atom atom in atoms)
        {
            GameObject n_atom = InstantiateAtom(atom, center, atomObjects);
        }

        // generate bonds between atoms where appropriate using inter-atom distance
        // for each atom, check distance to other atoms. If distance between them is
        // between 0.4 and 1.9 Angstroms
        for (int i = 0; i < atoms.Count - 1; i++)
        {
            for (int j = (i + 1); j < atoms.Count; j++)
            {
                Vector3 bond_center = (atoms[i].Position + atoms[j].Position) / (float)2.0;
                float distance = Vector3.Distance(atoms[i].Position, atoms[j].Position);

                // These values are the same used as the RasMol visualiser when no CONNECT fields are in a pdb file 
                // to determine whether a bond exists or not
                if ((atoms[i].Element.CompareTo("H") == 0) || (atoms[j].Element.CompareTo("H") == 0))
                {
                    // instantiate a bond
                    if ((0.4 <= distance && distance <= 1.2))
                    {
                        GameObject bondP1 = InstantiateBond(center, ((bond_center + atoms[j].Position) / (float)2.0), atomObjects[j], distance);
                        bondP1.transform.localScale = new Vector3(10, 10, distance * 25);
                        GameObject bondP2 = InstantiateBond(center, ((bond_center + atoms[i].Position) / (float)2.0), atomObjects[i], distance);
                        bondP2.transform.localScale = new Vector3(10, 10, distance * 25);
                    }
                }
                else if ((0.4 <= distance && distance <= 1.9))
                {
                    GameObject bondP1 = InstantiateBond(center, ((bond_center + atoms[j].Position) / (float)2.0), atomObjects[j], distance);
                    bondP1.transform.localScale = new Vector3(10, 10, distance * 25);
                    GameObject bondP2 = InstantiateBond(center, ((bond_center + atoms[i].Position) / (float)2.0), atomObjects[i], distance);
                    bondP2.transform.localScale = new Vector3(10, 10, distance * 25);
                }
            }
        }
    }

    private GameObject InstantiateAtom(Atom atom, Vector3 center, List<GameObject> atomObjects)
    {
        // instantiate an atom sphere
        GameObject n_atom = Instantiate(AtomSphere, atom.Position - center, Quaternion.identity);
        if (!atom.IsDisplayed) n_atom.GetComponent<Renderer>().enabled = false;
        n_atom.transform.parent = this.transform;
        n_atom.GetComponent<Renderer>().material.color = atom.Colour;
        atomObjects.Add(n_atom);
        return n_atom;
    }

    private GameObject InstantiateBond(Vector3 center, Vector3 bond_pos, GameObject atomObject, float distance)
    {
        // instantiate a bond
        GameObject n_bond = Instantiate(AtomBond, bond_pos - center, Quaternion.identity);
        n_bond.transform.parent = this.transform;
        n_bond.transform.LookAt(atomObject.transform, Vector3.up);
        n_bond.GetComponent<Renderer>().material.color = atomObject.GetComponent<Renderer>().material.color;
        return n_bond;
    }
}