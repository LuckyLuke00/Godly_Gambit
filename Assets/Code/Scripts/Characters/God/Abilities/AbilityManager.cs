using System.Collections.Generic;
using UnityEngine;

namespace GodlyGambit
{
    public class AbilityManager : MonoBehaviour
    {
        private CursorController _cursorController = null;
        private readonly List<Ability> _abilities = new List<Ability>();

        private void Awake()
        {
            _abilities.AddRange(GetComponents<Ability>());

            if (_abilities == null || _abilities.Count == 0)
            {
                Debug.LogError("No abilities assigned to AbilityManager");
                return;
            }

            _cursorController = GetComponentInChildren<CursorController>();

            if (!_cursorController) Debug.LogError("No CursorController found in children");

            _abilities.ForEach(ability => ability.SetParent(_cursorController.transform));
        }

        private void Update()
        {
            _abilities.ForEach(ability => ability.CanPlace = _cursorController.IsPositionValid);
        }

        private void OnEnable()
        {
            PlayerManager.OnGameEnd += DisableAbility;
        }

        private void OnDisable()
        {
            PlayerManager.OnGameEnd -= DisableAbility;
        }

        private void OnPlace()
        {
            if (!_cursorController.IsPositionValid) return;

            foreach (var ability in _abilities)
            {
                if (!ability.IsActive || !ability.HasCooledDown) continue;

                ability.Place();

                _cursorController.CanPlaceAnywhere = false;
                _cursorController.IgnorePlayer = false;

                break;
            }
        }

        private void OnToggleAbility1() => UpdateAbility(0);

        private void OnToggleAbility2() => UpdateAbility(1);

        private void OnCancelAbility() => DisableAbility();

        private void UpdateAbility(int abilityIndex)
        {
            if (_abilities.Count < abilityIndex + 1) return;

            if (_abilities[abilityIndex].IsActive)
            {
                OnPlace();
                return;
            }

            ToggleAbility(abilityIndex);
        }

        private void ToggleAbility(int abilityIndex)
        {
            for (int i = 0; i < _abilities.Count; ++i)
            {
                if (i != abilityIndex)
                {
                    DisableAbility(i);
                }

                if (i == abilityIndex)
                {
                    _cursorController.IgnorePlayer = _abilities[i].CanPlaceOnPlayer;
                    _cursorController.CanPlaceAnywhere = _abilities[i].CanPlaceAnyWhere;

                    _abilities[i].Toggle();
                }
            }
        }

        private void DisableAbility(int abilityIndex)
        {
            if (!_abilities[abilityIndex].IsActive) return;

            _abilities[abilityIndex].IsActive = false;
            _abilities[abilityIndex].DeactivateGhosts();

            _cursorController.IgnorePlayer = false;
            _cursorController.CanPlaceAnywhere = false;
        }

        private void DisableAbility()
        {
            for (int i = 0; i < _abilities.Count; ++i)
            {
                DisableAbility(i);
            }
        }
    }
}
