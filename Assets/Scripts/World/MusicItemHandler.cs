using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Zeltex.Guis.Players
{
	public class MusicItemHandler : MonoBehaviour, IPointerClickHandler
    {
		public MusicList MyMusicList;
		public int ListIndex;
		
		public void OnPointerClick(PointerEventData eventData)
        {
			MyMusicList.PlaySong (ListIndex);
		}
	}
}
