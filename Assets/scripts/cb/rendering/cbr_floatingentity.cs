using UnityEngine;

// Data class for any object being rendered with floating origin + scale
// the player, planets, ships
[System.Serializable]
public class cbr_floatingentity {
    public Transform reference;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 velocity;
    public float defaultScale; // the scale the object should be at

    public cbr_floatingentity(Transform _ref) {
        reference = _ref;
        position = _ref.position;
        rotation = _ref.eulerAngles;
        defaultScale = _ref.localScale.x;
        Refresh();
    }

    public cbr_floatingentity(Transform _ref, Vector3 _pos, Vector3 _rot) {
        reference = _ref;
        position = _pos;
        rotation = _rot;
        defaultScale = _ref.localScale.x;
        Refresh();
    }

    public cbr_floatingentity(Transform _ref, Vector3 _pos, float _scl) {
        reference = _ref;
        position = _pos;
        rotation = _ref.eulerAngles;
        defaultScale = _scl;
        Refresh();
    }

    // Update the position (unity space) based on position (game space)
    public void Refresh() {
        if (reference.gameObject.GetComponent<Rigidbody>() != null) {
            if (CBManager.Instance.EntityInsideRenderRadius(this)) {
                // for now just set the unity position to game position
                position = reference.position - CBManager.Instance.GetOriginInGameSpace();
                rotation = reference.eulerAngles;
                velocity = reference.gameObject.GetComponent<Rigidbody>().velocity;
            }
            else { // outside render radius (not doing this yet)
                reference.position = position + CBManager.Instance.worldOffset;
                reference.eulerAngles = rotation;
            }
        }
        else {
            if (CBManager.Instance.entityInControl.reference != null) {
                reference.position = position + CBManager.Instance.worldOffset;
                Vector3 camPosition = CBManager.Instance.entityInControl.position + CBManager.Instance.GetCameraOffset();

                if ((camPosition - position).magnitude > CBManager.Instance.renderRadius + 1) {
                    if ((camPosition - position).magnitude < 2000 * 0.525f) {
                        // inflate
                        reference.localScale = Vector3.one * defaultScale;
                        reference.position = CBManager.Instance.entityInControl.reference.position + CBManager.Instance.GetCameraOffset() + (position - camPosition);
                    }
                    else { // far from planet
                        reference.localScale = Vector3.one * defaultScale * (CBManager.Instance.renderRadius / (camPosition - position).magnitude);
                        reference.position = CBManager.Instance.entityInControl.reference.position + CBManager.Instance.GetCameraOffset() + (position - camPosition).normalized * CBManager.Instance.renderRadius;
                    }
                }
                else {
                    reference.localScale = Vector3.one;
                    reference.position = CBManager.Instance.entityInControl.reference.position + CBManager.Instance.GetCameraOffset() + (position - camPosition);
                }
            }
            
            rotation += new Vector3(0, CBManager.Instance.planetRotSpeed, 0);
            reference.eulerAngles = rotation;
        }
    }
}