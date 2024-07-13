using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Toolkit
{
    public class ToolbarBuilder : MonoBehaviour
    {
        public List<ToolbarItem<object>> items = new List<ToolbarItem<object>>();
        public Toolbar<object> toolbar;

        [Header("Position Options")]
        public bool absolutePosition = false;
        [Space(20)]
        public bool useLeft = false;
        public int left = 0;
        [Space(20)]
        public bool useTop = false;
        public int top = 0;
        [Space(20)]
        public bool useRight = false;
        public int right = 0;
        [Space(20)]
        public bool useBottom = false;
        public int bottom = 0;


        void Start()
        {
            toolbar = new Toolbar<object>();
            foreach (var item in items)
            {
                toolbar.AddItem(item, null);
            }

            toolbar.Create(UIManager.rootElement);
        }

        void Update()
        {
            if (absolutePosition)
            {
                toolbar.container.style.position = Position.Absolute;
                if (useLeft) toolbar.container.style.left = left;
                if (useTop) toolbar.container.style.top = top;
                if (useRight) toolbar.container.style.right = right;
                if (useBottom) toolbar.container.style.bottom = bottom;
            }
        }
    }
}