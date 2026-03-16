using UnityEngine;

public class WeaponColliderDetector : MonoBehaviour
{
    public delegate void OnCollisionStayCallBack(Collider other);
    public OnCollisionStayCallBack onCollisionStayCallBack;

    private void OnTriggerStay(Collider collider) {
        if (onCollisionStayCallBack != null)
            onCollisionStayCallBack(collider);
    }
}
