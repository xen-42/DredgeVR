using DredgeVR.Helpers;
using DredgeVR.VRCamera;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace DredgeVR.Ability.Spyglass;

[RequireComponent(typeof(HarvestPOI))]
public class SpyglassHarvestPOIUI : MonoBehaviour
{
	private static GameObject _prefab;

	private HarvestPOI _harvestPOI;
	private HarvestableItemData _firstHarvestableItem;

	private GameObject _container;
	private LocalizeStringEvent _itemNameString;
	// private TextMeshProUGUI _itemDistanceTextField; //Unused in SpyglassUI
	private Image _itemImage;
	private Image _invalidEquipmentImage;
	private Sprite _hiddenItemSprite;
	private HarvestableTypeTagUI _harvestableTypeTagUI;
	private LocalizedString _obscuredString;

	private float _containerScale = 0.005f;
	private bool _shouldShowContainer;

	private AbilityData _spyglassAbilityData;
	private bool _usingSpyglass;
	
	private float _updateTimer;
	public const float UPDATE_TIME = 1f;

	private void Awake()
	{
		GameEvents.Instance.OnPlayerAbilityToggled += this.OnPlayerAbilityToggled;
	}

	private void OnDestroy()
	{
		GameEvents.Instance.OnPlayerAbilityToggled -= this.OnPlayerAbilityToggled;
	}

	public void Start()
	{
		var spyglassUI = GameManager.Instance.UI.spyglassUI;
		_spyglassAbilityData = spyglassUI.spyglassAbilityData;

		if (_prefab == null)
		{
			_prefab = GameObject.Instantiate(spyglassUI.container);
			_prefab.SetActive(false);
			// Needs to be worldspace
			var canvas = _prefab.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.WorldSpace;
			GameObject.DontDestroyOnLoad(_prefab);
		}

		_harvestPOI = GetComponent<HarvestPOI>();

		_container = GameObject.Instantiate(_prefab);
		_container.SetParent(transform);
		_container.transform.localPosition = Vector3.up;
		_container.transform.localScale = Vector3.zero;

		// Grab all the components
		_itemNameString = _container.transform.Find("Backplate/BackplateInner/NameText").GetComponent<LocalizeStringEvent>();
		_itemImage = _container.transform.Find("Backplate/BackplateInner/Image").GetComponent<Image>();
		_invalidEquipmentImage = _container.transform.Find("Backplate/BackplateInner/InvalidEquipmentImage").GetComponent<Image>();
		_hiddenItemSprite = spyglassUI.hiddenItemSprite;
		_harvestableTypeTagUI = _container.transform.Find("Backplate/HarvestableTypeTag").GetComponent<HarvestableTypeTagUI>();
		_obscuredString = spyglassUI.obscuredString;

		_updateTimer = Random.Range(0, UPDATE_TIME);
	}

	public void Update()
	{
		if (!_usingSpyglass)
		{
			return;
		}

		// Spread out update checks
		_updateTimer -= Time.deltaTime;
		if (_updateTimer < 0)
		{
			_updateTimer += UPDATE_TIME;
			CheckPlayerPosition();
		}

		// Lerp scale based on if active
		var targetScale = _shouldShowContainer ? Vector3.one * _containerScale : Vector3.zero;
		if (_container.transform.localScale != targetScale)
		{
			var t = Mathf.Clamp01(Mathf.InverseLerp(_shouldShowContainer ? 0f : _containerScale, targetScale.x, _container.transform.localScale.x) + Time.deltaTime);
			_container.transform.localScale = Vector3.Slerp(_container.transform.localScale, targetScale, t);

			if (!_shouldShowContainer && _container.transform.localScale == Vector3.zero)
			{
				_container.SetActive(false);
			}
		}
	}

	private void CheckPlayerPosition()
	{
		// Only show objects within a certain range
		if ((GameManager.Instance.Player.transform.position - transform.position).magnitude < 60)
		{
			_shouldShowContainer = true;

			if (!_container.activeInHierarchy)
			{
				_container.SetActive(true);
			}

			// First harvestable item can change
			UpdateHarvestableItem();

			if (_firstHarvestableItem == null)
			{
				_shouldShowContainer = false;
			}
			else
			{
				// TODO: Ideally we'd also check that you're looking towards it
				GameManager.Instance.SaveData.SetHasSpiedHarvestCategory(_firstHarvestableItem.harvestableType, true);
				GameManager.Instance.AchievementManager.EvaluateAchievement(DredgeAchievementId.ABILITY_SPYGLASS);

				// LookAt makes them backwards though
				_container.transform.LookAt(VRCameraManager.VRPlayer.transform.position);
				_container.transform.Rotate(Vector3.up, 180f);
			}
		}
		else
		{
			_shouldShowContainer = false;
		}
	}

	private void UpdateHarvestableItem()
	{
		var currentFirstHarvestableItem = _harvestPOI.Harvestable.GetActiveFirstHarvestableItem();

		if (_firstHarvestableItem != currentFirstHarvestableItem)
		{
			_firstHarvestableItem = currentFirstHarvestableItem;

			if (_firstHarvestableItem != null)
			{
				// Most of this is adapted from SpyglassUI which has a single focused UI for the harvestPOI you're looking at
				if (GameManager.Instance.SaveData.GetCaughtCountById(_firstHarvestableItem.id) > 0)
				{
					_itemNameString.StringReference = _firstHarvestableItem.itemNameKey;
				}
				else
				{
					_itemNameString.StringReference = _obscuredString;
				}
				_itemNameString.StringReference.RefreshString();
				_itemImage.sprite = ((_firstHarvestableItem.itemSubtype == ItemSubtype.TRINKET) ? _hiddenItemSprite : _firstHarvestableItem.sprite);
				_harvestableTypeTagUI.Init(_firstHarvestableItem.harvestableType, _firstHarvestableItem.requiresAdvancedEquipment);
				_invalidEquipmentImage.gameObject.SetActive(!GameManager.Instance.PlayerStats.HarvestableTypes.Contains(_firstHarvestableItem.harvestableType));
			}
		}
	}

	private void OnPlayerAbilityToggled(AbilityData abilityData, bool enabled)
	{
		if (_spyglassAbilityData.name == abilityData.name)
		{
			_usingSpyglass = enabled;
			if (enabled)
			{
				// Reset them when entering into spyglass
				_shouldShowContainer = false;
				_container.transform.localScale = Vector3.zero;
			}
			else
			{
				_container.SetActive(false);
			}
		}
	}
}
