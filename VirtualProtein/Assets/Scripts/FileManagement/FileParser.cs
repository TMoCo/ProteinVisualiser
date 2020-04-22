﻿using Structures;
using UnityEngine;

using System;
using System.IO;

using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FileParser
{

    public bool DSSPstatus;

    // dictionary containing the VDw radius and colours of each element
    readonly VDWRadii radii = new VDWRadii();
    readonly AtomColours colours = new AtomColours();

    public FileParser()
    {
        DSSPstatus = false;
    }
    

    // parses a PDB file
    public List<List<Atom>> ParsePDB(string path)
    {
        List<List<Atom>> chains = new List<List<Atom>>();

        List<Chain> chainsList = new List<Chain>();

        try
        {            
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader input = new StreamReader(file))
                {

                    // Regular expression to catch either an ATOM or TER in the .pdb file format using capture groups to separate information

                    Regex pdbRegex = new Regex(@"^ATOM\s+(?<atom_serial>\d+)\s+(?<atom_name>[\w\W]{0,4})\s+"+
                        @"(?<residue_name>\w+)\s(?<chain_id>\w?)\s+(?<res_seq_nb>\d+)\s*"+
                        @"(?<x_pos>\W?\d+.\d{3})\s*(?<y_pos>\W?\d+.\d{3})\s*(?<z_pos>\W?\d+.\d{3})\s*"+
                        @"(?<occupancy>\d.\d{2})\s*(?<temp_factor>\d+.\d{2})[\s\S\w\W]{0,10}(?<element>[\w\s]{0,2})\s*"+
                        @"|^(?<ter_entry>TER)\s*(?<ter_serial>\d+)\s*(?<ter_res>\w+)\s*(?<ter_chain>\w+)\s*(?<ter_res_seq_num>\d+)");

                    int chainIndex = -1;
                    bool newChain = true;

                    while (!input.EndOfStream)
                    {
                        string line = input.ReadLine();
                        Match match = pdbRegex.Match(line);

                        // found a new atom
                        if (match.Success)
                        {
                            
                            GroupCollection groups = match.Groups;

                            // initialise new list in case of new chain
                            if(newChain)
                            {
                                newChain = false;
                                chains.Add(new List<Atom>());
                                chainIndex += 1;
                            }

                            if (Equals(groups[12].ToString(), "TER"))
                            {
                                newChain = true;
                                continue;
                            }

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
                                n_atom.Element = n_atom.Element.Replace(" ", string.Empty);
                            }

                            // set the atom's radius
                            try
                            {
                                n_atom.VDWRadius = radii.vdwRadii[n_atom.Element.Replace(" ", String.Empty)];
                                n_atom.Colour = colours.atomColours[n_atom.Element.Replace(" ", String.Empty)];
                            }
                            catch (System.SystemException)
                            {
                                n_atom.VDWRadius = (float)1.0;
                                n_atom.Colour = Color.black;
                            }

                            chains[chainIndex].Add(n_atom);
                        }
                    }
                }
            }        
            foreach(List<Atom> chain in chains)
            {
                FindNeighbours(chain);
                chainsList.Add(new Chain(GetChainResidues(chain)));
            }
            return chains;
                
        }
        catch (System.IO.IOException e)
        {
            return chains;
        }
    }

    public List<Chain> ParsePDBtoChain(string path)
    {
        List<List<Atom>> chains = new List<List<Atom>>();
        List<Chain> chainsList = new List<Chain>();

        try
        {

            VDWRadii radii = new VDWRadii();
            AtomColours colours = new AtomColours();

            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader input = new StreamReader(file))
                {

                    // Regular expression to catch either an ATOM or TER in the .pdb file format using capture groups to separate information

                    Regex pdbRegex = new Regex(@"^ATOM\s+(?<atom_serial>\d+)\s+(?<atom_name>[\w\W]{0,4})\s+" +
                        @"(?<residue_name>\w+)\s(?<chain_id>\w?)\s+(?<res_seq_nb>\d+)\s*" +
                        @"(?<x_pos>\W?\d+.\d{3})\s*(?<y_pos>\W?\d+.\d{3})\s*(?<z_pos>\W?\d+.\d{3})\s*" +
                        @"(?<occupancy>\d.\d{2})\s*(?<temp_factor>\d+.\d{2})[\s\S\w\W]{0,10}(?<element>[\w\s]{0,2})\s*" +
                        @"|^(?<ter_entry>TER)\s*(?<ter_serial>\d+)\s*(?<ter_res>\w+)\s*(?<ter_chain>\w+)\s*(?<ter_res_seq_num>\d+)");

                    int chainIndex = -1;
                    bool newChain = true;

                    while (!input.EndOfStream)
                    {
                        string line = input.ReadLine();
                        Match match = pdbRegex.Match(line);

                        // found a new atom
                        if (match.Success)
                        {

                            GroupCollection groups = match.Groups;

                            // initialise new list in case of new chain
                            if (newChain)
                            {
                                newChain = false;
                                chains.Add(new List<Atom>());
                                chainIndex += 1;
                            }

                            if (Equals(groups[12].ToString(), "TER"))
                            {
                                newChain = true;
                                continue;
                            }

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
                                n_atom.Element = n_atom.Element.Replace(" ", string.Empty);
                            }

                            // set the atom's radius
                            try
                            {
                                n_atom.VDWRadius = radii.vdwRadii[n_atom.Element.Replace(" ", String.Empty)];
                                n_atom.Colour = colours.atomColours[n_atom.Element.Replace(" ", String.Empty)];
                            }
                            catch (System.SystemException)
                            {
                                n_atom.VDWRadius = (float)1.0;
                                n_atom.Colour = Color.black;
                            }

                            chains[chainIndex].Add(n_atom);
                        }
                    }
                }
            }
            foreach (List<Atom> chain in chains)
            {
                FindNeighbours(chain);
                FindBackbone(chain);
                chainsList.Add(new Chain(GetChainResidues(chain)) { ChainId = chain.First().ChainId });
            }
            return chainsList;
        }
        catch (System.IO.IOException)
        {
            return chainsList;
        }
    }

    // parse the DSSP seondary structure data to find which residues form alpha helices and beta sheets
    public List<SecondaryStructure> ParseDSSP(List<List<Atom>> chains, string path)
    {
        List<SecondaryStructure> modelStructInfo = new List<SecondaryStructure>();

        if (!chains.Any() || !chains[0].Any())
        {
            DSSPstatus = false;
            return modelStructInfo;
        }

        try
        {
            // load file content
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                // read file content line by line
                using (StreamReader input = new StreamReader(file))
                {
                    Regex dsspRegex = new Regex(@"^\s*(?<line>\d+)\s*(?<resNum>\d+)\s+(?<chainId>\w)\s+(?<AminoAcid>[^0-9])\s{2}(?<structure>[^0-9])|^\s*\d+\s*(!\*)");

                    int resCount = 0;
                    foreach(List<Atom> chain in chains)
                    {
                        foreach(Residue res in GetResidues(chains.First()))
                        {
                            resCount += 1;
                        }
                    }
                    Debug.Log(resCount);


                    int chainIndex = 0;
                    List<Residue> chainResidues = new List<Residue>();
                    int residueIndex = 0;
                    bool newChain = true;

                    //Debug.Log("starting with chain " + curChainIndex);

                    while (!input.EndOfStream)
                    {
                        string line = input.ReadLine();
                        Match match = dsspRegex.Match(line);

                        if (match.Success)
                        {
                            GroupCollection groups = match.Groups;

                            // at the start of a chain so get residues for that chain
                            if (newChain)
                            {
                                if (chainIndex == chains.Count)
                                {
                                    DSSPstatus = false;
                                    break;
                                }
                                chainResidues = GetResidues(chains[chainIndex]);
                                newChain = false;
                            }

                            // reached an end of chain line
                            if(string.Equals(groups[1].ToString(), "!*"))
                            {
                                // increment chain index
                                chainIndex += 1;
                                // initialise residue index
                                residueIndex = 0;
                                // signal new chain
                                newChain = true;
                                // if there are more lines than there are chains, we have the wrong file

                                // skip res comparison lower down
                                continue;
                            }

                            // we haven't reached the end of the chain but check that the chain is not too longs
                            if(residueIndex == chainResidues.Count)
                            {
                                DSSPstatus = false;
                                modelStructInfo.Clear();
                                return modelStructInfo;
                            }

                            // the chain is not too small so proceed to comparison
                            if (chainResidues[residueIndex].ResidueSeq != int.Parse(groups[3].ToString()))
                            {
                                DSSPstatus = false;
                                modelStructInfo.Clear();
                                return modelStructInfo;
                            }
                            else if (string.Equals(groups[6].ToString(), "H"))
                            {
                                modelStructInfo.Add(SecondaryStructure.AlphaHelix);
                            }
                            else if (string.Equals(groups[6].ToString(), "E"))
                            {
                                modelStructInfo.Add(SecondaryStructure.BetaSheet);
                            }
                            else
                            {
                                modelStructInfo.Add(SecondaryStructure.Other);
                            }
                            residueIndex += 1;
                        }

                    }
                }
            }
            DSSPstatus = true;
            return modelStructInfo;

        }
        catch (System.IO.IOException e)
        {
            Debug.Log(e);
            DSSPstatus = false;
            modelStructInfo.Clear();
            return modelStructInfo;
        }
    }

    private void SetFieldsPDB(GroupCollection groups, Atom atom)
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
    public void FindBackbone(List<Atom> atoms)
    {

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
    }
    // same as above but for a chain
    public static void FindBackbone(Chain chain)
    {
        for(int i = 0; i < chain.chainResidues.Count; i++)
        {
            foreach (Atom atom in chain.chainResidues[i].resAtoms)
            {
                foreach (Atom neighbour in atom.neighbours)
                {
                    if (atom.ResSeqNum != neighbour.ResSeqNum)
                    {
                        atom.IsBackbone = true;
                    }
                }

                if ((i == 0 || i == chain.chainResidues.Count - 1) && atom.IsBackbone)
                {
                    if (atom.Element.CompareTo("C") == 0)
                    {
                        foreach (Atom subAtom in atom.neighbours)
                        {
                            if (subAtom.Element.CompareTo("C") == 0)
                            {
                                foreach (Atom subSubAtom in subAtom.neighbours)
                                {
                                    if (subSubAtom.Element.CompareTo("N") == 0)
                                    {
                                        subSubAtom.IsBackbone = true;
                                        break;
                                    }
                                }
                                subAtom.IsBackbone = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (atom.Element.CompareTo("N") == 0)
                        {
                            foreach (Atom subAtom in atom.neighbours)
                            {
                                if (subAtom.Element.CompareTo("C") == 0)
                                {
                                    foreach (Atom subSubAtom in atom.neighbours)
                                    {
                                        foreach (Atom subSubSubAtom in subSubAtom.neighbours)
                                        {
                                            if (subSubSubAtom.Element.CompareTo("O") == 0)
                                            {
                                                subSubAtom.IsBackbone = true;
                                                break;
                                            }
                                        }
                                    }
                                    subAtom.IsBackbone = true;
                                }
                            }
                        }
                    }
                    break;
                }
            }
            foreach (Atom atom in chain.chainResidues[i].resAtoms)
            {
                if (!atom.IsBackbone)
                {
                    int count = 0;
                    foreach (Atom neighbour in atom.neighbours)
                    {
                        if (neighbour.IsBackbone)
                        {
                            count++;
                        }
                    }

                    // the atom neighbours two backbone atoms, it belongs to the backbone
                    if (count == 2)
                    {
                        atom.IsBackbone = true;
                    }
                }
            }
        }
    }

    public static List<Residue> GetResidues (List<Atom> chain)
    {


        if (chain.Any())
        {
            // atom bucket store all atoms belonging to one residue
            List<Residue> residues = new List<Residue>
            {
                new Residue(0, chain[0].ResName , chain[0].ChainId, chain[0].ResSeqNum)
            };
            
            int currRes = chain[0].ResSeqNum;


            foreach(Atom atom in chain)
            {
                // reached a new residue, add current atom bucket to it
                if (atom.ResSeqNum > currRes)
                {
                    residues.Add(new Residue(0, atom.ResName, atom.ChainId, atom.ResSeqNum));

                    currRes = atom.ResSeqNum;
                }
                // increment atom count
                residues.Last().AtomCount += 1;
            }
            return residues;
        }
        else
        {
            throw new Exception("No chain provided");
        }
    }

    public static List<Residue> GetChainResidues(List<Atom> chain)
    {
        if (chain.Any())
        {
            // atom bucket store all atoms belonging to one residue
            List<Residue> residues = new List<Residue>
            {
                new Residue(0, chain[0].ResName , chain[0].ChainId, chain[0].ResSeqNum)
            };

            int currRes = chain[0].ResSeqNum;


            foreach (Atom atom in chain)
            {
                // reached a new residue, add current atom bucket to it
                if (atom.ResSeqNum > currRes)
                {
                    residues.Add(new Residue(0, atom.ResName, atom.ChainId, atom.ResSeqNum));

                    currRes = atom.ResSeqNum;
                }
                // increment atom count
                residues.Last().AtomCount += 1;
                // add atom to the current residue
                residues.Last().resAtoms.Add(atom);
            }
            return residues;
        }
        else
        {
            throw new Exception("No chain provided");
        }
    }

    public static List<Residue> GetResidues(List<List<Atom>> chains)
    {
        List<Residue> residues = new List<Residue>();
        foreach(List<Atom> chain in chains)
        {
            residues.Concat(GetResidues(chain));
        }
        return residues;
    }

    // A funtion that gets the backbone and reorders atoms in case atom sequence does not follow backbone (seen in some files)
    public static List<Atom> GetBackbone(List<Atom> atoms)
    {
        List<Atom> backbone = new List<Atom>();
        // order the backbone atoms for the tube representations in case wrong atom is at the start

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

    // iterate over the atoms to determine which atoms are bonded together
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

    // Find the residue count within a list of atoms
    public static int GetResCount(List<List<Atom>> chains)
    {
        int resCount = 0;

        foreach(List<Atom> chain in chains)
        {
            foreach(Residue res in GetResidues(chain))
            {
                resCount += 1;
            }
        }
        return resCount;
    }

    // Find the number of bonds between atoms in a list of atoms
    public static int GetBondCount(List<Atom> atoms)
    {
        int bondCount = 0;
        List<int> checkedAtoms = new List<int>();

        // loop over each atom and check its neighbours
        // for each neighbour (i.e bonded) add its serial to the list of ints
        // and increment bond count. If the neighbour has already been check (its serial is already
        // in the list) then don't count the bond again.

        foreach(Atom atom in atoms)
        {
            checkedAtoms.Add(atom.AtomSerial);

            foreach(Atom neighbour in atom.neighbours)
            {
                if (!checkedAtoms.Contains(neighbour.AtomSerial))
                {
                    bondCount += 1;
                }
            }
        }
        
        return bondCount;
    }
}
