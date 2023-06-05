using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

public enum CarPosition
{
    UP_LEFT = 0,
    UP_RIGHT = 1,
    DOWN_LEFT = 2,
    DOWN_RIGHT = 3,
    CENTER = 4,
}

public enum CarDirection
{
    STRAIGHT = 0,
    STRAIGHT_LEFT_10 = 1,
    STRAIGHT_LEFT_20 = 2,
    STRAIGHT_LEFT_30 = 3,
    STRAIGHT_LEFT_40 = 4,
    STRAIGHT_LEFT_50 = 5,
    STRAIGHT_LEFT_60 = 6,
    STRAIGHT_LEFT_70 = 7,
    STRAIGHT_LEFT_80 = 8,
    STRAIGHT_LEFT_90 = 9,
    STRAIGHT_LEFT_100 = 10,
    STRAIGHT_LEFT_110 = 11,
    STRAIGHT_LEFT_120 = 12,
    STRAIGHT_LEFT_130 = 13,
    STRAIGHT_LEFT_140 = 14,
    STRAIGHT_LEFT_150 = 15,
    STRAIGHT_LEFT_160 = 16,
    STRAIGHT_LEFT_170 = 17,
    STRAIGHT_RIGHT_10 = 18,
    STRAIGHT_RIGHT_20 = 19,
    STRAIGHT_RIGHT_30 = 20,
    STRAIGHT_RIGHT_40 = 21,
    STRAIGHT_RIGHT_50 = 22,
    STRAIGHT_RIGHT_60 = 23,
    STRAIGHT_RIGHT_70 = 24,
    STRAIGHT_RIGHT_80 = 25,
    STRAIGHT_RIGHT_90 = 26,
    STRAIGHT_RIGHT_100 = 27,
    STRAIGHT_RIGHT_110 = 28,
    STRAIGHT_RIGHT_120 = 29,
    STRAIGHT_RIGHT_130 = 30,
    STRAIGHT_RIGHT_140 = 31,
    STRAIGHT_RIGHT_150 = 32,
    STRAIGHT_RIGHT_160 = 33,
    STRAIGHT_RIGHT_170 = 34,
    STRAIGHT_RIGHT_180 = 35,
}

public static class PosAndDirUtility
{
    // car position
    public static Vector3 UP_LEFT_POS = new Vector3(23, 40, 475);
    public static Vector3 UP_RIGHT_POS = new Vector3(490, 40, 475);
    public static Vector3 DOWN_LEFT_POS = new Vector3(23, 40, 38);
    public static Vector3 DOWN_RIGHT_POS = new Vector3(490, 40, 38);
    public static Vector3 CENTER_POS = new Vector3(256, 40, 256);

    // car initial direction
    public static Vector3 UP_LEFT_STRAIGHT = new Vector3(0, 120, 0);
    public static Vector3 UP_RIGHT_STRAIGHT = new Vector3(0, -120, 0);
    public static Vector3 DOWN_LEFT_STRAIGHT = new Vector3(0, 60, 0);
    public static Vector3 DOWN_RIGHT_STRAIGHT = new Vector3(0, -60, 0);
    public static Vector3 CENTER_STRAIGHT = new Vector3(0, -90, 0);

    public static void initLocation(Transform obj)
    {
        obj.position = UP_LEFT_POS;
        obj.localEulerAngles = UP_LEFT_STRAIGHT;
    }

    public static void changeLocation(Transform obj, CarPosition loccode)
    {

        switch ((int)loccode)
        {
            case 0:
                obj.position = UP_LEFT_POS;
                obj.localEulerAngles = UP_LEFT_STRAIGHT;
                break;
            case 1:
                obj.position = UP_RIGHT_POS;
                obj.localEulerAngles = UP_RIGHT_STRAIGHT;
                break;
            case 2:
                obj.position = DOWN_LEFT_POS;
                obj.localEulerAngles = DOWN_LEFT_STRAIGHT;
                break;
            case 3:
                obj.position = DOWN_RIGHT_POS;
                obj.localEulerAngles = DOWN_RIGHT_STRAIGHT;
                break;
            case 4:
                obj.position = CENTER_POS;
                obj.localEulerAngles = CENTER_STRAIGHT;
                break;
            default:
                obj.position = UP_LEFT_POS;
                obj.localEulerAngles = UP_LEFT_STRAIGHT;
                break;
        }
    }

    public static void changeRotation(Transform obj, CarPosition loccode, CarDirection rotcode, bool isEdge)
    {
        changeLocation(obj, loccode);
        if (isEdge)
        {
            switch ((int)rotcode)
            {
                case 0:
                    break;
                case 1:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 10, obj.localEulerAngles.z);
                    break;
                case 2:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 20, obj.localEulerAngles.z);
                    break;
                case 3:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 30, obj.localEulerAngles.z);
                    break;
                case 18:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 10, obj.localEulerAngles.z);
                    break;
                case 19:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 20, obj.localEulerAngles.z);
                    break;
                case 20:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 30, obj.localEulerAngles.z);
                    break;
                default:
                    Debug.LogError("Rotation Not allowed");
                    break;
            }
        }
        else
        {
            switch ((int)rotcode)
            {
                case 0:
                    break;
                case 1:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 10, obj.localEulerAngles.z);
                    break;
                case 2:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 20, obj.localEulerAngles.z);
                    break;
                case 3:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 30, obj.localEulerAngles.z);
                    break;
                case 4:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 40, obj.localEulerAngles.z);
                    break;
                case 5:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 50, obj.localEulerAngles.z);
                    break;
                case 6:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 60, obj.localEulerAngles.z);
                    break;
                case 7:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 70, obj.localEulerAngles.z);
                    break;
                case 8:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 80, obj.localEulerAngles.z);
                    break;
                case 9:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 90, obj.localEulerAngles.z);
                    break;
                case 10:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 100, obj.localEulerAngles.z);
                    break;
                case 11:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 110, obj.localEulerAngles.z);
                    break;
                case 12:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 120, obj.localEulerAngles.z);
                    break;
                case 13:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 130, obj.localEulerAngles.z);
                    break;
                case 14:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 140, obj.localEulerAngles.z);
                    break;
                case 15:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 150, obj.localEulerAngles.z);
                    break;
                case 16:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 160, obj.localEulerAngles.z);
                    break;
                case 17:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 170, obj.localEulerAngles.z);
                    break;
                case 18:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 10, obj.localEulerAngles.z);
                    break;
                case 19:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 20, obj.localEulerAngles.z);
                    break;
                case 20:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 30, obj.localEulerAngles.z);
                    break;
                case 21:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 40, obj.localEulerAngles.z);
                    break;
                case 22:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 50, obj.localEulerAngles.z);
                    break;
                case 23:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 60, obj.localEulerAngles.z);
                    break;
                case 24:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 70, obj.localEulerAngles.z);
                    break;
                case 25:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 80, obj.localEulerAngles.z);
                    break;
                case 26:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 90, obj.localEulerAngles.z);
                    break;
                case 27:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 100, obj.localEulerAngles.z);
                    break;
                case 28:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 110, obj.localEulerAngles.z);
                    break;
                case 29:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 120, obj.localEulerAngles.z);
                    break;
                case 30:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 130, obj.localEulerAngles.z);
                    break;
                case 31:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 140, obj.localEulerAngles.z);
                    break;
                case 32:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 150, obj.localEulerAngles.z);
                    break;
                case 33:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 160, obj.localEulerAngles.z);
                    break;
                case 34:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 170, obj.localEulerAngles.z);
                    break;
                case 35:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 180, obj.localEulerAngles.z);
                    break;
                default:
                    Debug.LogError("Rotation Not allowed");
                    break;
            }
        }
    }

    public static void remainRotation(Transform obj, CarPosition loccode, CarDirection rotcode, bool isEdge)
    {
        switch ((int)loccode)
        {
            case 0:
                obj.localEulerAngles = UP_LEFT_STRAIGHT;
                break;
            case 1:
                obj.localEulerAngles = UP_RIGHT_STRAIGHT;
                break;
            case 2:
                obj.localEulerAngles = DOWN_LEFT_STRAIGHT;
                break;
            case 3:
                obj.localEulerAngles = DOWN_RIGHT_STRAIGHT;
                break;
            case 4:
                obj.localEulerAngles = CENTER_STRAIGHT;
                break;
            default:
                obj.localEulerAngles = UP_LEFT_STRAIGHT;
                break;
        }

        if (isEdge)
        {
            switch ((int)rotcode)
            {
                case 0:
                    break;
                case 1:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 10, obj.localEulerAngles.z);
                    break;
                case 2:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 20, obj.localEulerAngles.z);
                    break;
                case 3:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 30, obj.localEulerAngles.z);
                    break;
                case 18:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 10, obj.localEulerAngles.z);
                    break;
                case 19:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 20, obj.localEulerAngles.z);
                    break;
                case 20:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 30, obj.localEulerAngles.z);
                    break;
                default:
                    Debug.LogError("Rotation Not allowed");
                    break;
            }
        }
        else
        {
            switch ((int)rotcode)
            {
                case 0:
                    break;
                case 1:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 10, obj.localEulerAngles.z);
                    break;
                case 2:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 20, obj.localEulerAngles.z);
                    break;
                case 3:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 30, obj.localEulerAngles.z);
                    break;
                case 4:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 40, obj.localEulerAngles.z);
                    break;
                case 5:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 50, obj.localEulerAngles.z);
                    break;
                case 6:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 60, obj.localEulerAngles.z);
                    break;
                case 7:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 70, obj.localEulerAngles.z);
                    break;
                case 8:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 80, obj.localEulerAngles.z);
                    break;
                case 9:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 90, obj.localEulerAngles.z);
                    break;
                case 10:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 100, obj.localEulerAngles.z);
                    break;
                case 11:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 110, obj.localEulerAngles.z);
                    break;
                case 12:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 120, obj.localEulerAngles.z);
                    break;
                case 13:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 130, obj.localEulerAngles.z);
                    break;
                case 14:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 140, obj.localEulerAngles.z);
                    break;
                case 15:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 150, obj.localEulerAngles.z);
                    break;
                case 16:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 160, obj.localEulerAngles.z);
                    break;
                case 17:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y - 170, obj.localEulerAngles.z);
                    break;
                case 18:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 10, obj.localEulerAngles.z);
                    break;
                case 19:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 20, obj.localEulerAngles.z);
                    break;
                case 20:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 30, obj.localEulerAngles.z);
                    break;
                case 21:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 40, obj.localEulerAngles.z);
                    break;
                case 22:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 50, obj.localEulerAngles.z);
                    break;
                case 23:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 60, obj.localEulerAngles.z);
                    break;
                case 24:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 70, obj.localEulerAngles.z);
                    break;
                case 25:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 80, obj.localEulerAngles.z);
                    break;
                case 26:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 90, obj.localEulerAngles.z);
                    break;
                case 27:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 100, obj.localEulerAngles.z);
                    break;
                case 28:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 110, obj.localEulerAngles.z);
                    break;
                case 29:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 120, obj.localEulerAngles.z);
                    break;
                case 30:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 130, obj.localEulerAngles.z);
                    break;
                case 31:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 140, obj.localEulerAngles.z);
                    break;
                case 32:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 150, obj.localEulerAngles.z);
                    break;
                case 33:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 160, obj.localEulerAngles.z);
                    break;
                case 34:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 170, obj.localEulerAngles.z);
                    break;
                case 35:
                    obj.localEulerAngles = new Vector3(obj.localEulerAngles.x, obj.localEulerAngles.y + 180, obj.localEulerAngles.z);
                    break;
                default:
                    Debug.LogError("Rotation Not allowed");
                    break;
            }
        }
    }
}
