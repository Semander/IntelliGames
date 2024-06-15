using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreElement : MonoBehaviour
{

    public TMP_Text usernameText;
    public TMP_Text gameText;
    public TMP_Text scoreText;
    public TMP_Text IQimproveText;

    public void NewScoreElement(string _username, int _game, int _score, int _IQimprove)
    {
        usernameText.text = _username;
        gameText.text = _game.ToString();
        scoreText.text = _score.ToString();
        IQimproveText.text = _IQimprove.ToString();
    }

}
