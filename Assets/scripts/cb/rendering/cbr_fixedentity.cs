using UnityEngine;

// These entities are bound to a specific celestial body, and are rendered based on the position, rotation, and scale of that body
// in other words, they do not need to be a part of the floating rendering system
[System.Serializable]
public class cbr_fixedentity {
    public Transform reference;
    public int bodyIndex; // What celestial body is this bound to?
    public Vector3 defaultPosition; // local position at planet's full scale
    public Vector3 defaultRotation; // local rotation at planet's full scale (although scale doesn't matter for this one)
    public float defaultScale;

    public cbr_fixedentity(Transform _ref, int _body) {
        reference = _ref;
        bodyIndex = _body;
        Transform planetTransform = CBManager.Instance.celestialBodies[bodyIndex].reference;
        defaultPosition = (_ref.position - planetTransform.position) / planetTransform.localScale.x; // difference in position relative to planet
        defaultRotation = _ref.eulerAngles - planetTransform.eulerAngles; // difference in rotation relative to planet
        defaultScale = _ref.localScale.x;
    }

    public void Refresh() {
        // for now just set the unity position to game position
        Transform planetTransform = CBManager.Instance.celestialBodies[bodyIndex].reference;
        reference.position = planetTransform.position + CBManager.Instance.AdjustVector(defaultPosition, 0) * planetTransform.localScale.x;
        reference.eulerAngles = planetTransform.eulerAngles + defaultRotation;
        reference.localScale = Vector3.one * defaultScale * (planetTransform.localScale.x / CBManager.Instance.celestialBodies[bodyIndex].defaultScale);
    }

    public Vector3 GetPosition() {
        Transform planetTransform = CBManager.Instance.celestialBodies[bodyIndex].reference;
        return planetTransform.position + defaultPosition * planetTransform.localScale.x;
    }

    public Vector3 GetRotation() {
        Transform planetTransform = CBManager.Instance.celestialBodies[bodyIndex].reference;
        return reference.eulerAngles = planetTransform.eulerAngles + defaultRotation;
    }
}