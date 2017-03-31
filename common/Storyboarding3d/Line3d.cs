﻿using OpenTK;
using StorybrewCommon.Animations;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Storyboarding.Util;
using System;

namespace StorybrewCommon.Storyboarding3d
{
    public class Line3d : Object3d, HasOsbSprite
    {
        public OsbSprite Sprite { get; private set; }
        public string SpritePath;
        public bool Additive;
        public bool UseDistanceFade = true;
        
        public readonly KeyframedValue<Vector3> StartPosition = new KeyframedValue<Vector3>(InterpolatingFunctions.Vector3);
        public readonly KeyframedValue<Vector3> EndPosition = new KeyframedValue<Vector3>(InterpolatingFunctions.Vector3);
        public readonly KeyframedValue<float> Thickness = new KeyframedValue<float>(InterpolatingFunctions.Float, 1);

        public readonly CommandGenerator Generator = new CommandGenerator();

        public override void GenerateSprite(StoryboardLayer layer)
        {
            Sprite = Sprite ?? layer.CreateSprite(SpritePath, OsbOrigin.CentreLeft);
        }

        public override void GenerateKeyframes(double time, CameraState cameraState, Object3dState object3dState)
        {
            var bitmap = StoryboardObjectGenerator.Current.GetMapsetBitmap(SpritePath);

            var wvp = object3dState.WorldTransform * cameraState.ViewProjection;
            var startVector = cameraState.ToScreen(wvp, StartPosition.ValueAt(time));
            var endVector = cameraState.ToScreen(wvp, EndPosition.ValueAt(time));

            var delta = startVector - endVector;
            var angle = Math.PI + Math.Atan2(delta.Y, delta.X);
            var rotation = InterpolatingFunctions.DoubleAngle(Generator.EndState?.Rotation ?? 0, angle, 1);

            var scale = new Vector2(delta.Xy.Length / bitmap.Width, Thickness.ValueAt(time));

            var opacity = startVector.W < 0 && endVector.W < 0 ? 0 : object3dState.Opacity;
            if (UseDistanceFade) opacity *= Math.Max(cameraState.OpacityAt(startVector.W), cameraState.OpacityAt(endVector.W));

            Generator.Add(new CommandGenerator.State()
            {
                Time = time,
                Position = startVector.Xy,
                Scale = scale,
                Rotation = rotation,
                Color = object3dState.Color,
                Opacity = opacity,
            });
        }

        public override void GenerateCommands(Action<Action, OsbSprite> action)
        {
            if (Generator.GenerateCommands(Sprite, action))
            {
                if (Additive)
                    Sprite.Additive(Sprite.CommandsStartTime, Sprite.CommandsEndTime);
            }
        }
    }
}
