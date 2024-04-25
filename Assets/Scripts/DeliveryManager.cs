using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeliveryManager : MonoBehaviour {
    public event EventHandler OnDeliveryFailed;
    public event EventHandler OnDeliverySuccess;
    public event EventHandler<RecipeEventArgs> OnRecipeSpawned;
    public event EventHandler<RecipeEventArgs> OnRecipeCompleted;

    public class RecipeEventArgs : EventArgs {
        public RecipeSO RecipeSO;
        public List<RecipeSO> RecipeSOList;
    }

    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListSO recipeListSO;
    private List<RecipeSO> _waitingRecipeSOs;
    private float _spawnRecipeTimer;
    private int _waitingRecipesCount;
    private int _successRecipeDelivered;
    private int _mostRecentRecipeID;

    private const float SpawnRecipeTimeMax = 5f;
    private const int WaitingRecipesMax = 4;

    private void Awake() {
        Instance = this;
        _waitingRecipeSOs = new List<RecipeSO>();
    }

    private void Start() {
        _spawnRecipeTimer = SpawnRecipeTimeMax;
        _waitingRecipesCount = 0;
        _successRecipeDelivered = 0;
        _mostRecentRecipeID = 0;
    }

    private void Update() {
        _spawnRecipeTimer -= Time.deltaTime;
        if (_spawnRecipeTimer <= 0f) {
            _spawnRecipeTimer = SpawnRecipeTimeMax;
            if (_waitingRecipesCount < WaitingRecipesMax) {
                // Don't fix the yellow warning
                // Or otherwise you will have to copy the RecipeSO field one by one
                var waitingRecipeSO = new RecipeSO(recipeListSO.recipeSOList[Random.Range(0, recipeListSO.recipeSOList.Count)]);
                waitingRecipeSO.id = ++ _mostRecentRecipeID;
                Debug.Log("New recipe: " + waitingRecipeSO.recipeName + " is waiting!");
                Debug.Log("Recipe ID: " + waitingRecipeSO.id);
                _waitingRecipeSOs.Add(waitingRecipeSO);
                _waitingRecipesCount ++;
                OnRecipeSpawned?.Invoke(this, new RecipeEventArgs() {
                    RecipeSO = waitingRecipeSO,
                    RecipeSOList = _waitingRecipeSOs
                });
            }
        }
    }

    public bool DeliverRecipe(PlateKitchenObject plateKitchenObject) {
        for (var i = 0; i < _waitingRecipeSOs.Count; i ++) {
            var waitingRecipeSO = _waitingRecipeSOs[i];
            waitingRecipeSO.kitchenObjectSOList.Sort((x, y) =>
                string.Compare(x.objectName, y.objectName, StringComparison.Ordinal));
            plateKitchenObject.GetKitchenObjectSOList().Sort((x, y) =>
                string.Compare(x.objectName, y.objectName, StringComparison.Ordinal));

            var plateContentsMatch =
                waitingRecipeSO.kitchenObjectSOList.SequenceEqual(plateKitchenObject.GetKitchenObjectSOList());

            if (plateContentsMatch) {
                OnRecipeCompleted?.Invoke(this, new RecipeEventArgs() {
                    RecipeSO = waitingRecipeSO,
                    RecipeSOList = _waitingRecipeSOs
                });
                OnDeliverySuccess?.Invoke(this, EventArgs.Empty);

                _waitingRecipeSOs.RemoveAt(i);
                _waitingRecipesCount --;
                _successRecipeDelivered ++;

                return true;
            }
        }

        OnDeliveryFailed?.Invoke(this, EventArgs.Empty);
        return false;
    }

    public List<RecipeSO> GetWaitingRecipeSOs() {
        return _waitingRecipeSOs;
    }

    public int GetSuccessRecipeDelivered() {
        return _successRecipeDelivered;
    }
}