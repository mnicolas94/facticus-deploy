using System.Collections.Generic;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UIElements.Toggle;

namespace Deploy.Editor.Utility
{
    public static class TabBarUtility
    {
        public static void SetupTabBar(List<(Toggle, VisualElement)> buttonsAndContainers)
        {
            var toggleGroup = buttonsAndContainers.ConvertAll(tuple => tuple.Item1);
            var containers = buttonsAndContainers.ConvertAll(tuple => tuple.Item2);

            buttonsAndContainers.ForEach(tuple =>
            {
                var (toggle, container) = tuple;

                toggle.RegisterCallback<ClickEvent>(evt =>
                {
                    if (toggle.value)
                    {
                        EnableTab(toggle, container, toggleGroup, containers);
                    }
                    else
                    {
                        toggle.value = true;
                    }
                });
            });

            // enable the first tab
            var (firstToggle, firstContainer) = buttonsAndContainers[0];
            EnableTab(firstToggle, firstContainer, toggleGroup, containers);
            firstToggle.value = true;
        }

        private static void EnableTab(
            Toggle toggle, VisualElement container, List<Toggle> toggleGroup, List<VisualElement> containers)
        {
            DisableGroup(toggle, toggleGroup);
            DisableAllContainers(containers);
            container.style.display = DisplayStyle.Flex;
        }

        private static void DisableGroup(Toggle except, List<Toggle> toggleGroup)
        {
            toggleGroup.ForEach(toggle =>
            {
                if (toggle != except)
                {
                    toggle.value = false;
                }
            });
        }

        private static void DisableAllContainers(List<VisualElement> containers)
        {
            containers.ForEach(container => container.style.display = DisplayStyle.None);
        }
    }
}