using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dmicade
{
    [RequireComponent(typeof(ContentDisplayManager))]
    public class GridOffsetController : MonoBehaviour
    {
        public GameObject topGrid;
        public GameObject bottomGrid;

        public float moveFactor = 1.0f;

        private ContentDisplayManager _cdm;
        private Material _staticTopGridMaterial; 
        private Material _staticBottomGridMaterial;
        private int _offsetShaderId;

        private void Awake()
        {
            _cdm = gameObject.GetComponent<ContentDisplayManager>();

            _cdm.OnScrollStart += MoveAccelerate;
            _cdm.OnScrollEnd += MoveDecelerate;
            _cdm.OnScrollContinueOnStart += MoveContinue;
            _cdm.OnScrollContinueOnEnd += MoveContinue;

            _staticBottomGridMaterial = bottomGrid.GetComponent<Renderer>().material;
            _staticTopGridMaterial = topGrid.GetComponent<Renderer>().material;
            
            _offsetShaderId = Shader.PropertyToID("_Offset");
        }

        private void MoveAccelerate(Vector3 distance) =>
            MoveGrids(distance, _cdm.accelerationTime, _cdm.accelerationType);

        private void MoveDecelerate(Vector3 distance) =>
            MoveGrids(distance, _cdm.decelerationTime, _cdm.decelerationType);

        private void MoveContinue(Vector3 distance) => MoveGrids(distance,
            _cdm.decelerationTime, LeanTweenType.linear);

        private void MoveGrids(Vector3 distance, float duration, LeanTweenType easeType) 
        {
            Vector2 offsetStartBottom = _staticBottomGridMaterial.GetVector(_offsetShaderId);
            Vector2 offsetStartTop = _staticTopGridMaterial.GetVector(_offsetShaderId);
            Vector2 destBottom = offsetStartBottom + new Vector2(distance.x, distance.z) * moveFactor;
            Vector2 destTop = offsetStartTop + new Vector2(-distance.x, distance.z) * moveFactor;

            LeanTween.value(gameObject, MoveBottomGrid, offsetStartBottom, destBottom, duration).setEase(easeType);
            LeanTween.value(gameObject, MoveTopGrid, offsetStartTop, destTop, duration).setEase(easeType);
        }
        private void MoveBottomGrid(Vector2 offset) => _staticBottomGridMaterial.SetVector(_offsetShaderId, offset);

        private void MoveTopGrid(Vector2 offset) => _staticTopGridMaterial.SetVector(_offsetShaderId, offset);
        
    }
    
}
