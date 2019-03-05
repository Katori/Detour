using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Detour.Examples.Client
{
    public class MapCellComponent : MonoBehaviour, IPointerClickHandler
    {

        public Vector2Int Position;

        public void OnPointerClick(PointerEventData eventData)
        {
            ExampleGameController.Instance.ClickedCell(Position);
        }

    }
}