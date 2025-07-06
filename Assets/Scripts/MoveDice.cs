using UnityEngine;

public class MoveDice : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    private GameObject dice;

    private void Update()
    {
        GetTargetDice();

        MoveTargetDice();

        if (Input.GetMouseButtonUp(0) && dice != null)
        {
            dice.GetComponent<Rigidbody>().isKinematic = false; // Re-enable physics
            dice.GetComponent<DiceCollision>().isMoving = false;
            dice = null;
        }
    }

    private void MoveTargetDice()
    {
        if (Input.GetMouseButton(0) && dice != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            RaycastHit hit;
            Vector3 targetPosition;

            plane.Raycast(ray, out float point);
            targetPosition = ray.GetPoint(point);

            // Keep dice at its original Y position
            targetPosition.y = dice.transform.position.y;

            if (targetPosition != dice.transform.position)
            {
                // Move the dice to the new position
                dice.transform.position = Vector3.MoveTowards(
                    dice.transform.position,
                    targetPosition,
                    moveSpeed * Time.deltaTime
                );
            }
        }
    }

    private void GetTargetDice()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.gameObject.tag == "Dice")
            {
                dice = hit.transform.gameObject;
                dice.GetComponent<Rigidbody>().isKinematic = true; // Disable gravity and other forces while moving
                dice.GetComponent<DiceCollision>().isMoving = true;
            }
        }
    }
}