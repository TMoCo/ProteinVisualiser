using System;
using System.Collections.Generic;

using System.Linq;

using UnityEngine;

using Structures;
using BezierFunctions;

public class Model : MonoBehaviour
{
    //                  //
    //      FIELDS      //
    //                  //

    //                          //
    //      PUBLIC FIELDS       //
    //                          //

    // fields for comunication wuth UI
    public string PdbPath { get; set; }
    public string DsspPath { get; set; }

    public bool showModel = false;
    public bool newModel = false;
    public bool newRep = false;
    public bool hasSelected = false;
    public bool hasBackbone = false;
    public bool hasSecStruct = false;
    public bool hasDssp = false;

    // The game objects that display the model's info and interact with player
    public GameObject UI;

    // The game objects that are used by the representations
    public GameObject AtomSphere;
    public GameObject AtomBond;
    public GameObject HelixSide;
    public GameObject SheetSide;

    public CharacterController controller;
    public Transform playerPosition;

    //                           //
    //      PRIVATE FIELDS       //
    //                           //

    // Lists containing all the GameObjects relevant to the model 
    public List<GameObject> atomGameObjects = new List<GameObject>();
    public List<GameObject> bondGameObjects = new List<GameObject>();


    // Fields containing lists of the atom objects and the representations used for the model
    public List<Atom> atoms;

    public List<List<Atom>> chains;
    public List<Atom> backbone;

    public List<Chain> modelChains;

    public List<Representation> representations = new List<Representation>();

    public int AtomCount { get; set; }
    public int BondCount { get; set; }
    public int ResidueCount { get; set; }

    public int repCounter = 0;

    // Fields for User Input
    Vector3 mousePrevPos = Vector3.zero;
    Vector3 mousePosDelta = Vector3.zero;
    Vector3 minScale = new Vector3(0.01f, 0.01f, 0.01f);
    Vector3 maxScale = new Vector3(100f, 100f, 100f);

    // parser for files
    private FileParser parser;

    // Extra fields for representation specifics
    private const int steps = 20;
    public float speed = 10f;

    //                  //
    //      START       //
    //                  //

    private void Start()
    {
        parser = new FileParser();
    }

    //                   //
    //      UPDATE       //
    //                   //

    private void Update()
    {
        
        if (!showModel == false)
        {
            if (newModel == true)
            {
                // destroy the previous model
                DestroyModelObjects();

                // initialise model info
                AtomCount = 0;
                BondCount = 0;
                ResidueCount = 0;

                // load all the required data from desired file
                GetPdbData(PdbPath);

                // Let UI know it can display data
                ModelInfo.fileLoaded = true;
                SelectionHandler.hasNewModel = true;
                ChainsDropdown.hasNewModel = true;
                ResiduesDropdown.hasNewModel = true;
                    
                // intitialse representations
                representations.Clear();
                UI.GetComponent<RepresentationsDropdown>().repDropdown.options.Clear();

                GetComponent<SphereCollider>().radius = GetModelSize()/2;

                ChainToList();

                // default for first initialisation      
                List<int> test = new List<int>();
                for (int i = 0; i < 76; i++)
                {
                        test.Add(i); 

                }

                foreach(Chain chain in modelChains)
                {
                    DrawVDW(chain, test);
                }
                newModel = false;
            }


            // Detect that the user has created a new representation
            if (newRep == true)
            {
                //signal from dropdown received, means we take data selected and create a rep with it
                RepCreateDropdown repCreateScript = UI.GetComponent<RepCreateDropdown>();                
                // update the select rep dropdown
                RepresentationsDropdown.hasNewRep = true;

                newRep = false;
            }

            if (hasSelected == true)
            {
                // remove previous rep
                DestroyModelObjects();
                
                // deactivate all other representations
                foreach (Representation rep in representations)
                {
                    rep.IsDisplayed = false;
                }

                // set the selected rep IsDisplay to true
                representations[UI.GetComponent<RepresentationsDropdown>().SelectedRep].IsDisplayed = true;

                RenderRepresentations();

                hasSelected = false;
            }

            if (hasDssp == true)
            {
                GetDsspData(DsspPath);
                if (parser.DSSPstatus)
                {
                    hasSecStruct = true;
                    foreach(List<Atom> chain in chains)
                    {
                        RenderCartoon(chain);
                    }
                }
                hasDssp = false;
            }

            if (ShowHideUI.showUI)
            {

                float x = Input.GetAxis("Horizontal");
                float y = Input.GetAxis("Vertical");
                float z = Input.GetAxis("Vertical");

                
                Vector3 movement = playerPosition.right * x + playerPosition.forward * z;

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    movement = playerPosition.up * y;
                }

                transform.Translate(movement * speed * Time.deltaTime, Space.World);



                if (Input.GetMouseButton(0))
                {
                    RotateModel();
                }
                else if (Input.GetMouseButton(1))
                {
                    ScaleModel();
                }
            }

            mousePrevPos = Input.mousePosition;
        }
    }

    //                                //
    //         USEFUL METHODS         //
    //                                //

    // returns the chains as a long list of atoms
    public List<Atom> ChainToList()
    {
        List<Atom> atomList = new List<Atom>();
        foreach(Chain chain in modelChains)
        {
            foreach(Residue residue in chain.chainResidues)
            {
                atomList.AddRange(residue.resAtoms);
            }
        }

        return atomList;
    }
    
    // destroys all GameObjects in the scene relevant to the representation
    public void DestroyModelObjects()
    {
        if(atomGameObjects.Any())
        {
            foreach(GameObject atomObject in atomGameObjects)
            {
                Destroy(atomObject);
            }
            atomGameObjects.Clear();
        }
        if (bondGameObjects.Any())
        {
            foreach (GameObject bondObject in bondGameObjects)
            {
                Destroy(bondObject);
            }
            bondGameObjects.Clear();
        }
    }

    // adds all the atoms and their data to the Model fields from a PDB file
    public void GetPdbData(string path)
    {
        chains = parser.ParsePDB(path);
        modelChains = parser.ParsePDBtoChain(path);

        /* FOR DEBUG */
        // if for whatever reason the algorithm can't generate a backbone, notify the user and don't render tube and cartoon for that model

        foreach(List<Atom> chain in chains)
        {
            try
            {
                parser.FindBackbone(chain);
                hasBackbone = true;
            }
            catch (System.Exception)
            {
                hasBackbone = false;
            }

            BondCount += FileParser.GetBondCount(chain);
            AtomCount += chain.Count;
        }
        ResidueCount += FileParser.GetResCount(chains);

        FileParser.GetResidues(chains);

        //List<SecondaryStructure> list = FileParser.ParseDSSP(chains, "C:\\Users\\Tommy\\Desktop\\proteins\\6lu7.dssp");
        //SetStructureInfo(list);
    }

    // adds the secondary structure information to each residue
    private void GetDsspData(string path)
    {
        Debug.Log(path);
        List<SecondaryStructure> fileData = parser.ParseDSSP(chains, path);
        if (parser.DSSPstatus)
        {
            SetStructureInfo(fileData);
        }
    }

    // sets all the residues (atoms) of the model to their correct secondary structure
    private void SetStructureInfo(List<SecondaryStructure> infoList)
    {
        // info list should not be longer than the residue count 
        if (ResidueCount != infoList.Count)
        {
            Debug.Log("Error could not set data");
        }
        else
        {
            int atomChainIndex = 0;
            int index = 0;
            foreach(List<Atom> chain in chains)
            {
                foreach(Residue res in FileParser.GetResidues(chain))
                {
                    for(int i = 0; i < res.AtomCount; i++)
                    {
                        chain[atomChainIndex+i].SecStructInf = infoList[index];
                    }
                    atomChainIndex += res.AtomCount;
                    index += 1;
                }
                atomChainIndex = 0;
            }
        }
    }

    // Get the distance between the atoms that are furthest appart
    public float GetModelSize()
    {
        float atomSpan = 0;
        float atomDistance = 0;
        Vector3 center = this.GetCenterPointObject(this.transform.position);


        // init empty list
        List<Atom> atomTotal = new List<Atom>();

        foreach(List<Atom> chain in chains)
        {
            atomTotal.Concat(chain);
        }

        Debug.Log(atomTotal.Count);

        // get distance between atoms that are furthest appart
        foreach (Atom atom_a in atomTotal)
        {
            foreach (Atom atom_b in atomTotal)
            {
                atomDistance = Vector3.Distance(atom_a.Position - center, atom_b.Position - center);
                if (atomDistance > atomSpan)
                {
                    atomSpan = atomDistance;
                }
            }
        }

        return atomSpan;
    }

    // scale the model to fit within a certain span
    public void ScaleTo(float f)
    {
        float modelSize = GetModelSize();

        transform.localScale = new Vector3(f / modelSize, f / modelSize, f / modelSize);

    }

    //                          //
    //          GETTERS         //
    //                          //

    // gets the list of representations as a list of strings
    public List<string> RepsToString()
    {
        List<string> repsAsStrings = new List<string>();
        foreach(Representation rep in representations)
        {
            repsAsStrings.Add(rep.ToString());
        }
        return repsAsStrings;
    }

    // gets the average position of all atoms in the molecule and returns position as Vector3
    public Vector3 GetCenterPointOrigin()
    {
        Vector3 center = Vector3.zero;
        foreach (List<Atom> chain in chains)
        {
            foreach (Atom atom in chain)
            {
                center += atom.Position;
            }
        }
        return center / AtomCount;
    }

    // same as above but includes an offset
    public Vector3 GetCenterPointObject(Vector3 objectOrigin)
    {
        Vector3 center = Vector3.zero;
        foreach(List<Atom> chain in chains)
        {
            foreach (Atom atom in chain)
            {
                center += atom.Position;
            }
        }
        return center / AtomCount - objectOrigin;
    }

    // return the list of atoms associated to this model
    public List<Atom> GetAtoms()
    {
        return atoms;
    }

    // returns a list of all atoms that have been marked as backbone atoms
    public List<Atom> GetBackbone(List<Atom> atoms)
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
        foreach(Representation representation in representations)
        {
            // check if rep shoulf be displayed
            if (representation.IsDisplayed)
            {
                // first set the colour of the atoms depending on selection
                switch (representation.repType)
                {
                    case RepresentationType.VanDerWalls:
                        break;
                    case RepresentationType.Tube:
                        break;
                    case RepresentationType.WireFrame:
                        break;
                    case RepresentationType.BallAndStick:
                        break;
                    case RepresentationType.Cartoon:
                        break;
                }
            }
        }
    }

    /*  OLD BALL AND STICK 
    // Renders the atoms in a chain as a BALL AND STICK representation
    private void RenderBallAndStick(List<Atom> atoms)
    {
        float res = atoms[0].ResSeqNum;
        float last = atoms.Last().ResSeqNum;
        // initialise scale
        transform.localScale = new Vector3(1f, 1f, 1f);
        // Whenever a gameobject is instantiated, substract center vector (average of all
        // atom positions) to create structure at origin of model
        Vector3 center = this.GetCenterPointObject(this.transform.position);

        // generate the spheres representing the atoms as children of Ball and Stick GameObject (which this script is attached to)
        foreach (Atom atom in atoms)
        {
            GameObject n_atom = Instantiate(AtomSphere, atom.Position - center, Quaternion.identity);
            n_atom.transform.parent = this.transform;
            if (atom.ResSeqNum == res)
            {
                n_atom.GetComponent<Renderer>().material.color = Color.yellow;
            }
            else if (atom.ResSeqNum == (res+1))
            {
                n_atom.GetComponent<Renderer>().material.color = Color.cyan;
            }
            else if (atom.ResSeqNum == last)
            {
                n_atom.GetComponent<Renderer>().material.color = Color.green;
            }
            else
            {
                n_atom.GetComponent<Renderer>().material.color = atom.Colour;
            }
            // add to list of objects 
            atomGameObjects.Add(n_atom);
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
                        bondP1.transform.LookAt(atomGameObjects[j].transform, Vector3.up);
                        bondP2.transform.LookAt(atomGameObjects[i].transform, Vector3.up);
                        // change the bond's colour to the same as the closest atom's
                        bondP1.GetComponent<Renderer>().material.color = atomGameObjects[j].GetComponent<Renderer>().material.color;
                        bondP2.GetComponent<Renderer>().material.color = atomGameObjects[i].GetComponent<Renderer>().material.color;
                        // scale down the bonds to fit the specific inter-atom distance
                        bondP1.transform.localScale = new Vector3(10, 10, distance * 25);
                        bondP2.transform.localScale = new Vector3(10, 10, distance * 25);

                        bondGameObjects.Add(bondP1);
                        bondGameObjects.Add(bondP2);
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
                    bondP1.transform.LookAt(atomGameObjects[j].transform, Vector3.up);
                    bondP2.transform.LookAt(atomGameObjects[i].transform, Vector3.up);
                    // change the bond's colour to the same as the closest atom's
                    bondP1.GetComponent<Renderer>().material.color = atomGameObjects[j].GetComponent<Renderer>().material.color;
                    bondP2.GetComponent<Renderer>().material.color = atomGameObjects[i].GetComponent<Renderer>().material.color;
                    // scale down the bonds to fit the specific inter-atom distance
                    bondP1.transform.localScale = new Vector3(10, 10, distance * 25);
                    bondP2.transform.localScale = new Vector3(10, 10, distance * 25);

                    bondGameObjects.Add(bondP1);
                    bondGameObjects.Add(bondP2);
                }
            }
        }
    }
        */

    // Renders the atoms in a chain as a BALL AND STICK representation
    private void DrawBallAndStick(Chain chain, List<int> selectedResidues)
    {
        int currentRes = 0;
        Vector3 modelCenter = this.GetCenterPointObject(this.transform.position);

        foreach (Residue residue in chain.chainResidues)
        {
            foreach(Atom atom in residue.resAtoms)
            {
                if(selectedResidues.Contains(currentRes))
                {
                    GameObject n_atom = Instantiate(AtomSphere, atom.Position - modelCenter, Quaternion.identity);
                    n_atom.transform.parent = this.transform;
                    n_atom.GetComponent<Renderer>().material.color = atom.Colour;
                    residue.residueGameObjects.Add(n_atom);

                    foreach(Atom neighbour in atom.neighbours)
                    {
                        if (neighbour.AtomSerial > atom.AtomSerial)
                        {
                            Vector3 bond_center = (atom.Position + neighbour.Position) / (float)2.0;
                            float distance = Vector3.Distance(atom.Position, neighbour.Position);

                            GameObject bondP1 = Instantiate(AtomBond, (bond_center + atom.Position) / (float)2.0 - modelCenter, Quaternion.identity);
                            GameObject bondP2 = Instantiate(AtomBond, (bond_center + neighbour.Position) / (float)2.0 - modelCenter, Quaternion.identity);
                            // set parent transform
                            bondP1.transform.parent = this.transform;
                            bondP2.transform.parent = this.transform;
                            // orient bond to look at closest atom
                            bondP1.transform.LookAt(atom.Position - modelCenter, Vector3.up);
                            bondP2.transform.LookAt(neighbour.Position - modelCenter, Vector3.up);
                            // change the bond's colour to the same as the closest atom's
                            bondP1.GetComponent<Renderer>().material.color = atom.Colour;
                            bondP2.GetComponent<Renderer>().material.color = neighbour.Colour;
                            // scale down the bonds to fit the specific inter-atom distance
                            bondP1.transform.localScale = new Vector3(10, 10, distance * 25);
                            bondP2.transform.localScale = new Vector3(10, 10, distance * 25);

                            residue.residueGameObjects.Add(bondP1);
                            residue.residueGameObjects.Add(bondP2);
                        }
                    }
                }
            }
            currentRes += 1;
        }
    }

    /* OLD VDW
    private void RenderVDW(List<Atom> atoms)
    {
        Vector3 center = this.GetCenterPointObject(this.transform.position);

        foreach (Atom atom in atoms)
        {
            GameObject n_atom = Instantiate(AtomSphere, atom.Position - center, Quaternion.identity);

            n_atom.transform.parent = this.transform;
            n_atom.GetComponent<Renderer>().material.color = atom.Colour;
            n_atom.transform.localScale *= atom.VDWRadius;

            atomGameObjects.Add(n_atom);
        }
    }
    */

    // Renders the given atoms as a VAN DER WALLS/CPK representation
    private void DrawVDW(Chain chain, List<int> selectedResidues)
    {
        int currentRes = 0;
        Vector3 modelCenter = this.GetCenterPointObject(this.transform.position);

        foreach (Residue residue in chain.chainResidues)
        {
            foreach (Atom atom in residue.resAtoms)
            {
                if (selectedResidues.Contains(currentRes))
                {
                    GameObject n_atom = Instantiate(AtomSphere, atom.Position - modelCenter, Quaternion.identity);

                    n_atom.transform.parent = this.transform;
                    n_atom.GetComponent<Renderer>().material.color = atom.Colour;
                    n_atom.transform.localScale *= 2 * atom.VDWRadius;
                    residue.residueGameObjects.Add(n_atom);
                }
            }
            currentRes += 1;
        }
    }

    /* OLD WIREFRAME
    private void RenderWireFrame(List<Atom> atoms)
    {
        Vector3 center = this.GetCenterPointObject(this.transform.position);

        float res = atoms[0].ResSeqNum;

        foreach (Atom atom in atoms)
        {
            GameObject n_atom = Instantiate(AtomSphere, atom.Position - center, Quaternion.identity);
            n_atom.transform.parent = this.transform;
            n_atom.transform.localScale = new Vector3(10f, 10f, 10f);
            n_atom.GetComponent<Renderer>().material.color = atom.Colour;
            // add to list of objects 
            atomGameObjects.Add(n_atom);
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
                        // instantiate 2 parts of the bond 
                        GameObject bondP1 = Instantiate(AtomBond, (bond_center + atoms[j].Position) / (float)2.0 - center, Quaternion.identity);
                        GameObject bondP2 = Instantiate(AtomBond, (bond_center + atoms[i].Position) / (float)2.0 - center, Quaternion.identity);
                        // set parent transform
                        bondP1.transform.parent = this.transform;
                        bondP2.transform.parent = this.transform;
                        // orient bond to look at closest atom

                        bondP1.transform.LookAt(atomGameObjects[i].transform, Vector3.up);
                        bondP2.transform.LookAt(atomGameObjects[j].transform, Vector3.up);
                        // change the bond's colour to the same as the closest atom's
                        bondP1.GetComponent<Renderer>().material.color = atomGameObjects[j].GetComponent<Renderer>().material.color;
                        bondP2.GetComponent<Renderer>().material.color = atomGameObjects[i].GetComponent<Renderer>().material.color;
                        // scale down the bonds to fit the specific inter-atom distance
                        bondP1.transform.localScale = new Vector3(10, 10, distance * 25);
                        bondP2.transform.localScale = new Vector3(10, 10, distance * 25);

                        bondGameObjects.Add(bondP1);
                        bondGameObjects.Add(bondP2);
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
                    bondP1.transform.LookAt(atomGameObjects[i].transform, Vector3.up);
                    bondP2.transform.LookAt(atomGameObjects[j].transform, Vector3.up);
                    // change the bond's colour to the same as the closest atom's
                    bondP1.GetComponent<Renderer>().material.color = atomGameObjects[j].GetComponent<Renderer>().material.color;
                    bondP2.GetComponent<Renderer>().material.color = atomGameObjects[i].GetComponent<Renderer>().material.color;
                    // scale down the bonds to fit the specific inter-atom distance
                    bondP1.transform.localScale = new Vector3(10, 10, distance * 25);
                    bondP2.transform.localScale = new Vector3(10, 10, distance * 25);

                    bondGameObjects.Add(bondP1);
                    bondGameObjects.Add(bondP2);
                }
            }
        }
    }
    */

    // Renders the given atoms as a WIREFRAME representation
    private void DrawWireFrame(Chain chain, List<int> selectedResidues)
    {
        int currentRes = 0;
        Vector3 modelCenter = this.GetCenterPointObject(this.transform.position);

        foreach (Residue residue in chain.chainResidues)
        {
            foreach (Atom atom in residue.resAtoms)
            {
                if (selectedResidues.Contains(currentRes))
                {
                    GameObject n_atom = Instantiate(AtomSphere, atom.Position - modelCenter, Quaternion.identity);
                    n_atom.transform.parent = this.transform;
                    n_atom.transform.localScale = new Vector3(10f, 10f, 10f);
                    n_atom.GetComponent<Renderer>().material.color = atom.Colour;
                    residue.residueGameObjects.Add(n_atom);

                    foreach (Atom neighbour in atom.neighbours)
                    {
                        if (neighbour.AtomSerial > atom.AtomSerial)
                        {
                            Vector3 bond_center = (atom.Position + neighbour.Position) / (float)2.0;
                            float distance = Vector3.Distance(atom.Position, neighbour.Position);

                            GameObject bondP1 = Instantiate(AtomBond, (bond_center + atom.Position) / (float)2.0 - modelCenter, Quaternion.identity);
                            GameObject bondP2 = Instantiate(AtomBond, (bond_center + neighbour.Position) / (float)2.0 - modelCenter, Quaternion.identity);
                            // set parent transform
                            bondP1.transform.parent = this.transform;
                            bondP2.transform.parent = this.transform;
                            // orient bond to look at closest atom
                            bondP1.transform.LookAt(atom.Position - modelCenter, Vector3.up);
                            bondP2.transform.LookAt(neighbour.Position - modelCenter, Vector3.up);
                            // change the bond's colour to the same as the closest atom's
                            bondP1.GetComponent<Renderer>().material.color = atom.Colour;
                            bondP2.GetComponent<Renderer>().material.color = neighbour.Colour;
                            // scale down the bonds to fit the specific inter-atom distance
                            bondP1.transform.localScale = new Vector3(10, 10, distance * 25);
                            bondP2.transform.localScale = new Vector3(10, 10, distance * 25);

                            residue.residueGameObjects.Add(bondP1);
                            residue.residueGameObjects.Add(bondP2);
                        }
                    }
                }
            }
            currentRes += 1;
        }
    }

    /* OLD TUBE
    private void RenderTube(List<Atom> atoms)
    {
        if (!atoms.Any() || (hasBackbone == false))
        {
            return;
        }

        AtomColours colours = new AtomColours();

        Vector3 center = this.GetCenterPointObject(this.transform.position);

        // store all curves as a sub list of 3D points that the curve should follow to concatinate into a spline
        List<List<Vector3>> curves = new List<List<Vector3>>
        {
            new List<Vector3>()
        };

        int currentRes = atoms.First().ResSeqNum;
        int currentCurve = 0;

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

        Vector3 prevSeg = curves[0][0];

        for (int j = 0; j < steps; j++)
        {
            Vector3 segmentPos = Bezier.GetPointQuad(curves[0][0], curves[0][1], curves[0][2], j / (float)steps);
            GameObject n_Tube = Instantiate(AtomBond, segmentPos - center, Quaternion.identity);
            n_Tube.transform.parent = transform;
            n_Tube.transform.LookAt(prevSeg);
            n_Tube.transform.localScale = new Vector3(10f, 10f, 15f);
            //n_Tube.transform.Rotate(Vector3.up, 90f);
            prevSeg = n_Tube.transform.position;
        }

        if (!(curves.Count == 1))
        {
            for (int i = 1; i < curves.Count; i++)
            {
                for (int j = 0; j < steps; j++)
                {
                    Vector3 segmentPos = Bezier.GetPointCube(curves[i - 1][2], curves[i - 1][2] + (curves[i - 1][2] - curves[i - 1][1]), curves[i][1], curves[i][2], j / (float)steps);
                    GameObject n_Tube = Instantiate(AtomBond, segmentPos - center, Quaternion.identity);
                    n_Tube.transform.parent = transform;
                    n_Tube.transform.LookAt(prevSeg);
                    n_Tube.transform.localScale = new Vector3(10f, 10f, 15f);
                    //n_Helix.transform.Rotate(Vector3.up, 90f);
                    prevSeg = n_Tube.transform.position;
                }
            }
        }
    }
     */

    // Render the given atoms as a TUBE representation
    private void DrawTube(Chain chain, List<int> selectedResidues)
    {
        Vector3 center = this.GetCenterPointObject(this.transform.position);
        List<List<Vector3>> curves = GetCurves(chain);

        Vector3 prevSegment = curves[0][0];
        for (int i = 0; i < curves.Count; i++)
        {
            if (selectedResidues.Contains(i))
            {
                for(int j = 0; j < steps; j++)
                {
                    Vector3 segmentPosition;
                    if (i == 0)
                    {
                        segmentPosition = Bezier.GetPointQuad(curves[0][0], curves[0][1], curves[0][2], j / (float)steps);
                    }
                    else
                    {
                        segmentPosition = Bezier.GetPointCube(curves[i - 1][2], curves[i - 1][2] + (curves[i - 1][2] - curves[i - 1][1]), curves[i][1], curves[i][2], j / (float)steps);
                    }
                    GameObject n_Tube = Instantiate(AtomBond, segmentPosition - center, Quaternion.identity);
                    n_Tube.transform.parent = transform;
                    n_Tube.transform.LookAt(prevSegment);
                    n_Tube.transform.localScale = new Vector3(10f, 10f, 15f);
                    prevSegment = n_Tube.transform.position;
                    // NB There are as many curves as there are residues, so use i as the residue index!
                    chain.chainResidues[i].residueGameObjects.Add(n_Tube);
                }
            }
        }
    }

    // Renders the given atoms as a CARTOON representation
    private void RenderCartoon(List<Atom> chain)
    {
        if ((hasBackbone == false) || (hasSecStruct == false))
        {
            Debug.Log("exiting");
            return;
        }

        Vector3 center = this.GetCenterPointObject(this.transform.position);

        // create list of structure (list of atoms that form secondary structure)
        List<List<Atom>> structures = new List<List<Atom>>
        {
            new List<Atom>()
        };

        SecondaryStructure currentStructure = chain[0].SecStructInf;
        int index = 0;

        foreach(Atom atom in chain)
        {
            // ignore non backbone atoms
            if (!atom.IsBackbone)
            {
                continue;
            }

            if (atom.SecStructInf == currentStructure)
            {
                structures[index].Add(atom);
            }
            else
            {
                structures.Add(new List<Atom> { atom });
                currentStructure = atom.SecStructInf;
                index += 1;
            }
        }


        List<Vector3> prevCurve = new List<Vector3>
        {
            structures[0][0].Position,
            structures[0][1].Position,
            structures[0][2].Position
        };

        bool isFirst = true;


        // render each secondary structure depending on type
        foreach(List<Atom> list in structures)
        {
            Vector3 prevSeg = Vector3.zero;

            switch (list[0].SecStructInf)
            {


                case SecondaryStructure.Other:

                    List<List<Vector3>> curves = GetCurves(list);

                    if (prevSeg.Equals(Vector3.zero))
                    {
                        prevSeg = curves[0][0];
                    }

                    Vector3 prevSegTube = curves[0][0];

                    if (isFirst)
                    {
                        for (int j = 0; j < steps; j++)
                        {
                            Vector3 segmentPos = Bezier.GetPointQuad(curves[0][0], curves[0][1], curves[0][2], j / (float)steps);
                            GameObject n_Tube = Instantiate(AtomBond, segmentPos - center, Quaternion.identity);
                            n_Tube.transform.parent = transform;
                            n_Tube.transform.LookAt(prevSegTube);
                            if (j == 0)
                            {
                                n_Tube.transform.LookAt(prevSeg);
                            }
                            n_Tube.transform.localScale = new Vector3(10f, 10f, 15f);
                            //n_Helix.transform.Rotate(Vector3.up, 90f);
                            prevSegTube = n_Tube.transform.position;
                        }
                        isFirst = false;
                    }
                    else
                    {
                        for (int j = 0; j < steps; j++)
                        {
                            Vector3 segmentPos = Bezier.GetPointCube(prevCurve[2], prevCurve[2] + (prevCurve[2] - prevCurve[1]), curves[0][1], curves[0][2], j / (float)steps);
                            GameObject n_Tube = Instantiate(AtomBond, segmentPos - center, Quaternion.identity);
                            n_Tube.transform.parent = transform;
                            n_Tube.transform.LookAt(prevSegTube);
                            n_Tube.transform.localScale = new Vector3(10f, 10f, 15f);
                            //n_Helix.transform.Rotate(Vector3.up, 90f);
                            prevSegTube = n_Tube.transform.position;
                        }
                    }


                    if (curves.Count > 1)
                    {
                        for (int i = 1; i < curves.Count; i++)
                        {
                            for (int j = 0; j < steps; j++)
                            {
                                Vector3 segmentPos = Bezier.GetPointCube(curves[i - 1][2], curves[i - 1][2] + (curves[i - 1][2] - curves[i - 1][1]), curves[i][1], curves[i][2], j / (float)steps);
                                GameObject n_Tube = Instantiate(AtomBond, segmentPos - center, Quaternion.identity);
                                n_Tube.transform.parent = transform;
                                n_Tube.transform.LookAt(prevSegTube);
                                n_Tube.transform.localScale = new Vector3(10f, 10f, 15f);
                                //n_Helix.transform.Rotate(Vector3.up, 90f);
                                prevSegTube = n_Tube.transform.position;
                            }
                        }
                    }
                    prevCurve = curves.Last();
                    break;



                case SecondaryStructure.AlphaHelix:

                    // initialise curves list
                    List<List<Vector3>> curvesAlpha = GetCurves(list);

                    // why this?
                    if (curvesAlpha.Count == 1)
                    {
                        break;
                    }

                    // axis vector of helix 
                    Vector3 helixAxis = list.Last().Position - list.First().Position;
                    // plane to compute origin of axis 
                    Plane helixPlane = new Plane(helixAxis.normalized, list.First().Position);

                    // FOR DEBUG, COMMENT OUT
                    /*
                    GameObject helixPlaneObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    helixPlaneObj.transform.position = list.First().Position - center;
                    helixPlaneObj.transform.LookAt(list.Last().Position - center);
                    helixPlaneObj.transform.Rotate(Vector3.right, 90f);
                    helixPlaneObj.transform.parent = transform;
                     */

                    List<Vector3> projections = new List<Vector3>();

                    // get all the projected position of atoms on helix plane 
                    foreach (Atom atom in list)
                    {
                        projections.Add(helixPlane.ClosestPointOnPlane(atom.Position));
                    }

                    // FOR DEBUG, COMMENT OUT
                    /*
                    foreach (Vector3 point in projections)
                    {
                        GameObject projObj = Instantiate(AtomSphere, point - center, Quaternion.identity);
                        projObj.transform.localScale = new Vector3(10f, 10f, 10f);
                        projObj.transform.parent = transform;
                        projObj.GetComponent<Renderer>().material.color = Color.yellow;
                    }
                     */

                    // avergae mid point of all projected points is new axis origin
                    Vector3 avg = Vector3.zero;
                    foreach (Vector3 point in projections)
                    {
                        avg += point;
                    }

                    // FOR DEBUG, COMMENT OUT
                    /*
                    GameObject projAvg = Instantiate(AtomSphere, avg / projections.Count - center, Quaternion.identity);
                    projAvg.transform.parent = transform;
                    projAvg.transform.localScale = new Vector3(30f, 30f, 30f);
                    projAvg.transform.LookAt(helixAxis + avg/projections.Count - center);
                    projAvg.GetComponent<Renderer>().material.color = Color.blue;
                    for (int i = 1; i < 10; i++)
                    {
                        GameObject axis = Instantiate(AtomBond, avg / projections.Count + (i * helixAxis / 10) - center, Quaternion.identity);
                        axis.transform.parent = transform;
                        axis.transform.LookAt(avg / projections.Count - center);
                        axis.GetComponent<Renderer>().material.color = Color.blue;
                        axis.transform.localScale = new Vector3(10f, 10f, 80f);
                    }

                     */
                    // draw the first residue only containing three atoms
                    GameObject axisOrigin = Instantiate(AtomSphere, avg / projections.Count - center, Quaternion.identity);
                    axisOrigin.transform.LookAt(avg / projections.Count - helixAxis - center);
                    axisOrigin.transform.rotation *= Quaternion.Euler(-90, 0, 0);
                    Quaternion rotation = axisOrigin.transform.rotation;

                    if (prevSeg.Equals(Vector3.zero))
                    {
                        prevSeg = curvesAlpha[0][0];
                    }

                    Vector3 prevSegAlpha = curvesAlpha[0][0];

                    if (isFirst)
                    {
                        for (int j = 0; j < steps; j++)
                        {
                            Vector3 segmentPos = Bezier.GetPointQuad(curvesAlpha[0][0], curvesAlpha[0][1], curvesAlpha[0][2], j / (float)steps);
                            GameObject n_Helix = Instantiate(HelixSide, segmentPos - center, rotation);
                            n_Helix.transform.parent = transform;
                            prevSegAlpha = n_Helix.transform.position;
                        }
                    }
                    else
                    {
                        for (int j = 0; j < steps; j++)
                        {
                            Vector3 segmentPos = Bezier.GetPointCube(prevCurve[2], prevCurve[2] +
                                (prevCurve[2] - prevCurve[1]), curvesAlpha[0][1], curvesAlpha[0][2], j / (float)steps);
                            GameObject n_Helix = Instantiate(HelixSide, segmentPos - center, rotation);
                            n_Helix.transform.parent = transform;
                            prevSegAlpha = n_Helix.transform.position;
                        }
                    }

                    if (curvesAlpha.Count > 1)
                    {
                        for (int i = 1; i < curvesAlpha.Count; i++)
                        {
                            for (int j = 0; j < steps; j++)
                            {
                                Vector3 segmentPos = Bezier.GetPointCube(curvesAlpha[i - 1][2], curvesAlpha[i - 1][2] +
                                    (curvesAlpha[i - 1][2] - curvesAlpha[i - 1][1]), curvesAlpha[i][1], curvesAlpha[i][2], j / (float)steps);
                                GameObject n_Helix = Instantiate(HelixSide, segmentPos - center, rotation);
                                n_Helix.transform.parent = transform;
                                prevSegAlpha = n_Helix.transform.position;
                            }
                        }
                    }

                    Destroy(axisOrigin);
                    prevCurve = curvesAlpha.Last();
                    break;



                case SecondaryStructure.BetaSheet:

                    int currentResidueBeta = list[0].ResSeqNum;
                    index = 0;

                    List<List<Vector3>> curvesBeta = GetCurves(list);

                    if (curvesBeta.Count == 1)
                    {
                        break;
                   }

                    if (prevSeg.Equals(Vector3.zero))
                    {
                        prevSeg = curvesBeta[0][0];
                    }

                    // draw the first residue only containing three atoms
                    Vector3 prevSegBeta = curvesBeta[0][0];

                    if (isFirst)
                    {
                        for (int j = 0; j < steps; j++)
                        {
                            Vector3 segmentPos = Bezier.GetPointQuad(curvesBeta[0][0], curvesBeta[0][1], curvesBeta[0][2], j / (float)steps);
                            GameObject n_Sheet = Instantiate(SheetSide, segmentPos - center, Quaternion.identity);
                            n_Sheet.transform.parent = transform;
                            n_Sheet.transform.LookAt(prevSegBeta);
                            prevSegBeta = n_Sheet.transform.position;
                        }

                    }
                    else
                    {
                        for (int j = 0; j < steps; j++)
                        {
                            Vector3 segmentPos = Bezier.GetPointCube(prevCurve[2], prevCurve[2] + (prevCurve[2] - prevCurve[1]), curvesBeta[0][1], curvesBeta[0][2], j / (float)steps);
                            GameObject n_Sheet = Instantiate(SheetSide, segmentPos - center, Quaternion.identity);
                            n_Sheet.transform.parent = transform;
                            n_Sheet.transform.LookAt(prevSegBeta);
                            prevSegBeta = n_Sheet.transform.position;
                        }


                    }

                    for (int i = 1; i < curvesBeta.Count; i++)
                    {
                        for (int j = 0; j < steps; j++)
                        {
                            Vector3 segmentPos = Bezier.GetPointCube(curvesBeta[i-1][2], curvesBeta[i-1][2] +
                                (curvesBeta[i-1][2] - curvesBeta[i-1][1]), curvesBeta[i][1], curvesBeta[i][2], j / (float)steps);
                            
                            GameObject n_Sheet = Instantiate(SheetSide, segmentPos - center, Quaternion.identity);
                            n_Sheet.transform.parent = transform;
                            n_Sheet.transform.LookAt(prevSegBeta);
                            prevSegBeta = n_Sheet.transform.position;

                        }
                    }

                    prevCurve = curvesBeta.Last();
                    break;

            }
        }
    }

    private void DrawCartoon(Chain chain, List<int> selectedResidues)
    {
        Vector3 center = this.GetCenterPointObject(this.transform.position);

        //List<List<Vector3>> structureCurves;

    }
    
    // Utility function for the cartoon method, divides the list of atoms into curves
    private List<List<Vector3>> GetCurves(List<Atom> atoms)
    {
        // initialise the first curve in list
        List<List<Vector3>> curves = new List<List<Vector3>>
        {
        new List<Vector3>()
        };

        // initialise indices
        // index of residue of each atom
        int currRes = atoms[0].ResSeqNum;
        // index of curve
        int curveIndex = 0;

        // create the sub lists of the positions of the atoms within a residue (1 list = 1 residue = 1 curve)
        foreach (Atom atom in atoms)
        {
            // part of same residue, add to current curve
            if (currRes == atom.ResSeqNum)
            {
                curves[curveIndex].Add(atom.Position);
            }
            // reached a new residue, create a new curve
            if (currRes < atom.ResSeqNum)
            {
                curves.Add(new List<Vector3>());
                curveIndex += 1;
                curves[curveIndex].Add(atom.Position);
                currRes = atom.ResSeqNum;
            }
        }

        return curves;
    }

    // returns the backbone atoms of each residue, used as the influence points of each best fit curve for tube and cartoon
    private List<List<Vector3>> GetCurves(Chain chain)
    {
        List<List<Vector3>> curves = new List<List<Vector3>>();

        foreach (Residue residue in chain.chainResidues)
        {
            List<Vector3> curve = new List<Vector3>();
            foreach (Atom atom in residue.resAtoms)
            {
                if (atom.IsBackbone)
                {
                    curve.Add(atom.Position);
                }
            }

            if (curve.Any())
            {
                curves.Add(curve);
            }
        }
        return curves;
    }
    
    //                                                        //
    //      Methods for user input and Model manipulation     //
    //                                                        //

    // Use mouse input to detect how to rotate the model
    void RotateModel()
    {
        mousePosDelta = Input.mousePosition - mousePrevPos;

        // Using the dot product, which returns a float, indicates by how much the model should be rotated around the y and x axes!
        // No need to compute a separate z as a combination of x and y rotations results in a z rotation

        // for rotation around the model's y axis, we need to check dot product of the model's y axis with world y axis
        // and invert the rotation if they are in the opposite direction
        if(Vector3.Dot(transform.up, Vector3.up) >= 0)
        {
            transform.Rotate(transform.up, -Vector3.Dot(mousePosDelta, Camera.main.transform.right), Space.World);
        }
        else
        {
            transform.Rotate(transform.up, Vector3.Dot(mousePosDelta, Camera.main.transform.right), Space.World);
        }

        transform.Rotate(Camera.main.transform.right, Vector3.Dot(mousePosDelta, Camera.main.transform.up), Space.World);
    }

    void ScaleModel()
    {
        mousePosDelta = Input.mousePosition - mousePrevPos;

        Vector3 modelScale = transform.localScale;
        
        if (Vector3.Dot(mousePosDelta, Camera.main.transform.up) > 0)
        {
            transform.localScale = Vector3.Min(modelScale * Mathf.Max(Vector3.Dot(mousePosDelta, Camera.main.transform.up), 1), maxScale);
        }
        else
        {
            transform.localScale = Vector3.Max(modelScale / Mathf.Max(Math.Abs(Vector3.Dot(mousePosDelta, Camera.main.transform.up)), 1), minScale);
        }
    }
}
