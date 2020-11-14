using UnityEngine;

public class TestBattleSceneUI : MonoBehaviour
{
    public GameObject playerScrollRoot;
    public GameObject playerlistSlot;
    public UIGrid playerListGrid;
    public UILabel playerButtonLabel;

    public GameObject enemyScrollRoot;
    public GameObject enemylistSlot;
    public UIGrid enemyListGrid;
    public UILabel enemyButtonLabel;

    public void OnEnable()
    {
        DataBaseManager.Instance.EndDataLoadEvent += SetPlayerCharacterDataList;
        DataBaseManager.Instance.EndDataLoadEvent += SetEnemyCharacterDataList;
    }

    public void TogglePlayerList()
    {
        bool isActive = playerScrollRoot.activeInHierarchy;
        playerScrollRoot.SetActive(!isActive);
    }

    public void ToggleEnemyList()
    {
        bool isActive = enemyScrollRoot.activeInHierarchy;
        enemyScrollRoot.SetActive(!isActive);
    }

    public void SetPlayerCharacterDataList()
    {
        var characterList = DataBaseManager.Instance.loader.GetDataList("CharacterDB");

        foreach (var data in characterList)
        {
            var character = data as CharacterData;
            var newSlot = Instantiate(playerlistSlot, playerListGrid.transform);

            var newSlotUI = newSlot.GetComponent<TestBattleSceneScrollSlotUI>();
            newSlotUI.Init(character, ClickPlayerSlotEvent);

            if (GameManager.Instance.EnemyID == character.id)
            {
                playerButtonLabel.text = newSlotUI.GetName();
            }

            newSlot.SetActive(true);
        }
    }

    public void SetEnemyCharacterDataList()
    {
        var characterList = DataBaseManager.Instance.loader.GetDataList("CharacterDB");

        foreach (var data in characterList)
        {
            var character = data as CharacterData;
            var newSlot = Instantiate(enemylistSlot, enemyListGrid.transform);

            var newSlotUI = newSlot.GetComponent<TestBattleSceneScrollSlotUI>();
            newSlotUI.Init(character, ClicEnemySlotEvent);

            if (GameManager.Instance.playerID == character.id)
            {
                enemyButtonLabel.text = newSlotUI.GetName();
            }

            newSlot.SetActive(true);
        }
    }

    public void ClickPlayerSlotEvent(TestBattleSceneScrollSlotUI slot, CharacterData data)
    {
        playerButtonLabel.text = slot.GetName();
        GameManager.Instance.SetPlayerCharacter(data);
    }

    public void ClicEnemySlotEvent(TestBattleSceneScrollSlotUI slot, CharacterData data)
    {
        enemyButtonLabel.text = slot.GetName();
        GameManager.Instance.SetEnemyCharacter(data);
    }

    public void ClickRefreshButton()
    {
        GameManager.Instance.TestSceneRefresh();
    }
}
