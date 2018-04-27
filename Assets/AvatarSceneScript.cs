
using ItSeez3D.AvatarSdk.Core;
using ItSeez3D.AvatarSdkSamples.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarSceneScript : MonoBehaviour
{
    public GameObject playerObj = null;
    public GameObject cameraObj = null;

    private IAvatarProvider avatarProvider = null;
    
    void Start()
    {
        Screen.fullScreen = false;
        Screen.orientation = ScreenOrientation.Portrait;

        AppSocket.SetOnReceiveHandler(OnReceive);

        if (!AvatarSdkMgr.IsInitialized)
            AvatarSdkMgr.Init();

        avatarProvider = AvatarSdkMgr.IoCContainer.Create<IAvatarProvider>();
        QualitySettings.antiAliasing = 2;

        AppSocket.SendCommand(AppSocket.SendType.UNITY_LOADED);

        if (Application.platform != RuntimePlatform.Android)
        {
            QualitySettings.antiAliasing = 8;
            String marcoBody = "mech_ferrari";
            String marcoAvatar = "3907ed35-7071-4ed1-b242-169cb3abc237";
            String marcoHair = "male_NewSea_J082m";
            String marcoHairColor = "#3E271F";

            String paolaBody = "sport_female_m";
            String paolaAvatar = "7d910c92-b62c-4084-8f42-749a2cb94eb2";
            String paolaHair = "female_NewSea_J096f";
            String paolaHairColor = "#202020";

            String aliceBody = "medic_female";
            String aliceAvatar = "063f8e92-4675-4bee-8f4f-a303779b4e3c";
            String aliceHair = "female_NewSea_J123f";
            String aliceHairColor = "#3E271F";
            

            if (playerObj != null)
                CreateAvatar("b7ce00b9-3589-4dbd-9b5b-2466233e9d89", paolaAvatar, paolaHair, paolaHairColor);
            else            
                CreateAvatar("b7ce00b9-3589-4dbd-9b5b-2466233e9d89", marcoBody, marcoAvatar, marcoHair, marcoHairColor);           
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            AnimGreetings(playerObj);
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            GameObject avatar = playerObj.transform.Find("Avatar").gameObject;
            AudioSource audioSource = avatar.GetComponent<AudioSource>();
            
            audioSource.clip = (AudioClip)Resources.Load("Registrazione");
            audioSource.Play();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            GameObject avatar = playerObj.transform.Find("Avatar").gameObject;
            AudioSource audioSource = avatar.GetComponent<AudioSource>();
            
            audioSource.Pause();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            SetBackgroundColor("#952f48");
        }
    }
   
    private void OnReceive(AppSocket.ReceiveType receiveType, params string[] parameters)
    {
        Debug.Log("Received command: " + receiveType.ToString());

        switch(receiveType)
        {
            case AppSocket.ReceiveType.BG_COLOR:
                if(parameters.Length != 1 || !SetBackgroundColor(parameters[0]))
                    AppSocket.SendCommand(AppSocket.SendType.ERROR_PARAMS);
                break;
            case AppSocket.ReceiveType.AVATAR_REMOVE:
                Destroy(playerObj);
                playerObj = null;
                break;
            case AppSocket.ReceiveType.ANIM_RUN_TRUE:
                AnimRunning(playerObj, true);
                break;
            case AppSocket.ReceiveType.ANIM_RUN_FALSE:
                AnimRunning(playerObj, false);
                break;
            case AppSocket.ReceiveType.ANIM_GREETINGS:
                AnimGreetings(playerObj);
                break;
            case AppSocket.ReceiveType.AVATAR_CREATE:
                if(parameters.Length != 5)
                    AppSocket.SendCommand(AppSocket.SendType.ERROR_PARAMS);
                else
                    CreateAvatar(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4]);
                break;

            case AppSocket.ReceiveType.CAMERA_TARGET:
                if (parameters.Length != 1)
                    AppSocket.SendCommand(AppSocket.SendType.ERROR_PARAMS);
                else
                    SetCameraTarget(parameters[0]);
                break;

            case AppSocket.ReceiveType.CAMERA_ANGLE:
                if (parameters.Length != 1)
                    AppSocket.SendCommand(AppSocket.SendType.ERROR_PARAMS);
                else
                    SetCameraAngle(parameters[0]);
                break;

            case AppSocket.ReceiveType.CAMERA_DISTANCE:
                if (parameters.Length != 1)
                    AppSocket.SendCommand(AppSocket.SendType.ERROR_PARAMS);
                else
                    SetCameraDistance(parameters[0]);
                break;

            case AppSocket.ReceiveType.CAMERA_HEIGHT:
                if (parameters.Length != 1)
                    AppSocket.SendCommand(AppSocket.SendType.ERROR_PARAMS);
                else
                    SetCameraHeight(parameters[0]);
                break;

            case AppSocket.ReceiveType.SPEECH:
                if(parameters.Length != 1)
                    AppSocket.SendCommand(AppSocket.SendType.ERROR_PARAMS);
                else
                    StartCoroutine(Speech(playerObj.transform.Find("Avatar").gameObject, parameters[0]));
                break;

            case AppSocket.ReceiveType.TEST:
                CreateAvatar("34fab30e-dd0b-4c38-bf43-86360c348db5", "sport_male_m", "c969ecd1-eb27-418c-a26c-1395cb01abbf", "male_makehuman_short02", "#222");
                break;
            default:
                break;
        }
    }

    private bool SetBackgroundColor(string hex)
    {
        if (cameraObj == null)
            return false;
       
        Camera camera = cameraObj.GetComponent<Camera>();
        if (camera == null)
            return false;

        Color color;
        bool validColor = ColorUtility.TryParseHtmlString(hex, out color);

        if (!validColor)
            return false;

        camera.backgroundColor = color;
        return true;
    }

    private void SetCameraTarget(string target)
    {
        if (cameraObj == null)
            return;

        switch (target)
        {
            case AppProtocol.AppToUnity.PARAM_CAMERA_TARGET_HEAD:
                cameraObj.GetComponent<CameraScript>().rotationTarget = CameraScript.RotationTarget.HEAD;
                break;
            default:
            case AppProtocol.AppToUnity.PARAM_CAMERA_TARGET_UPPER_BODY:
                cameraObj.GetComponent<CameraScript>().rotationTarget = CameraScript.RotationTarget.UPPER_BODY;
                break;
            case AppProtocol.AppToUnity.PARAM_CAMERA_TARGET_HALF_BODY:
                cameraObj.GetComponent<CameraScript>().rotationTarget = CameraScript.RotationTarget.HALF_BODY;
                break;
        }
    }

    private void SetCameraAngle(string angleStr)
    {
        if (cameraObj == null)
            return;

        int angle;
        if (!int.TryParse(angleStr, out angle))
            return;

        cameraObj.GetComponent<CameraScript>().angle = angle;
    }

    private void SetCameraDistance(string distanceStr)
    {
        if (cameraObj == null)
            return;

        float distance;
        if (!float.TryParse(distanceStr, out distance))
            return;

        cameraObj.GetComponent<CameraScript>().distance = distance;
    }

    private void SetCameraHeight(string heightStr)
    {
        if (cameraObj == null)
            return;

        float height;
        if (!float.TryParse(heightStr, out height))
            return;

        cameraObj.GetComponent<CameraScript>().height = height;
    }

    private void CreateAvatar(string playerId, string bodyId, string avatarCode, string haircutIdentity = null, string hairColor = "#222")
    {
        AppSocket.SendCommand(AppSocket.SendType.AVATAR_LOADING);
        StartCoroutine(Initialize(playerId, bodyId, avatarCode, haircutIdentity, hairColor));
    }

    private void CreateAvatar(string playerId, string avatarCode, string haircutIdentity = null, string hairColor = "#222")
    {
        AppSocket.SendCommand(AppSocket.SendType.AVATAR_LOADING);
        StartCoroutine(Initialize(playerId, avatarCode, haircutIdentity, hairColor));
    }

    #region Avatar creation
    private IEnumerator Initialize(string playerId, string bodyId, string avatarCode, string haircutIdentity, string hairColor)
    {
        yield return Await(avatarProvider.InitializeAsync(playerId));

        playerObj = CreateBody(bodyId);       

        BodyAttachment bodyAttach = playerObj.transform.Find("body_attachment").GetComponent<BodyAttachment>();
        StartCoroutine(DownloadAvatar(bodyAttach, avatarCode, haircutIdentity, hairColor));
    }

    private IEnumerator Initialize(string playerId, string avatarCode, string haircutIdentity, string hairColor)
    {
        yield return Await(avatarProvider.InitializeAsync(playerId));
        
        BodyAttachment bodyAttach = playerObj.transform.Find("body_attachment").GetComponent<BodyAttachment>();
        StartCoroutine(DownloadAvatar(bodyAttach, avatarCode, haircutIdentity, hairColor));
    }

    private GameObject CreateBody(string bodyId)
    {
        GameObject body = Instantiate(Resources.Load(bodyId), Vector3.zero, Quaternion.identity) as GameObject;
        return body;
    }

    private IEnumerator DownloadAvatar(BodyAttachment bodyAttachment, string avatarCode, string haircutIdentity, string hairColor)
    {
        // download head
        var headRequest = avatarProvider.GetHeadMeshAsync(avatarCode, true);
        yield return Await(headRequest);
        TexturedMesh headTexturedMesh = headRequest.Result;

        // download  haircut
        TexturedMesh haircutTexturedMesh = null;
        if (haircutIdentity != null)
        {
            var haircutRequest = avatarProvider.GetHaircutMeshAsync(avatarCode, haircutIdentity);
            yield return Await(haircutRequest);
            haircutTexturedMesh = haircutRequest.Result;
        }
        
        // display head
        DisplayHead(bodyAttachment, headTexturedMesh, haircutTexturedMesh, hairColor);

        // finish!
        FinishAvatar();
    }
    
    private void DisplayHead(BodyAttachment bodyAttachment, TexturedMesh headMesh, TexturedMesh haircutMesh, string hairColor)
    {
        var avatarObject = new GameObject("Avatar");

        // create head object in the scene
        SkinnedMeshRenderer headRenderer = null;
        if (headMesh != null) {
            var meshObject = new GameObject("HeadObject");
            headRenderer = meshObject.AddComponent<SkinnedMeshRenderer>();

            headRenderer.sharedMesh = headMesh.mesh;
            var material = new Material(Shader.Find("AvatarUnlitShader"));
            material.mainTexture = headMesh.texture;
            headRenderer.material = material;
            meshObject.transform.SetParent(avatarObject.transform);
        }

        // create haircut object in the scene
        if (haircutMesh != null)
        {
            var meshObject = new GameObject("HaircutObject");
            var meshRenderer = meshObject.AddComponent<SkinnedMeshRenderer>();

            meshRenderer.sharedMesh = haircutMesh.mesh;
            var material = new Material(Shader.Find("AvatarUnlitHairShader"));
            material.mainTexture = haircutMesh.texture;
            meshRenderer.material = material;

            if (!ChangeMeshTint(meshRenderer, hairColor))
                Debug.LogError("Error changing hair color!");

            meshObject.transform.SetParent(avatarObject.transform);
        }

        // attach head 
        if (bodyAttachment != null)
        {
            bodyAttachment.AttachHeadToBody(avatarObject);
        }

        // change body skin color match head color
        if (bodyAttachment != null && headRenderer != null)
        {
            MatchTwoMeshTint(bodyAttachment.body.GetComponent<SkinnedMeshRenderer>(), headRenderer);
        }
    }

    private void FinishAvatar()
    {
        GameObject avatar = playerObj.transform.Find("Avatar").gameObject;
        GameObject head = avatar.transform.Find("HeadObject").gameObject;
        SkinnedMeshRenderer skinnedMeshRenderer = head.GetComponent<SkinnedMeshRenderer>();

        // Add audio source to player for speaking sound
        AudioSource audioSource = avatar.AddComponent<AudioSource>();
       
        OVRLipSyncContext ovrContext = avatar.AddComponent<OVRLipSyncContext>();
        ovrContext.audioSource = audioSource;
        ovrContext.audioMute = false;
        
        OVRLipSyncContextMorphTarget ovrMorphTarget = avatar.AddComponent<OVRLipSyncContextMorphTarget>();
        ovrMorphTarget.skinnedMeshRenderer = skinnedMeshRenderer;

        ovrMorphTarget.VisemeToBlendTargets[0] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("sil");
        ovrMorphTarget.VisemeToBlendTargets[1] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("pp");
        ovrMorphTarget.VisemeToBlendTargets[2] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("ff");
        ovrMorphTarget.VisemeToBlendTargets[3] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("TH");
        ovrMorphTarget.VisemeToBlendTargets[4] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("dd");
        ovrMorphTarget.VisemeToBlendTargets[5] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("kk");
        ovrMorphTarget.VisemeToBlendTargets[6] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("CH");
        ovrMorphTarget.VisemeToBlendTargets[7] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("ss");
        ovrMorphTarget.VisemeToBlendTargets[8] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("nn");
        ovrMorphTarget.VisemeToBlendTargets[9] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("rr");
        ovrMorphTarget.VisemeToBlendTargets[10] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("AA");
        ovrMorphTarget.VisemeToBlendTargets[11] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("EE");
        ovrMorphTarget.VisemeToBlendTargets[12] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("IH");
        ovrMorphTarget.VisemeToBlendTargets[13] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("OH");
        ovrMorphTarget.VisemeToBlendTargets[14] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("OU");

        ovrMorphTarget.KeySendVisemeSignal = new int[15];
        ovrMorphTarget.KeySendVisemeSignal[0] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("sil");
        ovrMorphTarget.KeySendVisemeSignal[1] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("pp");
        ovrMorphTarget.KeySendVisemeSignal[2] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("ff");
        ovrMorphTarget.KeySendVisemeSignal[3] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("TH");
        ovrMorphTarget.KeySendVisemeSignal[4] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("dd");
        ovrMorphTarget.KeySendVisemeSignal[5] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("kk");
        ovrMorphTarget.KeySendVisemeSignal[6] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("CH");
        ovrMorphTarget.KeySendVisemeSignal[7] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("ss");
        ovrMorphTarget.KeySendVisemeSignal[8] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("nn");
        ovrMorphTarget.KeySendVisemeSignal[9] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("rr");
        ovrMorphTarget.KeySendVisemeSignal[10] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("AA");
        ovrMorphTarget.KeySendVisemeSignal[11] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("EE");
        ovrMorphTarget.KeySendVisemeSignal[12] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("IH");
        ovrMorphTarget.KeySendVisemeSignal[13] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("OH");
        ovrMorphTarget.KeySendVisemeSignal[14] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("OU");

        ovrMorphTarget.SmoothAmount = 10;
        
        ovrContext.enabled = false;
        ovrContext.enabled = true;
        
        EyeController eyeController = avatar.AddComponent<EyeController>();
        eyeController.head = avatar.transform.Find("HeadObject").gameObject;
        eyeController.positiveXAxis = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("EyesRight");
        eyeController.positiveYAxis = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("EyesUp");
        eyeController.negativeXAxis = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("EyesLeft");
        eyeController.negativeYAxis = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("EyesDown");

        eyeController.blinkL = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("EyeBlink_L");
        eyeController.blinkR = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("EyeBlink_R");
        
        // Set camera
        if (cameraObj != null)
        {
            eyeController.target = cameraObj.transform;
            cameraObj.GetComponent<CameraScript>().target = playerObj.transform;
        }

        // send finish command
        AppSocket.SendCommand(AppSocket.SendType.AVATAR_FINISH);
    }
    #endregion
    #region Animations

    private static void AnimGreetings(GameObject avatar)
    {
        Animator animator = avatar.GetComponent<Animator>();
        animator.SetTrigger("isGreeting");
    }

    private static void AnimRunning(GameObject avatar, bool run)
    {
        Animator animator = avatar.GetComponent<Animator>();
        animator.SetBool("isRunning", run);
    }
    #endregion

    #region Speaking 
    private static IEnumerator Speech(GameObject avatar, string fullPath)
    {
        AudioSource audioSource = avatar.GetComponent<AudioSource>();
        if (fullPath != null && fullPath.Trim().Length > 0)
        {
            AudioClip audioClip = null;
            
            // Application.temporaryCachePath = /storage/emulated/0/Android/data/it.unitn.robertonegro.facetoface/cache
            // using (WWW www = new WWW("file://" + Application.temporaryCachePath + "/" + fileName))
            
            using (WWW www = new WWW(fullPath))
            {
                while (!www.isDone)
                    yield return null;

                if (www.error != null)
                    Debug.LogError("Error loading file!");
                else
                    audioClip = www.GetAudioClip(true, false, AudioType.MPEG);
            }

            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                audioSource.Play();
                AppSocket.SendCommand(AppSocket.SendType.SPEECH_START, fullPath);
                yield return new WaitForSeconds(audioClip.length);
                AppSocket.SendCommand(AppSocket.SendType.SPEECH_FINISH, fullPath);
            }
        }
        else
        {
            audioSource.Pause();
        }
        yield break;
    }
   
    #endregion

    #region Static methods
    public static Vector3 CalculateCentroid(Transform obj)
    {
        Vector3 targetCentroid;
        var targetRenderer = obj.GetComponent<Renderer>();
        if (targetRenderer == null)
        {
            Transform[] allChildren = obj.GetComponentsInChildren<Transform>();

            var minPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var maxPoint = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            bool atLeastOne = false;
            foreach (Transform child in allChildren)
            {
                var renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (!atLeastOne)
                        atLeastOne = true;

                    Vector3 pos = renderer.bounds.center;
                    if (pos.x < minPoint.x)
                        minPoint.x = pos.x;
                    if (pos.x > maxPoint.x)
                        maxPoint.x = pos.x;
                    if (pos.y < minPoint.y)
                        minPoint.y = pos.y;
                    if (pos.y > maxPoint.y)
                        maxPoint.y = pos.y;
                    if (pos.z < minPoint.z)
                        minPoint.z = pos.z;
                    if (pos.z > maxPoint.z)
                        maxPoint.z = pos.z;
                }
            }
            if (!atLeastOne)
                return Vector3.zero;

            targetCentroid = minPoint + 0.5f * (maxPoint - minPoint);
        }
        else
        {
            targetCentroid = targetRenderer.bounds.center;
        }

        return targetCentroid;
    }

    private static bool ChangeMeshTint(SkinnedMeshRenderer meshRenderer, string hex, float coefficient = 0.8f)
    {
        Color color;
        bool validColor = ColorUtility.TryParseHtmlString(hex, out color);

        if (meshRenderer == null || !validColor)
            return false;
        
        Color averageColor = CoreTools.CalculateAverageColor(meshRenderer.material.mainTexture as Texture2D);               
        Vector4 tint = CoreTools.CalculateTint(color, averageColor);

        meshRenderer.material.SetVector("_ColorTarget", color);
        meshRenderer.material.SetVector("_ColorTint", tint);
        meshRenderer.material.SetFloat("_TintCoeff", coefficient);

        return true;
    }
   
    private static bool MatchTwoMeshTint(SkinnedMeshRenderer meshChanging, SkinnedMeshRenderer meshTarget, float coefficient = 0f, float threshold = 0.55f)
    {
        if (meshChanging == null || meshTarget == null)
            return false;

        Color colorChanging = CoreTools.CalculateAverageColor(meshChanging.material.mainTexture as Texture2D);
        Color colorTarget = CoreTools.CalculateAverageColor(meshTarget.material.mainTexture as Texture2D);
        Vector4 tint = CoreTools.CalculateTint(colorTarget, colorChanging);

        meshChanging.material.SetVector("_ColorTarget", colorTarget);
        meshChanging.material.SetVector("_ColorTint", tint);
        meshChanging.material.SetFloat("_TintCoeff", coefficient);
        meshChanging.material.SetFloat("_MultiplicationTintThreshold", threshold);

        return true;
    }

    private static IEnumerator Await(params AsyncRequest[] requests)
    {
        foreach (var r in requests)
            while (!r.IsDone)
            {
                // yield null to wait until next frame (to avoid blocking the main thread)
                yield return null;

                // This function will throw on any error. Such primitive error handling only provided as
                // an example, the production app probably should be more clever about it.
                if (r.IsError)
                {
                    Debug.LogError(r.ErrorMessage);
                    throw new Exception(r.ErrorMessage);
                }

                // Each requests may or may not contain "subrequests" - the asynchronous subtasks needed to
                // complete the request. The progress for the requests can be tracked overall, as well as for
                // every subtask. The code below shows how to recursively iterate over current subtasks
                // to display progress for them.
                var progress = new List<string>();
                AsyncRequest request = r;
                while (request != null)
                {
                    progress.Add(string.Format("{0}: {1}%", request.State, request.ProgressPercent.ToString("0.0")));
                    request = request.CurrentSubrequest;
                }

                Debug.Log(string.Join("\n", progress.ToArray()));
            }
    }
    #endregion
}

