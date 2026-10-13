using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Minigame2 : Minigame1
{
    private static bool canSpawnFirefly = true;
    public float delayDash = 0.0f;


    public static bool getCanSpawnFirefly()
    {
        return canSpawnFirefly;
    }

    public static void setCanSpawnFirefly(bool value)
    {
        canSpawnFirefly = value;
    }

    protected override IEnumerator MoveTo(Vector3 target)
    {
        canSpawnFirefly = false;
        yield return new WaitForSeconds(delayDash);
        hasHit = false;
        while (transform.position != target) {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }

    protected override IEnumerator TurnAround()
    {
        canSpawnFirefly = true;
        Quaternion target = transform.rotation * Quaternion.Euler(0f, 180f, 0f);
        while (transform.rotation != target) {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, 360f* Time.deltaTime);
            yield return null;
        }
    }
}
