using Structures;
using UnityEngine;

using System;
using System.IO;

using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FileParser
{

	public FileParser(string file_path, string file_type)
	{
        // do something
	}

    public FileParser()
    {

    }

    // use a regex for a simple pdb file
    public static List<Atom> ParsePDB(string path)
    {
        try
        {
            List<Atom> atoms = new List<Atom>();

            VDWRadii radii = new VDWRadii();
            AtomColours colours = new AtomColours();

            int j = 0;
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader input = new StreamReader(file))
                {

                    // Regular expression for an ATOM record in the .pdb file format
                    // Separating each field into groups for collection
                    // @ symbol start of strings to avoid "unrecognised escape sequence" error
                    Regex pdbRegex = new Regex(@"^ATOM\s+(?<atom_serial>\d+)" + // ATOM entry + atom serial number
                    @"\s+(?<atom_name>\w+)" +                                      // atom name
                    @"\s+(?<residue_name>\w+)" +                                // residue name
                    @"\s(?<chain_id>\w?)" +                                  // chain id
                    @"\s+(?<res_seq_nb>\d+)" +                                     // residue sequence number
                    @"\s*(?<x_pos>\W?\d+.\d{3})" +                              // x position
                    @"\s*(?<y_pos>\W?\d+.\d{3})" +                              // y position
                    @"\s*(?<z_pos>\W?\d+.\d{3})" +                              // z position
                    @"\s*(?<occupancy>\d.\d{2})" +                              // occupancy
                    @"\s*(?<temp_factor>\d+.\d{2})" +                           // temperature (B) factor
                    @"[\s\S\w\W]{0,10}" +                                       // some padding, doesn't contain any useful info
                    @"(?<element>[\w\s]{0,2})\s*",                               // element type, not always provided
                    RegexOptions.IgnoreCase);
                    
                    int i = 0;
                    while (!input.EndOfStream)
                    {
                        string line = input.ReadLine();
                        Match match = pdbRegex.Match(line);
                        if (match.Success)
                        {
                            GroupCollection groups = match.Groups;
                            Atom n_atom = new Atom();

                            // Set the fields obtained from file
                            SetFieldsPDB(groups, n_atom);
                            // if no element given, deduce from atom name
                            if (string.IsNullOrEmpty(n_atom.Element))
                            {
                                n_atom.Element = GetElementFromName(n_atom).Replace(" ", String.Empty);
                            }
                            else
                            {
                                // if element provided, could contain artifact whitespace
                                foreach(Atom atom in atoms)
                                {
                                    atom.Element = atom.Element.Replace(" ", string.Empty); 
                                }
                            }
                            try
                            {
                                n_atom.VDWRadius = radii.vdwRadii[n_atom.Element.Replace(" ", String.Empty)];
                                n_atom.Colour = colours.atomColours[n_atom.Element.Replace(" ", String.Empty)];
                            }
                            catch (System.SystemException)
                            {
                                n_atom.VDWRadius = (float)1.0;
                            }
                            j++;
                            atoms.Add(n_atom);
                        }
                        i++;
                    }
                }

            }
            FindNeighbours(atoms);
            FindBackbone(atoms);
            return atoms;
                
        }
        catch (System.IO.IOException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    private static void SetFieldsPDB(GroupCollection groups, Atom atom)
    {
        atom.AtomSerial = Int32.Parse(groups[1].ToString());
        atom.AtomName = groups[2].ToString();
        atom.ResName = groups[3].ToString();
        atom.ChainId = groups[4].ToString();
        atom.ResSeqNum = Int32.Parse(groups[5].ToString());
        atom.Position = new Vector3(float.Parse(groups[6].ToString()), 
                                    float.Parse(groups[7].ToString()), 
                                    float.Parse(groups[8].ToString()));
        atom.Occupancy = float.Parse(groups[9].ToString());
        atom.TempFactor = float.Parse(groups[10].ToString());
        atom.Element = groups[11].ToString();
        atom.IsDisplayed = true;
    }

    // using the naming convention of atoms with their element always at the start, for pdb files
    // with an empty element field in their atom record
    private static string GetElementFromName(Atom atom)
    {
        return atom.AtomName.Substring(0,1).ToUpper();
    }

    // iterate over the atoms, checking their neighbours. If one neighbour belongs to a different residue,
    // then it must belong to a peptide bond (either the N from amino or the C from carboxyl)
    public static List<Atom> FindBackbone(List<Atom> atoms)
    {
        List<Atom> backbone = new List<Atom>();
        int res_curr_nb;
        // finds the atoms that bridge two residues 
        for (int i = 0; i < atoms.Count; i++)
        {
            res_curr_nb = atoms[i].ResSeqNum;

            // compare current atom's res nb with it's neighbours'
            foreach (Atom atom in atoms[i].neighbours)
            {
                // atom neighbours another residue, it belongs to the peptide bond
                if ((atom.ResSeqNum > res_curr_nb) || (atom.ResSeqNum < res_curr_nb))
                {
                    atom.IsBackbone = true;
                }
            }
        }

        // then reiterate to set the atoms between the peptide bonds as belonging to the backbone
        // as well as the atoms that belong to the backbone but are not between peptide bonds

        for (int i = 0; i < atoms.Count; i++)
        {
            // only check atoms we don't already know belong to the backbone
            if (!atoms[i].IsBackbone)
            {
                int count = 0;
                foreach (Atom atom in atoms[i].neighbours)
                {
                    if (atom.IsBackbone)
                    {
                        count++;
                    }
                }

                // the atom neighbours two backbone atoms, it belongs to the backbone
                if (count == 2)
                {
                    atoms[i].IsBackbone = true;
                }
            }
        }

        // iterate over all the atoms, detect first and last current backbone atom
        int first = -1;
        int last = -1;
        for (int i = 0; i < atoms.Count; i++)
        {
            if (atoms[i].IsBackbone)
            {
                // this is the first backbone atom
                if (first == -1)
                    first = i;
                // this is the last backbone atom
                last = i;
            }
        }

        // from these two atoms, detect their element to determine what the final backbone atoms are
        // if its carbon, then its connected to another carbon and a nitrogen
        //Debug.Log("\"" + atoms[first].element + "\"");
        if (atoms[first].Element.CompareTo("C") == 0)
        {
           // Debug.Log("First is indeed a carbon");
            foreach(Atom atom in atoms[first].neighbours)
            {
                if (atom.Element.CompareTo("C") == 0)
                {
                    atom.IsBackbone = true;
                    foreach(Atom sub_atom in atom.neighbours)
                    {
                        if(sub_atom.Element.CompareTo("N") == 0)
                        {
                            sub_atom.IsBackbone = true;
                            break;
                        }
                    }
                    break;
                }
            }
        }
        
        
        // if it is nitrogen then its connected to two consecutive carbons belonging to a carboxyl group
        if (atoms[last].Element.CompareTo("N") == 0)
        {
            foreach (Atom atom in atoms[last].neighbours)
            {
                if (atom.Element.CompareTo("C") == 0)
                {
                    atom.IsBackbone = true;
                    foreach (Atom sub_atom in atom.neighbours)
                    {
                        foreach (Atom subsub_atom in sub_atom.neighbours)
                        {
                            if (subsub_atom.Element.CompareTo("O") == 0)
                            {
                                sub_atom.IsBackbone = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        // order the backbone atoms for the tube representations in case wron atom is at the start

        int indexOfN = 0;
        foreach(Atom atom in atoms)
        {
            if (atom.IsBackbone)
            {
                backbone.Add(atom);
            }
        }
        if (!backbone.First().Element.Equals("N"))
        {
            Debug.Log(backbone[indexOfN].Element);
            // find the first instance of N and set is as the first N of the backbone
            while (!backbone[indexOfN].Element.Equals("N"))
            {
                indexOfN += 1;
            }
            Swap(backbone, indexOfN, 0);
        }

        if (backbone[0].neighbours.IndexOf(backbone[indexOfN]) == -1)
        {
            Swap(backbone, indexOfN, indexOfN + 1);
        }

        return backbone;
    }

    // swap two objects in a list
    public static void Swap<T>(IList<T> list, int indexA, int indexB)
    {
        T tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
    }

    // iterate over the atoms to determine which atoms are linked together
    private static void FindNeighbours(List<Atom> atoms)
    {
        for(int i=0; i<atoms.Count; i++)
        {
            for(int j=0; j<atoms.Count; j++)
            {
                float distance = Vector3.Distance(atoms[i].Position, atoms[j].Position);

                // These values are the same used as the RasMol visualiser when no CONNECT fields are in a pdb file 
                // to determine whether a bond exists or not
                if ((atoms[i].Element.CompareTo("H") == 0) || (atoms[j].Element.CompareTo("H") == 0))
                {
                    // instantiate a bond
                    if ((0.4 <= distance && distance <= 1.2))
                    {
                        atoms[i].neighbours.Add(atoms[j]);
                    }
                }
                else if ((0.4 <= distance && distance <= 1.9))
                {
                    atoms[i].neighbours.Add(atoms[j]);
                }
            }
        }
    }
}
