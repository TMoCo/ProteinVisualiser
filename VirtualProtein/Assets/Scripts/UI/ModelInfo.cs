using UnityEngine;
using UnityEngine.UI;

public class ModelInfo : MonoBehaviour
{

    public GameObject modelObject;
    public Text atomCount;
    public Text residueCount;
    public Text bondCount;

    public bool fileLoaded;

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
        Model modelScript = modelObject.GetComponent<Model>();
        atomCount.text = modelScript.AtomCount.ToString();
        bondCount.text = modelScript.BondCount.ToString();
        residueCount.text = modelScript.ResidueCount.ToString();
    }
}

