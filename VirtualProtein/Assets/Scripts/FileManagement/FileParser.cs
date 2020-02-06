using Structures;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FileParser
{

	public FileParser(string path, string type)
	{
        // do something
	}

    public FileParser()
    {
        Debug.Log("created a parser");
    }

    // use a regex for a simple pdb file
    public List<Atom> ParsePDB(string path)
    {
        try
        {
            List<Atom> atoms = new List<Atom>();
            Debug.Log("created a list");
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader input = new StreamReader(file)) 
                {

                    // Regular expression for an ATOM record in the .pdb file format
                    // Separating each field into groups for collection
                    // @ symbol start of strings to avoid "unrecognised escape sequence" error
                    Regex pdbRegex = new Regex(@"^ATOM\s+(?<atom_serial>\d+)" + // ATOM entry + atom serial number
                    @"\s+(?<atom_name>\w+)" +                    // atom name
                    @"\s+(?<residue_name>\w+)" +                 // residue name
                    @"\s(?<chain_id>\w?)\s+" +                   // chain id
                    @"(?<res_seq_nb>\d+)" +                      // residue sequence number
                    @"\s*(?<x_pos>\W?\d+.\d{3})" +               // x position
                    @"\s*(?<y_pos>\W?\d+.\d{3})" +               // y position
                    @"\s*(?<z_pos>\W?\d+.\d{3})" +               // z position
                    @"\s*(?<occupancy>\d.\d{2})" +               // occupancy
                    @"\s*(?<temp_factor>\d+.\d{2})" +            // temperature (B) factor
                    @"[\s\S\w\W]{0,10}" +                        // some padding, doesn't contain any useful info
                    @"(?<element>[\w\s]{0,2})\s",                // element type, not always provided
                    RegexOptions.IgnoreCase);

                    while (!input.EndOfStream)
                    {
                        MatchCollection matches = pdbRegex.Matches(input.ReadLine());
                        foreach (Match match in matches)
                        {
                            // collect groups and create atoms from their information
                            GroupCollection groups = match.Groups;

                            Debug.Log(groups[0].ToString());

                            Atom n_atom = new Atom();
                            SetFieldsPDB(groups, n_atom);
                            atoms.Add(n_atom);
                            Debug.Log("added atom");
                            
                        }
                    }

                }

            }

            return atoms;
                
        }
        catch (System.IO.IOException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    private void SetFieldsPDB(GroupCollection groups, Atom atom)
    {
        atom.SetAtomSerial(Int32.Parse(groups[1].ToString()));
        atom.SetAtomName(groups[2].ToString());
        atom.SetResidueName(groups[3].ToString());
        atom.SetChainId(groups[4].ToString());
        atom.SetResidueSequenceNb(Int32.Parse(groups[5].ToString()));
        atom.SetAtomPos(new Vector3(float.Parse(groups[6].ToString()), 
                                    float.Parse(groups[7].ToString()), 
                                    float.Parse(groups[8].ToString())));
        atom.SetOccupancy(float.Parse(groups[9].ToString()));
        atom.SetTempFactor(float.Parse(groups[10].ToString()));
        atom.SetElement(groups[11].ToString());
    }

}
