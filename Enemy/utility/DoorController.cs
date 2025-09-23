using UnityEngine;
using System.Collections.Generic;

public class DoorController : MonoBehaviour
{
    [System.Serializable]
    public class DoorEntry
    {
        public ActiveDoor door;
        public bool startOpen = false; // true: mở khi bắt đầu, false: đóng khi bắt đầu
    }

    [System.Serializable]
    public class DoorGroup
    {
        public string groupName;
        public List<DoorEntry> doors;
    }

    public List<DoorGroup> doorGroups;

    void Start()
    {
        // Đặt trạng thái cửa theo startOpen riêng từng cửa
        foreach (var group in doorGroups)
        {
            foreach (var entry in group.doors)
            {
                if (entry != null && entry.door != null)
                {
                    if (entry.startOpen)
                        entry.door.OpenDoor();
                    else
                        entry.door.CloseDoor();
                }
            }
        }
    }

    public void CloseDoors(string groupName)
    {
        var group = doorGroups.Find(g => g.groupName == groupName);
        if (group != null)
            foreach (var entry in group.doors)
                if (entry != null && entry.door != null) entry.door.CloseDoor();
    }

    public void OpenDoors(string groupName)
    {
        var group = doorGroups.Find(g => g.groupName == groupName);
        if (group != null)
            foreach (var entry in group.doors)
                if (entry != null && entry.door != null) entry.door.OpenDoor();
    }
}
