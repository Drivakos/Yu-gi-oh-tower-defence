using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class SaveLoadUI : MonoBehaviour
    {
        [Header("Save/Load Panel")]
        [SerializeField] private GameObject saveLoadPanel;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteSaveButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI statusText;
        
        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject confirmationDialog;
        [SerializeField] private TextMeshProUGUI confirmationText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        
        private SaveSystem saveSystem;
        private GameManager gameManager;
        
        private void Start()
        {
            saveSystem = FindObjectOfType<SaveSystem>();
            gameManager = FindObjectOfType<GameManager>();
            
            // Set up button listeners
            saveButton.onClick.AddListener(OnSaveButtonClicked);
            loadButton.onClick.AddListener(OnLoadButtonClicked);
            deleteSaveButton.onClick.AddListener(OnDeleteSaveButtonClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
            
            // Hide panels initially
            saveLoadPanel.SetActive(false);
            confirmationDialog.SetActive(false);
        }
        
        public void ShowSaveLoadPanel()
        {
            saveLoadPanel.SetActive(true);
            UpdateStatusText();
        }
        
        private void OnSaveButtonClicked()
        {
            saveSystem.SaveGame();
            UpdateStatusText();
        }
        
        private void OnLoadButtonClicked()
        {
            ShowConfirmationDialog("Load saved game? This will replace your current game.", () => {
                if (saveSystem.LoadGame())
                {
                    gameManager.ResumeGame();
                    HideSaveLoadPanel();
                }
                else
                {
                    UpdateStatusText("No save file found or error loading save.");
                }
            });
        }
        
        private void OnDeleteSaveButtonClicked()
        {
            ShowConfirmationDialog("Delete saved game? This cannot be undone.", () => {
                saveSystem.DeleteSave();
                UpdateStatusText();
            });
        }
        
        private void OnCloseButtonClicked()
        {
            HideSaveLoadPanel();
        }
        
        private void ShowConfirmationDialog(string message, System.Action onConfirm)
        {
            confirmationText.text = message;
            confirmationDialog.SetActive(true);
            
            // Store the confirm action
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => {
                onConfirm?.Invoke();
                confirmationDialog.SetActive(false);
            });
        }
        
        private void OnConfirmButtonClicked()
        {
            confirmationDialog.SetActive(false);
        }
        
        private void OnCancelButtonClicked()
        {
            confirmationDialog.SetActive(false);
        }
        
        private void HideSaveLoadPanel()
        {
            saveLoadPanel.SetActive(false);
        }
        
        private void UpdateStatusText(string customMessage = null)
        {
            if (!string.IsNullOrEmpty(customMessage))
            {
                statusText.text = customMessage;
                return;
            }
            
            if (System.IO.File.Exists(saveSystem.GetSavePath()))
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(saveSystem.GetSavePath());
                statusText.text = $"Save file exists. Last modified: {fileInfo.LastWriteTime}";
            }
            else
            {
                statusText.text = "No save file exists.";
            }
        }
    }
} 