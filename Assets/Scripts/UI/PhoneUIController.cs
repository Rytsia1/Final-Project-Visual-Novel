using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhoneUIController : MonoBehaviour
{
    private static PhoneUIController _instance;
    public static PhoneUIController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PhoneUIController>(FindObjectsInactive.Include);
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Panels")]
    public GameObject panelPhoneApp;
    public GameObject panelSelectContact;
    public GameObject panelSelectVenue;

    [Header("State Seleksi")]
    private int selectedNpcId;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this);
        }
    }

    void Start()
    {
        TutupPhone();
    }

    public void BukaPhone()
    {
        gameObject.SetActive(true);
        if (panelPhoneApp != null) panelPhoneApp.SetActive(true);
        if (panelSelectContact != null) panelSelectContact.SetActive(false);
        if (panelSelectVenue != null) panelSelectVenue.SetActive(false);
    }

    public void TutupPhone()
    {
        if (panelPhoneApp != null) panelPhoneApp.SetActive(false);
        if (panelSelectContact != null) panelSelectContact.SetActive(false);
        if (panelSelectVenue != null) panelSelectVenue.SetActive(false);
        gameObject.SetActive(false);
    }

    // Aksi 1: Konsultasi Edelweiss (Information Broker)
    public void OnClick_TanyaEdelweiss(int targetNpcId)
    {
        TutupPhone();
        if (PhoneOutingManager.Instance != null)
        {
            PhoneOutingManager.Instance.CallEdelweissIntel(targetNpcId);
        }
        else
        {
            Debug.LogError("<color=red>[PhoneUIController]</color> PhoneOutingManager.Instance tidak ditemukan!");
        }
    }

    // Aksi 2: Buka Menu Janjian Hangout
    public void OnClick_BukaMenuHangout()
    {
        if (panelPhoneApp != null) panelPhoneApp.SetActive(false);
        if (panelSelectContact != null) panelSelectContact.SetActive(true);
        if (panelSelectVenue != null) panelSelectVenue.SetActive(false);
    }

    public void OnSelectNpcForHangout(int npcId)
    {
        selectedNpcId = npcId;
        if (panelSelectContact != null) panelSelectContact.SetActive(false);
        if (panelSelectVenue != null) panelSelectVenue.SetActive(true);
    }

    public void OnSelectVenueForHangout(int venueId)
    {
        TutupPhone();
        if (PhoneOutingManager.Instance != null)
        {
            PhoneOutingManager.Instance.AjakHangout(selectedNpcId, venueId);
        }
        else
        {
            Debug.LogError("<color=red>[PhoneUIController]</color> PhoneOutingManager.Instance tidak ditemukan!");
        }
    }

    // Navigasi Tombol Kembali
    public void OnClick_KembaliKeMenuUtama()
    {
        BukaPhone();
    }

    public void OnClick_KembaliKePilihKontak()
    {
        OnClick_BukaMenuHangout();
    }
}
