using UnityEngine;

public class DiceCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object has the tag "Dice"
        if (collision.gameObject.CompareTag("Dice"))
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
