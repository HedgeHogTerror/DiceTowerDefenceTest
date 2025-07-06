using UnityEngine;

public class DiceCollision : MonoBehaviour
{
    public bool isMoving = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (isMoving)
        {
            var collidedDiceHeight = collision.gameObject.transform.position.y;

            // Log a message to the console
            Debug.Log("Dice collided with: " + collision.gameObject.name);

            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + collidedDiceHeight,
                transform.position.z
            );
        }
    }
}
