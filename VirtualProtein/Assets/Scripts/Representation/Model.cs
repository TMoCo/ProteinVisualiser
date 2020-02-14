using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

public class Model : MonoBehaviour
{
    // Use this for initialization

    public List<Atom> atoms;

    public virtual void Start()
    {
        AddAtomsFromFile(@"C:\");
	}

    public void AddAtomsFromFile(string path)
    {
        FileParser parser = new FileParser();
        atoms = parser.ParsePDB(path);
    }

    public void ScaleTo(float f)
    {
        float atomSpan = 0;
        float atomDistance = 0;

        // get distance between atoms that are furthest appart
        foreach (Atom atom_a in GetAtoms())
        {
            foreach (Atom atom_b in GetAtoms())
            {
                atomDistance = Vector3.Distance(atom_a.position, atom_b.position);
                if (atomDistance > atomSpan)
                {
                    atomSpan = atomDistance;
                }
            }
        }

        this.transform.localScale = new Vector3(f / atomSpan, f / atomSpan, f / atomSpan);
    }
    // gets the average position of all atoms in the molecule and returns position as Vector3
    public Vector3 GetCenterPointOrigin()
    {
        Vector3 center = Vector3.zero;
        foreach (Atom atom in GetAtoms())
        {
            center += atom.position;
        }
        return center / GetAtomCount();
    }

    public Vector3 GetCenterPointObject(Vector3 objectOrigin)
    {
        Vector3 center = Vector3.zero;
        foreach (Atom atom in GetAtoms())
        {
            center += atom.position;
        }
        return center / GetAtomCount() - objectOrigin;
    }

    public List<Atom> GetAtoms()
    {
        return atoms;
    }

    public float GetAtomCount()
    {
        return atoms.Count;
    }
}
