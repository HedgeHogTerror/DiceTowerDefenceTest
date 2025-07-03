using UnityEngine;

public class MoveDice : MonoBehaviour
{
    [SerializeField] private GameObject dice;

    private float diceX;
    private float diceY;
    private float diceZ;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.gameObject.tag == "Dice")
            {
                dice = hit.transform.gameObject;
                diceX = dice.transform.position.x;
                diceY = dice.transform.position.y;
            }
        }

        if (Input.GetMouseButton(0) && dice != null)
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Move the dice to the new position
                dice.transform.position = new Vector3(diceX, diceY, hit.point.z);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            dice = null;
        }
    }
}