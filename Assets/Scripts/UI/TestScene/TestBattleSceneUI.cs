using System;
using UnityEngine;

public class TestBattleSceneUI : UIBase
{
    public GameObject editModeRoot;
    public GameObject playModeRoot;

    public GameObject playerScrollRoot;
    public GameObject playerlistSlot;
    public UIGrid playerListGrid;
    public UILabel playerButtonLabel;

    public GameObject teamScrollRoot;
    public GameObject teamlistSlot;
    public UIGrid teamListGrid;
    public UILabel teamButtonLabel;

    public UIToggle createModeToggle;
    public UIToggle removeModeToggle;

    public CharacterData curCharacterData;
    public Team curTeam;    
    public bool isPlayerChacter;

    private void Update()
    {
        if (!UICamera.isOverUI)
        {
            if (editModeRoot.activeSelf)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (createModeToggle.value)
                    {
                        var clickStartScreenPos = Input.mousePosition;
                        var clickStartWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                        if (curTeam == Team.NONE)
                            return;

                        GameManager.Instance.AddCharacter(curTeam, curCharacterData, clickStartWorldPos, isPlayerChacter);
                    }
                    else if(removeModeToggle.value)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit))
                        {
                            var obj = hit.transform.gameObject.GetComponent<Character>();
                            if (obj != null)
                            {
                                GameManager.Instance.RemoveCharacter(obj.Physics.PID);
                                GameManager.Instance.RemoveTestCreateCharacterData(obj.Team, obj.Data, obj.transform.position);
                            }
                        }
                    }
                }
            }
        }
    }

    public void Init()
    {
        isPlayerChacter = false;

        EditModeInit();

        SetCharacterDataList();
        SetTeamDataList();
    }

    public void EditModeInit()
    {
        GameManager.Instance.None();
        GameManager.Instance.TestSceneReset();
        
        createModeToggle.value = true;
        removeModeToggle.value = false;

        ClickMoveHousePosButton();

        editModeRoot.SetActive(true);
        playModeRoot.SetActive(false);        
    }

    public void PlayModeInit()
    {
        CameraManager.Instance.InitCreatePos();

        ClickResetButton();

        editModeRoot.SetActive(false);
        playModeRoot.SetActive(true);
    }

    public void TogglePlayerList()
    {
        bool isActive = playerScrollRoot.activeInHierarchy;
        playerScrollRoot.SetActive(!isActive);
    }

    public void ToggleTeamList()
    {
        bool isActive = teamScrollRoot.activeInHierarchy;
        teamScrollRoot.SetActive(!isActive);
    }

    public void SetCharacterDataList()
    {
        var characterList = DataBaseManager.Instance.loader.GetDataList("CharacterDB");

        foreach (var data in characterList)
        {
            var character = data as CharacterData;
            var newSlot = Instantiate(playerlistSlot, playerListGrid.transform);

            var newSlotUI = newSlot.GetComponent<TestBattleSceneScrollSlotUI>();
            newSlotUI.Init(character, ClickCharacterSlotEvent);

            if (curCharacterData == null)
            {
                curCharacterData = character;
                playerButtonLabel.text = newSlotUI.GetCharacterName();
            }

            newSlot.SetActive(true);
        }
    }

    public void SetTeamDataList()
    {
        var teamList = Enum.GetValues(typeof(Team));

        foreach (var data in teamList)
        {
            var team = (Team)data;
            var newSlot = Instantiate(teamlistSlot, teamListGrid.transform);

            var newSlotUI = newSlot.GetComponent<TestBattleSceneScrollSlotUI>();
            newSlotUI.Init(team, ClickTeamSlotEvent);

            if (curTeam == Team.NONE)
            {
                curTeam = team;
                teamButtonLabel.text = newSlotUI.GetTeamName();
            }

            newSlot.SetActive(true);
        }
    }

    public void ClickCharacterSlotEvent(TestBattleSceneScrollSlotUI slot, CharacterData data)
    {
        playerButtonLabel.text = slot.GetCharacterName();
        curCharacterData = data;
    }

    public void ClickTeamSlotEvent(TestBattleSceneScrollSlotUI slot, Team team)
    {
        teamButtonLabel.text = slot.GetTeamName();
        curTeam = team;
    }

    public void ClickChagneModeButton()
    {
        editModeRoot.SetActive(!editModeRoot.activeSelf);
        playModeRoot.SetActive(!playModeRoot.activeSelf);

        if (editModeRoot.activeSelf)
        {
            EditModeInit();
        }
        else if (playModeRoot.activeSelf)
        {
            PlayModeInit();
        }
    }

    public void ClickMoveStartPosButton()
    {
        isPlayerChacter = true;
        CameraManager.Instance.InitCreatePos();        
    }

    public void ClickMoveHousePosButton()
    {
        isPlayerChacter = false;
        GameManager.Instance.SetCameraToHousePos();
    }

    public void ClickResetButton()
    {
        GameManager.Instance.TestSceneReset();
        GameManager.Instance.Ready();
    }

    public void ClickMainMenuButton()
    {
        GameManager.Instance.RemoveAllData();
        GameManager.Instance.SetCameraToHousePos();

        Close();        
        UIManager.Instance.Get<MainMenuUI>();        
    }
}
