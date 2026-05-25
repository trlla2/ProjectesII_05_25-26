using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectManager : MonoBehaviour
{
    [SerializeField] private RectTransform _scrollContent;
    [SerializeField] private StageButton _stageButtonPrefab;
    [SerializeField] private Button _startButton;
    [SerializeField] private string _boardSceneName = "newBoard";

    [Header("Layout")]
    [SerializeField] private float _spacingY = 160f;
    [SerializeField] private float _offsetX = 100f;
    [SerializeField] private float _paddingY = 150f;

    [Header("Path")]
    [SerializeField] private RectTransform _dotPrefab;
    [SerializeField] private float _dotSpacing = 22f;

    private int _selectedStage;
    private StageButton _selectedButton;

    private void Start()
    {
        _selectedStage = GameManager.Instance.StageLevel;

        BuildStageList();

        _startButton.onClick.AddListener(StartSelectedStage);
    }

    private void BuildStageList()
    {
        int currentStage = GameManager.Instance.StageLevel;
        int maxStage = GameManager.Instance.MaxStageLevel;

        // padding arriba y abajo para que los botones extremos no queden recortados
        _scrollContent.sizeDelta = new Vector2(_scrollContent.sizeDelta.x, maxStage * _spacingY + _paddingY * 2f);

        List<Vector2> positions = new List<Vector2>();

        for (int i = 1; i <= maxStage; i++)
        {
            StageLevelData data = GameManager.Instance.GetStageData(i);
            if (data == null) continue;

            // Stage 1 abajo, maxStage arriba, desplazados _paddingY desde el borde del content
            float y = -(maxStage - i + 0.5f) * _spacingY - _paddingY;
            //odd left, even right
            float x = (i % 2 == 1) ? -_offsetX : _offsetX;
            Vector2 pos = new Vector2(x, y);
            positions.Add(pos);

            bool isLocked = i > currentStage;
            bool isDefault = i == currentStage;

            StageButton btn = Instantiate(_stageButtonPrefab, _scrollContent);
            btn.GetComponent<RectTransform>().anchoredPosition = pos;
            btn.Setup(i, data.DisplayName, isLocked, isDefault, OnStageSelected);
        }

        for (int i = 0; i < positions.Count - 1; i++)
            DrawPath(positions[i], positions[i + 1]);
    }

    private void DrawPath(Vector2 from, Vector2 to)
    {
        if (_dotPrefab == null) return;

        int count = Mathf.Max(1, Mathf.RoundToInt(Vector2.Distance(from, to) / _dotSpacing));
        for (int d = 1; d < count; d++)
        {
            float t = (float)d / count;
            RectTransform dot = Instantiate(_dotPrefab, _scrollContent);
            dot.anchoredPosition = Vector2.Lerp(from, to, t);
            dot.SetAsFirstSibling(); // set dots under the butons
        }
    }

    private void OnStageSelected(StageButton button, int stageIndex)
    {
        if (_selectedButton != null)
            _selectedButton.SetSelected(false);

        _selectedButton = button;
        _selectedButton.SetSelected(true);
        _selectedStage = stageIndex;
    }

    private void StartSelectedStage()
    {
        GameManager.Instance.SetStageLevel(_selectedStage);
        SceneTransition.Instance.LoadScene(_boardSceneName, false);
    }
}
