using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScoreData
{
    public Character character;
    public float distance;

    public ScoreData(Character character, float distance)
    {
        this.character = character;
        this.distance = distance;
    }
}

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager instance = null;
    public static ScoreManager Instance
    {
        get
        {
            //인스턴스가 없다면 씬에서 찾는다.
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(ScoreManager)) as ScoreManager;
            }
            return instance;
        }
    }

    public void GetScoreProcess(List<Character> chracterList, Vector3 housePos)
    {
        //버튼의 중앙에 가장 가까운 본인의 캐릭터부터, 
        //버튼의 중앙에 가장 가까운 상대편의 캐릭터 사이에 있는 
        //본인의 캐릭터의 숫자가 해당 라운드의 점수가 됨.
        //하우스를 벗어난 경우 계산에서 제외

        Vector3 houseXZ = new Vector3(housePos.x, 0, housePos.z);

        var scoreAbleList = new List<ScoreData>();
        foreach (var character in chracterList)
        {
            Vector3 posXZ = new Vector3(
                character.transform.position.x, 
                0, 
                character.transform.position.z);

            //하우스의 캐릭터의 거리
            var dis = Vector3.Distance(houseXZ, posXZ);

            //하우스에 조금이라도 닿아도 점수로 판정하기 위해
            //하우스의 반지름에 캐릭터의 반지름 추가
            if (dis < GameManager.IN_HOUSE_DISTANCE + character.Physics.Radius)
            {
                scoreAbleList.Add(new ScoreData(character, dis));
            }
        }

        //점수화가 가능한 캐릭터들을
        //거리순으로 정렬한다.
        scoreAbleList.Sort((x, y) =>
        {
            return x.distance.CompareTo(y.distance);
        });

        int score = 0;
        var scoreGetTeam = Team.NONE;

        //가장 가까운 캐릭터
        var firstData = scoreAbleList.FirstOrDefault();        
        if (firstData != null)
        {
            scoreGetTeam = firstData.character.Team;
            
            foreach (var data in scoreAbleList)
            {
                //가장 가까운 캐릭터와 같은 팀 이라면
                if (data.character.Team == scoreGetTeam)
                {
                    score += 1;
                }
                //다른 팀인 경우 점수 계산 중지
                else break;
            }
        }

        Debug.Log(string.Format("{0} Team Get Score : {1}", scoreGetTeam, score));
    }
}
