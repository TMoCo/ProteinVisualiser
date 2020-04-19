using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BezierFunctions;
using Structures;

public class Tube : Model {

    private const int steps = 25;

    // Use this for initialization
    public override void Start()
    {
        //this.AddAtomsFromFile(@"C:\Program Files (x86)\University of Illinois\VMD\proteins\dna.pdb");
        this.AddAtomsFromFile(@"C:/Users/Tommy/Desktop/1ubq.pdb");
        //this.AddAtomsFromFile(@"C:/Users/Tommy/Desktop/test.pdb");
        this.RenderTube(backbone);
        this.ScaleTo(2);
    }

    void RenderTube(List<Atom> atoms)
    {
        Vector3 center = this.GetCenterPointObject(this.transform.position);

        List<List<Vector3>> curves = new List<List<Vector3>>();

        int currentRes = atoms.First().ResSeqNum;
        int currentList = 0;
        curves.Add(new List<Vector3>());

        // create the sub lists of the positions of the atoms within a residue
        foreach (Atom atom in atoms)
        {
            // part of same residue 
            if (currentRes == atom.ResSeqNum)
            {
                curves[currentList].Add(atom.Position);
            }
            // reached a new residue
            if(currentRes < atom.ResSeqNum)
            {
                curves.Add(new List<Vector3>());
                currentList += 1;
                curves[currentList].Add(atom.Position);
                currentRes = atom.ResSeqNum;
            }
            // reached the end of the atom list
            if (atoms.IndexOf(atom) == atoms.Count - 1)
            {
                // do something
            }
        }

        //iterate over list of lists to create curves
        Vector3 first;
        Vector3 secnd;
        for(int i=0; i < curves.Count; i++)
        {
            // check if last residue
            if (i == curves.Count - 1)
            {
                for (int j = 0; j < steps - 1; j++)
                {
                    // find midpoint 
                    first = Bezier.GetPointQuad(curves[i][0], curves[i][1], curves[i][2], j / (float)steps);
                    secnd = Bezier.GetPointQuad(curves[i][0], curves[i][1], curves[i][2], (j + 1f) / (float)steps);
                    Vector3 currentPosition = (first + secnd) / 2f;
                    // instantiate a tube at midpoint
                    GameObject n_atom;
                    if (j == 0)
                    {
                        n_atom = Instantiate(AtomSphere, currentPosition - center, Quaternion.identity);
                        n_atom.transform.localScale = new Vector3(10.5f, 10.5f, 16f);
                    }
                    else
                    {
                        if (j == 1)
                        {
                            continue;
                        }
                       n_atom = Instantiate(AtomBond, currentPosition - center, Quaternion.identity);
                        // scale down to an arbitrary size that fits the style
                        n_atom.transform.localScale = new Vector3(10f, 10f, 20f);
                    }
                    // look at the 2nd position
                    n_atom.transform.LookAt(secnd - center, Vector3.up);
                    n_atom.transform.parent = this.transform;
                }
            }
            else
            {
                for(int j = 0; j < steps - 1; j++)
                {

                    // find midpoint
                    first = Bezier.GetPointCube(curves[i][0], curves[i][1], curves[i][2], curves[i + 1][0], j / (float)steps);
                    secnd = Bezier.GetPointCube(curves[i][0], curves[i][1], curves[i][2], curves[i + 1][0], (j + 1f) / (float)steps);
                    Vector3 currentPosition = (first + secnd) / 2f;
                    // intstantiate a tube at midpoint
                    GameObject n_atom;
                    if (j == 0)
                    {
                        n_atom = Instantiate(AtomSphere, currentPosition - center, Quaternion.identity);
                    }
                    else
                    {
                        n_atom = Instantiate(AtomBond, currentPosition - center, Quaternion.identity);
                    }
                    // look at 2nd position
                    // n_atom.transform.Rotate(90.0f, 0.0f, 0.0f, Space.Self);
                    // if this is the start of any curve other than the first one, then the first tube in the curve must look at the previous curves's last
                    // position
                    n_atom.transform.LookAt(secnd - center, Vector3.up);
                    // scale down to the size of a step
                    n_atom.transform.localScale = new Vector3(10f, 10f, 20f);
                    n_atom.transform.parent = this.transform;
                    
                }
            }
        }
    }
}
