using System;
using UnityEngine;

[Serializable]
public class ScheduleDetails : IComparable<ScheduleDetails>
{
    public int hour, minute, day;
    public Season season;
    public int priority;  //越小越优先执行
    public string targetScene;
    public Vector2Int targetGridPosition;
    public AnimationClip clipAtStop;
    public bool interactable;

    public int Time => (hour * 100) + minute;

    public ScheduleDetails(int hour, int minute, int day, Season season, int priority, 
                           string targerScene, Vector2Int targetGridPosition, AnimationClip clipAtStop, bool interactable)
    {
        this.hour = hour;
        this.minute = minute;
        this.day = day;
        this.season = season;
        this.priority = priority;
        this.targetScene = targerScene;
        this.targetGridPosition = targetGridPosition;
        this.clipAtStop = clipAtStop;
        this.interactable = interactable;
    }

    public int CompareTo(ScheduleDetails other)
    {
        if(Time == other.Time)
        {
            //默认升序排序，other排在前面优先执行
            if(priority > other.priority)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
        else if(Time > other.Time)
        {
            return 1;
        }
        else if(Time < other.Time)
        {
            return -1;
        }
        return 0;
    }
}
