using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeController : MonoBehaviour
{
    public GameObject head;
    public int positiveXAxis;
    public int negativeXAxis;
    public int positiveYAxis;
    public int negativeYAxis;
    public int blinkL;
    public int blinkR;

    public Transform target;
    public float targetMultiplierX = 2.0f;
    public float targetMultiplierY = 1.0f;

    public float target2dX = 0;
    public float target2dY = 0;

    public bool randomEyes = false;
    public float randomCompressor = 10.0f;

    public bool blinkable = true;
    public float blinkInterval = 3.0f;
    public float blinkSpeed = 0.33f;

    public float gizmoSize = 0.02f;
    
    private GameObject randomizerTarget;
    private bool isBlinking = false;
    private bool isBlinkedFull = false;
    private bool isBlinkinged = false;
    private float minz = -1.0f;
    
    void Update()
    {
        SkinnedMeshRenderer blendshapeEyeMesh = null;
        if (head != null)
        {
            blendshapeEyeMesh = head.GetComponent<SkinnedMeshRenderer>();
        }

        if (blendshapeEyeMesh)
        {
            if (target)
            {
                Transform headTransform = head.transform;
                headTransform.position = AvatarSceneScript.CalculateCentroid(head.transform);

                Vector3 newTgt = headTransform.InverseTransformPoint(target.transform.position);
               
                float x = newTgt.x * targetMultiplierX;
                float y = newTgt.y * targetMultiplierY;
                
                blendshapeEyeMesh.SetBlendShapeWeight(positiveXAxis, x > 0 ? x * 100f : 0); // +X
                blendshapeEyeMesh.SetBlendShapeWeight(negativeXAxis, x < 0 ? -x * 100f : 0); // -X
                blendshapeEyeMesh.SetBlendShapeWeight(positiveYAxis, y > 0 ? y * 100f : 0); // +Y
                blendshapeEyeMesh.SetBlendShapeWeight(negativeYAxis, y < 0 ? -y * 100f : 0); // -Y
            }
            else
            {
                blendshapeEyeMesh.SetBlendShapeWeight(positiveXAxis, target2dX > 0 ? target2dX * 100f : 0); // +X
                blendshapeEyeMesh.SetBlendShapeWeight(negativeXAxis, target2dX < 0 ? -target2dX * 100f : 0); // -X
                blendshapeEyeMesh.SetBlendShapeWeight(positiveYAxis, target2dY > 0 ? target2dY * 100f : 0); // +Y
                blendshapeEyeMesh.SetBlendShapeWeight(negativeYAxis, target2dY < 0 ? -target2dY * 100f : 0); // -Y
            }

            if (randomEyes)
            {
                if (!randomizerTarget)
                {
                    randomizerTarget = new GameObject("RNDTRG");
                    randomizerTarget.transform.position = blendshapeEyeMesh.transform.position;
                    target = randomizerTarget.transform;
                }
                else
                {
                    target.position += Random.onUnitSphere * Time.fixedDeltaTime / randomCompressor;
                }
            }

            if (blinkable)
            {
                float blinkVal = blendshapeEyeMesh.GetBlendShapeWeight(blinkL);

                if (Time.time - blinkInterval > minz && !isBlinking && !isBlinkedFull && !isBlinkinged)
                {
                    minz = Time.time - Time.deltaTime;
                    isBlinking = true;
                }

                if (isBlinking)
                {
                    if (!isBlinkinged)
                    {
                        if (blinkVal < 99 && !isBlinkedFull)
                        {
                            blinkVal = Mathf.Lerp(blinkVal, 100, blinkSpeed);
                            if (blinkVal >= 99) isBlinkedFull = true;
                        }

                        if (isBlinkedFull)
                        {
                            blinkVal = Mathf.Lerp(blinkVal, 0.0f, blinkSpeed);
                            if (blinkVal <= 2.0f) isBlinkinged = true;
                        }
                    }

                    blendshapeEyeMesh.SetBlendShapeWeight(blinkL, blinkVal);
                    blendshapeEyeMesh.SetBlendShapeWeight(blinkR, blinkVal);

                    if (isBlinkinged)
                    {
                        blendshapeEyeMesh.SetBlendShapeWeight(blinkL, 0);
                        blendshapeEyeMesh.SetBlendShapeWeight(blinkR, 0);
                        isBlinkinged = false;
                        isBlinking = false;
                        isBlinkedFull = false;
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (target && head)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, gizmoSize);
            Gizmos.DrawLine(AvatarSceneScript.CalculateCentroid(head.transform), target.position);
        }
    }
}
