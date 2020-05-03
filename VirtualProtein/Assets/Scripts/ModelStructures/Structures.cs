using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace Structures
{
    //      ENUMS     //

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
        AtomType,
        ResidueType,
        StructureType,
        Arbitrary
    };

    //  REFERENCE STATIC DICTIONARIES   //

    public class StructureColours
    {
        public static Dictionary<SecondaryStructure, Color> structureColours = new Dictionary<SecondaryStructure, Color>
            {
                {SecondaryStructure.AlphaHelix, Color.red },
                {SecondaryStructure.BetaSheet, Color.blue },
                {SecondaryStructure.Other, Color.green }
            };
    }

    public class ResidueColours
    {
        public static Dictionary<string, Color> aminoColours = new Dictionary<string, Color>
            {
                { "ASP", new Color(0.9f, 0.9f, 0.04f, 1) },
                { "GLU", new Color(0.9f, 0.9f, 0.04f, 1) },
                { "CYS", Color.yellow            },
                { "MET", Color.yellow            },
                { "LYS", Color.blue              },
                { "ARG", Color.blue              },
                { "SER", new Color(0.98f, 0.59f, 0, 1)  },
                { "THR", new Color(0.98f, 0.59f, 0, 1)  },
                { "PHE", new Color(0.20f, 0.20f, 0.67f, 1)  },
                { "TYR", new Color(0.20f, 0.20f, 0.67f, 1)  },
                { "ASN", Color.cyan              },
                { "GLN", Color.cyan              },
                { "GLY", new Color(0.92f, 0.92f, 0.92f, 1)},
                { "LEU", Color.green             },
                { "VAL", Color.green             },
                { "ILE", Color.green             },
                { "ALA", new Color(0.78f, 0.78f, 0.78f, 1)},
                { "TRP", new Color(0.70f, 0.35f, 0.70f, 1) },
                { "HIS", new Color(0.51f, 0.51f, 0.82f, 1)},
                { "PRO", new Color(0.86f, 0.59f, 0.51f, 1)},

                { "OTHER", new Color(0.74f, 0.63f, 0.43f, 1)}
            };
    }

    public class AminoAcids
    {
        public static Dictionary<string, string> aminoAcidDictionary = new Dictionary<string, string>
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
  
    public class VDWRadii
    {
        public static Dictionary<string, float> vdwRadii = new Dictionary<string, float>
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

    public class AtomColours
    {        
        public static Dictionary<string, Color> atomColours = new Dictionary<string, Color>
            {
                { "H", Color.white },
                { "C", Color.black },
                { "N", Color.blue },
                { "O", Color.red },
                { "F", Color.green },
                { "CL", Color.green },
                { "BR", new Color(0.54f, 0, 0, 1) },
                { "I", new Color(0.58f, 0, 0.83f, 1) },
                { "P", new Color(1, 0.65f, 0, 1) },
                { "S", Color.yellow }
            };
        
    }

    //  PROTEIN DATA STRUCTURES  //

    //
    // The idea is to create structure classes that build into each other
    //
    //  |----> Model (one or more chains)
    //  |---------> Chain (one or more secondary structures)
    //  |--------------> Residue (various nb of atoms for each amino acid)
    //  |-------------------> Atom
    //
    //

    // A protein's chain, essentially a list of residues
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
        public List<List<GameObject>> residueGameObjects = new List<List<GameObject>>();

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

    // Atom containing an individual atom's data such as position, element etc...
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
    // A representation has no idea of what model or part of a model it is representing
    public class Representation
    {
        // fields

        // the indices of the residues selected in representation
       public List<List<int>> residueIndices;
 
        // representation parameters
        public RepresentationType repType;
        public ColourScheme colScheme;

        // representation variables
        public bool IsDisplayed { get; set; }

        // constructor
        public Representation(List<List<int>> indices, RepresentationType n_rep, ColourScheme n_scheme, bool display)
        {
            // to display new representation upon creation
            residueIndices = indices;
            repType = n_rep;
            colScheme = n_scheme;
            IsDisplayed = display;
        }
        
        // methods

        // create custom tostring method that shows the rep type and colour scheme
        public int GetResidueCount()
        {
            int count = 0;
            foreach(List<int> sublist in residueIndices)
            {
                count += sublist.Count;
            }
            return count;
        }
    }
}


