using UnityEngine;
using UnityEngine.UIElements;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    UIChannelSliderElement.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public class UIChannelSliderElement : UIBlitSliderBase
    {
        private readonly Label _prefixLabel;
        private readonly FloatField _valueField;
        private readonly IntegerField _intValueField;

        private float _min;
        private float _max;
        private float _currentRawValue;
        private bool _asInt;
        private int _shaderModeId;
        private Vector4 _currentInColor;

        private const float TRACK_HEIGHT = 20f;
        private const float KNOB_SIZE = 14f;

        public UIChannelSliderElement() : base()
        {
            AddToClassList("channel-slider__row");

            _prefixLabel = new Label();
            _prefixLabel.AddToClassList("channel-slider__label");
            Add(_prefixLabel);

            var track = new VisualElement();
            track.AddToClassList("channel-slider__track");
            track.style.height = TRACK_HEIGHT;

            track.RegisterCallback<PointerDownEvent>(OnTrackPointerDown);
            track.RegisterCallback<PointerMoveEvent>(OnTrackPointerMove);
            track.RegisterCallback<PointerUpEvent>(OnTrackPointerUp);
            track.RegisterCallback<GeometryChangedEvent>(OnTrackGeometryChanged);
            Add(track);

            InitializeTrack(track, KNOB_SIZE);
            KnobElement.style.top = (TRACK_HEIGHT - KNOB_SIZE) / 2f;

            _valueField = new FloatField();
            _valueField.AddToClassList("channel-slider__field");
            _valueField.RegisterValueChangedCallback(evt => UpdateFromField(evt.newValue));
            Add(_valueField);

            _intValueField = new IntegerField();
            _intValueField.AddToClassList("channel-slider__field");
            _intValueField.RegisterValueChangedCallback(evt => UpdateFromField(evt.newValue));
            Add(_intValueField);
        }

        public void Setup(string label, float value, float min, float max, int shaderModeId, bool asInt)
        {
            _prefixLabel.text = label;
            _min = min;
            _max = max;
            _shaderModeId = shaderModeId;
            _asInt = asInt;

            _valueField.style.display = _asInt ? DisplayStyle.None : DisplayStyle.Flex;
            _intValueField.style.display = _asInt ? DisplayStyle.Flex : DisplayStyle.None;
            _currentInColor = Vector4.one;

            SetRawValueWithoutNotify(value);
        }

        public void UpdateShaderTrack(Vector4 inColor)
        {
            _currentInColor = inColor;
            float width = TrackElement.layout.width;
            if (width <= 0f || float.IsNaN(width)) return;

            BlitTrackTexture((int)width, (int)TRACK_HEIGHT, _currentInColor, _shaderModeId, isHorizontal: true);
        }

        public void SetRawValueWithoutNotify(float value)
        {
            _currentRawValue = Mathf.Clamp(value, _min, _max);

            if (_asInt) _intValueField.SetValueWithoutNotify(Mathf.RoundToInt(_currentRawValue));
            else _valueField.SetValueWithoutNotify(_currentRawValue);

            RepositionKnob();
        }

        private void OnTrackGeometryChanged(GeometryChangedEvent evt)
        {
            float width = evt.newRect.width;
            if (width <= 1f) return;

            BlitTrackTexture((int)width, (int)TRACK_HEIGHT, _currentInColor, _shaderModeId, isHorizontal: true);
            RepositionKnob();
        }

        private void RepositionKnob()
        {
            float trackWidth = TrackElement.layout.width;
            if (trackWidth <= 0f) return;

            float percentage = Mathf.InverseLerp(_min, _max, _currentRawValue);
            float targetLeft = Mathf.Lerp(0f, trackWidth - KNOB_SIZE, percentage);
            KnobElement.style.left = targetLeft;
        }

        private void UpdateFromDragPosition(Vector2 localPos)
        {
            float trackWidth = TrackElement.layout.width;
            if (trackWidth <= 0f) return;

            float pct = Mathf.Clamp01(localPos.x / trackWidth);
            float newValue = Mathf.Lerp(_min, _max, pct);
            if (_asInt) newValue = Mathf.RoundToInt(newValue);

            if (!Mathf.Approximately(_currentRawValue, newValue))
            {
                _currentRawValue = newValue;
                if (_asInt) _intValueField.SetValueWithoutNotify(Mathf.RoundToInt(_currentRawValue));
                else _valueField.SetValueWithoutNotify(_currentRawValue);

                RepositionKnob();
                FireValueChanged(_currentRawValue);
            }
        }

        private void UpdateFromField(float value)
        {
            float clamped = Mathf.Clamp(value, _min, _max);
            if (!Mathf.Approximately(_currentRawValue, clamped))
            {
                _currentRawValue = clamped;
                RepositionKnob();
                FireValueChanged(_currentRawValue);
            }
        }

        private void OnTrackPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;
            TrackElement.CapturePointer(evt.pointerId);
            UpdateFromDragPosition(evt.localPosition);
            evt.StopPropagation();
        }

        private void OnTrackPointerMove(PointerMoveEvent evt)
        {
            if (TrackElement.HasPointerCapture(evt.pointerId))
            {
                UpdateFromDragPosition(evt.localPosition);
                evt.StopPropagation();
            }
        }

        private void OnTrackPointerUp(PointerUpEvent evt)
        {
            if (TrackElement.HasPointerCapture(evt.pointerId))
            {
                TrackElement.ReleasePointer(evt.pointerId);
                evt.StopPropagation();
            }
        }
    }
}