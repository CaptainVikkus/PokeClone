﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;

    public Pokemon Pokemon { get; set; }

    Image image;
    Vector3 orginalPos;
    Color orginalColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        orginalPos = image.transform.localPosition;
        orginalColor = image.color;
    }

    public void Setup()
    {

        if (isPlayerUnit)
        {
            Pokemon = PlayerController.pokemon;
            image.sprite = Pokemon.Base.BackSprite;
        }
        else
        {
            if (Pokemon == null)
            {
                Pokemon = new Pokemon(PokemonBaseList.getRandomPokemonBase(), level);
            }
            image.sprite = Pokemon.Base.FrontSprite;
        }

        image.color = orginalColor;
       
        PlayEnterAnimation();
    }

    public void Setup(string PokemonName, int lvl, int hp)
    {
        Pokemon = new Pokemon(PokemonBaseList.GetPokemonBase(PokemonName), lvl);
        image.sprite = Pokemon.Base.FrontSprite;
        Pokemon.HP = hp;
        PlayEnterAnimation();
    }

    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
            image.transform.localPosition = new Vector3(-500f, orginalPos.y);
        else
            image.transform.localPosition = new Vector3(500f, orginalPos.y);

        image.transform.DOLocalMoveX(orginalPos.x, 1.0f);
    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
            sequence.Append(image.transform.DOLocalMoveX(orginalPos.x + 50, 0.25f));
        else
            sequence.Append(image.transform.DOLocalMoveX(orginalPos.x - 50, 0.25f));

        sequence.Append(image.transform.DOLocalMoveX(orginalPos.x, 0.25f));
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(orginalColor, 0.1f));
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(orginalPos.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0.0f, 0.5f));
    }
}
