using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppSocket : MonoBehaviour
{
    public enum SendType
    {
        UNITY_LOADED, AVATAR_LOADING, AVATAR_FINISH, SPEECH_START, SPEECH_FINISH, ERROR_COMMAND, ERROR_PARAMS
    }
    public enum ReceiveType
    {
        BG_COLOR, ANIM_RUN_TRUE, ANIM_RUN_FALSE, ANIM_GREETINGS, AVATAR_REMOVE, AVATAR_CREATE, SPEECH, CAMERA_TARGET, CAMERA_ANGLE, CAMERA_DISTANCE, CAMERA_HEIGHT, TEST, ERROR_PARAMS, ERROR_COMMAND
    }

    public static void SendCommand(SendType sendType, params string[] parameters)
    {
        switch (sendType)
        {
            case SendType.UNITY_LOADED:
                SendRawValue(AppProtocol.CreateCommand(AppProtocol.UnityToApp.UNITY_LOADED));
                break;
            case SendType.AVATAR_LOADING:
                SendRawValue(AppProtocol.CreateCommand(AppProtocol.UnityToApp.AVATAR_LOADING));
                break;
            case SendType.AVATAR_FINISH:
                SendRawValue(AppProtocol.CreateCommand(AppProtocol.UnityToApp.AVATAR_FINISH));
                break;
            case SendType.SPEECH_START:
                SendRawValue(AppProtocol.CreateCommand(AppProtocol.UnityToApp.SPEECH_START, parameters));
                break;
            case SendType.SPEECH_FINISH:
                SendRawValue(AppProtocol.CreateCommand(AppProtocol.UnityToApp.SPEECH_FINISH, parameters));
                break;
            case SendType.ERROR_PARAMS:
                SendRawValue(AppProtocol.CreateCommand(AppProtocol.UnityToApp.ERROR_PARAMS));
                break;
            case SendType.ERROR_COMMAND:
            default:
                SendRawValue(AppProtocol.CreateCommand(AppProtocol.UnityToApp.ERROR_COMMAND));
                break;
        }
    }

    private static void CheckReceivedValue(string value)
    {
        if (AppProtocol.IsCommand(value))
        {
            string command = AppProtocol.GetCommand(value);

            switch (command)
            {
                case AppProtocol.AppToUnity.BG_COLOR:
                    string color = AppProtocol.GetParam(value);
                    OnReceive(ReceiveType.BG_COLOR, color);
                    break;

                case AppProtocol.AppToUnity.AVATAR_REMOVE:
                    OnReceive(ReceiveType.AVATAR_REMOVE);
                    break;

                case AppProtocol.AppToUnity.ANIM_RUN:
                    switch (AppProtocol.GetParam(value))
                    {
                        case AppProtocol.AppToUnity.PARAM_TRUE:
                            OnReceive(ReceiveType.ANIM_RUN_TRUE);
                            break;
                        case AppProtocol.AppToUnity.PARAM_FALSE:
                            OnReceive(ReceiveType.ANIM_RUN_FALSE);
                            break;
                        default:
                            SendCommand(SendType.ERROR_PARAMS);
                            break;
                    }
                    break;

                case AppProtocol.AppToUnity.ANIM_GREETINGS:
                    OnReceive(ReceiveType.ANIM_GREETINGS);
                    break;

                case AppProtocol.AppToUnity.AVATAR_CREATE:
                    string[] avatarAttrs = AppProtocol.GetParams(value);
                    if (avatarAttrs.Length != 5)
                        SendCommand(SendType.ERROR_PARAMS);
                    else
                        OnReceive(ReceiveType.AVATAR_CREATE, avatarAttrs);
                    break;

                case AppProtocol.AppToUnity.SPEECH:
                    string filename = AppProtocol.GetParam(value);
                    OnReceive(ReceiveType.SPEECH, filename);
                    break;

                case AppProtocol.AppToUnity.CAMERA_TARGET:
                    string target = AppProtocol.GetParam(value);

                    if (!target.Equals(AppProtocol.AppToUnity.PARAM_CAMERA_TARGET_HALF_BODY) && !target.Equals(AppProtocol.AppToUnity.PARAM_CAMERA_TARGET_UPPER_BODY) && !target.Equals(AppProtocol.AppToUnity.PARAM_CAMERA_TARGET_HEAD))
                    {
                        Debug.LogError("[APPSOCKET] Received invalid target param: " + target);
                        SendCommand(SendType.ERROR_PARAMS);
                    }
                    else
                        OnReceive(ReceiveType.CAMERA_TARGET, target);
                    break;

                case AppProtocol.AppToUnity.CAMERA_ANGLE:
                    string angleStr = AppProtocol.GetParam(value);
                    int angle;

                    if (!int.TryParse(angleStr, out angle))
                    {
                        Debug.LogError("[APPSOCKET] Received invalid angle param. It's not a valid integer: " + angleStr);
                        SendCommand(SendType.ERROR_PARAMS);
                    }
                    else
                    {
                        if (angle < 0 || angle > 360)
                        {
                            Debug.LogError("[APPSOCKET] Received invalid angle param. Should be [0, 360]: " + angleStr);
                            SendCommand(SendType.ERROR_PARAMS);
                        }
                        else
                            OnReceive(ReceiveType.CAMERA_ANGLE, angleStr);
                    }
                    break;

                case AppProtocol.AppToUnity.CAMERA_DISTANCE:
                    string distanceStr = AppProtocol.GetParam(value);
                    float distance;

                    if (!float.TryParse(distanceStr, out distance))
                    {
                        Debug.LogError("[APPSOCKET] Received invalid distance param. It's not a valid float: " + distanceStr);
                        SendCommand(SendType.ERROR_PARAMS);
                    }
                    else
                    {
                        if (distance < 0 || distance > 100)
                        {
                            Debug.LogError("[APPSOCKET] Received invalid distance param. Should be [0, 100]: " + distanceStr);
                            SendCommand(SendType.ERROR_PARAMS);
                        }
                        else
                            OnReceive(ReceiveType.CAMERA_DISTANCE, distanceStr);
                    }

                    break;

                case AppProtocol.AppToUnity.CAMERA_HEIGHT:
                    string heightStr = AppProtocol.GetParam(value);
                    float height;


                    if (!float.TryParse(heightStr, out height))
                    {
                        Debug.LogError("[APPSOCKET] Received invalid height param. It's not a valid float: " + heightStr);
                        SendCommand(SendType.ERROR_PARAMS);
                    }
                    else
                    {
                        if (height < -100 || height > 100)
                        {
                            Debug.LogError("[APPSOCKET] Received invalid height param. Should be [-100, 100]: " + heightStr);
                            SendCommand(SendType.ERROR_PARAMS);
                        }
                        else
                            OnReceive(ReceiveType.CAMERA_HEIGHT, heightStr);
                    }

                    break;

                case AppProtocol.AppToUnity.TEST:
                    OnReceive(ReceiveType.TEST);
                    break;

                case AppProtocol.AppToUnity.ERROR_PARAMS:
                    Debug.LogError("[APPSOCKET] Received error from App: Wrong parameters number");
                    OnReceive(ReceiveType.ERROR_PARAMS);
                    break;

                case AppProtocol.AppToUnity.ERROR_COMMAND:
                    Debug.LogError("[APPSOCKET] Received error from App: Invalid command");
                    OnReceive(ReceiveType.ERROR_COMMAND);
                    break;

                default:
                    SendCommand(SendType.ERROR_COMMAND);
                    break;
            }
        }
    }

    private static event ReceiveHandler OnReceive;
    public static void SetOnReceiveHandler(ReceiveHandler handler)
    {
        OnReceive = handler;
    }
    public delegate void ReceiveHandler(ReceiveType receiveType, params string[] parameters);

    private static void SendRawValue(string value)
    {
        Debug.Log("[APPSOCKET] Sent: " + value);

        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJNI.AttachCurrentThread();
            using (AndroidJavaClass javaSocket = new AndroidJavaClass(AppProtocol.JAVA_CLASS))
            {
                javaSocket.CallStatic(AppProtocol.JAVA_METHOD, value.ToString());
            }
        }
    }
    public void ReceiveRawValue(string value)
    {
        Debug.Log("[APPSOCKET] Received: " + value);

        CheckReceivedValue(value);

    }
}
