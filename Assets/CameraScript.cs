using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    public enum RotationTarget
    {
        HEAD, UPPER_BODY, HALF_BODY
    }

    public Transform target;

    public float angle = 165f;
    public float distance = 2f;
    public float height = 1f;

    public RotationTarget rotationTarget = RotationTarget.UPPER_BODY;

    public float angleDamping = 1f;
    public float angleSharpness = 0.5f;

    public float positionDamping = 20f;
    public float positionSharpness = 0.5f;

    public float heightDamping = 1f;
    public float heightSharpness = 0.5f;

    public float rotationHorizontalDamping = 10f;
    public float rotationHorizontalSharpness = 0.5f;

    public float rotationVerticalDamping = 1f;
    public float rotationVerticalSharpness = 0.5f;
    
    void LateUpdate()
    {
        float blend;

        if (!target)
        {
            transform.position = new Vector3(0, 7.5f, 0);
            transform.rotation = Quaternion.Euler(new Vector3(10, 180, 0));
            return;
        }

        // calculate centroid
        Vector3 targetCentroid = AvatarSceneScript.CalculateCentroid(target);
        Vector3 targetHead = AvatarSceneScript.CalculateCentroid(target.Find("Avatar"));

        // calculate angle
        float wantedAngle = angle;
        //float currentAngle = Quaternion.FromToRotation(target.forward, transform.position - target.position).eulerAngles.y - 180f;
        //blend = 1f - Mathf.Pow(1f - angleSharpness, Time.deltaTime * angleDamping);
        //float lerpedAngle = Mathf.LerpAngle(currentAngle, wantedAngle, blend);
        
        // TODO: Lerped angle change, con questo codice currentAngle è calcolato
        // correttamente ma la vibrazione in caso di corsa porta la telecamera
        // a non avanzare correttamente e bloccarsi ad angolazioni sbagliate.
        // Possibile soluzione: Lerp pesato in base alla distanza del valore wanted e current!
        
        // calculate position
        Vector3 currentPosition = transform.position;
        Vector3 wantedPosition = target.position - (Quaternion.Euler(0, target.rotation.y + wantedAngle, 0) * target.forward * distance);
        blend = 1f - Mathf.Pow(1f - positionSharpness, Time.deltaTime * positionDamping);
        Vector3 lerpedPosition = Vector3.Lerp(currentPosition, wantedPosition, blend);
       
        // calculate height
        float currentHeight = transform.position.y;
        float wantedHeight = targetCentroid.y + height;
        blend = 1f - Mathf.Pow(1f - heightSharpness, Time.deltaTime * heightDamping);
        float lerpedHeight = Mathf.Lerp(currentHeight, wantedHeight, blend);

        // move camera to new position
        Vector3 newPosition;
        newPosition = lerpedPosition;
        newPosition.y = lerpedHeight;
        transform.position = newPosition;

        // calculate rotation
        Quaternion currentRotation = transform.rotation;
        Quaternion wantedRotation;

        switch(rotationTarget)
        {
            default:
            case RotationTarget.HEAD:
                wantedRotation = Quaternion.LookRotation(targetHead - transform.position);
                break;

            case RotationTarget.UPPER_BODY:
                wantedRotation = Quaternion.LookRotation((targetHead + targetCentroid) / 2 - transform.position);
                break;

            case RotationTarget.HALF_BODY:
                wantedRotation = Quaternion.LookRotation(targetCentroid - transform.position);
                break;
        }
       

        float horizontalBlend = 1f - Mathf.Pow(1f - rotationHorizontalSharpness, Time.deltaTime * rotationHorizontalDamping);
        float verticalBlend = 1f - Mathf.Pow(1f - rotationVerticalSharpness, Time.deltaTime * rotationVerticalDamping);
        Quaternion lerpedRotation = Quaternion.Euler(new Vector3(Mathf.LerpAngle(currentRotation.eulerAngles.x, wantedRotation.eulerAngles.x, verticalBlend), Mathf.LerpAngle(currentRotation.eulerAngles.y, wantedRotation.eulerAngles.y, horizontalBlend), Mathf.LerpAngle(currentRotation.eulerAngles.z, wantedRotation.eulerAngles.z, verticalBlend)));

        // rotate camera to lerped rotation
        Quaternion newRotation;
        newRotation = lerpedRotation;
        transform.rotation = newRotation;

    }
    
}