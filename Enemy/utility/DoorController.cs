using UnityEngine;
using System.Collections.Generic;

public enum DoorGroupType { Boss, Map, Puzzle, Other }

public class DoorController : MonoBehaviour
{
    [System.Serializable]
    public class DoorGroup
    {
        public string groupName;
        public DoorGroupType groupType;
        public List<ActiveDoor> doors;
        public bool startOpen = false; // true: mở khi bắt đầu, false: đóng khi bắt đầu
    }

    public List<DoorGroup> doorGroups;

    void Start()
    {
        // Đặt trạng thái cửa theo startOpen
        foreach (var group in doorGroups)
        {
            foreach (var door in group.doors)
            {
                if (door != null)
                {
                    if (group.startOpen)
                        door.OpenDoor();
                    else
                        door.CloseDoor();
                }
            }
        }
    }

    public void CloseDoors(string groupName)
    {
        var group = doorGroups.Find(g => g.groupName == groupName);
        if (group != null)
            foreach (var door in group.doors)
                if (door != null) door.CloseDoor();
    }

    public void OpenDoors(string groupName)
    {
        var group = doorGroups.Find(g => g.groupName == groupName);
        if (group != null)
            foreach (var door in group.doors)
                if (door != null) door.OpenDoor();
    }
}
