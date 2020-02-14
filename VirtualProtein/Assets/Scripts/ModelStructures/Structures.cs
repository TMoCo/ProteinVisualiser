using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;


// idea is to create structure classes that build into each other
//
//  |
//  |-------> 
//  |-----------> Secondary structure
//  |----------------> Atom
//
//
namespace Structures
{
    public class VDWRadii
    {
        //
        public Dictionary<string, float> vdwRadii;

        public VDWRadii()
        {
            this.vdwRadii = new Dictionary<string, float>();

            this.vdwRadii.Add("H", (float)1.200);
            this.vdwRadii.Add("C", (float)1.700);
            this.vdwRadii.Add("N", (float)1.550);
            this.vdwRadii.Add("O", (float)1.520);
            this.vdwRadii.Add("F", (float)1.470);
            this.vdwRadii.Add("CL", (float)1.750);
            this.vdwRadii.Add("BR", (float)1.850);
            this.vdwRadii.Add("I", (float)1.980);
            this.vdwRadii.Add("P", (float)1.800);
            this.vdwRadii.Add("S", (float)1.800);
        }
    }

    public class AtomColours
    {
        public Dictionary<string, Color> atomColours;

        public AtomColours()
        {
            this.atomColours = new Dictionary<string, Color>();

            this.atomColours.Add("H", Color.white);
            this.atomColours.Add("C", Color.black);
            this.atomColours.Add("N", Color.blue);
            this.atomColours.Add("O", Color.red);
            this.atomColours.Add("F", Color.green);
            this.atomColours.Add("CL", Color.green);
            this.atomColours.Add("BR", new Color(139,0,0));
            this.atomColours.Add("I", new Color(148,0,211));
            this.atomColours.Add("P", new Color(255,165,0));
            this.atomColours.Add("S", Color.yellow);
        }
    }

    public class Atom
    {

        // Members obtained from a pdb file
        public int atom_serial;
        public string atom_name;
        public char alt_loc;
        public string residue_name;
        public string chain_id;
        public int res_seq_nb;
        public Vector3 position;
        public float occupancy;
        public float temp_factor;
        public string element;

        public bool isDisplayed;
        public float VDWRadius;

        public Color colour;

        // Constructor
        public Atom()
	    {
            
	    }
        


        // *** SETTERS ***
        public void Show(bool show)
        {
            isDisplayed = show;
        }

        public void SetAtomSerial(int n_atom_serial)
        {
            atom_serial = n_atom_serial;
        }

        public void SetAtomName(string n_atom_name)
        {
            atom_name = n_atom_name;
        }

        public void SetResidueName(string n_residue_name)
        {
            residue_name = n_residue_name;
        }

        public void SetChainId(string n_chain_id)
        {
            chain_id = n_chain_id;
        }

        public void SetResidueSequenceNb(int n_res_seq_nb)
        {
            res_seq_nb = n_res_seq_nb;
        }

        public void SetAtomPos(Vector3 n_position)
        {
            position = n_position;
        }

        public void SetOccupancy(float n_occupancy)
        {
            occupancy = n_occupancy;
        }

        public void SetTempFactor(float n_temp_factor)
        {
            temp_factor = n_temp_factor;
        }

        public void SetElement(string n_element)
        {
            element = n_element;
        }

        // *** GETTERS ***

        public Vector3 GetPosition()
        {
            return position;
        }

        public string getAtomSerial()
        {
            return atom_serial.ToString();
        }

        public string[] GetFieldsAsStringArray()
        {
            string[] fields = new string[9];

            fields[0] = atom_serial.ToString();
            fields[1] = atom_name;
            fields[2] = residue_name;
            fields[3] = chain_id;
            fields[4] = res_seq_nb.ToString();
            fields[5] = position.ToString();
            fields[6] = occupancy.ToString();
            fields[7] = temp_factor.ToString();
            fields[8] = element;

            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = fields[i] + "\n";
            }

            return fields;
        }


    }
}


