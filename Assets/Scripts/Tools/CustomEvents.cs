using UnityEngine;
using UnityEngine.Events;

namespace Zeltex.Util
{
	[System.Serializable]
	public class EventObject : UnityEvent<GameObject>
    {

    }
	
	[System.Serializable]
	public class MyEvent2 : UnityEvent<GameObject,GameObject>
    {
		
	}
	[System.Serializable]
	public class EventObjectString : UnityEvent<GameObject,string>
    {
		
	}
	[System.Serializable]
	public class EventString : UnityEvent<string>
    {
		
	}

	[System.Serializable]
	public class MyEventInt : UnityEvent<int>
    {

    }

    [System.Serializable]
    public class MyEventVector2 : UnityEvent<Vector2>
    {

    }
    [System.Serializable]
    public class MyEventVector3 : UnityEvent<Vector3>
    {

    }

    [System.Serializable]
    public class MyEventColor32 : UnityEvent<Color32>
    {

    }

    [System.Serializable]
    public class MyEventColor : UnityEvent<Color>
    {

    }

    [System.Serializable]
    public class EventGuiListElement : UnityEvent<string, Guis.GuiListElement>
    {

    }
}