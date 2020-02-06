using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Structures {


    public class Atom {

        // private members
        private int atom_serial;
        private string atom_name;

        private char alt_loc;

        private string residue_name;
        private string chain_id;
        private int res_seq_nb;

        private Vector3 atom_pos;

        private float occupancy;
        private float temp_factor;

        private string element;

        // Constructor
        public Atom()
	    {
            
	    }


        // Setters for new atom creation
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

        public void SetAtomPos(Vector3 n_atom_pos)
        {
            atom_pos = n_atom_pos;
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

        public Vector3 GetPosition()
        {
            return atom_pos;
        }

        public string[] GetFieldsAsStrings()
        {
            string[] fields = new string[9];

            fields[0] = atom_serial.ToString();
            fields[1] = atom_name;
            fields[2] = residue_name;
            fields[3] = chain_id;
            fields[4] = res_seq_nb.ToString();
            fields[5] = atom_pos.ToString();
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


