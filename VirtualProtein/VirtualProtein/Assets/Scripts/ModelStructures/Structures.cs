using System;
using UnityEngine;

namespace Structures {


    public class Atom {

        // private members
        private int atom_serial_number;
        private string atom_name;

        private char alt_loc;

        private string residue_name;
        private char chain_id;
        private int residue_seq;

        private Vector3 atom_pos;

        private float occupancy;
        private float temp_factor;

        // Constructor
	    public Atom()
	    {

	    }

    }
}


