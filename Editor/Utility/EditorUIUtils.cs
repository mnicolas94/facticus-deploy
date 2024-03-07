using System.Threading;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Deploy.Editor.Utility
{
    public static class EditorUIUtils
    {
        public static async Task<T> GetChildFromPropertyFieldAsync<T>(PropertyField propertyField)
            where T : VisualElement
        {
            var cts = new CancellationTokenSource(1000);
            var ct = cts.Token;

            while (!ct.IsCancellationRequested)
            {
                var list = propertyField.Q<T>();
                if (list != null)
                {
                    return list;
                }

                await Task.Yield();
            }

            return null;
        }
    }
}