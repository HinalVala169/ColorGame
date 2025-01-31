
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    [SerializeField]
    private CanvasScaler canvasScaler;
   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    

        if (IsTablet())
        {
            Debug.Log("Running on a Tablet");

            canvasScaler.matchWidthOrHeight = 1.0f;

            
        }
        else
        {
            Debug.Log("Running on a Phone");

            canvasScaler.matchWidthOrHeight = 0.0f;
        }
    }

    bool IsTablet()
    {
        float screenWidthInches = Screen.width / Screen.dpi;
        float screenHeightInches = Screen.height / Screen.dpi;
        float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidthInches, 2) + Mathf.Pow(screenHeightInches, 2));

        Debug.Log("Diagonal Inches : " +diagonalInches);

      
        return diagonalInches >= 6.5f;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
