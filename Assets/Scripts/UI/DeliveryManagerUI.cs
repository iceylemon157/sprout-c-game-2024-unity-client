using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManagerUI : MonoBehaviour {
    // Container is the parent of the waiting recipe UI elements
    [SerializeField] private Transform container;
    [SerializeField] private Transform recipeTemplate;

    private void Awake() {
        recipeTemplate.gameObject.SetActive(false);
    }
    
    private void Start() {
        DeliveryManager.Instance.OnRecipeSpawned += DeliveryManager_OnRecipeSpawned;
        DeliveryManager.Instance.OnRecipeCompleted += DeliveryManager_OnRecipeCompleted;
        UpdateVisual();
    }

    private void DeliveryManager_OnRecipeCompleted(object sender, DeliveryManager.RecipeEventArgs e) {
        Debug.Log("You should definitely update the visual now!");
        UpdateVisual();
    }

    private void DeliveryManager_OnRecipeSpawned(object sender, EventArgs e) {
        UpdateVisual();
    }

    private void UpdateVisual() {
        foreach (Transform child in container) {
            if (child == recipeTemplate) continue;
            Destroy(child.gameObject);
        }
        foreach (var waitingRecipeSO in DeliveryManager.Instance.GetWaitingRecipeSOs()) {
            var recipeTransform = Instantiate(recipeTemplate, container);
            recipeTransform.gameObject.SetActive(true);
            recipeTransform.GetComponent<RecipeUI>().SetRecipeSO(waitingRecipeSO);
        }
        Debug.Log("Finishing Updating visual for waiting recipes");
    }
}