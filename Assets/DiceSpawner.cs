using UnityEngine;

public class DiceSpawner : MonoBehaviour
{
    [SerializeField] private Transform platform;
    [SerializeField] private Transform abovePlatform;
    [SerializeField] private GameObject[] diceCollection;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            SpawnDice();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            RollDice(Random.Range(1, 4));
        }
    }

    /**
     * Spawn a dice on the platform.
     */
    public void SpawnDice()
    {
        int diceIndex = Random.Range(0, diceCollection.Length);
        Vector3 spawnPosition = platform.position;

        spawnPosition.y += 2f; // Adjust the height to be slightly above the platform
        var newDice = Instantiate(diceCollection[diceIndex], spawnPosition, Quaternion.identity);
        newDice.transform.SetParent(transform);
    }

    public void RollDice(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int diceIndex = Random.Range(0, diceCollection.Length);
            GameObject newDice = Instantiate(diceCollection[diceIndex], abovePlatform.position, Quaternion.identity);

            newDice.transform.SetParent(transform);
            RollDice(newDice);
        }
    }

    /**
    * Roll newDice above the platform.
    */
    public void RollDice(GameObject newDice)
    {
        Rigidbody newDiceRigidbody = newDice.GetComponent<Rigidbody>();
        Vector3 randomForce = new Vector3(
            Random.Range(-2f, 2f),
            Random.Range(-2f, 2f),
            Random.Range(-2f, 2f)
        );
        Vector3 randomTorque = Random.insideUnitSphere * 5f;

        newDiceRigidbody.AddForce(randomForce, ForceMode.Impulse);
        newDiceRigidbody.AddTorque(randomTorque, ForceMode.Impulse);
    }
}
