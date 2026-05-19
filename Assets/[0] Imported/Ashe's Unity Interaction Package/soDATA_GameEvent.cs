using System;
using UnityEngine;

namespace Ash {
    //REPRESENTS AN ACTION/EVENT BEING UNDERTAKEN//
    [CreateAssetMenu(fileName = "[GameEvent] New", menuName = "++GAMEEVENT || Represents an Action or Event happening in Game.")]
    public class soDATA_GameEvent : ScriptableObject
    {
        /*
        GAME EVENT Script by Ash+ [w/ Thanks to Paul Hedly]
        -
        Game Events represent Actions or Events happening During Runtime.
        -
        They trade execution order for accessibilty by programmers and Designers.
        -
        To use this SO Register a Listener(Method or other *Listener.type[pseudo]) to it and when the same Event is Raised the Listener is run,#
        The Listener is run as if it were called when it was registered ei [RegLis(Method(InputType input))]
        -
        */

        private event Action listeners;

        public void Raise()
        {
            listeners?.Invoke();
        }

        public void RegisterListener(Action listener)
        {
            listeners += listener;
        }

        public void UnregisterListener(Action listener)
        {
            listeners -= listener;
        }
    }
}