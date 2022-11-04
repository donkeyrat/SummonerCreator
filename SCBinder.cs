using System.Collections;
using UnityEngine;
using System.Reflection;

namespace SummonerCreator
{
    public class SCBinder : MonoBehaviour
    {
        public static void UnitGlad()
        {
            if (!instance)
            {
                instance = new GameObject
                {
                    hideFlags = HideFlags.HideAndDontSave
                }.AddComponent<SCBinder>();
            }
            instance.StartCoroutine(StartUnitgradLate());
        }

        private static IEnumerator StartUnitgradLate()
        {
            yield return new WaitUntil(() => FindObjectOfType<ServiceLocator>() != null);
            yield return new WaitUntil(() => ServiceLocator.GetService<ISaveLoaderService>() != null);
            yield return new WaitForSeconds(0.2f);
            new SCMain();
            yield break;
        }

        private static SCBinder instance;
    }
}