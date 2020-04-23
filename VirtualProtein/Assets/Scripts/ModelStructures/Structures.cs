using System;
using System.Linq;

using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;


//
// The idea is to create structure classes that build into each other
//
//  |----> Model (one or more chains)
//  |---------> chain (one or more secondary structures)
//  |--------------> Structure (one or more residues)
//  |-------------------> Residue (various nb of atoms for each amino acid)
//  |------------------------> Atom
//
//
namespace Structures
{
    public enum SecondaryStructure
    {
        Other,
        AlphaHelix,
        BetaSheet
    };

    public enum RepresentationType
    {
        VanDerWalls,
        Tube,
        BallAndStick,
        WireFrame,
        Cartoon
    };

    public enum ColourScheme
    {
        ByAtomType,
        ByResidueType,
        ByStructure
    };

    public class StructureColours
    {
        public Dictionary<SecondaryStructure, Color> structureColours;

        public StructureColours()
        {
            structureColours = new Dictionary<SecondaryStructure, Color>
            {
                {SecondaryStructure.AlphaHelix, Color.red },
                {SecondaryStructure.BetaSheet, Color.blue },
                {SecondaryStructure.Other, Color.green }
            };
        }
    }

    public class AminoColours
    {
        public Dictionary<string, Color> aminoColours;

        public AminoColours()
        {
            this.aminoColours = new Dictionary<string, Color>
            {
                { "ASP", new Color(230, 230, 10) },
                { "GLU", new Color(230, 230, 10) },
                { "CYS", Color.yellow            },
                { "MET", Color.yellow            },
                { "LYS", Color.blue              },
                { "ARG", Color.blue              },
                { "SER", new Color(250, 150, 0)  },
                { "THR", new Color(250, 150, 0)  },
                { "PHE", new Color(50, 50, 170)  },
                { "TYR", new Color(50, 50, 170)  },
                { "ASN", Color.cyan              },
                { "GLN", Color.cyan              },
                { "GLY", new Color(235, 235, 235)},
                { "LEU", Color.green             },
                { "VAL", Color.green             },
                { "ILE", Color.green             },
                { "ALA", new Color(200, 200, 200)},
                { "TRP", new Color(180, 90, 180) },
                { "HIS", new Color(130, 130, 210)},
                { "PRO", new Color(220, 150, 130)},

                { "OTHER", new Color(190, 160, 110)}
            };
        }
    }

    public class AminoAcidDict
    {
        public Dictionary<string, string> aminoAcidDictionary;

        public AminoAcidDict()
        {
            aminoAcidDictionary = new Dictionary<string, string>
            {
                {"A", "ALA"},
                {"R", "ARG"},
                {"N", "ASN"},
                {"D", "ASP"},
                {"C", "CYS"},
                {"Q", "GLN"},
                {"E", "GLU"},
                {"G", "GLY"},
                {"H", "HIS"},
                {"I", "ILE"},
                {"L", "LEU"},
                {"K", "LYS"},
                {"M", "MET"},
                {"F", "PHE"},
                {"P", "PRO"},
                {"S", "SER"},
                {"T", "THR"},
                {"W", "TRP"},
                {"Y", "TYR"},
                {"V", "VAL"}
            };
        }
    }
  
    public class VDWRadii
    {
        /*
        public const float H  = 1.200f;
        public const float C  = 1.700f;
        public const float N  = 1.550f;
        public const float O  = 1.520f;
        public const float F  = 1.470f;
        public const float CL = 1.750f;
        public const float BR = 1.850f;
        public const float I  = 1.980f;
        public const float P  = 1.800f;
        public const float S  = 1.800f;
        */

        public Dictionary<string, float> vdwRadii;

        public VDWRadii()
        {
            this.vdwRadii = new Dictionary<string, float>
            {
                { "H", (float)1.200 },
                { "C", (float)1.700 },
                { "N", (float)1.550 },
                { "O", (float)1.520 },
                { "F", (float)1.470 },
                { "CL", (float)1.750 },
                { "BR", (float)1.850 },
                { "I", (float)1.980 },
                { "P", (float)1.800 },
                { "S", (float)1.800 }
            };
        }
    }

    public class AtomColours
    {
        /*
        public static Color H  = Color.white;
        public static Color C  = Color.black;
        public static Color N  = Color.blue;
        public static Color O  = Color.red;
        public static Color F  = Color.green;
        public static Color CL = Color.green;
        public static Color BR = new Color(139, 0, 0);
        public static Color I  = new Color(148, 0, 221);
        public static Color P  = new Color(255, 165, 0);
        public static Color S  = Color.yellow;
        */
        
        public Dictionary<string, Color> atomColours;

        public AtomColours()
        {
            this.atomColours = new Dictionary<string, Color>
            {
                { "H", Color.white },
                { "C", Color.black },
                { "N", Color.blue },
                { "O", Color.red },
                { "F", Color.green },
                { "CL", Color.green },
                { "BR", new Color(139, 0, 0) },
                { "I", new Color(148, 0, 211) },
                { "P", new Color(255, 165, 0) },
                { "S", Color.yellow }
            };
        }
    }

    public class Chain
    {
        public List<Residue> chainResidues = new List<Residue>();
        public string ChainId { get; set; }

        public Chain()
        {

        }

        public Chain(List<Residue> residues)
        {
            chainResidues = residues;
        }

        public void AddResidue(Residue residue)
        {
            chainResidues.Add(residue);
        }
    }


    // Each residue contains a certain number of atoms...
    // They are linked together via a peptide bond, the bonding of one's carboxyl with another's amino group
    
    public class Residue
    {
        public List<Atom> resAtoms = new List<Atom>();
        public List<GameObject> residueGameObjects = new List<GameObject>();

        public int AtomCount { get; set; }
        public int ResidueSeq { get; set; }
        public string ResidueName { get; set; }
        public string ChainId { get; set; }
        public SecondaryStructure ResStructureInf { get; set; }

        public Residue(int count, string name, string chainId, int resSeq)
        {
            AtomCount = count;
            ResidueSeq = resSeq;
            ResidueName = name;
            ChainId = chainId;
            // initialise structure information to default Other (rendered as tube for cartoon)
            ResStructureInf = SecondaryStructure.Other;
        }

        public Residue(List<Atom> i_atoms)
        {
            AddAtoms(i_atoms);

            if (i_atoms.Any())
            {
                ResidueSeq = i_atoms.First().ResSeqNum;
                ResidueName = i_atoms.First().ResName;
            }
        }

        public Residue(List<Atom> i_atoms, int seqNum)
        {
            AddAtoms(i_atoms);
            ResidueSeq = seqNum;
            if (i_atoms.Any())
            {
                ResidueName = i_atoms.First().ResName;
            }
        }

        public Residue(List<Atom> i_atoms, int seqNum, string resName)
        {
            AddAtoms(i_atoms);
            ResidueSeq = seqNum;
            ResidueName = resName;
        }

        public void AddAtoms(List<Atom> i_atoms)
        {
            foreach(Atom atom in i_atoms)
            {
                resAtoms.Add(atom);
            }
        }

        public void HideResidue()
        {
            foreach(Atom atom in resAtoms)
            {
                atom.IsDisplayed = false;
            }
        }

        public void ShowResidue()
        {
            foreach(Atom atom in resAtoms)
            {
                atom.IsDisplayed = true;
            }
        }

        public string ResidueToString()
        {
            return ResidueSeq.ToString() + " " +  ResidueName ;
        }
    }

    public class Atom
    {
        // Fields obtained from a file

        // From PDB
        public int AtomSerial { get; set; }
        public string AtomName { get; set; }
        public char AltLoc { get; set; }
        public string ResName { get; set; }
        public string ChainId { get; set; }
        public int ResSeqNum { get; set; }
        public float Occupancy { get; set; }
        public float TempFactor { get; set; }
        public string Element { get; set; }
        public Vector3 Position { get; set; }

        // From DSSP
        public SecondaryStructure SecStructInf { get; set; }


        // Fields for representations
        public bool IsDisplayed { get; set; }
        public float VDWRadius { get; set; }
        public bool IsBackbone { get; set; }
        public Color Colour { get; set; }

        public List<Atom> neighbours;

        // Constructor
        public Atom()
	    {
            neighbours = new List<Atom>();
            IsBackbone = false;
            SecStructInf = SecondaryStructure.Other;
	    }


        public string[] GetFieldsAsStringArray()
        {
            string[] fields = new string[9];

            fields[0] = AtomSerial.ToString();
            fields[1] = AtomName;
            fields[2] = ResName;
            fields[3] = ChainId;
            fields[4] = ResSeqNum.ToString();
            fields[5] = Position.ToString();
            fields[6] = Occupancy.ToString();
            fields[7] = TempFactor.ToString();
            fields[8] = Element;

            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = fields[i] + "\n";
            }

            return fields;
        }
    }

    // A class to define each representation that a user may want to create
    // A representation has no idea of what model or part of a model it representing
    public class Representation
    {
        // fields

        // the indices of the residues selected in representation
       public List<List<int>> residueIndices;
 
        // representation parameters
        public RepresentationType repType;

        // representation variables
        public bool IsDisplayed { get; set; }

        // constructor
        public Representation(List<List<int>> indices, RepresentationType n_rep, bool display)
        {
            // to display new representation upon creation
            residueIndices = indices;
            repType = n_rep;
            IsDisplayed = display;
        }
        
        // default to Van Der Walls if no parameters provided
        public Representation(List<List<int>> indices)
        {
            residueIndices = indices;
            repType = RepresentationType.VanDerWalls;
            IsDisplayed = true;
        }

        // methods

        // create custom tostring method that shows the rep type and colour scheme
        public override string ToString()
        {
            string repAsString = repType.ToString();
            return repAsString;
        }
    }
}


