using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Structures;
using BezierFunctions;

public class Model : MonoBehaviour
{
    //                  //
    //      FIELDS      //
    //                  //

    private static string modelPath;

    // The game objects that are used by the representations
    public GameObject AtomSphere;
    public GameObject AtomBond;


    // Fields containing lists of the atom objects and the representations used for the model
    public List<Atom> atoms;
    public List<Atom> backbone;
    public List<Representation> representations;


    // Extra fields for representation specifics
    private const int steps = 25;

    public Model()
    {
        modelPath = @"C:/Users/Tommy/Desktop/star.pdb";
    }

    public Model(string path)
    {
        modelPath = @"C:/Users/Tommy/Desktop/star.pdb";
    }
    

    //                  //
    //      START       //
    //                  //

    public virtual void Start()
    {
        // Initialise parent Model gameobject
        // When loading a pdb file for the first time, VMD automatically creates a representation of the protein
        // showing all the atoms using the wireframe/lines representation
        this.AddAtomsFromFile(modelPath);
        representations = new List<Representation>
        {
            //new Representation(atoms, RepresentationType.WireFrame, ColourScheme.ByAtomType),
            new Representation(atoms, RepresentationType.Tube, ColourScheme.ByStructure)
        };
        RenderRepresentations();
        this.ScaleTo(2);
    }


    //                   //
    //      UPDATE       //
    //                   //

    private void Update()
    {
        // TODO: handle UI and change representations accordingly
        // TODO: Animate model (one representation at a time?)
        
    }


    //                                //
    //         USEFUL METHODS         //
    //                                //

    
    // adds all the atoms and their data to the list of atoms field from a PDB file
    public void AddAtomsFromFile(string path)
    {
        atoms = FileParser.ParsePDB(path);
        backbone = FileParser.FindBackbone(atoms);
    }

    // scale the model to fit within a certain span
    public void ScaleTo(float f)
    {
        float atomSpan = 0;
        float atomDistance = 0;

        // get distance between atoms that are furthest appart
        foreach (Atom atom_a in GetAtoms())
        {
            foreach (Atom atom_b in GetAtoms())
            {
                atomDistance = Vector3.Distance(atom_a.Position, atom_b.Position);
                if (atomDistance > atomSpan)
                {
                    atomSpan = atomDistance;
                }
            }
        }

        this.transform.localScale = new Vector3(f / atomSpan, f / atomSpan, f / atomSpan);
    }


    //                          //
    //          GETTERS         //
    //                          //


    // gets the average position of all atoms in the molecule and returns position as Vector3
    public Vector3 GetCenterPointOrigin()
    {
        Vector3 center = Vector3.zero;
        foreach (Atom atom in GetAtoms())
        {
            center += atom.Position;
        }
        return center / GetAtomCount();
    }

    // same as above but includes an offset
    public Vector3 GetCenterPointObject(Vector3 objectOrigin)
    {
        Vector3 center = Vector3.zero;
        foreach (Atom atom in GetAtoms())
        {
            center += atom.Position;
        }
        return center / GetAtomCount() - objectOrigin;
    }

    // return the list of atoms associated to this model
    public List<Atom> GetAtoms()
    {
        return atoms;
    }

    // return the total number of atoms associated to this account
    public float GetAtomCount()
    {
        return atoms.Count;
    }

    // returns a list of all atoms that have been marked as backbone atoms
    public List<Atom> GetBackBone(List<Atom> atoms)
    {
        List<Atom> backbone = new List<Atom>();

        // add atoms to the backbone list
        foreach(Atom atom in atoms)
        {
            if (atom.IsBackbone)
            {
                backbone.Add(atom);
            }
        }
        
        return backbone;
    }

    //                                        //
    //         REPRESENTATION METHODS         //
    //                                        //


    // iterate over the representations list and render them
    private void RenderRepresentations()
    {
        foreach(Representation rep in representations)
        {
            // check the representation type
            Debug.Log(rep.repType);
            switch (rep.repType)
            {
                case RepresentationType.BallAndStick:
                    RenderBallAndStick(rep.atoms, rep.atomObjects);
                    break;
                case RepresentationType.Tube:
                    RenderTube(GetBackBone(rep.atoms));
                    break;
                case RepresentationType.VanDerWalls:
                    RenderVDW(rep.atoms, rep.atomObjects);
                    break;
                case RepresentationType.WireFrame:
                    RenderWireFrame(rep.atoms, rep.atomObjects);
                    break;
            }
        }
    }
    

    // Renders the given atoms as a BALL AND STICK representation
    private void RenderBallAndStick(List<Atom> atoms, List<GameObject> atomObjects)
    {
        // initialise empty list of atom game objects and get center point of molecule
        // Whenever a gameobject is instantiated, substract center vector (average of all
        // atom positions) to create structure at origin of model
        Vector3 center = this.GetCenterPointObject(this.transform.position);

        // generate the spheres representing the atoms as children of Ball and Stick GameObject (which this script is attached to)
        foreach (Atom atom in atoms)
        {
            GameObject n_atom = Instantiate(AtomSphere, atom.Position - center, Quaternion.identity);
            n_atom.transform.parent = this.transform;
            n_atom.GetComponent<Renderer>().material.color = atom.Colour;
            // add to list of objects 
            atomObjects.Add(n_atom);
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
                        // instantiate 2 parts of the bond 
                        GameObject bondP1 = Instantiate(AtomBond, (bond_center + atoms[j].Position) / (float)2.0 - center, Quaternion.identity);
                        GameObject bondP2 = Instantiate(AtomBond, (bond_center + atoms[i].Position) / (float)2.0 - center, Quaternion.identity);
                        // set parent transform
                        bondP1.transform.parent = this.transform;
                        bondP2.transform.parent = this.transform;
                        // orient bond to look at closest atom
                        bondP1.transform.LookAt(atomObjects[j].transform, Vector3.up);
                        bondP2.transform.LookAt(atomObjects[i].transform, Vector3.up);
                        // change the bond's colour to the same as the closest atom's
                        bondP1.GetComponent<Renderer>().material.color = atomObjects[j].GetComponent<Renderer>().material.color;
                        bondP2.GetComponent<Renderer>().material.color = atomObjects[i].GetComponent<Renderer>().material.color;
                        // scale down the bonds to fit the specific inter-atom distance
                        bondP1.transform.localScale = new Vector3(10, 10, distance * 25);
                        bondP2.transform.localScale = new Vector3(10, 10, distance * 25);

                    }
                }
                else if ((0.4 <= distance && distance <= 1.9))
                {
                    // instantiate 2 parts of the bond 
                    GameObject bondP1 = Instantiate(AtomBond, (bond_center + atoms[j].Position) / (float)2.0 - center, Quaternion.identity);
                    GameObject bondP2 = Instantiate(AtomBond, (bond_center + atoms[i].Position) / (float)2.0 - center, Quaternion.identity);
                    // set parent transform
                    bondP1.transform.parent = this.transform;
                    bondP2.transform.parent = this.transform;
                    // orient bond to look at closest atom
                    bondP1.transform.LookAt(atomObjects[j].transform, Vector3.up);
                    bondP2.transform.LookAt(atomObjects[i].transform, Vector3.up);
                    // change the bond's colour to the same as the closest atom's
                    bondP1.GetComponent<Renderer>().material.color = atomObjects[j].GetComponent<Renderer>().material.color;
                    bondP2.GetComponent<Renderer>().material.color = atomObjects[i].GetComponent<Renderer>().material.color;
                    // scale down the bonds to fit the specific inter-atom distance
                    bondP1.transform.localScale = new Vector3(10, 10, distance * 25);
                    bondP2.transform.localScale = new Vector3(10, 10, distance * 25);
                }
            }
        }
    }

    // Renders the given atoms as a VAN DER WALLS/CPK representation
    private void RenderVDW(List<Atom> atoms, List<GameObject> atomObjects)
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

    // Renders the given atoms as a WIREFRAME representation
    void RenderWireFrame(List<Atom> atoms, List<GameObject> atomObjects)
    {

        Vector3 center = this.GetCenterPointObject(this.transform.position);

        foreach (Atom atom in atoms)
        {
            GameObject n_atom = Instantiate(AtomSphere, atom.Position - center, Quaternion.identity);
            n_atom.transform.parent = this.transform;
            n_atom.transform.localScale = new Vector3(10f, 10f, 10f);
            n_atom.GetComponent<Renderer>().material.color = atom.Colour;
            // add to list of objects 
            atomObjects.Add(n_atom);
        }
        Debug.Log(atomObjects.Count);

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
                        // instantiate 2 parts of the bond 
                        GameObject bondP1 = Instantiate(AtomBond, (bond_center + atoms[j].Position) / (float)2.0 - center, Quaternion.identity);
                        GameObject bondP2 = Instantiate(AtomBond, (bond_center + atoms[i].Position) / (float)2.0 - center, Quaternion.identity);
                        // set parent transform
                        bondP1.transform.parent = this.transform;
                        bondP2.transform.parent = this.transform;
                        // orient bond to look at closest atom
                        Debug.Log("The atom is at position " + atoms[j].Position);
                        Debug.Log("The object is at position " + atomObjects[j].transform);

                        bondP1.transform.LookAt(atomObjects[i].transform, Vector3.up);
                        bondP2.transform.LookAt(atomObjects[j].transform, Vector3.up);
                        // change the bond's colour to the same as the closest atom's
                        bondP1.GetComponent<Renderer>().material.color = atomObjects[j].GetComponent<Renderer>().material.color;
                        bondP2.GetComponent<Renderer>().material.color = atomObjects[i].GetComponent<Renderer>().material.color;
                        // scale down the bonds to fit the specific inter-atom distance
                        bondP1.transform.localScale = new Vector3(10, 10, distance * 25);
                        bondP2.transform.localScale = new Vector3(10, 10, distance * 25);
                    }
                }
                else if ((0.4 <= distance && distance <= 1.9))
                {
                    // instantiate 2 parts of the bond 
                    GameObject bondP1 = Instantiate(AtomBond, (bond_center + atoms[j].Position) / (float)2.0 - center, Quaternion.identity);
                    GameObject bondP2 = Instantiate(AtomBond, (bond_center + atoms[i].Position) / (float)2.0 - center, Quaternion.identity);
                    // set parent transform
                    bondP1.transform.parent = this.transform;
                    bondP2.transform.parent = this.transform;
                    // orient bond to look at closest atom
                    bondP1.transform.LookAt(atomObjects[i].transform, Vector3.up);
                    bondP2.transform.LookAt(atomObjects[j].transform, Vector3.up);
                    // change the bond's colour to the same as the closest atom's
                    bondP1.GetComponent<Renderer>().material.color = atomObjects[j].GetComponent<Renderer>().material.color;
                    bondP2.GetComponent<Renderer>().material.color = atomObjects[i].GetComponent<Renderer>().material.color;
                    // scale down the bonds to fit the specific inter-atom distance
                    bondP1.transform.localScale = new Vector3(10, 10, distance * 25);
                    bondP2.transform.localScale = new Vector3(10, 10, distance * 25);
                }
            }
        }
    }

    // Render the given atoms as a TUBE representation
    void RenderTube(List<Atom> atoms)
    {
        Vector3 center = this.GetCenterPointObject(this.transform.position);

        // store all curves as a sub list of 3D points that the curve should follow to concatinate into a spline
        List<List<Vector3>> curves = new List<List<Vector3>>();

        int currentRes = atoms.First().ResSeqNum;
        int currentCurve = 0;
        curves.Add(new List<Vector3>());

        // create the sub lists of the positions of the atoms within a residue
        foreach (Atom atom in atoms)
        {
            // part of same residue 
            if (currentRes == atom.ResSeqNum)
            {
                curves[currentCurve].Add(atom.Position);
            }
            // reached a new residue
            if (currentRes < atom.ResSeqNum)
            {
                curves.Add(new List<Vector3>());
                currentCurve += 1;
                curves[currentCurve].Add(atom.Position);
                currentRes = atom.ResSeqNum;
            }
        }

        //iterate over points in each sub-list of the curves list to create the Bezier curves
        Vector3 first;
        Vector3 secnd;
        for (int i = 0; i < curves.Count; i++)
        {
            // check if last residue, we use the QUADRATIC Bezier function for this one since there will always
            // be only 3 positions in the last curve 
            if (i == curves.Count - 1)
            {
                for (int j = 0; j < steps - 1; j++)
                {
                    // find midpoint 
                    // we instantiate at midpoint because tube origin's is in tube's middle
                    first = Bezier.GetPointQuad(curves[i][0], curves[i][1], curves[i][2], j / (float)steps);
                    secnd = Bezier.GetPointQuad(curves[i][0], curves[i][1], curves[i][2], (j + 1f) / (float)steps);
                    Vector3 currentPosition = (first + secnd) / 2f;

                    GameObject n_tube;

                    if (j == 0)
                    {
                        // instantiate as a sphere to act as a joint with previous curve    
                        n_tube = Instantiate(AtomSphere, currentPosition - center, Quaternion.identity);
                        n_tube.transform.localScale = new Vector3(40f, 40f, 30f);
                    }
                    else
                    {
                        if (j == 1)
                        {
                            continue;
                        }
                        n_tube = Instantiate(AtomBond, currentPosition - center, Quaternion.identity);
                        // scale down to an arbitrary size that fits the style
                        n_tube.transform.localScale = new Vector3(30f, 30f, 20f);
                    }

                    //oritent the tube to look at the next Bezier position
                    //color the tube to carbon (main atom in the backbone)
                    n_tube.transform.LookAt(secnd - center, Vector3.up);
                    n_tube.transform.parent = this.transform;

                }
            }
            else
            {
                for (int j = 0; j < steps - 1; j++)
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
                        n_atom.transform.localScale = new Vector3(30f, 30f, 30f);
                    }
                    else
                    {
                        n_atom = Instantiate(AtomBond, currentPosition - center, Quaternion.identity);
                        n_atom.transform.localScale = new Vector3(30f, 30f, 20f);
                    }
                    // look at 2nd position
                    // n_atom.transform.Rotate(90.0f, 0.0f, 0.0f, Space.Self);
                    // if this is the start of any curve other than the first one, then the first tube in the curve must look at the previous curves's last
                    // position
                    n_atom.transform.LookAt(secnd - center, Vector3.up);
                    // scale down to the size of a step
                    n_atom.transform.parent = this.transform;

                }
            }
        }
    }
}
