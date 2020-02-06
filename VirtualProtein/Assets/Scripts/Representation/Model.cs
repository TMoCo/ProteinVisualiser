using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

public class Model : MonoBehaviour {

    public GameObject atomSphere;
	// Use this for initialization
	void Start () {
        FileParser parser = new FileParser();
        List<Atom> atoms = parser.ParsePDB(@"C:\Program Files (x86)\University of Illinois\VMD\proteins\1ubq.pdb");

        Debug.Log(atoms);

        foreach(Atom atom in atoms)
        {
            Instantiate(atomSphere, atom.GetPosition(), Quaternion.identity);
        }
       
	}
}
