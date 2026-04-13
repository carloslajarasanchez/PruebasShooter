using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    public float pushForce = 10f;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb == null || rb.isKinematic) return;

        //comprobar si es una puerta empujable
        var pusheable = hit.collider.GetComponent<IPusheable>();
        Debug.Log("Collided with: " + hit.collider.name);
        if (pusheable == null) return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        if (pushDir.magnitude < 0.1f) return;

        pusheable.Push(pushDir * pushForce);
    }
}