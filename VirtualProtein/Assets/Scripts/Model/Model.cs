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

    // fields for comunication wuth UI
    public static string PdbPath { get; set; }
    public static string DsspPath { get; set; }

    public static bool showModel = false;
    public static bool newModel = false;
    public static bool newRep = false;
    public static bool hasSelected = false;
    public static bool hasBackbone = false;
    public static bool hasSecStruct = false;
    public static bool hasDssp = false;

    // reference to the game object with the scripts that display the model's info and interact with player
    public GameObject UI;

    // references to the game object models that are used by the representationsn 
    public GameObject AtomSphere;
    public GameObject AtomBond;
    public GameObject HelixSide;
    public GameObject SheetSide;

    // references to the scene components that handle movement of camera in the scene
    public CharacterController controller;
    public Transform playerPosition;

    // fields containing protein model 
    public static List<Chain> chains = new List<Chain>();
    public static List<Representation> representations = new List<Representation>();

    public static int AtomCount { get; set; }
    public static int BondCount { get; set; }
    public static int ResidueCount { get; set; }
    
    // Fields for User Input
    private Vector3 mousePrevPos = Vector3.zero;
    private Vector3 mousePosDelta = Vector3.zero;
    private Vector3 minScale = new Vector3(0.01f, 0.01f, 0.01f);
    private Vector3 maxScale = new Vector3(100f, 100f, 100f);
    private const float speed = 20f;

    // Extra field for representation specifics
    private const int steps = 20;


    //                   //
    //      UPDATE       //
    //                   //

    private void Update()
    {
        
        if (!showModel == false)
        {
            if (newModel == true)
            {
                // destroy the previous model if it exists
                if (chains.Any())
                {
                    DestroyModelObjects();
                }

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
                SelectRepresentation.hasNewModel = true;
                    
                // intitialse representations
                representations.Clear();
                
                newModel = false;
            }

            if (hasDssp == true)
            {
                GetDsspData(DsspPath);
                hasDssp = false;
            }

            if (newRep)
            {
                representations.Add(GetRepresentation());

                // save the model's current transform
                Vector3 modelPosition = transform.position;
                Vector3 modelScale = transform.localScale;
                Quaternion modelRotation = transform.rotation;

                // initialise the transform
                ResetTransform();

                // Instantiate the representation and place it at the origin of the scene
                InstantiateRepresentation(GetRepresentation());
                CenterRepresentation(representations.Count - 1);

                // return the model to its original transform
                transform.position = modelPosition;
                transform.localScale = modelScale;
                transform.rotation = modelRotation;

                // tell the UI to update 
                SelectionHandler.initSelection = true;
                SelectRepresentation.createdNewRep = true;
                newRep = false;
            }

            if (hasSelected)
            {
                if (SelectRepresentation.selectedRepresentationStatus)
                {
                    ShowRepresentation(SelectRepresentation.selectedRepresentationIndex);
                }
                else
                {
                    HideRepresentation(SelectRepresentation.selectedRepresentationIndex);
                }
                hasSelected = false;
            }

            if (ShowHideUI.showUI)
            {
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");

                Vector3 movement = playerPosition.right * horizontal + playerPosition.forward * vertical;

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    movement = playerPosition.up * vertical;
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

            if (Input.GetKey(KeyCode.C))
            {
                Quaternion modelRotation = transform.rotation;
                transform.position = playerPosition.transform.position;
                transform.rotation = playerPosition.rotation;

                transform.Translate(new Vector3(0f, 0f, 30f), Space.Self);

                transform.rotation = modelRotation;
            }

            mousePrevPos = Input.mousePosition;
        }
    }

    //                                //
    //         USEFUL METHODS         //
    //                                //

    // Destroy all the gameobjects associated to the model
    public void DestroyModelObjects()
    {
        foreach(Chain chain in chains)
        {
            foreach(Residue residue in chain.chainResidues)
            {
                foreach(List<GameObject> residueRepresentation in residue.residueGameObjects)
                {
                    foreach(GameObject representationGameObject in residueRepresentation)
                    {
                        Destroy(representationGameObject);
                    }
                }
            }
        }
    }

    // adds all the atoms and their data to the Model fields from a PDB file
    public void GetPdbData(string path)
    {
        chains  = FileParser.ParsePDB(path);
        
        // if for whatever reason the algorithm can't generate a backbone, notify the user and don't render tube and cartoon for that model

        foreach(Chain chain in chains)
        {
            try
            {
                FileParser.FindBackbone(chain);
                hasBackbone = true;
            }
            catch (System.Exception)
            {
                hasBackbone = false;
            }

            BondCount += FileParser.GetBondCount(chain);
            ResidueCount += chain.chainResidues.Count;

            foreach(Residue residue in chain.chainResidues)
            {
                AtomCount += residue.resAtoms.Count;
            }
        }
    }
    
    // resets the transfom of the model
    public void ResetTransform()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    // adds the secondary structure information to each residue
    private void GetDsspData(string path)
    {
        List<SecondaryStructure> fileData = FileParser.ParseDSSP(chains, path, ResidueCount);
        if (FileParser.DSSPstatus)
        {
            SetStructureInfo(fileData);
            hasSecStruct = true;
        }
        Debug.Log(hasSecStruct);
    }

    // sets all the secondary structure field of the residues of the model to their correct secondary structure
    private void SetStructureInfo(List<SecondaryStructure> infoList)
    {
        // info list should not be longer than the residue count 
        if (ResidueCount == infoList.Count)
        {
            int index = 0;
            foreach(Chain chain in chains)
            {
                foreach(Residue residue in chain.chainResidues)
                {
                    residue.ResStructureInf = infoList[index];
                    index += 1;
                }
            }
        }
    }

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

    // returns the point at the centre of the protein
    public Vector3 GetProteinCentre()
    {
        Vector3 centre = Vector3.zero;
        int count = 0;
        foreach(Chain chain in chains)
        {
            foreach(Residue residue in chain.chainResidues)
            {
                foreach(Atom atom in residue.resAtoms)
                {
                    centre += atom.Position;
                    count += 1;
                }
            }
        }

        return centre / count;
    }

    //                                  //
    //      REPRESENTATION METHODS      //
    //                                  //

    // Gets the representation data from UI
    public Representation GetRepresentation()
    {
        return new Representation(SelectionHandler.selectedResidues, (RepresentationType)CreateRepresentation.SelectedRepType, true);
    }

    // set the representation's gameobjects to inactive
    public void HideRepresentation(int representationIndex)
    {
        foreach(Chain chain in chains)
        {
            foreach(Residue residue in chain.chainResidues)
            {
                foreach(GameObject representationGameObject in residue.residueGameObjects[representationIndex])
                {
                    representationGameObject.SetActive(false);
                }
            }
        }
    }

    // set the representation's gameobject to active
    public void ShowRepresentation(int representationIndex)
    {
        foreach (Chain chain in chains)
        {
            foreach (Residue residue in chain.chainResidues)
            {
                foreach (GameObject representationGameObject in residue.residueGameObjects[representationIndex])
                {
                    representationGameObject.SetActive(true);
                }
            }
        }
    }

    // Iterate through list of representations and render accordingly
    public void InstantiateRepresentation(Representation representation)
    {
        if (representation.IsDisplayed)
        {
            for(int i = 0; i < chains.Count; i++)
            {
                switch (representation.repType)
                {
                    case RepresentationType.VanDerWalls:
                        InstantiateVDW(chains[i], representation.residueIndices[i]);
                        break;
                    case RepresentationType.BallAndStick:
                        InstantiateBallAndStick(chains[i], representation.residueIndices[i]);
                        break;
                    case RepresentationType.WireFrame:
                        InstantiateWireFrame(chains[i], representation.residueIndices[i]);
                        break;
                    case RepresentationType.Tube:
                        InstantiateTube(chains[i], representation.residueIndices[i]);
                        break;
                    case RepresentationType.Cartoon:
                        InstantiateCartoon(chains[i], representation.residueIndices[i]);
                        break;
                }
            }
        }
    }
    
    // Centers the model around the origin
    public void CenterRepresentation(int representationIndex)
    {
        // get the centre point
        Vector3 centre = GetProteinCentre();

        foreach(Chain chain in chains)
        {
            foreach(Residue residue in chain.chainResidues)
            {
                foreach(GameObject residueObject in residue.residueGameObjects[representationIndex])
                {
                    residueObject.transform.position -= centre;
                }
            }
        }
    }

    //                                        //
    //         MODELLING METHODS              //
    //                                        //
    
    // Renders the atoms in a chain as a BALL AND STICK representation
    private void InstantiateBallAndStick(Chain chain, List<int> selectedResidues)
    {
        int currentRes = 0;
        foreach (Residue residue in chain.chainResidues)
        {
            List<GameObject> newRepresentationObjects = new List<GameObject>();
            foreach(Atom atom in residue.resAtoms)
            {
                if(selectedResidues.Contains(currentRes))
                {
                    GameObject n_atom = Instantiate(AtomSphere, atom.Position, Quaternion.identity, transform);
                    n_atom.GetComponent<Renderer>().material.color = atom.Colour;
                    newRepresentationObjects.Add(n_atom);

                    foreach(Atom neighbour in atom.neighbours)
                    {
                        if (neighbour.AtomSerial > atom.AtomSerial)
                        {
                            Vector3 bond_center = (atom.Position + neighbour.Position) / (float)2.0;
                            float distance = Vector3.Distance(atom.Position, neighbour.Position);

                            GameObject bondP1 = Instantiate(AtomBond, (bond_center + atom.Position) / (float)2.0, Quaternion.identity, transform);
                            GameObject bondP2 = Instantiate(AtomBond, (bond_center + neighbour.Position) / (float)2.0, Quaternion.identity, transform);
                            // orient bond to look at closest atom
                            bondP1.transform.LookAt(atom.Position, Vector3.up);
                            bondP2.transform.LookAt(neighbour.Position, Vector3.up);
                            // change the bond's colour to the same as the closest atom's
                            bondP1.GetComponent<Renderer>().material.color = atom.Colour;
                            bondP2.GetComponent<Renderer>().material.color = neighbour.Colour;
                            // scale down the bonds to fit the specific inter-atom distance
                            bondP1.transform.localScale = new Vector3(10, 10, distance * 25);
                            bondP2.transform.localScale = new Vector3(10, 10, distance * 25);

                            newRepresentationObjects.Add(bondP1);
                            newRepresentationObjects.Add(bondP2);
                        }
                    }
                }
            }
            residue.residueGameObjects.Add(newRepresentationObjects);
            currentRes += 1;
        }
    }
    
    // Renders the given atoms as a VAN DER WALLS/CPK representation
    private void InstantiateVDW(Chain chain, List<int> selectedResidues)
    {
        int currentRes = 0;

        foreach (Residue residue in chain.chainResidues)
        {
            List<GameObject> newRepresentationObjects = new List<GameObject>();
            foreach (Atom atom in residue.resAtoms)
            {
                if (selectedResidues.Contains(currentRes))
                {
                    GameObject n_atom = Instantiate(AtomSphere, atom.Position, Quaternion.identity, transform);

                    n_atom.GetComponent<Renderer>().material.color = atom.Colour;

                    //n_atom.transform.localScale = n_atom.transform.parent.localScale;
                    n_atom.transform.localScale *= 2 * atom.VDWRadius;

                    newRepresentationObjects.Add(n_atom);

                }
            }
            residue.residueGameObjects.Add(newRepresentationObjects);
            currentRes += 1;
        }
    }

    // Renders the given atoms as a WIREFRAME representation
    private void InstantiateWireFrame(Chain chain, List<int> selectedResidues)
    {
        int currentRes = 0;
       
        foreach (Residue residue in chain.chainResidues)
        {
            List<GameObject> newRepresentationObjects = new List<GameObject>();
            foreach (Atom atom in residue.resAtoms)
            {
                if (selectedResidues.Contains(currentRes))
                {
                    GameObject n_atom = Instantiate(AtomSphere, atom.Position, Quaternion.identity, transform);
                    n_atom.transform.localScale = new Vector3(10f, 10f, 10f);
                    n_atom.GetComponent<Renderer>().material.color = atom.Colour;
                    newRepresentationObjects.Add(n_atom);

                    foreach (Atom neighbour in atom.neighbours)
                    {
                        if (neighbour.AtomSerial > atom.AtomSerial)
                        {
                            Vector3 bond_center = (atom.Position + neighbour.Position) / (float)2.0;
                            float distance = Vector3.Distance(atom.Position, neighbour.Position);

                            GameObject bondP1 = Instantiate(AtomBond, (bond_center + atom.Position) / (float)2.0, Quaternion.identity, transform);
                            GameObject bondP2 = Instantiate(AtomBond, (bond_center + neighbour.Position) / (float)2.0, Quaternion.identity, transform);
                            // orient bond to look at closest atom
                            bondP1.transform.LookAt(atom.Position, Vector3.up);
                            bondP2.transform.LookAt(neighbour.Position, Vector3.up);
                            // change the bond's colour to the same as the closest atom's
                            bondP1.GetComponent<Renderer>().material.color = atom.Colour;
                            bondP2.GetComponent<Renderer>().material.color = neighbour.Colour;
                            // scale down the bonds to fit the specific inter-atom distance
                            bondP1.transform.localScale = new Vector3(10, 10, distance * 25);
                            bondP2.transform.localScale = new Vector3(10, 10, distance * 25);

                            newRepresentationObjects.Add(bondP1);
                            newRepresentationObjects.Add(bondP2);
                        }
                    }
                }
            }
            residue.residueGameObjects.Add(newRepresentationObjects);
            currentRes += 1;
        }
    }

    // Render the given atoms as a TUBE representation
    private void InstantiateTube(Chain chain, List<int> selectedResidues)
    {
        List<List<Vector3>> curves = GetCurves(chain);

        Vector3 prevSegment = curves[0][0];
        for (int i = 0; i < curves.Count; i++)
        {
            List<GameObject> newRepresentationObjects = new List<GameObject>();
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
                    GameObject n_tube = Instantiate(AtomBond, segmentPosition, Quaternion.identity, transform);
                    n_tube.transform.LookAt(prevSegment);
                    n_tube.transform.localScale = new Vector3(10f, 10f, 15f);
                    prevSegment = n_tube.transform.position;
                    // NB There are as many curves as there are residues, so use i as the residue index!
                    newRepresentationObjects.Add(n_tube);
                }
            }
            chain.chainResidues[i].residueGameObjects.Add(newRepresentationObjects);
        }
    }

    // Renders the given atoms as a CARTOON representation
    private void RenderCartoon(List<Atom> chain)
    {
        if ((hasBackbone == false) || (hasSecStruct == false))
        {
            return;
        }
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
                            GameObject n_Tube = Instantiate(AtomBond, segmentPos , Quaternion.identity);
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
                            GameObject n_Tube = Instantiate(AtomBond, segmentPos, Quaternion.identity);
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
                                GameObject n_Tube = Instantiate(AtomBond, segmentPos, Quaternion.identity);
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
                    GameObject axisOrigin = Instantiate(AtomSphere, avg / projections.Count, Quaternion.identity);
                    axisOrigin.transform.LookAt(avg / projections.Count - helixAxis );
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
                            GameObject n_Helix = Instantiate(HelixSide, segmentPos , rotation, transform);
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
                            GameObject n_Helix = Instantiate(HelixSide, segmentPos , rotation);
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
                                GameObject n_Helix = Instantiate(HelixSide, segmentPos , rotation);
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
                            GameObject n_Sheet = Instantiate(SheetSide, segmentPos , Quaternion.identity);
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
                            GameObject n_Sheet = Instantiate(SheetSide, segmentPos , Quaternion.identity);
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
                            
                            GameObject n_Sheet = Instantiate(SheetSide, segmentPos , Quaternion.identity);
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

    private void InstantiateCartoon(Chain chain, List<int> selectedResidues)
    {
        // convert chain into curves
        List<List<Vector3>> curves = GetCurves(chain);
        
        // iterate over the curves, ignoring those that refer to unselected residues
        Vector3 prevSegment = curves[0][0];
        for (int i = 0; i < curves.Count; i++)
        {
            List<GameObject> newRepresentationObjects = new List<GameObject>();
            if (selectedResidues.Contains(i))
            {
                switch (chain.chainResidues[i].ResStructureInf)
                {
                    case SecondaryStructure.AlphaHelix:
                    // get the axis of the current residue and create a plane with axis as normal
                    Vector3 axis = curves[i][2] - curves[i][0];
                    Plane axisPlane = new Plane(axis.normalized, curves[i][0]);
                    // We need to place the axis at the average point of the curve's atoms projections onto the plane
                    Vector3 avgPoint = (axisPlane.ClosestPointOnPlane(curves[i][0]) + axisPlane.ClosestPointOnPlane(curves[i][1]) + axisPlane.ClosestPointOnPlane(curves[i][2])) / 3 ;
                    
                    break;

                    case SecondaryStructure.BetaSheet:
                    // render a sheet (tube with sheet gameobjects instead of cylindres)
                    for (int j = 0; j < steps; j++)
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

                        GameObject n_sheet = Instantiate(SheetSide, segmentPosition, Quaternion.identity, transform);
                        n_sheet.transform.LookAt(prevSegment);
                        prevSegment = n_sheet.transform.position;
                        // NB There are as many curves as there are residues, so use i as the residue index!
                        newRepresentationObjects.Add(n_sheet);
                    }
                    break;

                    case SecondaryStructure.Other:
                    // render a tube
                    for (int j = 0; j < steps; j++)
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

                        GameObject n_tube = Instantiate(AtomBond, segmentPosition, Quaternion.identity, transform);
                        n_tube.transform.LookAt(prevSegment);
                        n_tube.transform.localScale = new Vector3(10f, 10f, 15f);
                        prevSegment = n_tube.transform.position;
                        // NB There are as many curves as there are residues, so use i as the residue index!
                        newRepresentationObjects.Add(n_tube);
                    }
                    break;
                }
            }
            // add list of gameobjects (empty is completely fine) to the residue's list
            chain.chainResidues[i].residueGameObjects.Add(newRepresentationObjects);
        }
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

    // return the curve of a residue
    private List<Vector3> GetCurve(Residue residue)
    {
        List<Vector3> curve = new List<Vector3>();
        foreach(Atom atom in residue.resAtoms)
        {
            if (atom.IsBackbone)
            {
                curve.Add(atom.Position);
            }
        }
        return curve;
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
    //         USER INPUT AND MODEL TRANSFORM METHODS         //
    //                                                        //

    // Use mouse input to detect how to rotate the model
    void RotateModel()
    {
        mousePosDelta = Input.mousePosition - mousePrevPos;

        // Using the dot product, which returns a float, indicates by how much the model should be rotated around the y and x axes!
        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.Rotate(Camera.main.transform.forward, Vector3.Dot(mousePosDelta, Camera.main.transform.up), Space.World);
        }
        else
        {
            transform.Rotate(Vector3.up, -Vector3.Dot(mousePosDelta, Camera.main.transform.right), Space.World);

            transform.Rotate(Camera.main.transform.right, Vector3.Dot(mousePosDelta, Camera.main.transform.up), Space.World);

            // for rotation around the model's y axis, we need to check dot product of the model's y axis with world y axis
            // and invert the rotation if they are in the opposite direction
            /*
            if (Vector3.Dot(transform.up, Vector3.up) >= 0)
            {
                transform.Rotate(Vector3.up, -Vector3.Dot(mousePosDelta, Camera.main.transform.right), Space.World);
            }
            else
            {
            }
             */
        }

    }

    // Use mouse to detect hot to scale the model
    void ScaleModel()
    {
        // get the position of the mouse as a vector
        mousePosDelta = Input.mousePosition - mousePrevPos;

        Vector3 modelScale = transform.localScale;
        
        // compare dot product, positive means in same direction as the camera's y axis otherwise opposite, ignore 0.
        if (Vector3.Dot(mousePosDelta, Camera.main.transform.up) > 0)
        {
            transform.localScale = Vector3.Min(modelScale * Mathf.Max(Vector3.Dot(mousePosDelta, Camera.main.transform.up) * 0.5f, 1), maxScale);
        }
        else if (Vector3.Dot(mousePosDelta, Camera.main.transform.up) < 0)
        {
            transform.localScale = Vector3.Max(modelScale / Mathf.Max(Math.Abs(Vector3.Dot(mousePosDelta, Camera.main.transform.up) * 0.5f), 1), minScale);
        }
    }
}
