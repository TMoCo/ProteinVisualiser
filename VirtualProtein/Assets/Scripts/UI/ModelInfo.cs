using UnityEngine;
using UnityEngine.UI;

public class ModelInfo : MonoBehaviour
{

    public GameObject modelObject;
    public Text atomCount;
    public Text residueCount;
    public Text bondCount;

    public static bool fileLoaded;

    // Update is called once per frame
    void Update()
    {
        if (fileLoaded)
        {
            PopulateFields();
        }
    }

    void PopulateFields()
    {
        atomCount.text = Model.AtomCount.ToString();
        bondCount.text = Model.BondCount.ToString();
        residueCount.text = Model.ResidueCount.ToString();
    }
}

